using System.IO;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Source;

namespace TcfBackup.Test.Source;

public class DirSourceTest
{
    private const string Directory = "/dev/null";

    [Test]
    public void ThrowsIfDirectoryNotExists()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(Directory)).Returns(false);

        Assert.Catch<DirectoryNotFoundException>(() => _ = new DirSource(logger, fsMock.Object, Directory));
    }

    [Test]
    public void PrepareDoNothing()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(Directory)).Returns(true);

        var source = new DirSource(logger, fsMock.Object, Directory);

        source.Prepare(CancellationToken.None);

        fsMock.VerifyAll();
    }

    [Test]
    public void CleanupDoNothing()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(Directory)).Returns(true);

        var source = new DirSource(logger, fsMock.Object, Directory);

        source.Cleanup(CancellationToken.None);

        fsMock.VerifyAll();
    }

    [Test]
    public void GetFilesListFilesInDirectory()
    {
        var files = new[] { "/dev/null/1", "/dev/null/2", "/dev/null/3" };

        var logger = new LoggerConfiguration().CreateLogger();

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(Directory)).Returns(true);
        fsMock.Setup(fs => fs.Directory.GetFiles(Directory, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(files.ToArray());

        var source = new DirSource(logger, fsMock.Object, Directory);

        source.Prepare(CancellationToken.None);

        Assert.That(source.GetFiles().Select(f => f.Path), Is.EquivalentTo(files));

        fsMock.VerifyAll();
    }

    [Test]
    public void GetFilesReturnsImmutableFiles()
    {
        var files = new[] { "/dev/null/1", "/dev/null/2", "/dev/null/3" };

        var logger = new LoggerConfiguration().CreateLogger();

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(Directory)).Returns(true);
        fsMock.Setup(fs => fs.Directory.GetFiles(Directory, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(files.ToArray());

        var source = new DirSource(logger, fsMock.Object, Directory);

        source.Prepare(CancellationToken.None);

        Assert.That(source.GetFiles().All(f => f is ImmutableFile));

        fsMock.VerifyAll();
    }
}