using System.Collections.Generic;
using Google.Apis.Drive.v3;

namespace TcfBackup;

public static class FilesResourceExtensions
{
    public static string? CreateFolder(this FilesResource filesResource, string name, string? directoryId = null) =>
        filesResource.Create(new Google.Apis.Drive.v3.Data.File
        {
            Name = name,
            Parents = directoryId == null ? new List<string>() : new List<string> { directoryId },
            MimeType = "application/vnd.google-apps.folder"
        }).Execute().Id;
}