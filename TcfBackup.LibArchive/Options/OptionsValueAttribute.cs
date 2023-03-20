namespace TcfBackup.LibArchive.Options;

public class OptionsValueAttribute : Attribute
{
    public string Name { get; }
    public OptionsValueAttribute(string name) => Name = name;
}