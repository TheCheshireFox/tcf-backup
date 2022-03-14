using System.Runtime.InteropServices;

namespace TcfBackup.Shared;

public static class RuntimeInformation
{
    public static bool IsUnix() =>
        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
        System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);
}