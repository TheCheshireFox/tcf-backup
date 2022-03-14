using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Shared;

namespace TcfBackup.Managers
{
    public class TarCompressionManager : ICompressionManager
    {
        private readonly ILogger _logger;
        private readonly IFilesystem _filesystem;

        private static string AlgorithmToSwitch(CompressAlgorithm algorithm) => algorithm switch
        {
            CompressAlgorithm.Gzip => "--gzip",
            CompressAlgorithm.Lzma => "--lzma",
            CompressAlgorithm.Lzop => "--lzop",
            CompressAlgorithm.Xz => "--xz",
            CompressAlgorithm.BZip2 => "--bzip2",
            CompressAlgorithm.LZip => "--lzip",
            CompressAlgorithm.ZStd => "--zstd",
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };

        private static IDictionary<string, string>? AlgorithmToEnv(CompressAlgorithm algorithm) => algorithm switch
        {
            CompressAlgorithm.Gzip => new Dictionary<string, string>{{ "GZIP", "-9" }},
            CompressAlgorithm.BZip2 => new Dictionary<string, string>{{ "BZIP", "-9" }},
            CompressAlgorithm.Xz => new Dictionary<string, string>{{ "XZ_OPT", "-9" }},
            CompressAlgorithm.Lzma => new Dictionary<string, string>{{ "XZ_OPT", "-9" }},
            CompressAlgorithm.LZip => new Dictionary<string, string>{{ "XZ_OPT", "-9" }},
            CompressAlgorithm.Lzop => null,
            CompressAlgorithm.ZStd => new Dictionary<string, string>{{ "ZSTD_CLEVEL", "19" }},
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };

        public TarCompressionManager(ILogger logger, IFilesystem filesystem)
        {
            _logger = logger.ForContextShort<TarCompressionManager>();
            _filesystem = filesystem;
        }

        public void Compress(CompressAlgorithm algorithm, string archive, string[] files, string? changeDir = null, bool followSymlinks = false, CancellationToken cancellationToken = default)
        {
            var filesFile = _filesystem.CreateTempFile();

            try
            {
                File.WriteAllLines(filesFile, string.IsNullOrEmpty(changeDir) ? files : files.Select(f => PathUtils.GetRelativePath(changeDir, f)));

                var args = new List<string>();
                
                if (!string.IsNullOrEmpty(changeDir))
                {
                    args.Add($"-C \"{changeDir}\"");
                }
                
                args.Add($"-T \"{filesFile}\"");

                if (followSymlinks)
                {
                    args.Add("-h");
                }
                
                args.Add(AlgorithmToSwitch(algorithm));
                args.Add($"-cvf \"{archive}\"");

                if (!string.IsNullOrEmpty(changeDir))
                {
                    //args.Add(".");
                }
                
                Subprocess.Exec("tar", string.Join(' ', args), _logger.GetProcessRedirects(), AlgorithmToEnv(algorithm), cancellationToken);
            }
            finally
            {
                _filesystem.Delete(filesFile);
            }
        }

        public IEnumerable<string> Decompress(string archive, string destination, CancellationToken cancellationToken = default)
        {
            var files = new List<string>();

            _filesystem.CreateDirectory(destination);
            
            void ProcessRedirects(StreamWriter input, StreamReader output, StreamReader error)
            {
                var outputTask = Task.Factory.StartNew(() =>
                {
                    while (!output.EndOfStream)
                    {
                        var line = output.ReadLine();
                        if (line != null)
                        {
                            files.Add(line);
                            _logger.Information("{file}", line);
                        }
                    }
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously);
                
                var errorTask = Task.Factory.StartNew(() =>
                {
                    while (!error.EndOfStream)
                    {
                        var line = error.ReadLine();
                        if (line != null)
                        {
                            _logger.Error("{file}", line);
                        }
                    }
                }, TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously);

                Task.WaitAll(new[] { outputTask, errorTask }, cancellationToken);
            }

            Subprocess.Exec("tar", $"-xvapf \"{archive}\" -C \"{destination}\"", ProcessRedirects, cancellationToken: cancellationToken);

            return files.Select(f => Path.GetFullPath(Path.Join(destination, f)));
        }
    }
}