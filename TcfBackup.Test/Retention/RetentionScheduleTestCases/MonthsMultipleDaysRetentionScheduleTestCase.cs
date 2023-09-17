using System;

namespace TcfBackup.Test.Retention.RetentionScheduleTestCases;

public class MonthsMultipleDaysRetentionScheduleTestCase : IRetentionScheduleTestCase
{
    private static readonly DateTime s_start = new(2020, 01, 01);

    public string Schedule => "3m";

    public DateTime[] Backups => new[]
    {
        s_start.AddMonths(0), s_start.AddMonths(0).AddDays(1), s_start.AddMonths(0).AddDays(2),
        s_start.AddMonths(1), s_start.AddMonths(1).AddDays(3), s_start.AddMonths(1).AddDays(4),
        s_start.AddMonths(2), s_start.AddMonths(2).AddDays(5), s_start.AddMonths(2).AddDays(6),
        s_start.AddMonths(3), s_start.AddMonths(3).AddDays(7), s_start.AddMonths(3).AddDays(8),
        s_start.AddMonths(4), s_start.AddMonths(4).AddDays(9), s_start.AddMonths(4).AddDays(10),
    };

    public DateTime[] Expected => new[] { s_start.AddMonths(2).AddDays(6), s_start.AddMonths(3).AddDays(8), s_start.AddMonths(4).AddDays(10) };
}