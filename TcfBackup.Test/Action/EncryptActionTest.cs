using System.Linq;
using Moq;
using NUnit.Framework;
using TcfBackup.Action;
using TcfBackup.Filesystem;
using TcfBackup.Managers;
using TcfBackup.Source;

namespace TcfBackup.Test.Action
{
    public class EncryptActionTest
    {
        [Test]
        public void EncryptWithPassphrase()
        {
            const string passphrase = "1q2w3e4r";
            var files = new[] { (Src: "/dev/null/file1", Dst: "/dev/null/tmp/file1"), (Src: "/dev/null/file2", Dst: "/dev/null/tmp/file2") };
            
            var encryptionManagerMock = new Mock<IEncryptionManager>(MockBehavior.Strict);
            files.ToList().ForEach(f => encryptionManagerMock.Setup(m => m.EncryptWithPassphrase(passphrase, f.Src, f.Dst)));

            var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
            fsMock.Setup(fs => fs.CreateTempDirectory()).Returns("/dev/null/tmp");
            fsMock.Setup(fs => fs.Delete("/dev/null/tmp"));

            var source = new Mock<ISource>(MockBehavior.Strict);
            source.Setup(s => s.GetFiles()).Returns(files.Select(f => (IFile)new ImmutableFile(fsMock.Object, f.Src)).ToArray());
            
            var action = EncryptAction.CreateWithPassphrase(encryptionManagerMock.Object, fsMock.Object, passphrase);
            CollectionAssert.AreEquivalent(files.Select(f => f.Dst), action.Apply(source.Object).GetFiles().Select(f => f.Path));
        }
        
        [Test]
        public void EncryptWithPassphraseFile()
        {
            const string passphraseFile = "/dev/null/pf";
            const string passphrase = "1q2w3e4r";
            var files = new[] { (Src: "/dev/null/file1", Dst: "/dev/null/tmp/file1"), (Src: "/dev/null/file2", Dst: "/dev/null/tmp/file2") };
            
            var encryptionManagerMock = new Mock<IEncryptionManager>(MockBehavior.Strict);
            files.ToList().ForEach(f => encryptionManagerMock.Setup(m => m.EncryptWithKeyFile(passphraseFile, f.Src, f.Dst)));

            var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
            fsMock.Setup(fs => fs.CreateTempDirectory()).Returns("/dev/null/tmp");
            fsMock.Setup(fs => fs.Delete("/dev/null/tmp"));
            fsMock.Setup(fs => fs.ReadAllText(passphraseFile)).Returns(passphrase);

            var source = new Mock<ISource>(MockBehavior.Strict);
            source.Setup(s => s.GetFiles()).Returns(files.Select(f => (IFile)new ImmutableFile(fsMock.Object, f.Src)).ToArray());
            
            var action = EncryptAction.CreateWithPassphraseFile(encryptionManagerMock.Object, fsMock.Object, passphraseFile);
            CollectionAssert.AreEquivalent(files.Select(f => f.Dst), action.Apply(source.Object).GetFiles().Select(f => f.Path));
        }
    }
}