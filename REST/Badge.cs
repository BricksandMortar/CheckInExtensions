using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Http;
using Rock;
using Rock.Data;
using Rock.Rest.Controllers;
using Rock.Rest.Filters;

namespace com.bricksandmortarstudio.checkinextensions
{
    public class CheckInAdditionsBadgesController : PersonBadgesController
    {

        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route("api/PersonBadges/IndividualAttendanceGraph/{personId}/{monthCount}/{idList}/{recursive}")]
        public IQueryable<MonthlyAttendanceSummary> GetFamilyAttendanceForGroupType(int personId, int monthCount,  string idList, bool recursive)
        {
            //If recursive, id list is a list of group IDs, else it's a list of group type ids
            var attendanceSummary =
                new List<MonthlyAttendanceSummary>();

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PersonId", personId);
            parameters.Add("MonthCount", monthCount);
            parameters.Add("idList", idList);
            //Tell the SQL if it's dealing with groups or grouptypes
            parameters.Add("groups", recursive);

            var table = DbService.GetDataTable("spBricksandMortarStudio_BadgeAttendanceWithGroupType", CommandType.StoredProcedure,
                parameters);

            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    var item = new MonthlyAttendanceSummary();
                    item.AttendanceCount = row["AttendanceCount"].ToString().AsInteger();
                    item.SundaysInMonth = row["SundaysInMonth"].ToString().AsInteger();
                    item.Month = row["Month"].ToString().AsInteger();
                    item.Year = row["Year"].ToString().AsInteger();

                    attendanceSummary.Add(item);
                }
            }

            return attendanceSummary.AsQueryable();
        }


        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route("api/PersonBadges/IndividualWeeksAttendedInDuration/{personId}/{weekCount}/{idList}/{recursive}")]
        public int GetWeeksAttendedInDuration(int personId, int weekCount, string idList, bool recursive)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PersonId", personId);
            parameters.Add("WeekDuration", weekCount);
            parameters.Add("idList", idList);
            parameters.Add("recursive", recursive);

            var result = DbService.ExecuteScaler("spBricksandMortarStudio_Checkin_WeeksAttendedInDurationWithGroupType", System.Data.CommandType.StoredProcedure, parameters);
            if (result != null)
            {
                return (int)result;
            }

            return -1;
        }

    }
}