namespace TcfBackup.LibArchive.Options;

[Options(FilterCode.Xz)]
public record XzOptions(
    [property: OptionsValue("compression-level")] int CompressionLevel = 3,
    [property: OptionsValue("threads")] int Threads = 0
    ) : OptionsBase;