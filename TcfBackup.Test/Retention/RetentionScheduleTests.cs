// using System;
// using System.Linq;
// using NUnit.Framework;
// using TcfBackup.Retention;
// using TcfBackup.Shared;
//
// namespace TcfBackup.Test.Retention;
//
// public class RetentionScheduleTests
// {
//     private interface ITestSchedule
//     {
//         string Schedule { get; }
//         DateTime[] Backups { get; }
//         DateTime[] Expected { get; }
//     }
//
//     private class DaysTestSchedule : ITestSchedule
//     {
//         private static readonly DateTime s_start = new(2020, 01, 01);
//
//         public string Schedule => "3d";
//         public DateTime[] Backups => new[] { s_start.AddDays(1), s_start.AddDays(2), s_start.AddDays(3), s_start.AddDays(4), s_start.AddDays(5) };
//         public DateTime[] Expected => new[] { s_start.AddDays(3), s_start.AddDays(4), s_start.AddDays(5) };
//     }
//
//     private class WeeksForOneMonthTestSchedule : ITestSchedule
//     {
//         private static DateTime GetStartOfTheWeekForYear(DateTime start)
//         {
//             DateTime current;
//             while ((current = start.StartOfTheWeek()).Year != start.Year)
//             {
//                 start = start.AddDays(7);
//             }
//
//             return current;
//         }
//
//         protected static readonly DateTime Start = GetStartOfTheWeekForYear(new DateTime(2020, 01, 01));
//
//         public virtual string Schedule => "2w";
//         public virtual DateTime[] Backups => new[] { Start.AddDays(7 * 0), Start.AddDays(7 * 1), Start.AddDays(7 * 2), Start.AddDays(7 * 3) };
//         public virtual DateTime[] Expected => new[] { Start.AddDays(7 * 2), Start.AddDays(7 * 3) };
//     }
//
//     private class WeeksMultipleDaysForOneMonthTestSchedule : WeeksForOneMonthTestSchedule
//     {
//         public override string Schedule => "2w";
//
//         public override DateTime[] Backups => new[]
//         {
//             Start.AddDays(7 * 0), Start.AddDays(7 * 0).AddDays(1), Start.AddDays(7 * 0).AddDays(2),
//             Start.AddDays(7 * 1), Start.AddDays(7 * 1).AddDays(2), Start.AddDays(7 * 1).AddDays(3),
//             Start.AddDays(7 * 2), Start.AddDays(7 * 2).AddDays(3), Start.AddDays(7 * 2).AddDays(4)
//         };
//
//         public override DateTime[] Expected => new[] { Start.AddDays(7 * 1).AddDays(3), Start.AddDays(7 * 2).AddDays(4) };
//     }
//
//     private class WeeksForTwoMonthTestSchedule : WeeksForOneMonthTestSchedule
//     {
//         public override string Schedule => "4w";
//         public override DateTime[] Backups => new[] { Start.AddDays(7 * 0), Start.AddDays(7 * 1), Start.AddDays(7 * 2), Start.AddDays(7 * 3), Start.AddDays(7 * 4), Start.AddDays(7 * 5), Start.AddDays(7 * 6) };
//         public override DateTime[] Expected => new[] { Start.AddDays(7 * 3), Start.AddDays(7 * 4), Start.AddDays(7 * 5), Start.AddDays(7 * 6) };
//     }
//
//     private class MonthsTestSchedule : ITestSchedule
//     {
//         private static readonly DateTime s_start = new(2020, 01, 01);
//
//         public string Schedule => "3m";
//         public DateTime[] Backups => new[] { s_start.AddMonths(0), s_start.AddMonths(1), s_start.AddMonths(2), s_start.AddMonths(3), s_start.AddMonths(4) };
//         public DateTime[] Expected => new[] { s_start.AddMonths(2), s_start.AddMonths(3), s_start.AddMonths(4) };
//     }
//
//     private class MonthsMultipleDaysTestSchedule : ITestSchedule
//     {
//         private static readonly DateTime s_start = new(2020, 01, 01);
//
//         public string Schedule => "3m";
//
//         public DateTime[] Backups => new[]
//         {
//             s_start.AddMonths(0), s_start.AddMonths(0).AddDays(1), s_start.AddMonths(0).AddDays(2),
//             s_start.AddMonths(1), s_start.AddMonths(1).AddDays(3), s_start.AddMonths(1).AddDays(4),
//             s_start.AddMonths(2), s_start.AddMonths(2).AddDays(5), s_start.AddMonths(2).AddDays(6),
//             s_start.AddMonths(3), s_start.AddMonths(3).AddDays(7), s_start.AddMonths(3).AddDays(8),
//             s_start.AddMonths(4), s_start.AddMonths(4).AddDays(9), s_start.AddMonths(4).AddDays(10),
//         };
//
//         public DateTime[] Expected => new[] { s_start.AddMonths(2).AddDays(6), s_start.AddMonths(3).AddDays(8), s_start.AddMonths(4).AddDays(10) };
//     }
//
//     private class YearsTestSchedule : ITestSchedule
//     {
//         private static readonly DateTime s_start = new(2020, 01, 01);
//
//         public string Schedule => "3y";
//         public DateTime[] Backups => new[] { s_start.AddYears(0), s_start.AddYears(1), s_start.AddYears(2), s_start.AddYears(3), s_start.AddYears(4) };
//         public DateTime[] Expected => new[] { s_start.AddYears(2), s_start.AddYears(3), s_start.AddYears(4) };
//     }
//
//     private class YearsMultipleDaysTestSchedule : ITestSchedule
//     {
//         private static readonly DateTime s_start = new(2020, 01, 01);
//
//         public string Schedule => "3y";
//
//         public DateTime[] Backups => new[]
//         {
//             s_start.AddYears(0), s_start.AddYears(0).AddDays(1), s_start.AddYears(0).AddMonths(2),
//             s_start.AddYears(1), s_start.AddYears(1).AddDays(3), s_start.AddYears(1).AddMonths(4),
//             s_start.AddYears(2), s_start.AddYears(2).AddDays(5), s_start.AddYears(2).AddMonths(6),
//             s_start.AddYears(3), s_start.AddYears(3).AddDays(7), s_start.AddYears(3).AddMonths(8),
//             s_start.AddYears(4), s_start.AddYears(4).AddDays(9), s_start.AddYears(4).AddMonths(10),
//         };
//
//         public DateTime[] Expected => new[] { s_start.AddYears(2).AddMonths(6), s_start.AddYears(3).AddMonths(8), s_start.AddYears(4).AddMonths(10) };
//     }
//
//     [Test]
//     [TestCase(typeof(DaysTestSchedule))]
//     [TestCase(typeof(WeeksForOneMonthTestSchedule))]
//     [TestCase(typeof(WeeksMultipleDaysForOneMonthTestSchedule))]
//     [TestCase(typeof(WeeksForTwoMonthTestSchedule))]
//     [TestCase(typeof(MonthsTestSchedule))]
//     [TestCase(typeof(MonthsMultipleDaysTestSchedule))]
//     [TestCase(typeof(YearsTestSchedule))]
//     [TestCase(typeof(YearsMultipleDaysTestSchedule))]
//     public void TestSchedule(Type testScheduleType)
//     {
//         var testSchedule = (ITestSchedule)Activator.CreateInstance(testScheduleType)!;
//
//         var retentionSchedule = RetentionSchedule.Parse(testSchedule.Schedule);
//         var backupsToRemove = retentionSchedule.FilterForRemoval(testSchedule.Backups.ToDictionary(d => d, d => d));
//         var actual = testSchedule.Backups.Except(backupsToRemove).ToList();
//
//         CollectionAssert.AreEquivalent(testSchedule.Expected, actual);
//     }
// }