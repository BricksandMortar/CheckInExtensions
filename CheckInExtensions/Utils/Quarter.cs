using System;

namespace com.bricksandmortarstudio.checkinextensions.Utils
{
    public class Quarter
    {
        public Quarter(DateTime startDateTime, DateTime endDateTime)
        {
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }

        public DateTime? StartDateTime;
        public DateTime? EndDateTime;
    }
}