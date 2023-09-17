using System;
using TcfBackup.Shared;

namespace TcfBackup.Test.Retention.RetentionScheduleTestCases;

public class WeeksForOneMonthRetentionScheduleTestCase : IRetentionScheduleTestCase
{
    private static DateTime GetStartOfTheWeekForYear(DateTime start)
    {
        DateTime current;
        while ((current = start.StartOfTheWeek()).Year != start.Year)
        {
            start = start.AddDays(7);
        }

        return current;
    }

    protected static readonly DateTime Start = GetStartOfTheWeekForYear(new DateTime(2020, 01, 01));

    public virtual string Schedule => "2w";
    public virtual DateTime[] Backups => new[] { Start.AddDays(7 * 0), Start.AddDays(7 * 1), Start.AddDays(7 * 2), Start.AddDays(7 * 3) };
    public virtual DateTime[] Expected => new[] { Start.AddDays(7 * 2), Start.AddDays(7 * 3) };
}