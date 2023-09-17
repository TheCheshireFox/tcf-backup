using System;

namespace TcfBackup.Test.Retention.RetentionScheduleTestCases;

public class WeeksMultipleDaysForOneMonthRetentionScheduleTestCase : WeeksForOneMonthRetentionScheduleTestCase
{
    public override string Schedule => "2w";

    public override DateTime[] Backups => new[]
    {
        Start.AddDays(7 * 0), Start.AddDays(7 * 0).AddDays(1), Start.AddDays(7 * 0).AddDays(2),
        Start.AddDays(7 * 1), Start.AddDays(7 * 1).AddDays(2), Start.AddDays(7 * 1).AddDays(3),
        Start.AddDays(7 * 2), Start.AddDays(7 * 2).AddDays(3), Start.AddDays(7 * 2).AddDays(4)
    };

    public override DateTime[] Expected => new[] { Start.AddDays(7 * 1).AddDays(3), Start.AddDays(7 * 2).AddDays(4) };
}