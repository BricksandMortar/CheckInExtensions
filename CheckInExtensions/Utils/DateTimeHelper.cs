using System;
using System.Linq;

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

        public static int GetQuarter( this DateTime dt )
        {
            return ( dt.Month - 1 ) / 3 + 1;
        }

        public static int NumberOfThisDayOfWeekLeftInMonth(this DateTime dt )
        {
            int totalNumberOfDaysInMonth = DateTime.DaysInMonth(dt.Year, dt.Month);

            int answer = Enumerable
                .Range( dt.Day, totalNumberOfDaysInMonth)
                .Select( dayOfMonth => new DateTime( dt.Year, dt.Month, dayOfMonth ) )
                .Count(date => date.DayOfWeek == dt.DayOfWeek);

            return answer;
        }

        public static int NumberOfThisDayOfWeekLeftBeforeDate( this DateTime dt, DateTime endDateTime )
        {
            int answer = Enumerable
                .Range( dt.Day, endDateTime.Day )
                .Select( dayOfMonth => new DateTime( dt.Year, dt.Month, dayOfMonth ) )
                .Count( date => date.DayOfWeek == dt.DayOfWeek );

            return answer;
        }
    }
}
