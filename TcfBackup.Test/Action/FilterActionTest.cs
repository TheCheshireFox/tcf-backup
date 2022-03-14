using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Serilog;
using TcfBackup.Action;
using TcfBackup.Filesystem;
using TcfBackup.Source;

namespace TcfBackup.Test.Action
{
    public class FilterActionTest
    {
        private interface ITestSet
        {
            string[] IncludeRegex { get; }
            string[] ExcludeRegex { get; }
            string[] Files { get; }
            string[] Expected { get; }
        }

        private class IncludeTestSet1 : ITestSet
        {
            public string[] IncludeRegex => new[] { "/dev/null/[0-9]+" };
            public string[] ExcludeRegex => Array.Empty<string>();
            public string[] Files => new[] { "/dev/null/1", "/dev/null/2", "/dev/null/notInclude" };
            public string[] Expected => new[] { "/dev/null/1", "/dev/null/2" };
        }

        private class IncludeTestSet2 : ITestSet
        {
            public string[] IncludeRegex => new[] { "/dev/null/[0-9]+", "/dev/null/foo[0-9]+" };
            public string[] ExcludeRegex => Array.Empty<string>();
            public string[] Files => new[] { "/dev/null/1", "/dev/null/2", "/dev/null/foo1", "/dev/null/foo2", "/dev/null/notInclude" };
            public string[] Expected => new[] { "/dev/null/1", "/dev/null/2", "/dev/null/foo1", "/dev/null/foo2" };
        }

        private class ExcludeTestSet1 : ITestSet
        {
            public string[] IncludeRegex => Array.Empty<string>();
            public string[] ExcludeRegex => new[] { "/dev/null/[0-9]+" };
            public string[] Files => new[] { "/dev/null/1", "/dev/null/2", "/dev/null/notExclude1", "/dev/null/notExclude2" };
            public string[] Expected => new[] { "/dev/null/notExclude1", "/dev/null/notExclude2" };
        }

        private class ExcludeTestSet2 : ITestSet
        {
            public string[] IncludeRegex => Array.Empty<string>();
            public string[] ExcludeRegex => new[] { "/dev/null/[0-9]+", "/dev/null/foo[0-9]+" };
            public string[] Files => new[] { "/dev/null/1", "/dev/null/2", "/dev/null/foo1", "/dev/null/foo2", "/dev/null/notExclude1", "/dev/null/notExclude2" };
            public string[] Expected => new[] { "/dev/null/notExclude1", "/dev/null/notExclude2" };
        }

        private class IncludeExcludeTestSet : ITestSet
        {
            public string[] IncludeRegex => new[] { "/dev/null/foo[0-9]+" };
            public string[] ExcludeRegex => new[] { "/dev/null/foo[0-9]+bar" };
            public string[] Files => new[] { "/dev/null/foo1", "/dev/null/foo2", "/dev/null/foo1bar", "/dev/null/foo2bar", "/dev/null/notInclude" };
            public string[] Expected => new[] { "/dev/null/foo1", "/dev/null/foo2" };
        }

        [Test]
        [TestCase(typeof(IncludeTestSet1))]
        [TestCase(typeof(IncludeTestSet2))]
        [TestCase(typeof(ExcludeTestSet1))]
        [TestCase(typeof(ExcludeTestSet2))]
        [TestCase(typeof(IncludeExcludeTestSet))]
        public void FilterBy(Type testSetType)
        {
            var testSet = (ITestSet)Activator.CreateInstance(testSetType);

            var logger = new LoggerConfiguration().CreateLogger();

            var filter = new FilterAction(logger, testSet.IncludeRegex, testSet.ExcludeRegex, false);

            var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);

            var sourceMock = new Mock<ISource>(MockBehavior.Strict);
            sourceMock.Setup(s => s.GetFiles()).Returns(testSet.Files.Select(f => (IFile)new ImmutableFile(fsMock.Object, f)).ToArray());

            CollectionAssert.AreEquivalent(testSet.Expected, filter.Apply(sourceMock.Object).GetFiles().Select(f => f.Path));
        }
    }
}