using System.Diagnostics.CodeAnalysis;

namespace TcfBackup.LibArchive.Options;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public record XzOptions(
    [property: OptionsValue("compression-level")] int CompressionLevel = 3,
    [property: OptionsValue("threads")] int Threads = 0
    ) : OptionsBase(FilterCode.Xz);