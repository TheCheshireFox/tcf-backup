using System;
using TcfBackup.LibArchive.Options;
using TcfBackup.LibArchive.Tar;

namespace TcfBackup.Factory;

public static class TarCompressOptionsMapper
{
    public static TarOptions Map(TcfBackup.Configuration.Action.TarOptions? opts) => new()
    {
        ChangeDir = opts?.ChangeDir
    };

    public static OptionsBase Map<TOptions>(TOptions? opts) => opts switch
    {
        TcfBackup.Configuration.Action.GZipOptions gZipOptions => new GZipOptions(gZipOptions.Level),
        TcfBackup.Configuration.Action.BZip2Options bZip2Options => new BZip2Options(bZip2Options.Level),
        TcfBackup.Configuration.Action.XzOptions xzOptions => new XzOptions(xzOptions.Level, (int)(xzOptions.Threads ?? 0)),
        null => throw new Exception("BUG: Null option passed to mapper"),
        _ => throw new Exception($"BUG: Unknown option {opts.GetType()}")
    };
}