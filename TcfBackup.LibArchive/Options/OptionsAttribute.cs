namespace TcfBackup.LibArchive.Options;

public class OptionsAttribute : Attribute
{
    public FilterCode FilterCode { get; }
    public OptionsAttribute(FilterCode filterCode)
    {
        FilterCode = filterCode;
    }
}