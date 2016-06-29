using System;

namespace com.bricksandmortarstudio.checkinextensions.Utils
{
    public static class DateTimeHelper
    {
        // http://stackoverflow.com/questions/28191498/get-every-weeks-start-and-end-date-from-listdatetime-using-linq
        public static DateTime NextDayOfWeek(this DateTime from, DayOfWeek dayOfWeek, bool daysMatchReturnSameDate = false)
        {
            if (daysMatchReturnSameDate && from.DayOfWeek == dayOfWeek)
            {
             return from;
            }
            int start = (int)from.DayOfWeek;
            int target = (int)dayOfWeek;
            if (target <= start)
                target += 7;
            return from.AddDays(target - start);
        }

        public static DateTime PreviousOfWeek(this DateTime dt, DayOfWeek dayOfWeek)
        {
            int offsetDays = -(dt.DayOfWeek - dayOfWeek);
            return dt.AddDays(offsetDays);
        }

        public static DateTime OneSecondToMidnight(this DateTime dt)
        {
            return dt.AddHours(23).AddMinutes(59).AddSeconds(59);
        }
    }
}
