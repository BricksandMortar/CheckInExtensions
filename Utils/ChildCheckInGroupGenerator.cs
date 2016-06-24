using System.Collections.Generic;
using System.Linq;
using Rock.Model;
using Rock.Web.Cache;

namespace com.bricksandmortarstudio.checkinextensions.Utils
{
    public class ChildCheckInGroupGenerator
    {
        /// Given a list of groupTypes this classes returns their child groups that are related to check-in via recursively expanding the tree avoiding loops

        private int _checkInTemplateId;
        private List<int> _seenGroupTypeIds = new List<int>();
        private List<Group> _groups = new List<Group>();

        public List<Group> Get(IEnumerable<GroupType> groupTypes)
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
                var groupTypeGroups = groupType.Groups.Where(n => !_groups.Select(g => g.Id).Contains(n.Id));
                foreach (var group in groupTypeGroups)
                {
                    _groups.Add(group);
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
