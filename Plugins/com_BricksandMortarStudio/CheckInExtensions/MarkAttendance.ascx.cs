using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using com.bricksandmortarstudio.checkinextensions.Utils;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Mono.CSharp;
using Newtonsoft.Json;
using OpenXmlPowerTools;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Plugins.com_bricksandmortarstudio.CheckInExtensions
{
    [DisplayName( "Mark Attendance" )]
    [Category( "Bricks and Mortar Studio > Check-In Extensions" )]
    [Description( "Used to input attendance en mass after a group has concluded." )]
    [GroupTypeField( "Attendance Type", required: true, groupTypePurposeValueGuid: Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE, key: "GroupTypeTemplate" )]
    [IntegerField( "Number of Historical Weeks", "The number of weeks instances go back to", true, 4, key: "historicalweeks" )]
    public partial class MarkAttendance : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int? _campusId;
        private int? _groupId;
        private int? _locationId;
        private int? _scheduleId;
        private DateTime? _startDateTime;
        private ICollection<GroupLocation> _groupLocations;
        private List<PersonAttendance> _attendees;
        private List<Person> _people;

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( string.IsNullOrEmpty( GetAttributeValue( "GroupTypeTemplate" ) ) )
            {
                DisplayBox( "You have not yet set a group type in the block settings. A group type must be set in order for this block to function.", NotificationBoxType.Warning );
            }
            else
            {
                nbDisplayBox.Visible = false;
            }
            if ( !Page.IsPostBack )
            {
                var personIds = PageParameter( "personIds" ).SplitDelimitedValues().AsIntegerList();
                if ( personIds.Count == 0 )
                {
                    DisplayBox( "No people found", NotificationBoxType.Danger );
                    return;
                }

                _people = new PersonService( new RockContext() ).GetListByIds( personIds );
                _attendees = _people.Select( p => new PersonAttendance { Attended = false, PersonAliasId = p.PrimaryAliasId.Value, FullName = p.FullName } ).ToList();

                LoadPickers();
                if ( !String.IsNullOrEmpty( ddlInstances.SelectedValue ) )
                {
                    GetAttended();
                }
                UpdateAttendanceList();
            }
            nbDisplayBox.Visible = false;
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            if ( ViewState["CampusId"] != null )
            {
                _campusId = ViewState["CampusId"] as int?;
            }
            if ( ViewState["GroupId"] != null )
            {
                _groupId = ViewState["GroupId"] as int?;
            }
            if ( ViewState["LocationId"] != null )
            {
                _locationId = ViewState["LocationId"] as int?;
            }
            if ( ViewState["StartDateTime"] != null )
            {
                _startDateTime = ViewState["StartDateTime"] as DateTime?;
            }
            if ( ViewState["ScheduleId"] != null )
            {
                _scheduleId = ViewState["ScheduleId"] as int?;
            }

            if ( ViewState["Attendees"] != null )
            {
                _attendees = ViewState["Attendees"] as List<PersonAttendance>;
            }

            if ( ViewState["People"] != null )
            {
                _people = Person.FromJsonAsList( ViewState["People"].ToString() );
            }
        }

        protected override object SaveViewState()
        {
            if ( _campusId != null )
            {
                ViewState["CampusId"] = _campusId;
            }
            if ( _groupId != null )
            {
                ViewState["GroupId"] = _groupId;
            }

            if ( _locationId != null )
            {
                ViewState["LocationId"] = _locationId;
            }
            if ( _scheduleId != null )
            {
                ViewState["ScheduleId"] = _scheduleId;
            }
            if ( _startDateTime != null )
            {
                ViewState["StartDateTime"] = _startDateTime;
            }

            if ( _attendees != null )
            {
                ViewState["Attendees"] = _attendees;
            }

            if ( _people != null )
            {
                var jsonSetting = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                ViewState["People"] = JsonConvert.SerializeObject( _people, Formatting.None, jsonSetting );
            }

            return base.SaveViewState();
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadPickers();
            if ( !String.IsNullOrEmpty( ddlInstances.SelectedValue ) )
            {
                GetAttended();
            }
            UpdateAttendanceList();
        }

        protected void ddlCampuses_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            PopulateGroups();
            PopulateLocations();
            PopulateInstances();
            GetAttended();
        }

        protected void ddlLocations_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            if ( !String.IsNullOrEmpty( ddlLocations.SelectedValue ) )
            {
                _locationId = ddlLocations.SelectedValueAsId();
            }
            PopulateInstances();
            GetAttended();
            UpdateAttendanceList();
        }

        protected void ddlInstances_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            GetAttended();
            UpdateAttendanceList();
        }


        protected void ddlGroups_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            PopulateLocations();
            PopulateInstances();
            if ( ddlInstances.Items.Count != 0 )
            {
                GetAttended();
                UpdateAttendanceList();
            }
            else
            {
                DisplayBox( "No instances found for the selected group and location", NotificationBoxType.Warning );
            }
        }

        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( _groupId != null && _startDateTime != null )
            {
                var rockContext = new RockContext();
                var attendanceService = new AttendanceService( rockContext );
                var existingAttendees = attendanceService
                    .Queryable( "PersonAlias" )
                    .Where( a =>
                        a.GroupId == _groupId &&
                        a.LocationId == _locationId &&
                        a.ScheduleId == _scheduleId &&
                        a.StartDateTime == _startDateTime.Value );

                {
                    foreach ( var attendee in _attendees )
                    {
                        var attendance = existingAttendees
                            .FirstOrDefault( a => a.PersonAlias.Id == attendee.PersonAliasId );

                        if ( attendance == null )
                        {
                            attendance = new Attendance();
                            attendance.GroupId = _groupId;
                            attendance.ScheduleId = _scheduleId;
                            attendance.PersonAliasId = attendee.PersonAliasId;
                            attendance.StartDateTime = _startDateTime.Value;
                            attendance.LocationId = _locationId;
                            attendance.CampusId = _campusId;
                            attendance.ScheduleId = _scheduleId;
                            attendanceService.Add( attendance );
                        }

                        attendance.DidAttend = attendee.Attended;
                        attendance.DidNotOccur = null;
                    }
                }

                if ( _locationId.HasValue )
                {
                    Rock.CheckIn.KioskLocationAttendance.Flush( _locationId.Value );
                }

                rockContext.SaveChanges();
                NavigateToParentPage();
            }
        }

        #endregion

        #region Methods

        private void DisplayBox( string text, NotificationBoxType type )
        {
            nbDisplayBox.Text = text;
            nbDisplayBox.NotificationBoxType = type;
            nbDisplayBox.Visible = true;
        }

        private void GetAttended()
        {
            _scheduleId = GetScheduleIdandStart( ddlInstances.SelectedValue, out _startDateTime );

            if ( _scheduleId.HasValue && _startDateTime.HasValue && _people != null )
            {
                // Mark all attendance as false
                _attendees.ForEach(a => a.Attended = false);

                // Refresh _people's primary alias
                var rockContext = new RockContext();
                _people = new PersonService( rockContext ).GetListByIds(_people.Select(p => p.Id).ToList());

                // Get attendance for the people to the current group instance
                var attended = new AttendanceService( rockContext ).Queryable()
                                                                        .AsNoTracking()
                                                                       .Where( a =>
                                                                            a.GroupId == _groupId 
                                                                            && a.LocationId == _locationId 
                                                                            && a.ScheduleId == _scheduleId
                                                                            && a.StartDateTime == _startDateTime.Value
                                                                            && a.DidAttend != null )
                                                                       .ToList()
                                                                       .Where(a => _people.Any(p => a.PersonAliasId != null && p.PrimaryAliasId != null && p.PrimaryAliasId.Value == a.PersonAliasId.Value));

                // Mark attendance for each 
                foreach ( var attendance in attended )
                {
                    if (attendance.PersonAliasId != null)
                    {
                        var firstOrDefault = _attendees.FirstOrDefault( a => a.PersonAliasId == attendance.PersonAliasId );
                        if ( firstOrDefault != null )
                        {
                            firstOrDefault.Attended = attendance.DidAttend.Value;
                        }
                    }
                }
                UpdateAttendanceList();
            }
            else
            {
                DisplayBox( "Tried to fetch attendance but failed.", NotificationBoxType.Warning );
            }
        }

        private void LoadPickers()
        {
            ddlCampuses.Items.Clear();
            var campuses = CampusCache.All().OrderBy( a => a.Name );
            foreach ( var campus in campuses )
            {
                var listItem = new ListItem();
                listItem.Text = campus.Name;
                listItem.Value = campus.Id.ToString();
                ddlCampuses.Items.Add( listItem );
            }
            if ( campuses.Count() < 2 )
            {
                _campusId = campuses.First().Id;
                ddlCampuses.Enabled = false;
                ddlCampuses.Visible = false;
                campusContainer.AddCssClass("hidden" );
            }

            PopulateGroups();
            PopulateLocations();
            PopulateInstances();
        }

        private void UpdateAttendanceList()
        {
            lvAttendance.DataSource = _attendees;
            lvAttendance.DataBind();
        }

        private void PopulateGroups( Dictionary<string, string> userPreferences = null )
        {
            string groupTypeTemplateGuid = this.GetAttributeValue( "GroupTypeTemplate" );
            if ( !String.IsNullOrEmpty( groupTypeTemplateGuid ) )
            {
                var groupType = new GroupTypeService( new RockContext() ).Get( groupTypeTemplateGuid.AsGuid() );
                var groups = new ChildCheckInGroupGenerator().Get( new List<GroupType> { groupType } );
                ddlGroups.Items.Clear();
                //Find groups with schedules which folks can attend
                foreach ( var group in groups.Where( g => g.GroupLocations.Any( l => l.Schedules.Count > 0 ) ) )
                {
                    var listItem = new ListItem();
                    listItem.Text = group.Name;
                    listItem.Value = group.Id.ToString();
                    ddlGroups.Items.Add( listItem );
                }
                if ( ddlGroups.Items.Count < 1 )
                {
                    DisplayBox( "No groups found for the selected Attendance Type", NotificationBoxType.Warning );
                }
            }
        }

        protected void PopulateLocations()
        {
            string groupId = ddlGroups.SelectedValue;
            if ( !String.IsNullOrEmpty( groupId ) )
            {
                _groupId = int.Parse( groupId );
                _campusId = int.Parse( ddlCampuses.SelectedValue );
                var group = new GroupService( new RockContext() ).Get( int.Parse( groupId ) );
                _groupLocations =
                    group.GroupLocations.Where( gl => gl.Location.CampusId == _campusId || gl.Location.CampusId == null )
                        .ToList();
                ddlLocations.Items.Clear();
                if ( _groupLocations != null && _groupLocations.Count > 1 )
                {
                    foreach ( var groupLocation in _groupLocations )
                    {
                        var item = new ListItem();
                        item.Text = groupLocation.Location.Name;
                        item.Value = groupLocation.Id.ToString();
                        ddlLocations.Items.Add( item );
                    }

                    ddlLocations.Enabled = true;
                }
                else if ( _groupLocations != null && _groupLocations.Count == 1 )
                {
                    var item = new ListItem
                    {
                        Text = _groupLocations.First().Location.Name,
                        Value = _groupLocations.First().Location.Id.ToString()
                    };
                    ddlLocations.Items.Add( item );
                }
                else if ( _groupLocations != null && _groupLocations.Count == 0 )
                {
                    DisplayBox( "Tried to fetch attendance but failed.", NotificationBoxType.Warning );
                }
                if ( !String.IsNullOrEmpty( ddlLocations.SelectedValue ) )
                {
                    _locationId = ExtensionMethods.SelectedValueAsId( ddlLocations );
                }
            }
        }

        private void PopulateInstances()
        {
            ddlInstances.Items.Clear();
            int historicalWeeks = Convert.ToInt32( GetAttributeValue( "historicalweeks" ) );
            if ( !_groupId.HasValue )
            {
                _groupId = ExtensionMethods.SelectedValueAsId( ddlGroups );
            }
            if ( _groupId.HasValue )
            {
                if ( _locationId.HasValue )
                {
                    var location =
                        new GroupLocationService( new RockContext() ).Queryable()
                                                                   .AsNoTracking()
                                                                   .FirstOrDefault(
                                                                       gl =>
                                                                           gl.LocationId == _locationId.Value &&
                                                                           gl.GroupId == _groupId.Value );
                    var occurances = new List<KeyValuePair<string, DateTime>>();
                    if ( location != null )
                    {
                        foreach ( var schedule in location.Schedules.Where( s => s.HasSchedule() ) )
                        {
                            foreach (
                                var startDateTime in
                                schedule.GetScheduledStartTimes( RockDateTime.Now.AddDays( -7 * historicalWeeks ),
                                    RockDateTime.Now.AddDays( 7 * historicalWeeks ) ) )
                            {
                                occurances.Add( new KeyValuePair<string, DateTime>( schedule.Id.ToString(), startDateTime ) );
                            }
                        }
                        var orderedOccurances = occurances.OrderByDescending( o => o.Value );
                        foreach ( var occurance in orderedOccurances )
                        {
                            var item = new ListItem();
                            item.Text = occurance.Value.ToShortDateString() + " " + occurance.Value.ToShortTimeString();
                            item.Value = occurance.Value.ToString( "o" ) + "," + occurance.Key;
                            ddlInstances.Items.Add( item );
                        }
                        if ( ddlInstances.Items.Count > 1 )
                        {
                            ddlInstances.Enabled = true;
                        }
                        else if ( ddlInstances.Items.Count == 1 )
                        {
                            _scheduleId = orderedOccurances.FirstOrDefault().Key.AsIntegerOrNull();
                        }

                        btnSave.Enabled = ddlInstances.Items.Count >= 1;
                    }
                    else
                    {
                        DisplayBox( "Location not found", NotificationBoxType.Danger );
                    }
                }
            }
        }

        private static int? GetScheduleIdandStart( string itemText, out DateTime? startTime )
        {
            var split = itemText.Split( ',' );
            startTime = split[0].AsDateTime();
            if ( split.Length > 1 )
            {
                return split[1].AsInteger();
            }
            return null;
        }

        #endregion

        protected void cbMember_OnCheckedChanged( object sender, EventArgs e )
        {
            var checkbox = ( CheckBox ) sender;
            var listViewItem = ( ListViewItem ) checkbox.NamingContainer;
            var existingAttendance = _attendees[listViewItem.DataItemIndex];
            existingAttendance.Attended = !existingAttendance.Attended;

        }

        protected void lbSelectAll_Click( object sender, EventArgs e )
        {
            foreach ( var listViewItem in lvAttendance.Items )
            {
                var checkBox = listViewItem.FindControl( "cbMember" ) as CheckBox;
                if ( checkBox != null )
                {
                    checkBox.Checked = true;
                }

                var hf = listViewItem.FindControl("hfMember") as HiddenField;
                if (hf != null)
                {
                    int personAliasId = hf.Value.AsInteger();
                    var attendedPerson = _attendees.FirstOrDefault(a => a.PersonAliasId == personAliasId);
                    if (attendedPerson != null)
                    {
                        attendedPerson.Attended = true;
                    }
                }
            }
        }
    }

    [Serializable]
    public class PersonAttendance
    {
        public int PersonAliasId { get; set; }
        public bool Attended { get; set; }

        public string FullName { get; set; }
    }
}