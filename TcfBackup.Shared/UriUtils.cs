namespace TcfBackup.Shared;

public static class UriUtils
{
    public static string WithoutScheme(string uri)
        => new Uri(uri).GetComponents(UriComponents.AbsoluteUri & ~ UriComponents.Scheme, UriFormat.Unescaped);
}