using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Model;
using Rock.Web.Cache;

namespace com.bricksandmortarstudio.checkinextensions
{
    public class ChildCheckInGroupGenerator
    {
        private int _checkInTemplateId;
        private List<int> _seenGroupTypeIds = new List<int>();
        private List<int> _groups = new List<int>();

        public List<int> Get(IEnumerable<GroupType> groupTypes)
        {
            _checkInTemplateId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE).Id;
            GetValidGroups(groupTypes);
            return _groups;
        }

        private void GetValidGroups(IEnumerable<GroupType> groupTypes)
        {
            foreach (var groupType in groupTypes)
            {
                _seenGroupTypeIds.Add(groupType.Id);
                var groupTypeGroups = groupType.Groups.Where(g => !_groups.Contains(g.Id));
                foreach (var group in groupTypeGroups)
                {
                    _groups.Add(group.Id);
                }

                if (groupType.ChildGroupTypes != null)
                {
                    GetValidGroups(
                        groupType.ChildGroupTypes.Where(
                            g => (g.GroupTypePurposeValueId == _checkInTemplateId || g.GroupTypePurposeValueId == null) && !_seenGroupTypeIds.Contains(g.Id))
                            .ToList());
                }
            }
        }
    }
}
