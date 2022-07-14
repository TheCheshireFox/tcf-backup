using System;
using System.IO;
using System.Linq;
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

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Throws(new Exception("Unexpected call of DirectoryExists"));
        fsMock.Setup(fs => fs.DirectoryExists(Subvolume)).Returns(false);

        Assert.Catch<DirectoryNotFoundException>(() => _ = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, SnapshotDir));
    }

    [Test]
    public void ThrowsIfSnapshotDirAndHisParentDoesNotExists()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Throws(new Exception("Unexpected call of DirectoryExists"));
        fsMock.Setup(fs => fs.DirectoryExists(Subvolume)).Returns(true);
        fsMock.Setup(fs => fs.DirectoryExists(SnapshotDir)).Returns(false);
        fsMock.Setup(fs => fs.DirectoryExists(Path.GetDirectoryName(SnapshotDir))).Returns(false);

        Assert.Catch<DirectoryNotFoundException>(() => _ = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, SnapshotDir));
    }

    [Test]
    public void PrepareCreatesSnapshot()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.DirectoryExists(Subvolume)).Returns(true);
        fsMock.Setup(fs => fs.DirectoryExists(SnapshotDir)).Returns(true);

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, SnapshotDir);

        btrfsSource.Prepare();

        btrfsManagerMock.Verify(m => m.CreateSnapshot(Subvolume, SnapshotSubvolume));
    }

    [Test]
    public void PrepareCallsNothingIfSnapshotDirIsNull()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.DirectoryExists(Subvolume)).Returns(true);

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, null);

        btrfsSource.Prepare();

        btrfsManagerMock.Verify(m => m.CreateSnapshot(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void CleanupDeletesSnapshot()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.DirectoryExists(Subvolume)).Returns(true);
        fsMock.Setup(fs => fs.DirectoryExists(SnapshotDir)).Returns(true);

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, SnapshotDir);

        btrfsSource.Cleanup();

        btrfsManagerMock.Verify(m => m.DeleteSubvolume(SnapshotSubvolume));
    }

    [Test]
    public void CleanupCallsNothingIfSnapshotDirIsNull()
    {
        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.DirectoryExists(Subvolume)).Returns(true);

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, null);

        btrfsSource.Cleanup();

        btrfsManagerMock.Verify(m => m.DeleteSubvolume(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void GetFilesReturnsFilesFromSnapshotIfSnapshotDirProvided()
    {
        var files = new[] { "/dev/null/1", "/dev/null/2", "/dev/null/3" };

        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.DirectoryExists(Subvolume)).Returns(true);
        fsMock.Setup(fs => fs.DirectoryExists(SnapshotDir)).Returns(true);
        fsMock.Setup(fs => fs.GetFiles(SnapshotSubvolume, false, false)).Returns(files.ToArray());

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, SnapshotDir);

        btrfsSource.Prepare();

        Assert.That(btrfsSource.GetFiles().Select(f => f.Path), Is.EquivalentTo(files));
        fsMock.VerifyAll();
    }

    [Test]
    public void GetFilesReturnsFilesFromSubvolumeIfSnapshotDirIsNull()
    {
        var files = new[] { "/dev/null/1", "/dev/null/2", "/dev/null/3" };

        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>();

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.DirectoryExists(Subvolume)).Returns(true);
        fsMock.Setup(fs => fs.GetFiles(Subvolume, false, false)).Returns(files.ToArray());

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, null);

        btrfsSource.Prepare();

        Assert.That(btrfsSource.GetFiles().Select(f => f.Path), Is.EquivalentTo(files));
        fsMock.VerifyAll();
    }

    [Test]
    public void GetFilesReturnsFilesFromSubvolIsImmutable()
    {
        var files = new[] { "/dev/null/1", "/dev/null/2", "/dev/null/3" };

        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>(MockBehavior.Strict);
        btrfsManagerMock.Setup(bm => bm.CreateSnapshot(It.IsAny<string>(), It.IsAny<string>()));
        btrfsManagerMock.Setup(bm => bm.DeleteSubvolume(It.IsAny<string>()));

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.DirectoryExists(Subvolume)).Returns(true);
        fsMock.Setup(fs => fs.DirectoryExists(SnapshotDir)).Returns(true);
        fsMock.Setup(fs => fs.GetFiles(It.IsAny<string>(), false, false)).Returns(files);

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, SnapshotDir);

        btrfsSource.Prepare();

        Assert.That(btrfsSource.GetFiles().All(f => f is ImmutableFile));
    }

    [Test]
    public void GetFilesReturnsFilesFromSnapshotIsImmutable()
    {
        var files = new[] { "/dev/null/1", "/dev/null/2", "/dev/null/3" };

        var logger = new LoggerConfiguration().CreateLogger();

        var btrfsManagerMock = new Mock<IBtrfsManager>(MockBehavior.Strict);
        btrfsManagerMock.Setup(bm => bm.CreateSnapshot(It.IsAny<string>(), It.IsAny<string>()));
        btrfsManagerMock.Setup(bm => bm.DeleteSubvolume(It.IsAny<string>()));

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.DirectoryExists(Subvolume)).Returns(true);
        fsMock.Setup(fs => fs.GetFiles(It.IsAny<string>(), false, false)).Returns(files);

        var btrfsSource = new BtrfsSource(logger, btrfsManagerMock.Object, fsMock.Object, Subvolume, null);

        btrfsSource.Prepare();

        Assert.That(btrfsSource.GetFiles().All(f => f is ImmutableFile));
    }
}