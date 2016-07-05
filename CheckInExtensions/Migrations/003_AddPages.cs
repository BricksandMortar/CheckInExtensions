using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Model;
using Rock.Plugin;

namespace com.bricksandmortarstudio.checkinextensions.Migrations
{
    class AddPageStructureandBlocks : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.AddPage(Rock.SystemGuid.Page.INTERNAL_HOMEPAGE, "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Check-In Administration", "", SystemGuid.Page.ROOT_NAV, "fa fa-check-square-o");

            RockMigrationHelper.AddPage(SystemGuid.Page.ROOT_NAV, "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Add", "", SystemGuid.Page.ADD_SUB_NAV, "");
            RockMigrationHelper.AddPage(SystemGuid.Page.ROOT_NAV, "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Reports", "", SystemGuid.Page.REPORTS_SUB_NAV, "");
            RockMigrationHelper.AddPage(SystemGuid.Page.ROOT_NAV, "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Headcount", "", SystemGuid.Page.HEADCOUNT_SUB_NAV, "");

            RockMigrationHelper.AddPage(SystemGuid.Page.ADD_SUB_NAV, "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Add Attendance", "", SystemGuid.Page.ADD_ATTENDANCE);

            RockMigrationHelper.AddPage(SystemGuid.Page.REPORTS_SUB_NAV, "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Active Attendees", "", SystemGuid.Page.ACTIVE_ATTENDEES);
            RockMigrationHelper.AddPage(SystemGuid.Page.REPORTS_SUB_NAV, "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Inactive Attendees", "", SystemGuid.Page.INACTIVE_ATTENDEES);
            RockMigrationHelper.AddPage(SystemGuid.Page.REPORTS_SUB_NAV, "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Group Activity", "", SystemGuid.Page.GROUP_ACTIVITY);

            RockMigrationHelper.AddPage(SystemGuid.Page.ATTENDANCE_OVERERVIEW, "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Attendance Overview", "", SystemGuid.Page.ATTENDANCE_OVERERVIEW);
            RockMigrationHelper.AddPage(SystemGuid.Page.RECORD_HEADCOUNTS, "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Records Headcount", "", SystemGuid.Page.RECORD_HEADCOUNTS);

            RockMigrationHelper.AddBlock(SystemGuid.Page.ADD_ATTENDANCE, "", SystemGuid.BlockType.ATTENDANCE_INPUT, "Attendance Input", "Feature", "<div class='well'>Use keyboard shortcuts to speed up entry. < span class='navigation-tip'>Alt+Z</span> adds the currently selected person and<span class='navigation-tip'>Alt+S</span> saves the current attendance.</div>", "", 0, SystemGuid.Block.ATTENDANCE_INPUT);

            RockMigrationHelper.AddBlock(SystemGuid.Page.ACTIVE_ATTENDEES, "", SystemGuid.BlockType.FIND_ACTIVES, "Active Attendees", "Feature", "", "", 0, SystemGuid.Block.ACTIVE_ATTENDANCE);
            RockMigrationHelper.AddBlock(SystemGuid.Page.INACTIVE_ATTENDEES, "", SystemGuid.BlockType.FIND_INACTIVE_ATTENDEES, "Inactive Attendees", "Feature", "", "", 0, SystemGuid.Block.INACTIVE_ATTENDANCE);
            RockMigrationHelper.AddBlock(SystemGuid.Page.GROUP_ACTIVITY, "", SystemGuid.BlockType.ATTENDED_GROUPS, "Attended Groups", "Feature", "", "", 0, SystemGuid.Block.ATTENDED_GROUPS);

            RockMigrationHelper.AddBlock(SystemGuid.Page.ATTENDANCE_OVERERVIEW, "", SystemGuid.BlockType.ATTENDANCE_OVERVIEW, "Attendance Summary", "Feature", "", "", 0, SystemGuid.Block.ATTENDANCE_OVERVIEW);
            RockMigrationHelper.AddBlock(SystemGuid.Page.ATTENDANCE_OVERERVIEW, "", SystemGuid.BlockType.HEADCOUNT_METRIC_VALUE_LIST, "Headcount Values", "Feature", "", "", 0, SystemGuid.Block.HEADCOUNT_METRIC_VALUE_LIST);
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
