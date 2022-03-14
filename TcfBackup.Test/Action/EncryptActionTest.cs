using System.Linq;
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
    public void Encrypt()
    {
        var files = new[] { (Src: "/dev/null/file1", Dst: "/dev/null/tmp/file1"), (Src: "/dev/null/file2", Dst: "/dev/null/tmp/file2") };

        var logger = new LoggerConfiguration().CreateLogger();

        var encryptionManagerMock = new Mock<IEncryptionManager>(MockBehavior.Strict);
        files.ToList().ForEach(f => encryptionManagerMock.Setup(m => m.Encrypt(f.Src, f.Dst)));

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.CreateTempDirectory()).Returns("/dev/null/tmp");
        fsMock.Setup(fs => fs.FileExists(It.IsAny<string?>())).Returns(false);
        fsMock.Setup(fs => fs.Delete("/dev/null/tmp"));

        var source = new Mock<ISource>(MockBehavior.Strict);
        source.Setup(s => s.GetFiles()).Returns(files.Select(f => (IFile)new ImmutableFile(fsMock.Object, f.Src)).ToArray());

        var action = new EncryptAction(logger, fsMock.Object, encryptionManagerMock.Object);
        CollectionAssert.AreEquivalent(files.Select(f => f.Dst), action.Apply(source.Object).GetFiles().Select(f => f.Path));
    }
}