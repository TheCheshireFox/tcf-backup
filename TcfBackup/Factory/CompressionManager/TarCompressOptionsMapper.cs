using TcfBackup.Configuration.Action.CompressAction;
using TcfBackup.LibArchive.Options;
using TcfBackup.LibArchive.Tar;
using BZip2Options = TcfBackup.LibArchive.Options.BZip2Options;
using GZipOptions = TcfBackup.LibArchive.Options.GZipOptions;
using XzOptions = TcfBackup.LibArchive.Options.XzOptions;

namespace TcfBackup.Factory.CompressionManager;

public static class TarCompressOptionsMapper
{
    public static TarOptions Map(TarCompressActionOptions? opts) => new()
    {
        ChangeDir = opts?.ChangeDir
    };

    public static OptionsBase Map<TOptions>(TOptions? opts) => opts switch
    {
        Configuration.Action.CompressAction.GZipOptions gZipOptions => new GZipOptions(gZipOptions.Level),
        Configuration.Action.CompressAction.BZip2Options bZip2Options => new BZip2Options(bZip2Options.Level),
        Configuration.Action.CompressAction.XzOptions xzOptions => new XzOptions(xzOptions.Level, (int)(xzOptions.Threads ?? 0)),
        null => throw new Exception("BUG: Null option passed to mapper"),
        _ => throw new Exception($"BUG: Unknown option {opts.GetType()}")
    };
}