using Mono.Unix.Native;

namespace TcfBackup.Compressor;

internal static class FifoFactory
{
    private static readonly string s_fifoDir = new Filesystem.Filesystem().CreateTempDirectory();
    private static long s_nextFifo;

    private static string FindNextAvailableFifo()
    {
        while (true)
        {
            var path = Path.Combine(s_fifoDir, s_nextFifo++.ToString());
            if (File.Exists(path))
            {
                continue;
            }

            return path;
        }
    }
    
    public static FileStream MkFifo()
    {
        string path;
        if (Syscall.mkfifo(path = FindNextAvailableFifo(), FilePermissions.DEFFILEMODE |  FilePermissions.S_IFIFO) != 0)
        {
            throw new IOException(Stdlib.strerror(Stdlib.GetLastError()));
        }

        return new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    }
}