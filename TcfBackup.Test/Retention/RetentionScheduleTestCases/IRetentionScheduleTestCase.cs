using System;

namespace TcfBackup.Test.Retention.RetentionScheduleTestCases;

public interface IRetentionScheduleTestCase
{
    string Schedule { get; }
    DateTime[] Backups { get; }
    DateTime[] Expected { get; }
}