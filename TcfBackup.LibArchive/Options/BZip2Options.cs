using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.LibArchive.Options;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public record BZip2Options([property: OptionsValue("compression-level")] int CompressionLevel = 3) : OptionsBase(FilterCode.BZip2);