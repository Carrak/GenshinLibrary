using System;

namespace GenshinLibrary.Utility
{
    public enum TimePartition
    {
        None,
        Hour,
        Day,
        Month,
        Year
    }

    public static class TimeUtilities
    {
        public static DateTime DateTruncate(this DateTime d, TimePartition partition)
        {
            return partition switch
            {
                TimePartition.Hour => new DateTime(d.Year, d.Month, d.Day, d.Hour, 0, 0),
                TimePartition.Day => new DateTime(d.Year, d.Month, d.Day),
                TimePartition.Month => new DateTime(d.Year, d.Month, 1),
                TimePartition.Year => new DateTime(d.Year, 1, 1),
                _ => throw new NotImplementedException()
            };
        }
    }
}
