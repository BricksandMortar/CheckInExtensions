using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Model;

namespace com.bricksandmortarstudio.checkinextensions.Utils
{
    public static class QuartersHelper
    {
        public static Dictionary<int, Quarter> GetQuarterStartEndForYear(int year)
        {
            return new Dictionary<int, Quarter>
            {
                {1, new Quarter(new DateTime(year, 1, 1), new DateTime(year, 3, 31).OneSecondToMidnight())},
                {2, new Quarter(new DateTime(year, 4, 1), new DateTime(year, 6, 30).OneSecondToMidnight())},
                {3, new Quarter(new DateTime(year, 7, 1), new DateTime(year, 9, 30).OneSecondToMidnight())},
                {4, new Quarter(new DateTime(year, 10, 1), new DateTime(year, 12, 31).OneSecondToMidnight())}
            };
        }

        public static Dictionary<int, Quarter> FilterQuartersToRange( this Dictionary<int, Quarter> quarters, DateTime startDateTime, DateTime endDateTime )
        {
            foreach (var quarterSet in quarters)
            { 
                if (quarterSet.Value.StartDateTime < startDateTime && quarterSet.Value.EndDateTime < startDateTime || quarterSet.Value.StartDateTime > endDateTime && quarterSet.Value.EndDateTime > endDateTime)
                {
                    quarterSet.Value.StartDateTime = null;
                    quarterSet.Value.EndDateTime = null;
                }
                else 
                {
                    if (quarterSet.Value.StartDateTime < startDateTime)
                    {
                        quarterSet.Value.StartDateTime = startDateTime;
                    }

                    if (quarterSet.Value.EndDateTime > endDateTime)
                    {
                        quarterSet.Value.EndDateTime = endDateTime;
                    }
                }
            }
            return quarters;
        }
    }
}
