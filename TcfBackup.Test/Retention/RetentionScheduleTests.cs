using System;
using System.Linq;
using NUnit.Framework;
using TcfBackup.Retention;
using TcfBackup.Test.Retention.RetentionScheduleTestCases;

namespace TcfBackup.Test.Retention;

public class RetentionScheduleTests
{
    [Test]
    [TestCase(typeof(DaysRetentionScheduleTestCase))]
    [TestCase(typeof(WeeksForOneMonthRetentionScheduleTestCase))]
    [TestCase(typeof(WeeksMultipleDaysForOneMonthRetentionScheduleTestCase))]
    [TestCase(typeof(WeeksForTwoMonthRetentionScheduleTestCase))]
    [TestCase(typeof(MonthsRetentionScheduleTestCase))]
    [TestCase(typeof(MonthsMultipleDaysRetentionScheduleTestCase))]
    [TestCase(typeof(YearsRetentionScheduleTestCase))]
    [TestCase(typeof(YearsMultipleDaysRetentionScheduleTestCase))]
    public void TestSchedule(Type testScheduleType)
    {
        var testSchedule = (IRetentionScheduleTestCase)Activator.CreateInstance(testScheduleType)!;

        var retentionSchedule = RetentionSchedule.Parse(testSchedule.Schedule);
        var backupsToRemove = retentionSchedule.FilterForRemoval(testSchedule.Backups.ToDictionary(d => d, d => d));
        var actual = testSchedule.Backups.Except(backupsToRemove).ToList();

        CollectionAssert.AreEquivalent(testSchedule.Expected, actual);
    }
}