using System;
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

public class DirTargetTest
{
    private class MoveThrowableFileException : Exception
    {
            
    }
    
    private const string Directory = "/dev/null/target";

    [Test]
    public void CreatesTargetDirectory()
    {
        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Create(Directory));

        var logger = new LoggerConfiguration().CreateLogger();
        var target = new DirTarget(logger, fsMock.Object, Directory, false);
        
        var sourceMock = new Mock<IFileListSource>();
        target.Apply(sourceMock.Object, CancellationToken.None);

        fsMock.VerifyAll();
    }

    [Test]
    public void CopyEachFileToDirectoryFromFileListSource()
    {
        var files = new[] { "/dev/null/file1", "/dev/null/file2", "/dev/null/file3" };
        var expectedFiles = files.Select(f => Path.Combine(Directory, Path.GetFileName(f))).ToArray();

        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Create(Directory));
        foreach (var file in files)
        {
            fsMock.Setup(fs => fs.File.Copy(file, Path.Combine(Directory, Path.GetFileName(file)), true));
        }

        var sourceMock = new Mock<IFileListSource>(MockBehavior.Strict);
        sourceMock.Setup(s => s.GetFiles()).Returns(files.Select(f => (IFile)new ImmutableFile(fsMock.Object, f)).ToArray());

        var logger = new LoggerConfiguration().CreateLogger();
        var target = new DirTarget(logger, fsMock.Object, Directory, true);
        
        var result = target.Apply(sourceMock.Object, CancellationToken.None);
        CollectionAssert.AreEquivalent(expectedFiles, result);
    }

    [Test]
    public void CopyStreamSourceAsFile()
    {
        const string fileName = "qwerty";
        var expectedFiles = new[] { Path.Combine(Directory, fileName) };
        
        using var msSrc = new MemoryStream(new byte[]{ 0, 1, 2 ,3 ,4, 5 });
        using var msDst = new MemoryStream();
        var streamSourceMock = new Mock<IStreamSource>(MockBehavior.Strict);
        streamSourceMock.Setup(s => s.Name).Returns(fileName);
        streamSourceMock.Setup(s => s.GetStream()).Returns(msSrc);
        
        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.Directory.Create(Directory));
        fsMock.Setup(fs => fs.File.Open(Path.Combine(Directory, fileName), It.IsAny<FileMode>(), It.IsAny<FileAccess>())).Returns(msDst);
        fsMock.Setup(fs => fs.File.Open(Path.Combine(Directory, fileName), It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>())).Returns(msDst);
        
        var logger = new LoggerConfiguration().CreateLogger();
        var target = new DirTarget(logger, fsMock.Object, Directory, true);

        var result = target.Apply(streamSourceMock.Object, CancellationToken.None);
        
        CollectionAssert.AreEquivalent(expectedFiles, result);
        CollectionAssert.AreEquivalent(msSrc.ToArray(), msDst.ToArray());
    }
    
    [Test]
    public void RemovesFilesOnException()
    {
        var filesToThrow = new Dictionary<string, bool>
        {
            {"/dev/null/file1", false},
            {"/dev/null/file2", false},
            {"/dev/null/file3", true},
            {"/dev/null/file4", true},
        };

        var fsMock = new Mock<IFileSystem>();
        fsMock.Setup(fs => fs.Directory.Create(Directory));

        var prevThrow = false;
        foreach (var (file, @throw) in filesToThrow)
        {
            if (prevThrow)
            {
                continue;
            }
            
            var setup = fsMock.Setup(fs => fs.File.Copy(file, Path.Combine(Directory, Path.GetFileName(file)), true));
            if (@throw)
            {
                setup.Throws<MoveThrowableFileException>();
                prevThrow = true;
            }
            else
            {
                fsMock.Setup(fs => fs.File.Delete(Path.Combine(Directory, Path.GetFileName(file))));
            }
        }

        var sourceMock = new Mock<IFileListSource>(MockBehavior.Strict);
        sourceMock.Setup(s => s.GetFiles()).Returns(filesToThrow.Keys.Select(f => new ImmutableFile(fsMock.Object, f)));

        var logger = new LoggerConfiguration().CreateLogger();
        var target = new DirTarget(logger, fsMock.Object, Directory, true);

        Assert.Catch<MoveThrowableFileException>(() => target.Apply(sourceMock.Object, CancellationToken.None));
        fsMock.VerifyAll();
    }
}