using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.LibArchive.Options;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public record GZipOptions([property: OptionsValue("compression-level")] int CompressionLevel = 3) : OptionsBase(FilterCode.GZip);