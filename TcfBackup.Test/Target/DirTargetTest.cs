// using System.IO;
// using System.Linq;
// using System.Threading;
// using Moq;
// using NUnit.Framework;
// using TcfBackup.Filesystem;
// using TcfBackup.Source;
// using TcfBackup.Target;
//
// namespace TcfBackup.Test.Target;
//
// public class DirTargetTest
// {
//     private const string Directory = "/dev/null";
//
//     [Test]
//     public void CreatesTargetDirectory()
//     {
//         var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
//         fsMock.Setup(fs => fs.CreateDirectory(Directory));
//
//         _ = new DirTarget(fsMock.Object, Directory, false);
//
//         fsMock.VerifyAll();
//     }
//
//     [Test]
//     public void CopyEachFileToDirectory()
//     {
//         var files = new[] { "/dev/null/file1", "/dev/null/file2", "/dev/null/file3" };
//
//         var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
//         fsMock.Setup(fs => fs.CreateDirectory(Directory));
//         foreach (var file in files)
//         {
//             fsMock.Setup(fs => fs.CopyFile(file, Path.Combine(Directory, Path.GetFileName(file)), true));
//         }
//
//         var sourceMock = new Mock<ISource>();
//         sourceMock.Setup(s => s.GetFiles()).Returns(files.Select(f => (IFile)new ImmutableFile(fsMock.Object, f)).ToArray());
//
//         new DirTarget(fsMock.Object, Directory, true).Apply(sourceMock.Object, CancellationToken.None);
//
//         fsMock.VerifyAll();
//         sourceMock.VerifyAll();
//     }
// }