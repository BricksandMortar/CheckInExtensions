using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Model;
using Rock.Plugin;

namespace com.bricksandmortarstudio.checkinextensions.Migrations
{
    [MigrationNumber( 3, "1.4.0" )]
    class AddPageStructureandBlocks : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.AddPage(Rock.SystemGuid.Page.INTERNAL_HOMEPAGE, "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Check-In Administration", "", SystemGuid.Page.ROOT_NAV, "fa fa-check-square-o");

            RockMigrationHelper.AddPage(SystemGuid.Page.ROOT_NAV, "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Add", "", SystemGuid.Page.ADD_SUB_NAV, "");
            RockMigrationHelper.AddPage(SystemGuid.Page.ROOT_NAV, "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Reports", "", SystemGuid.Page.REPORTS_SUB_NAV, "");
            RockMigrationHelper.AddPage(SystemGuid.Page.ROOT_NAV, "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Headcount", "", SystemGuid.Page.HEADCOUNT_SUB_NAV, "");

            RockMigrationHelper.AddPage(SystemGuid.Page.ADD_SUB_NAV, "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Add Attendance", "", SystemGuid.Page.ADD_ATTENDANCE);

            RockMigrationHelper.AddPage(SystemGuid.Page.REPORTS_SUB_NAV, "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Active Attendees", "", SystemGuid.Page.ACTIVE_ATTENDEES);
            RockMigrationHelper.AddPage(SystemGuid.Page.REPORTS_SUB_NAV, "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Inactive Attendees", "", SystemGuid.Page.INACTIVE_ATTENDEES);
            RockMigrationHelper.AddPage(SystemGuid.Page.REPORTS_SUB_NAV, "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Group Activity", "", SystemGuid.Page.GROUP_ACTIVITY);

            RockMigrationHelper.AddPage( SystemGuid.Page.HEADCOUNT_SUB_NAV, "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Attendance Overview", "", SystemGuid.Page.ATTENDANCE_OVERERVIEW );
            RockMigrationHelper.AddPage( SystemGuid.Page.HEADCOUNT_SUB_NAV, "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Record Headcounts", "", SystemGuid.Page.RECORD_HEADCOUNTS );

            RockMigrationHelper.AddBlock(SystemGuid.Page.ADD_ATTENDANCE, "", SystemGuid.BlockType.ATTENDANCE_INPUT, "Attendance Input", "Feature", "<div class='well'>Use keyboard shortcuts to speed up entry. < span class='navigation-tip'>Alt+Z</span> adds the currently selected person and<span class='navigation-tip'>Alt+S</span> saves the current attendance.</div>", "", 0, SystemGuid.Block.ATTENDANCE_INPUT);

            RockMigrationHelper.AddBlock(SystemGuid.Page.ACTIVE_ATTENDEES, "", SystemGuid.BlockType.FIND_ACTIVES, "Active Attendees", "Feature", "", "", 0, SystemGuid.Block.ACTIVE_ATTENDANCE);
            RockMigrationHelper.AddBlock(SystemGuid.Page.INACTIVE_ATTENDEES, "", SystemGuid.BlockType.FIND_INACTIVE_ATTENDEES, "Inactive Attendees", "Feature", "", "", 0, SystemGuid.Block.INACTIVE_ATTENDANCE);
            RockMigrationHelper.AddBlock(SystemGuid.Page.GROUP_ACTIVITY, "", SystemGuid.BlockType.ATTENDED_GROUPS, "Attended Groups", "Feature", "", "", 0, SystemGuid.Block.ATTENDED_GROUPS);

            RockMigrationHelper.AddBlock(SystemGuid.Page.ATTENDANCE_OVERERVIEW, "", SystemGuid.BlockType.ATTENDANCE_OVERVIEW, "Attendance Summary", "Feature", "", "", 0, SystemGuid.Block.ATTENDANCE_OVERVIEW);
            RockMigrationHelper.AddBlock(SystemGuid.Page.RECORD_HEADCOUNTS, "", SystemGuid.BlockType.HEADCOUNT_METRIC_VALUE_LIST, "Headcount Values", "Feature", "", "", 0, SystemGuid.Block.HEADCOUNT_METRIC_VALUE_LIST);
        }

        public override void Down()
        {
            RockMigrationHelper.DeletePage(SystemGuid.Page.ROOT_NAV);
            RockMigrationHelper.DeletePage(SystemGuid.Page.ADD_SUB_NAV);
            RockMigrationHelper.DeletePage(SystemGuid.Page.REPORTS_SUB_NAV);
            RockMigrationHelper.DeletePage(SystemGuid.Page.HEADCOUNT_SUB_NAV);
            RockMigrationHelper.DeletePage(SystemGuid.Page.ADD_ATTENDANCE);
            RockMigrationHelper.DeletePage(SystemGuid.Page.GROUP_ACTIVITY);
            RockMigrationHelper.DeletePage(SystemGuid.Page.INACTIVE_ATTENDEES);
            RockMigrationHelper.DeletePage(SystemGuid.Page.ACTIVE_ATTENDEES);
            RockMigrationHelper.DeletePage(SystemGuid.Page.RECORD_HEADCOUNTS);
            RockMigrationHelper.DeletePage(SystemGuid.Page.ATTENDANCE_OVERERVIEW);
            RockMigrationHelper.DeleteBlock(SystemGuid.Block.ATTENDANCE_INPUT);
            RockMigrationHelper.DeleteBlock(SystemGuid.Block.ACTIVE_ATTENDANCE);
            RockMigrationHelper.DeleteBlock(SystemGuid.Block.INACTIVE_ATTENDANCE);
            RockMigrationHelper.DeleteBlock(SystemGuid.Block.ATTENDED_GROUPS);
            RockMigrationHelper.DeleteBlock(SystemGuid.Block.HEADCOUNT_METRIC_VALUE_LIST);
            RockMigrationHelper.DeleteBlock(SystemGuid.Block.ATTENDANCE_OVERVIEW);
        }
    }
}
