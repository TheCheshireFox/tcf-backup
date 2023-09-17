using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
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
using TcfBackup.Shared.ProgressLogger;

namespace TcfBackup;

public class GDriveAdapter : IGDriveAdapter
{
    private class ExceptionCodeReceiver : ICodeReceiver
    {
        public Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url, CancellationToken taskCancellationToken)
        {
            return Task.FromException<AuthorizationCodeResponseUrl>(new UnauthorizedAccessException("Access denied to google drive, use google-auth to authenticate"));
        }

        public string RedirectUri => "";
    }

    private class LocalhostCodeReceiver : ICodeReceiver
    {
        public Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url, CancellationToken taskCancellationToken)
        {
            Console.WriteLine("Please visit url below:");
            Console.WriteLine(url.Build().ToString());
            
            while (true)
            {
                Console.WriteLine("Please enter redirect url:");
                var redirectUrlStr = Console.ReadLine();

                try
                {
                    var redirectUrl = new Uri(redirectUrlStr!);
                    var query = HttpUtility.ParseQueryString(redirectUrl.Query);

                    return Task.FromResult(new AuthorizationCodeResponseUrl { Code = query["code"] });
                }
                catch (Exception)
                {
                    // NOP
                }
            }
        }

        public string RedirectUri => "http://localhost:1";
    }

    private const string AppName = "Backup Service Client";
    private const string TokensDirectory = $"{AppEnvironment.TcfPersistentDirectory}/tokens";

    private static readonly string[] s_scopes = { DriveService.Scope.DriveFile };

    private readonly ILogger _logger;
    private readonly IProgressLoggerFactory _progressLoggerFactory;
    private Lazy<DriveService> _driveService = new(() => Authenticate(new ExceptionCodeReceiver()));

    private static DriveService Authenticate(ICodeReceiver codeReceiver)
    {
        var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.ClientSecrets,
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

    private static IEnumerable<string> GetPathParts(string path)
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

        return parts;
    }

    private IList<Google.Apis.Drive.v3.Data.File> GetFiles()
    {
        var listRequest = _driveService.Value.Files.List();
        listRequest.Spaces = "drive";
        listRequest.Fields = "files(id, name, parents, mimeType)";
        listRequest.Q = "trashed = false";

        return listRequest.Execute().Files;
    }

    private string? GetDirectoryId(string? directoryPath)
    {
        if (directoryPath == null)
        {
            return null;
        }

        var parts = GetPathParts(directoryPath);
        var files = GetFiles();

        string? directoryId = null;
        foreach (var part in parts.Reverse())
        {
            var file = files.FirstOrDefault(f => f.Name == part && f.IsSubdirectoryOf(directoryId));
            if (file == null)
            {
                break;
            }

            directoryId = file.Id;
        }

        return directoryId;
    }

    private async Task<Google.Apis.Drive.v3.Data.File> GetFileAsync(string name, string? parentDirectoryId = null, CancellationToken cancellationToken = default)
    {
        var listRequest = _driveService.Value.Files.List();
        listRequest.Spaces = "drive";
        listRequest.Fields = "files(id, name, parents, webContentLink)";
        listRequest.Q = "trashed = false";

        var file = (await listRequest.ExecuteAsync(cancellationToken))
            .Files.FirstOrDefault(f => f.Name == name && (parentDirectoryId == null || f.Parents.Contains(parentDirectoryId)));
        if (file == null)
        {
            throw new FileNotFoundException("File not found", name);
        }

        return file;
    }

    public GDriveAdapter(ILogger logger, IProgressLoggerFactory progressLoggerFactory, IFileSystem fs)
    {
        _logger = logger.ForContextShort<GDriveAdapter>();
        _progressLoggerFactory = progressLoggerFactory;
        fs.Directory.Create(TokensDirectory);
    }

    public void Authorize()
    {
        _driveService = new Lazy<DriveService>(Authenticate(new LocalhostCodeReceiver()));
        _driveService.Value.About.Get();
    }

    public string? CreateDirectory(string path)
    {
        path = Path.TrimEndingDirectorySeparator(path);

        var parts = GetPathParts(path);
        var files = GetFiles();

        using var enumerator = parts.Reverse().GetEnumerator();

        string? directoryId = null;
        while (enumerator.MoveNext())
        {
            var file = files.FirstOrDefault(f => f.Name == enumerator.Current && f.IsSubdirectoryOf(directoryId));
            if (file == null)
            {
                break;
            }

            directoryId = file.Id;
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (enumerator.Current == null)
        {
            return directoryId;
        }

        do
        {
            directoryId = _driveService.Value.Files.CreateFolder(enumerator.Current, directoryId);
        } while (enumerator.MoveNext());

        return directoryId;
    }

    public async Task UploadFileAsync(Stream stream, string name, string? parentDirectoryId = null, CancellationToken cancellationToken = default)
    {
        var cmu = _driveService.Value.Files.Create(new Google.Apis.Drive.v3.Data.File
        {
            Name = name,
            Parents = parentDirectoryId != null ? new List<string> { parentDirectoryId } : null
        }, stream, "");

        var progressLogger = _progressLoggerFactory.Create(ProgressLogger.GetThreshold(stream));
        progressLogger.OnProgress += bytesSent => _logger.Information("Transferred: {Total}", StringExtensions.FormatBytes(bytesSent));
        cmu.ProgressChanged += uploadProgress => progressLogger.Set(uploadProgress.BytesSent);

        var uploadProgress = await cmu.UploadAsync(cancellationToken);

        if (uploadProgress.Status != UploadStatus.Completed)
        {
            throw uploadProgress.Exception ?? new Exception();
        }
    }

    public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var parentDirectory = Path.GetDirectoryName(path);
        var parentDirectoryId = GetDirectoryId(parentDirectory);
        try
        {
            await GetFileAsync(Path.GetFileName(path), parentDirectoryId, cancellationToken);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
    }

    public async Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var parentDirectory = Path.GetDirectoryName(path);
        var parentDirectoryId = GetDirectoryId(parentDirectory);
        var file = await GetFileAsync(Path.GetFileName(path), parentDirectoryId, cancellationToken);

        await _driveService.Value.Files.Delete(file.Id).ExecuteAsync(cancellationToken);
    }
}