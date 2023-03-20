using System.Reflection;
using System.Text;

namespace TcfBackup.LibArchive.Options;

public abstract record OptionsBase
{
    private static readonly IReadOnlyDictionary<FilterCode, string> s_moduleMapping = new Dictionary<FilterCode, string>
    {
        { FilterCode.BZip, "bzip" },
        { FilterCode.GZip, "gzip" },
        { FilterCode.Xz, "xz" },
    };

    private readonly OptionsAttribute _attr;

    public FilterCode FilterCode => _attr.FilterCode;

    public OptionsBase()
    {
        _attr = GetType().GetCustomAttribute<OptionsAttribute>() ?? throw new Exception("Archiver option hasn't OptionsAttribute");
    }
    
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