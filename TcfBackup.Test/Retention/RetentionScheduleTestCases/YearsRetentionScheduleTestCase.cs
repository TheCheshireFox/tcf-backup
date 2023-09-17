using System;

namespace TcfBackup.Test.Retention.RetentionScheduleTestCases;

public class YearsRetentionScheduleTestCase : IRetentionScheduleTestCase
{
    private static readonly DateTime s_start = new(2020, 01, 01);

    public string Schedule => "3y";
    public DateTime[] Backups => new[] { s_start.AddYears(0), s_start.AddYears(1), s_start.AddYears(2), s_start.AddYears(3), s_start.AddYears(4) };
    public DateTime[] Expected => new[] { s_start.AddYears(2), s_start.AddYears(3), s_start.AddYears(4) };
}