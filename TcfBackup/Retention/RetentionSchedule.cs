using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TcfBackup.Shared;

namespace TcfBackup.Retention;

public class RetentionSchedule
{
    private const string DayType = "d";
    private const string WeekType = "w";
    private const string MonthType = "m";
    private const string YearType = "y";
    
    private static readonly string[] s_scheduleTypes = { DayType, WeekType, MonthType, YearType };

    private readonly int? _days;
    private readonly int? _weeks;
    private readonly int? _months;
    private readonly int? _years;

    private RetentionSchedule(int? days, int? weeks, int? months, int? years)
    {
        _days = days;
        _weeks = weeks;
        _months = months;
        _years = years;
    }
    
    private static void FillMissingSchedules(IDictionary<string, int?> schedules)
    {
        foreach (var schedule in s_scheduleTypes)
        {
            if (!schedules.ContainsKey(schedule))
            {
                schedules.Add(schedule, null);
            }
        }
    }

    private static IEnumerable<TKey> FilterBy<TKey, TGroup>(IEnumerable<KeyValuePair<TKey, DateTime>> backups,
        Func<KeyValuePair<TKey, DateTime>, TGroup> groupBy,
        int count)
        where TKey : notnull =>
        backups
            .GroupBy(groupBy)
            .Select(g => g.OrderByDescending(b => b.Value).FirstOrDefault())
            .OrderByDescending(b => b.Value)
            .Select(b => b.Key)
            .Take(count);

    public IEnumerable<TKey> FilterForRemoval<TKey>(Dictionary<TKey, DateTime> backups)
        where TKey : notnull
    {
        var markedForKeep = Enumerable.Empty<TKey>();

        if (_years.HasValue)
        {
            markedForKeep = markedForKeep.Concat(FilterBy(backups, b => b.Value.Year, _years.Value));
        }
        
        if (_months.HasValue)
        {
            markedForKeep = markedForKeep.Concat(FilterBy(backups, b => (b.Value.Year, b.Value.Month), _months.Value));
        }

        if (_weeks.HasValue)
        {
            markedForKeep = markedForKeep.Concat(FilterBy(backups, b => b.Value.StartOfTheWeek(), _weeks.Value));
        }

        if (_days.HasValue)
        {
            markedForKeep = markedForKeep.Concat(FilterBy(backups, b => b.Value.Date, _days.Value));
        }

        return backups.Keys.Except(markedForKeep.ToHashSet());
    }

    public static RetentionSchedule Parse(string schedule)
    {
        var schedules = schedule
            .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(t => Regex.Match(t, @$"(\d+)({DayType}|{WeekType}|{MonthType}|{YearType})"))
            .Where(m => m.Success)
            .ToDictionary(m => m.Groups[2].Value, m => m.Groups[1].Value == "*" ? (int?)null : int.Parse(m.Groups[1].Value));

        FillMissingSchedules(schedules);

        return new RetentionSchedule(schedules[DayType], schedules[WeekType], schedules[MonthType], schedules[YearType]);
    }
}