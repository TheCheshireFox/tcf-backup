using System;

namespace TcfBackup.Test.Retention.RetentionScheduleTestCases;

public class MonthsRetentionScheduleTestCase : IRetentionScheduleTestCase
{
    private static readonly DateTime s_start = new(2020, 01, 01);

    public string Schedule => "3m";
    public DateTime[] Backups => new[] { s_start.AddMonths(0), s_start.AddMonths(1), s_start.AddMonths(2), s_start.AddMonths(3), s_start.AddMonths(4) };
    public DateTime[] Expected => new[] { s_start.AddMonths(2), s_start.AddMonths(3), s_start.AddMonths(4) };
}