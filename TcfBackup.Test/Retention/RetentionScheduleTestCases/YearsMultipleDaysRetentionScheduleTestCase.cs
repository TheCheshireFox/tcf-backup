using System;

namespace TcfBackup.Test.Retention.RetentionScheduleTestCases;

public class YearsMultipleDaysRetentionScheduleTestCase : IRetentionScheduleTestCase
{
    private static readonly DateTime s_start = new(2020, 01, 01);

    public string Schedule => "3y";

    public DateTime[] Backups => new[]
    {
        s_start.AddYears(0), s_start.AddYears(0).AddDays(1), s_start.AddYears(0).AddMonths(2),
        s_start.AddYears(1), s_start.AddYears(1).AddDays(3), s_start.AddYears(1).AddMonths(4),
        s_start.AddYears(2), s_start.AddYears(2).AddDays(5), s_start.AddYears(2).AddMonths(6),
        s_start.AddYears(3), s_start.AddYears(3).AddDays(7), s_start.AddYears(3).AddMonths(8),
        s_start.AddYears(4), s_start.AddYears(4).AddDays(9), s_start.AddYears(4).AddMonths(10),
    };

    public DateTime[] Expected => new[] { s_start.AddYears(2).AddMonths(6), s_start.AddYears(3).AddMonths(8), s_start.AddYears(4).AddMonths(10) };
}