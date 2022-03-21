using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Serilog;
using TcfBackup.Action;
using TcfBackup.Factory;
using TcfBackup.Filesystem;
using TcfBackup.Shared;
using TcfBackup.Source;
using TcfBackup.Target;

namespace TcfBackup.Test.Scenarios;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSingletonMock<T>(this IServiceCollection sc, Action<Mock<T>>? mockAction = null)
        where T : class
    {
        var mock = new Mock<T>();
        mockAction?.Invoke(mock);
        return sc.AddSingleton(_ => mock.Object);
    }

    public static IServiceCollection AddSingletonMock<T>(this IServiceCollection sc, Mock<T> mock)
        where T : class
    {
        return sc.AddSingleton(_ => mock.Object);
    }
}

[Description("dir-filter-dir")]
public class Scenarios
{
    [Test]
    public void DirToDirScenario()
    {
        const string sourceDir = "/dev/null/src";
        const string dstDir = "/dev/null/dst";

        var srcFiles = new[] { "/dev/null/src/f1", "/dev/null/src/f2", "/dev/null/src/f3" };
        var dstFiles = new Dictionary<string, string>
        {
            { "/dev/null/src/f1", "/dev/null/dst/f1" },
            { "/dev/null/src/f3", "/dev/null/dst/f3" }
        };

        var logger = new LoggerConfiguration().CreateLogger();

        var fsMock = new Mock<IFilesystem>(MockBehavior.Strict);
        fsMock.Setup(fs => fs.GetFiles(sourceDir, It.IsAny<bool>(), false)).Returns(srcFiles);
        fsMock.Setup(fs => fs.DirectoryExists(sourceDir)).Returns(true);
        fsMock.Setup(fs => fs.CreateDirectory(dstDir));
        dstFiles.ToList().ForEach(kv => fsMock.Setup(fs => fs.CopyFile(kv.Key, kv.Value, It.IsAny<bool>())));

        var factoryMock = new Mock<IFactory>(MockBehavior.Strict);
        factoryMock.Setup(f => f.GetSource()).Returns(new DirSource(logger, fsMock.Object, sourceDir));
        factoryMock.Setup(f => f.GetTarget()).Returns(new DirTarget(fsMock.Object, dstDir, true));
        factoryMock.Setup(f => f.GetActions()).Returns(new[] { new FilterAction(logger, Array.Empty<string>(), new[] { "f2" }, false) });

        var sp = new ServiceCollection()
            .AddSingleton<ILogger>(_ => logger)
            .AddSingletonMock(fsMock)
            .AddSingletonMock(factoryMock)
            .BuildServiceProvider();

        sp.CreateService<BackupManager>().Backup();

        fsMock.VerifyAll();
        factoryMock.VerifyAll();
    }
}