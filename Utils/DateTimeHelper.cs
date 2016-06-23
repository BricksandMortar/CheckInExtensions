using System;

namespace com.bricksandmortarstudio.checkinextensions.Utils
{
    public static class DateTimeHelper
    {
        // http://stackoverflow.com/questions/28191498/get-every-weeks-start-and-end-date-from-listdatetime-using-linq
        public static DateTime NextDayOfWeek(this DateTime dt, DayOfWeek dayOfWeek)
        {
            int offsetDays = dayOfWeek - dt.DayOfWeek;
            return dt.AddDays(offsetDays > 0 ? offsetDays : offsetDays + 7);
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
