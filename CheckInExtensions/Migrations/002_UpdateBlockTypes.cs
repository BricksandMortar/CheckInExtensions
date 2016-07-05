using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bricksandmortarstudio.checkinextensions.Migrations
{
    [MigrationNumber(2, "1.4.0")]
    public class _002UpdateBlockTypes : Migration
    {
        public override void Up()
        {
            //Update blocks to all have the same guid
            RockMigrationHelper.UpdateBlockType("Attendance Overview", "A summary of attendance",
                "~/Plugins/com_bricksandmortarstudio/CheckInExtensions/AttendanceSummary.ascx",
                "Bricks and Mortar Studio > Check-In Extensions", SystemGuid.BlockType.ATTENDANCE_OVERVIEW);
            RockMigrationHelper.UpdateBlockType("Attendance Input", "Used to input attendance en mass after a group has concluded.",
                 "~/Plugins/com_bricksandmortarstudio/CheckInExtensions/RetrospectiveAttendanceInput.ascx",
                 "Bricks and Mortar Studio > Check-In Extensions", SystemGuid.BlockType.ATTENDANCE_INPUT);
            RockMigrationHelper.UpdateBlockType("Headcount Metric Value List", "Displays a list of metric values.",
                            "~/Plugins/com_bricksandmortarstudio/CheckInExtensions/HeadcountValueList.ascx",
                            "Bricks and Mortar Studio > Check-In Extensions", SystemGuid.BlockType.HEADCOUNT_METRIC_VALUE_LIST);
            RockMigrationHelper.UpdateBlockType("Find Inactive Attendees", "Used to find people who have a recently checked into a group with a given record status.",
                            "~/Plugins/com_bricksandmortarstudio/CheckInExtensions/FindInactives.ascx",
                            "Bricks and Mortar Studio > Check-In Extensions", SystemGuid.BlockType.FIND_INACTIVE_ATTENDEES);
            RockMigrationHelper.UpdateBlockType("Attended Groups", "Used to find the groups checked into by people.",
                            "~/Plugins/com_bricksandmortarstudio/CheckInExtensions/FindAttendedClasses.ascx",
                            "Bricks and Mortar Studio > Check-In Extensions", SystemGuid.BlockType.ATTENDED_GROUPS);
            RockMigrationHelper.UpdateBlockType("Regular Active Attenders", "Used to find people who have recently checked into a group.",
                            "~/Plugins/com_bricksandmortarstudio/CheckInExtensions/FindActives.ascx",
                            "Bricks and Mortar Studio > Check-In Extensions", SystemGuid.BlockType.FIND_ACTIVES);

            // Ensure they're not set as system
            Sql(@"UPDATE [BlockType] SET
                        [IsSystem] = 0
                    WHERE [Guid] = 'E4CCA238-E786-4D91-BC40-872C67C9CC7D'
UPDATE [BlockType] SET
                        [IsSystem] = 0
                    WHERE [Guid] = '27F994A5-1C43-463D-AF31-F7BD25955455'
UPDATE [BlockType] SET
                        [IsSystem] = 0
                    WHERE [Guid] = '8369AD42-671A-436D-B28B-CF624B466688'
UPDATE [BlockType] SET
                        [IsSystem] = 0
                    WHERE [Guid] = 'A20B1EE0-196E-421A-AB83-3B8627727737'
UPDATE [BlockType] SET
                        [IsSystem] = 0
                    WHERE [Guid] = '3A6CD754-308C-45DD-B9F2-05462D499CE3'
UPDATE [BlockType] SET
                        [IsSystem] = 0
                    WHERE [Guid] = '41FFBA49-4F0A-4DD1-AB78-D4B61D721F65'
");
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteBlockType(SystemGuid.BlockType.ATTENDANCE_OVERVIEW);
            RockMigrationHelper.DeleteBlockType(SystemGuid.BlockType.ATTENDANCE_INPUT);
            RockMigrationHelper.DeleteBlockType(SystemGuid.BlockType.HEADCOUNT_METRIC_VALUE_LIST);
            RockMigrationHelper.DeleteBlockType(SystemGuid.BlockType.FIND_INACTIVE_ATTENDEES);
            RockMigrationHelper.DeleteBlockType(SystemGuid.BlockType.ATTENDED_GROUPS);
            RockMigrationHelper.DeleteBlockType(SystemGuid.BlockType.FIND_ACTIVES);
        }
    }
}
