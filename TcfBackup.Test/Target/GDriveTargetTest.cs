using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Serilog;
using TcfBackup.Filesystem;
using TcfBackup.Source;
using TcfBackup.Target;

namespace TcfBackup.Test.Target
{
    public class GDriveTargetTest
    {
        private const string Directory = "/dev/null";
        private const string DirectoryId = "some_id";
        
        [Test]
        public void CreatesTargetDirectory()
        {
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            
            var gDriveMock = new Mock<IGDriveAdapter>();
            gDriveMock.Setup(g => g.CreateDirectory(Directory)).Returns(DirectoryId);

            var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
            
            _ = new GDriveTarget(logger.Object, gDriveMock.Object, fsMock.Object, Directory);
            
            gDriveMock.VerifyAll();
            fsMock.VerifyAll();
        }

        [Test]
        public void UploadEachFileToDirectory()
        {
            var fileStreams = new Dictionary<string, Stream>
            {
                {"/dev/null/file1", new MemoryStream()},
                {"/dev/null/file2", new MemoryStream()},
                {"/dev/null/file3", new MemoryStream()},
            };
            
            var logger = new Mock<ILogger>(MockBehavior.Loose);
            
            var gDriveMock = new Mock<IGDriveAdapter>(MockBehavior.Strict);
            gDriveMock.Setup(g => g.CreateDirectory(Directory)).Returns(DirectoryId);
            fileStreams.ToList().ForEach(kv => gDriveMock.Setup(g => g.UploadFile(kv.Value, Path.GetFileName(kv.Key), DirectoryId)));

            var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
            fileStreams.ToList().ForEach(kv => fsMock.Setup(fs => fs.OpenRead(kv.Key)).Returns(kv.Value));
            
            var sourceMock = new Mock<ISource>(MockBehavior.Strict);
            sourceMock.Setup(s => s.GetFiles()).Returns(fileStreams.Keys.Select(f => (IFile)new ImmutableFile(fsMock.Object, f)).ToArray());
            
            new GDriveTarget(logger.Object, gDriveMock.Object, fsMock.Object, Directory).Apply(sourceMock.Object);
            
            gDriveMock.VerifyAll();
            fsMock.VerifyAll();
            sourceMock.VerifyAll();
        }
    }
}