using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;

namespace com.bricksandmortarstudio.checkinextensions.Utils
{
    public static class HeadCountHelper
    {
        public static decimal? GetHeadCountForGroup(Guid groupGuid, DateTime startDate, DateTime endDate, RockContext rockContext = null)
        {
            if (rockContext == null)
            {
                rockContext = new RockContext();
            }

            return new MetricService( rockContext ).Queryable( "MetricValues" ).AsNoTracking().Where( m => m.ForeignGuid.HasValue && groupGuid == m.ForeignGuid.Value ).Select( m => m.MetricValues.Where( v => v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value >= startDate && v.MetricValueDateTime.Value < endDate ) ).SelectMany( i => i ).Sum( v => v.YValue );
        }

        public static decimal? GetHeadCountForGroupType(GroupType groupType, DateTime startDate, DateTime endDate,
            RockContext rockContext = null)
        {
            if (rockContext == null)
            {
                rockContext = new RockContext();
            }
            var groups = new ChildCheckInGroupGenerator().Get(groupType);

            return new MetricService(rockContext).Queryable( "MetricValues" ).AsNoTracking().Where( m => m.ForeignGuid.HasValue && groups.Any(g => g.Guid == m.ForeignGuid.Value) ).Select( m => m.MetricValues.Where( v => v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value >= startDate && v.MetricValueDateTime.Value < endDate ) ).SelectMany( i => i ).Sum( v => v.YValue );
        }
    }
}
