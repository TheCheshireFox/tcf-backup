using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;

namespace TcfBackup
{
    public class GDriveAdapter : IGDriveAdapter
    {
        private class ExceptionCodeReceiver : ICodeReceiver
        {
            public Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url, CancellationToken taskCancellationToken)
            {
                return Task.FromException<AuthorizationCodeResponseUrl>(new UnauthorizedAccessException("Access denied to google drive, use tcf-google-drive-auth to authenticate"));
            }

            public string RedirectUri => "";
        }

        private const string AppName = "Backup Service Client";
        private const string TokensDirectory = $"{AppEnvironment.TcfPersistentDirectory}/tokens";

        private const string CredentialsResourceFile = "credentials.json";

        private static readonly string[] s_scopes = { DriveService.Scope.DriveFile };

        private readonly ILogger _logger;
        private Lazy<DriveService> _driveService = new(() => Authenticate(new ExceptionCodeReceiver()));

        private static Stream LoadEmbeddedResource(string resource)
        {
            var assembly = Assembly.GetAssembly(typeof(GDriveAdapter))!;
            var ns = typeof(GDriveAdapter).Namespace!;

            return assembly.GetManifestResourceStream($"{ns}.{resource}") ?? throw new FileNotFoundException(resource);
        }

        private static DriveService Authenticate(ICodeReceiver codeReceiver)
        {
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(LoadEmbeddedResource(CredentialsResourceFile)).Secrets,
                s_scopes,
                AppName,
                CancellationToken.None,
                new FileDataStore(TokensDirectory, true), codeReceiver).Result;

            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = AppName
            });
        }

        public GDriveAdapter(ILogger logger, IFilesystem fs)
        {
            _logger = logger.ForContextShort<GDriveAdapter>();
            fs.CreateDirectory(TokensDirectory);
        }

        public void Authorize()
        {
            _driveService = new Lazy<DriveService>(Authenticate(new PromptCodeReceiver()));
        }

        public string? CreateDirectory(string path)
        {
            var parts = new List<string>();
            while (true)
            {
                var part = Path.GetFileName(path);
                if (string.IsNullOrEmpty(part))
                {
                    break;
                }

                parts.Add(part);
                path = Path.GetDirectoryName(path) ?? string.Empty;
            }

            var listRequest = _driveService.Value.Files.List();
            listRequest.Spaces = "drive";
            listRequest.Fields = "files(id, name, parents, mimeType)";
            listRequest.Q = "trashed = false";

            var files = listRequest.Execute().Files;

            using var enumerator = ((IEnumerable<string>)parts).Reverse().GetEnumerator();

            string? directoryId = null;
            while (enumerator.MoveNext())
            {
                var file = files.FirstOrDefault(f =>
                    f.Name == enumerator.Current && f.MimeType == "application/vnd.google-apps.folder" &&
                    (directoryId == null || f.Parents != null && f.Parents.Contains(directoryId)));
                if (file == null)
                {
                    break;
                }

                directoryId = file.Id;
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (enumerator.Current == null)
            {
                return directoryId;
            }

            do
            {
                var newDir = _driveService.Value.Files.Create(new Google.Apis.Drive.v3.Data.File
                {
                    Name = enumerator.Current,
                    Parents = directoryId == null ? new List<string>() : new List<string> { directoryId },
                    MimeType = "application/vnd.google-apps.folder"
                }).Execute();

                directoryId = newDir.Id;
            } while (enumerator.MoveNext());

            return directoryId;
        }

        public void UploadFile(Stream stream, string name, string? parentDirectoryId = null)
        {
            var cmu = _driveService.Value.Files.Create(new Google.Apis.Drive.v3.Data.File
            {
                Name = name,
                Parents = parentDirectoryId != null ? new List<string> { parentDirectoryId } : null
            }, stream, "");

            const long kilobyte = 1024;
            long threshold;
            try
            {
                switch (stream.Length)
                {
                    case < 1 * 1024 * kilobyte:
                        threshold = 100 * kilobyte;
                        break;
                    case < 100 * 1024 * kilobyte:
                        threshold = 1 * 1024 * kilobyte;
                        break;
                    default:
                        threshold = 10 * 1024 * kilobyte;
                        break;
                }
            }
            catch (Exception)
            {
                threshold = 100 * kilobyte;
            }

            long lastTotal = 0;
            cmu.ProgressChanged += uploadProgress =>
            {
                if (uploadProgress.BytesSent / threshold > lastTotal / threshold)
                {
                    _logger.Information("Transferred: {total}", StringExtensions.FormatBytes(uploadProgress.BytesSent));
                }

                lastTotal = uploadProgress.BytesSent;
            };

            var uploadProgress = cmu.Upload();

            if (uploadProgress.Status != UploadStatus.Completed)
            {
                throw uploadProgress.Exception ?? new Exception();
            }
        }

        public void DownloadFile(string name, string destination, string? parentDirectoryId = null)
        {
            var listRequest = _driveService.Value.Files.List();
            listRequest.Spaces = "drive";
            listRequest.Fields = "files(id, name, parents, webContentLink)";
            listRequest.Q = "trashed = false";

            var file = listRequest.Execute().Files.FirstOrDefault(f => f.Name == name && (parentDirectoryId == null || f.Parents.Contains(parentDirectoryId)));
            if (file == null)
            {
                throw new FileNotFoundException("File not found", name);
            }

            using var dstFile = File.OpenWrite(destination);
            _driveService.Value.Files.Get(file.Id).Download(dstFile);
        }
    }
}