namespace TcfBackup;

public static class FileExtensions
{
    public static bool IsSubdirectoryOf(this Google.Apis.Drive.v3.Data.File file, string? directoryId) =>
        file.MimeType == "application/vnd.google-apps.folder" &&
        (directoryId == null || file.Parents != null && file.Parents.Contains(directoryId));
}