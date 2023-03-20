namespace TcfBackup.LibArchive.Options;

[Options(FilterCode.BZip)]
public record BZip2Options([property: OptionsValue("compression-level")] int CompressionLevel = 3) : OptionsBase;