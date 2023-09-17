using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Source;
using TcfBackup.Target;

namespace TcfBackup.Test.Target;

public class GDriveTargetTest
{
    private const string Directory = "/dev/null";
    private const string DirectoryId = "some_id";

    private Mock<IGDriveAdapter> InitGDriveAdapterMock(IReadOnlyDictionary<string, Stream> streams)
    {
        var gDriveMock = new Mock<IGDriveAdapter>(MockBehavior.Strict);
        gDriveMock.Setup(g => g.CreateDirectory(Directory)).Returns(DirectoryId);

        foreach (var (name, stream) in streams)
        {
            gDriveMock.Setup(g => g.UploadFileAsync(stream, name, DirectoryId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        }

        return gDriveMock;
    }
    
    [Test]
    public void CreatesTargetDirectory()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var gDriveMock = new Mock<IGDriveAdapter>();
        gDriveMock.Setup(g => g.CreateDirectory(Directory)).Returns(DirectoryId);

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);

        var sourceMock = new Mock<IFileListSource>(MockBehavior.Strict);
        sourceMock.Setup(s => s.GetFiles()).Returns(Array.Empty<IFile>());
        
        var target = new GDriveTarget(logger, gDriveMock.Object, fsMock.Object, Directory);
        target.Apply(sourceMock.Object, CancellationToken.None);
    }

    [Test]
    public void UploadEachFileToDirectory()
    {
        var fileStreams = new Dictionary<string, Stream>
        {
            { "/dev/null/file1", new MemoryStream() },
            { "/dev/null/file2", new MemoryStream() },
            { "/dev/null/file3", new MemoryStream() },
        };

        var expectedFiles = fileStreams.Keys.Select(p => Path.Combine(Directory, p));

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fileStreams.ToList().ForEach(kv => fsMock.Setup(fs => fs.File.Open(kv.Key, FileMode.Open, FileAccess.Read)).Returns(kv.Value));
        
        var sourceMock = new Mock<IFileListSource>(MockBehavior.Strict);
        sourceMock.Setup(s => s.GetFiles()).Returns(fileStreams.Keys.Select(f => (IFile)new ImmutableFile(fsMock.Object, f)).ToArray());

        var gDriveMock = InitGDriveAdapterMock(fileStreams.ToDictionary(kv => Path.GetFileName(kv.Key), kv => kv.Value));

        var logger = new LoggerConfiguration().CreateLogger();
        var target = new GDriveTarget(logger, gDriveMock.Object, fsMock.Object, Directory);
        var result = target.Apply(sourceMock.Object, CancellationToken.None);
        
        CollectionAssert.AreEquivalent(expectedFiles, result);
    }

    [Test]
    public void UploadStreamToDirectory()
    {
        const string fileName = "file1";

        using var ms = new MemoryStream();
        var expectedFiles = new[] { Path.Combine(Directory, fileName) };

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        
        var sourceMock = new Mock<IStreamSource>(MockBehavior.Strict);
        sourceMock.Setup(s => s.Name).Returns(fileName);
        sourceMock.Setup(s => s.GetStream()).Returns(ms);

        var gDriveMock = InitGDriveAdapterMock(new Dictionary<string, Stream>()
        {
            [fileName] = ms
        });
        
        var logger = new LoggerConfiguration().CreateLogger();
        var target = new GDriveTarget(logger, gDriveMock.Object, fsMock.Object, Directory);
        var result = target.Apply(sourceMock.Object, CancellationToken.None);
        
        CollectionAssert.AreEquivalent(expectedFiles, result);
    }
}