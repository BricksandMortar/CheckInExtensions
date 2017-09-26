using System.Collections.Generic;
using System.Linq;
using Rock.Model;
using Rock.Web.Cache;

namespace com.bricksandmortarstudio.checkinextensions.Utils
{
    public class ChildCheckInGroupGenerator
    {
        /// Given a list of groupTypes this classes returns their child groups that are related to check-in via recursively expanding the tree avoiding loops

        private int _checkInFilterId;
        private readonly List<int> _seenGroupTypeIds = new List<int>();
        private readonly List<Group> _groups = new List<Group>();

        public List<Group> Get(IEnumerable<GroupType> groupTypes)
        {
            _checkInFilterId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER).Id;
            GetValidGroups(groupTypes);
            return _groups;
        }

        public List<Group> Get( GroupType groupType )
        {
            _checkInFilterId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ).Id;
            GetValidGroup( groupType );
            return _groups;
        }

        public List<Group> Get( IEnumerable<int> groupTypeIds )
        {
            _checkInFilterId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ).Id;
            var groupTypeService = new GroupTypeService( new Rock.Data.RockContext() );
            var groupTypes = groupTypeService.GetByIds( groupTypeIds.ToList() );
            GetValidGroups( groupTypes );
            return _groups;
        }

        private void GetValidGroups(IEnumerable<GroupType> groupTypes)
        {
            foreach (var groupType in groupTypes)
            {
                GetValidGroup(groupType);
            }
        }

        private void GetValidGroup(GroupType groupType)
        {
            _seenGroupTypeIds.Add( groupType.Id );
            var groupTypeGroups = groupType.Groups.Where( n => n.IsActive && !_groups.Select( g => g.Id ).Contains( n.Id ) );
            foreach ( var group in groupTypeGroups )
            {
                _groups.Add( group );
            }

            if ( groupType.ChildGroupTypes != null )
            {
                GetValidGroups(
                    groupType.ChildGroupTypes.Where(
                        g => ( g.GroupTypePurposeValueId != _checkInFilterId || g.GroupTypePurposeValueId == null ) && !_seenGroupTypeIds.Contains( g.Id ) )
                        .ToList() );
            }
        }
    }
}
