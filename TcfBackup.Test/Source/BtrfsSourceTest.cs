using System;
using System.IO;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Source;

namespace TcfBackup.Test.Source;

public class BtrfsSourceTest
{
    private const string Subvolume = "/dev/null/btrfs/subvolume";
    private const string SnapshotDir = "/dev/null/snapshot";
    private const string SnapshotSubvolume = "/dev/null/snapshot/subvolume";

    [Test]
    public void ThrowsIfSubvolumeDoesNotExists()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Throws(new Exception("Unexpected call of Directory.Exists"));
        fsMock.Setup(fs => fs.Directory.Exists(Subvolume)).Returns(false);

        Assert.Catch<DirectoryNotFoundException>(() => _ = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, SnapshotDir));
    }

    [Test]
    public void ThrowsIfSnapshotDirAndHisParentDoesNotExists()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(It.IsAny<string>())).Throws(new Exception("Unexpected call of Directory.Exists"));
        fsMock.Setup(fs => fs.Directory.Exists(Subvolume)).Returns(true);
        fsMock.Setup(fs => fs.Directory.Exists(SnapshotDir)).Returns(false);
        fsMock.Setup(fs => fs.Directory.Exists(Path.GetDirectoryName(SnapshotDir))).Returns(false);

        Assert.Catch<DirectoryNotFoundException>(() => _ = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, SnapshotDir));
    }

    [Test]
    public void PrepareCreatesSnapshot()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(Subvolume)).Returns(true);
        fsMock.Setup(fs => fs.Directory.Exists(SnapshotDir)).Returns(true);

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, SnapshotDir);

        btrfsSource.Prepare(CancellationToken.None);

        btrfsManagerMock.Verify(m => m.CreateSnapshot(Subvolume, SnapshotSubvolume, true));
    }

    [Test]
    public void PrepareCallsNothingIfSnapshotDirIsNull()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(Subvolume)).Returns(true);

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, null);

        btrfsSource.Prepare(CancellationToken.None);

        btrfsManagerMock.Verify(m => m.CreateSnapshot(Subvolume, It.IsAny<string>(), true), Times.Never);
    }

    [Test]
    public void CleanupDeletesSnapshot()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(Subvolume)).Returns(true);
        fsMock.Setup(fs => fs.Directory.Exists(SnapshotDir)).Returns(true);

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, SnapshotDir);

        btrfsSource.Cleanup(CancellationToken.None);

        btrfsManagerMock.Verify(m => m.DeleteSubvolume(SnapshotSubvolume));
    }

    [Test]
    public void CleanupCallsNothingIfSnapshotDirIsNull()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(Subvolume)).Returns(true);

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, null);

        btrfsSource.Cleanup(CancellationToken.None);

        btrfsManagerMock.Verify(m => m.DeleteSubvolume(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void GetFilesReturnsFilesFromSnapshotIfSnapshotDirProvided()
    {
        var files = new[] { "/dev/null/1", "/dev/null/2", "/dev/null/3" };

        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(Subvolume)).Returns(true);
        fsMock.Setup(fs => fs.Directory.Exists(SnapshotDir)).Returns(true);
        fsMock.Setup(fs => fs.Directory.GetFiles(SnapshotSubvolume, true, true, It.IsAny<bool>(), It.IsAny<bool>())).Returns(files.ToArray());

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, SnapshotDir);

        btrfsSource.Prepare(CancellationToken.None);

        Assert.That(btrfsSource.GetFiles().Select(f => f.Path), Is.EquivalentTo(files));
        fsMock.VerifyAll();
    }

    [Test]
    public void GetFilesReturnsFilesFromSubvolumeIfSnapshotDirIsNull()
    {
        var files = new[] { "/dev/null/1", "/dev/null/2", "/dev/null/3" };

        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(Subvolume)).Returns(true);
        fsMock.Setup(fs => fs.Directory.GetFiles(Subvolume, true, true, It.IsAny<bool>(), It.IsAny<bool>())).Returns(files.ToArray());

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, null);

        btrfsSource.Prepare(CancellationToken.None);

        Assert.That(btrfsSource.GetFiles().Select(f => f.Path), Is.EquivalentTo(files));
        fsMock.VerifyAll();
    }

    [Test]
    public void GetFilesReturnsFilesFromSubvolIsImmutable()
    {
        var files = new[] { "/dev/null/1", "/dev/null/2", "/dev/null/3" };

        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>(MockBehavior.Strict);
        btrfsManagerMock.Setup(bm => bm.CreateSnapshot(Subvolume, SnapshotDir, true));
        btrfsManagerMock.Setup(bm => bm.DeleteSubvolume(It.IsAny<string>()));

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(Subvolume)).Returns(true);
        fsMock.Setup(fs => fs.Directory.Exists(SnapshotDir)).Returns(true);
        fsMock.Setup(fs => fs.Directory.GetFiles(It.IsAny<string>(), true, true, It.IsAny<bool>(), It.IsAny<bool>())).Returns(files);

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, It.IsAny<string>());

        btrfsSource.Prepare(CancellationToken.None);

        Assert.That(btrfsSource.GetFiles().All(f => f is ImmutableFile));
    }

    [Test]
    public void GetFilesReturnsFilesFromSnapshotIsImmutable()
    {
        var files = new[] { "/dev/null/1", "/dev/null/2", "/dev/null/3" };

        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>(MockBehavior.Strict);
        btrfsManagerMock.Setup(bm => bm.CreateSnapshot(Subvolume, It.IsAny<string>(), true));
        btrfsManagerMock.Setup(bm => bm.DeleteSubvolume(It.IsAny<string>()));

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Exists(Subvolume)).Returns(true);
        fsMock.Setup(fs => fs.Directory.GetFiles(It.IsAny<string>(), true, true, It.IsAny<bool>(), It.IsAny<bool>())).Returns(files);

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, null);

        btrfsSource.Prepare(CancellationToken.None);

        Assert.That(btrfsSource.GetFiles().All(f => f is ImmutableFile));
    }
}