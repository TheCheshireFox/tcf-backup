namespace TcfBackup.LibArchive.Options;

[Options(FilterCode.GZip)]
public record GZipOptions([property: OptionsValue("compression-level")] int CompressionLevel = 3) : OptionsBase;