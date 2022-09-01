namespace TcfBackup.Shared;

public static class DateTimeExtensions
{
    public static DateTime StartOfTheWeek(this DateTime dateTime)
        => dateTime.AddDays(-(int)dateTime.DayOfWeek);
}