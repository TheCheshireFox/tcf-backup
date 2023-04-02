using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace TcfBackup.LibArchive.Options;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public abstract record OptionsBase(FilterCode FilterCode)
{
    private static readonly IReadOnlyDictionary<FilterCode, string> s_moduleMapping = new Dictionary<FilterCode, string>
    {
        { FilterCode.BZip2, "bzip2" },
        { FilterCode.GZip, "gzip" },
        { FilterCode.Xz, "xz" },
    };

    public sealed override string ToString()
    {
        var sb = new StringBuilder();
        
        var module = s_moduleMapping[FilterCode];
        foreach (var prop in GetType().GetProperties())
        {
            var optionName = prop.GetCustomAttribute<OptionsValueAttribute>()?.Name;
            if (optionName == null)
            {
                continue;
            }

            sb.Append($"{module}:{optionName}={prop.GetValue(this)};");
        }

        return sb.Length == 0
            ? string.Empty
            : sb.ToString(0, sb.Length - 1);
    }
}