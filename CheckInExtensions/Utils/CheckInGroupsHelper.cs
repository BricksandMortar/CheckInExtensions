using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using DefinedValue = Rock.SystemGuid.DefinedValue;

namespace com.bricksandmortarstudio.checkinextensions.Utils
{
    public static class CheckInGroupsHelper
    {
        public static IEnumerable<GroupType> GetCheckInTemplatesGroupTypes(RockContext rockContext = null)
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }
            var groupTypeService = new GroupTypeService(rockContext);
            int checkInTemplateId = DefinedValueCache.Get( DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ).Id;
            return groupTypeService.Queryable().AsNoTracking().Where( g => g.GroupTypePurposeValueId == checkInTemplateId );
        }

        public static string CreateGroupListString(List<GroupTypeCache> groupTypes)
        {
            string groupTypeNames = String.Empty;
            string separator = groupTypes.Count != 2 ? ", and " : " and ";
            if (groupTypes.Count > 1)
            {
                groupTypeNames = String.Join(", ", groupTypes.Select(gt => gt.Name).ToArray(), 0, groupTypes.Count - 1) + separator +
                                 groupTypes.LastOrDefault().Name;
            }
            else if (groupTypes.Any())
            {
                groupTypeNames = groupTypes.FirstOrDefault().Name;
            }
            return groupTypeNames;
        }
    }
}
