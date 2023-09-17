using System;

namespace TcfBackup.Test.Retention.RetentionScheduleTestCases;

public class DaysRetentionScheduleTestCase : IRetentionScheduleTestCase
{
    private static readonly DateTime s_start = new(2020, 01, 01);

    public string Schedule => "3d";
    public DateTime[] Backups => new[] { s_start.AddDays(1), s_start.AddDays(2), s_start.AddDays(3), s_start.AddDays(4), s_start.AddDays(5) };
    public DateTime[] Expected => new[] { s_start.AddDays(3), s_start.AddDays(4), s_start.AddDays(5) };
}