using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

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
            int checkInTemplateId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ).Id;
            return groupTypeService.Queryable().AsNoTracking().Where( g => g.GroupTypePurposeValueId == checkInTemplateId );
        }
    }
}
