using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Serilog;
using TcfBackup.Action;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Source;

namespace TcfBackup.Test.Action;

public class EncryptActionTest
{
    [Test]
    public async Task EncryptFilesAsync()
    {
        var files = new[] { (Src: "/dev/null/file1", Dst: "/dev/null/tmp/file1"), (Src: "/dev/null/file2", Dst: "/dev/null/tmp/file2") };
        var srcFiles = files.Select(f => f.Src).ToArray();
        var tmpFiles = files.Select(f => f.Dst).ToArray();

        var encryptionManagerMock = new Mock<IEncryptionManager>(MockBehavior.Strict);
        files.ToList().ForEach(f => encryptionManagerMock.Setup(m => m.Encrypt(f.Src, f.Dst, It.IsAny<CancellationToken>())));
        
        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.GetTempPath()).Returns("/dev/null/tmp");
        fsMock.Setup(fs => fs.File.Exists(It.IsIn(files.Select(f => f.Dst)))).Returns(false);
        fsMock.Setup(fs => fs.Directory.GetFiles("/dev/null/tmp", It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(tmpFiles);
        fsMock.Setup(fs => fs.Directory.Delete("/dev/null/tmp", It.IsAny<bool>()));

        var source = new Mock<IFileListSource>(MockBehavior.Strict);
        source.Setup(s => s.GetFiles()).Returns(srcFiles.Select(f => new ImmutableFile(fsMock.Object, f)));

        var context = new ActionContextMock(fileListSource: source.Object);
        var logger = new LoggerConfiguration().CreateLogger();
        var action = new EncryptAction(logger, fsMock.Object, encryptionManagerMock.Object);
        
        await action.ApplyAsync(context, CancellationToken.None);
        
        CollectionAssert.AreEquivalent(tmpFiles, context.GetResult<IFileListSource>().GetFiles().Select(f => f.Path));
    }
    
    [Test]
    public async Task EncryptSingleFileToStreamResultAsync()
    {
        const string src = "/dev/null/file1";

        using var ms = new MemoryStream();
        
        var fsMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.File.OpenRead(src)).Returns(ms);
        
        var encryptionManagerMock = new Mock<IEncryptionManager>(MockBehavior.Strict);
        encryptionManagerMock.Setup(m => m.Encrypt(ms, It.IsAny<Stream>(), It.IsAny<CancellationToken>()));
        
        var source = new Mock<IFileListSource>(MockBehavior.Strict);
        source.Setup(s => s.GetFiles()).Returns(new []{new ImmutableFile(fsMock.Object, src)});
        
        var context = new ActionContextMock(fileListSource: source.Object);
        var logger = new LoggerConfiguration().CreateLogger();
        var action = new EncryptAction(logger, fsMock.Object, encryptionManagerMock.Object);
        
        await action.ApplyAsync(context, CancellationToken.None);
        
        Assert.That(context.GetResult<IStreamSource>().Name == Path.GetFileName(src));
    }
}