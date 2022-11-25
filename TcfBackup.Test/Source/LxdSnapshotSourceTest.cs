using System;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Serilog;
using TcfBackup.Managers;
using TcfBackup.Source;

namespace TcfBackup.Test.Source;

public class LxdSnapshotSourceTest
{
    private static readonly string[] s_containers = { "container1", "container2", "container3" };

    private const string BackupDirectory = "/dev/null";

    [Test]
    public void ThrowsIfContainersMissing()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var manager = new Mock<ILxdManager>(MockBehavior.Strict);
        manager.Setup(m => m.ListContainers()).Returns(s_containers);

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);

        Assert.Catch<Exception>(() => _ = new LxdSnapshotSource(logger, manager.Object, fsMock.Object, new[] { "container3", "container4" }, false));
    }

    [Test]
    public void NotThrowsIfContainersMissingAndIgnoreNonExistedIsSet()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var manager = new Mock<ILxdManager>(MockBehavior.Strict);
        manager.Setup(m => m.ListContainers()).Returns(s_containers);

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);

        _ = new LxdSnapshotSource(logger, manager.Object, fsMock.Object, new[] { "container3", "container4" }, true);
    }

    [Test]
    public void PrepareCreatesBackupsOfEachContainer()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var manager = new Mock<ILxdManager>(MockBehavior.Strict);
        manager.Setup(m => m.ListContainers()).Returns(s_containers);

        foreach (var container in s_containers)
        {
            manager.Setup(m => m.BackupContainer(container, Path.Combine(BackupDirectory, container + ".tar.gz")));
        }

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.CreateTempDirectory()).Returns(BackupDirectory);

        var source = new LxdSnapshotSource(logger, manager.Object, fsMock.Object, s_containers.ToArray(), true);

        source.Prepare();

        manager.VerifyAll();
        fsMock.VerifyAll();
    }

    [Test]
    public void PrepareCreatesBackupsOnlyForExistedContainers()
    {
        var existedContainers = new[] { "container1", "container2" };

        var logger = new LoggerConfiguration().CreateLogger();

        var manager = new Mock<ILxdManager>(MockBehavior.Strict);
        manager.Setup(m => m.ListContainers()).Returns(s_containers);

        foreach (var container in existedContainers)
        {
            manager.Setup(m => m.BackupContainer(container, Path.Combine(BackupDirectory, container + ".tar.gz")));
        }

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.CreateTempDirectory()).Returns(BackupDirectory);

        var source = new LxdSnapshotSource(logger, manager.Object, fsMock.Object, s_containers, true);

        source.Prepare();

        manager.VerifyAll();
        fsMock.VerifyAll();
    }

    [Test]
    public void GetFilesListFilesInBackupDirectory()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var manager = new Mock<ILxdManager>(MockBehavior.Strict);
        manager.Setup(m => m.ListContainers()).Returns(s_containers);

        foreach (var container in s_containers)
        {
            manager.Setup(m => m.BackupContainer(container, Path.Combine(BackupDirectory, container + ".tar.gz")));
        }

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.CreateTempDirectory()).Returns(BackupDirectory);
        fsMock.Setup(fs => fs.GetFiles(BackupDirectory, false, false, false)).Returns(Array.Empty<string>());

        var source = new LxdSnapshotSource(logger, manager.Object, fsMock.Object, s_containers, true);

        source.Prepare();
        _ = source.GetFiles();

        manager.VerifyAll();
        fsMock.VerifyAll();
    }

    [Test]
    public void CleanupDeletesBackupDirectory()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var manager = new Mock<ILxdManager>(MockBehavior.Strict);
        manager.Setup(m => m.ListContainers()).Returns(s_containers);

        foreach (var container in s_containers)
        {
            manager.Setup(m => m.BackupContainer(container, Path.Combine(BackupDirectory, container + ".tar.gz")));
        }

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.CreateTempDirectory()).Returns(BackupDirectory);
        fsMock.Setup(fs => fs.DirectoryExists(BackupDirectory)).Returns(true);
        fsMock.Setup(fs => fs.Delete(BackupDirectory));

        var source = new LxdSnapshotSource(logger, manager.Object, fsMock.Object, s_containers, true);

        source.Prepare();
        source.Cleanup();

        manager.VerifyAll();
        fsMock.VerifyAll();
    }

    [Test]
    public void BackupDirectoryDeletedIfPrepareThrowsException()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var manager = new Mock<ILxdManager>(MockBehavior.Strict);
        manager.Setup(m => m.ListContainers()).Returns(s_containers);
        manager.Setup(m => m.BackupContainer(It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>();

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.CreateTempDirectory()).Returns(BackupDirectory);
        fsMock.Setup(fs => fs.DirectoryExists(BackupDirectory)).Returns(true);
        fsMock.Setup(fs => fs.Delete(BackupDirectory));

        var source = new LxdSnapshotSource(logger, manager.Object, fsMock.Object, s_containers, true);

        source.Prepare();

        manager.VerifyAll();
        fsMock.VerifyAll();
    }
}