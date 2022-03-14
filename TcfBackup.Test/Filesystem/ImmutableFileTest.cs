using Moq;
using NUnit.Framework;
using TcfBackup.Filesystem;

namespace TcfBackup.Test.Filesystem;

public class ImmutableFileTest
{
    [Test]
    public void ImmutableFileDoesNotDeleteFile()
    {
        const string fileName = "/dev/null/file";

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        var file = new ImmutableFile(fsMock.Object, fileName);

        file.Delete();

        fsMock.VerifyAll();
    }

    [Test]
    public void ImmutableFileCopyInsteadOfMove()
    {
        const string fileName = "/dev/null/file";
        const string copyFileName = "/dev/null/file_copy";

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.CopyFile(fileName, copyFileName, It.IsAny<bool>()));

        var file = new ImmutableFile(fsMock.Object, fileName);

        file.Move(copyFileName, true);
        file.Move(copyFileName, false);

        fsMock.VerifyAll();
    }
}