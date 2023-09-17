using System;

namespace TcfBackup.Test.Retention.RetentionScheduleTestCases;

public class WeeksForTwoMonthRetentionScheduleTestCase : WeeksForOneMonthRetentionScheduleTestCase
{
    public override string Schedule => "4w";
    public override DateTime[] Backups => new[] { Start.AddDays(7 * 0), Start.AddDays(7 * 1), Start.AddDays(7 * 2), Start.AddDays(7 * 3), Start.AddDays(7 * 4), Start.AddDays(7 * 5), Start.AddDays(7 * 6) };
    public override DateTime[] Expected => new[] { Start.AddDays(7 * 3), Start.AddDays(7 * 4), Start.AddDays(7 * 5), Start.AddDays(7 * 6) };
}