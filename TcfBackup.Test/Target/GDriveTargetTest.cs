using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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

    [Test]
    public void CreatesTargetDirectory()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var gDriveMock = new Mock<IGDriveAdapter>();
        gDriveMock.Setup(g => g.CreateDirectory(Directory)).Returns(DirectoryId);

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);

        _ = new GDriveTarget(logger, gDriveMock.Object, fsMock.Object, Directory);

        gDriveMock.VerifyAll();
        fsMock.VerifyAll();
    }

    [Test]
    public void UploadEachFileToDirectory()
    {
        var fileStreams = new Dictionary<string, FileStream>
        {
            { "/dev/null/file1", new FileStream("/dev/null", FileMode.Open, FileAccess.Read) },
            { "/dev/null/file2", new FileStream("/dev/null", FileMode.Open, FileAccess.Read) },
            { "/dev/null/file3", new FileStream("/dev/null", FileMode.Open, FileAccess.Read) },
        };

        var logger = new LoggerConfiguration().CreateLogger();

        var gDriveMock = new Mock<IGDriveAdapter>(MockBehavior.Strict);
        gDriveMock.Setup(g => g.CreateDirectory(Directory)).Returns(DirectoryId);
        fileStreams.ToList().ForEach(kv => gDriveMock.Setup(g => g.UploadFile(kv.Value, Path.GetFileName(kv.Key), DirectoryId, CancellationToken.None)));

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fileStreams.ToList().ForEach(kv => fsMock.Setup(fs => fs.Open(kv.Key, FileMode.Open, FileAccess.Read)).Returns(kv.Value));

        var sourceMock = new Mock<ISource>(MockBehavior.Strict);
        sourceMock.Setup(s => s.GetFiles()).Returns(fileStreams.Keys.Select(f => (IFile)new ImmutableFile(fsMock.Object, f)).ToArray());

        new GDriveTarget(logger, gDriveMock.Object, fsMock.Object, Directory).Apply(sourceMock.Object, CancellationToken.None);

        gDriveMock.VerifyAll();
        fsMock.VerifyAll();
        sourceMock.VerifyAll();
    }
}