using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Plugins.com_bricksandmortarstudio.CheckInExtensions
{
    [DisplayName("Attendance Input")]
    [Category("Bricks and Mortar Studio > Check-In Extensions")]
    [Description("Used to input attendance en mass after a group has concluded.")]
    [GroupTypeField("Attendance Type",  required: true, groupTypePurposeValueGuid: Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE, key: "GroupTypeTemplate")]   
    [IntegerField("Number of Historical Weeks", "The number of weeks instances go back to", true, 4, key: "historicalweeks")]
    [BooleanField("Quick Input", "Should keyboard shortcuts be provided for quickly selecting people in the person picker?", false)]
    public partial class RetrospectiveAttendanceInput : Rock.Web.UI.RockBlock
    {
        #region Fields

        private RockContext _rockContext;
        private int? _campusId;
        private int? _groupId;
        private int? _locationId;
        private int? _scheduleId;
        private DateTime? _startDateTime;
        private ICollection<GroupLocation> _groupLocations;
        private List<Attendance> _attendance;
        private List<int> _attendanceToRemove;
        private List<Attendance> _attendanceToAdd;
        private List<int> _attendanceToChange;
        private PersonAliasService _personAliasService;

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            gList.GridRebind += gList_GridRebind;

            _rockContext = new RockContext();
            if (_personAliasService == null)
            {
                _personAliasService = new PersonAliasService(_rockContext);
            }


            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upnlContent);
            RegisterScript();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if ( string.IsNullOrEmpty( GetAttributeValue( "GroupTypeTemplate" ) ) )
            {
                DisplayBox( "You have not yet set a group type in the block settings. A group type must be set in order for this block to function.", NotificationBoxType.Warning );
            }
            else
            {
                nbDisplayBox.Visible = false;
            }
            if (!Page.IsPostBack)
            {
                LoadPickers();
                if (!String.IsNullOrEmpty(ddlInstances.SelectedValue))
                {
                    GetAttended();
                }
                BindGrid();
            }

            // if the person picker is empty then open it for quick entry

            string personPickerStartupScript;
            if (GetAttributeValue("QuickInput").AsBoolean())
            {
                personPickerStartupScript = @"Sys.Application.add_load(function () {

                    var personPicker = $('.personAdd');
                    var currentPerson = personPicker.find('.picker-selectedperson').html();
                    if (currentPerson != null && currentPerson.length == 0) {
                        $(personPicker).find('a.picker-label').trigger('click');
                    }
                    var targetNode = $('ul.picker-select').first();

                    $('#personAdd').find('.picker-actions a').first().attr('accesskey', 'z');
                    var observer = new MutationObserver(function(mutations) {
	                    mutations.forEach(function(mutation) {
                            var inputs = targetNode.find('input');
                            var count = 1;
                            inputs.each(function (input) {
                                $(this).attr('accesskey', count);
                                $(this).off('change').change(function() {
                                    if ($(this).is(':checked')){
                                        $('#personAdd').find('.picker-actions a')[0].click(function() { eval($(this).attr('href')); });
                                    }
                                });
                                count++;
                            });
	                    });    
                    });

                    var observerConfig = {
	                    attributes: false, 
	                    childList: true, 
	                    characterData: false 
                    };
 
                    observer.observe(targetNode[0], observerConfig);

                });";
            }
            else
            {
                personPickerStartupScript = @"Sys.Application.add_load(function () {
                    var personPicker = $('.personAdd');
                    var currentPerson = personPicker.find('.picker-selectedperson').html();
                    if (currentPerson != null && currentPerson.length == 0) {
                        $(personPicker).find('a.picker-label').trigger('click');
                    }
                    var targetNode = $('ul.picker-select').first();
                
                    // TODO Change access keys to keydown events that trigger select as well
                    $('#personAdd').find('.picker-actions a').first().attr('accesskey', 'z');
                    var observer = new MutationObserver(function(mutations) {
	                    mutations.forEach(function(mutation) {
                            var inputs = targetNode.find('input');
                    var count = 1;
                    inputs.each(function (input) {
                        $(this).attr('accesskey', count);
                        count++;
                        console.log('Added access key');
                    });
	                    });    
                    });
                    var observerConfig = {
	                    attributes: false, 
	                    childList: true, 
	                    characterData: false 
                    };
 
                    observer.observe(targetNode[0], observerConfig);
                });";
            }
           

            this.Page.ClientScript.RegisterStartupScript(this.GetType(), "StartupScript", personPickerStartupScript,
                true);
            nbDisplayBox.Visible = false;
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            if (ViewState["CampusId"] != null)
            {
                _campusId = ViewState["CampusId"] as int?;
            }
            if (ViewState["GroupId"] != null)
            {
                _groupId = ViewState["GroupId"] as int?;
            }
            if (ViewState["LocationId"] != null)
            {
                _locationId = ViewState["LocationId"] as int?;
            }
            if (ViewState["StartDateTime"] != null)
            {
                _startDateTime = ViewState["StartDateTime"] as DateTime?;
            }
            if (ViewState["ScheduleId"] != null)
            {
                _scheduleId = ViewState["ScheduleId"] as int?;
            }
            if (ViewState["Attendance"] != null)
            {
                string json = ViewState["Attendance"] as string;
                _attendance = Attendance.FromJsonAsList(json) ?? new List<Attendance>();
                foreach (var attendee in _attendance)
                {
                    attendee.PersonAlias = _personAliasService.Get(attendee.PersonAliasId.Value);
                }
            }
            else
            {
                _attendance = new List<Attendance>();
            }
            if (ViewState["ToAdd"] != null)
            {
                string json = ViewState["ToAdd"] as string;
                _attendanceToAdd = Attendance.FromJsonAsList(json) ?? new List<Attendance>();
                foreach (var attendee in _attendanceToAdd)
                {
                    attendee.PersonAlias = _personAliasService.Get(attendee.PersonAliasId.Value);
                }
            }
            else
            {
                _attendanceToAdd = new List<Attendance>();
            }
            if (ViewState["ToRemove"] != null)
            {
                _attendanceToRemove = ViewState["ToRemove"] as List<int>;
            }
            else
            {
                _attendanceToRemove = new List<int>();
            }
            if (ViewState["ToChange"] != null)
            {
                _attendanceToChange = ViewState["ToChange"] as List<int>;
            }
            else
            {
                _attendanceToChange = new List<int>();
            }
            if ( ViewState["IsDirty"] != null  )
            {
                hfIsDirty.Value = ( ViewState["IsDirty"] as bool? ).ToString();
            }
        }

        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            if (_campusId != null)
            {
                ViewState["CampusId"] = _campusId;
            }
            if (_groupId != null)
            {
                ViewState["GroupId"] = _groupId;
            }

            if (_locationId != null)
            {
                ViewState["LocationId"] = _locationId;
            }
            if (_scheduleId != null)
            {
                ViewState["ScheduleId"] = _scheduleId;
            }
            if (_startDateTime != null)
            {
                ViewState["StartDateTime"] = _startDateTime;
            }
            if (_attendance != null)
            {
                string json = JsonConvert.SerializeObject(_attendance, Formatting.None, jsonSetting);
                ViewState["Attendance"] = json;
            }
            if (_attendanceToAdd != null)
            {
                string json = JsonConvert.SerializeObject(_attendanceToAdd, Formatting.None, jsonSetting);
                ViewState["ToAdd"] = json;
            }
            if (_attendanceToRemove != null)
            {
                ViewState["ToRemove"] = _attendanceToRemove;
            }
            if (_attendanceToChange != null)
            {
                ViewState["ToChange"] = _attendanceToChange;
            }
            if ( hfIsDirty.Value.AsBoolean() )
            {
                ViewState["IsDirty"] = true;
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
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            LoadPickers();
            if (!String.IsNullOrEmpty(ddlInstances.SelectedValue))
            {
                GetAttended();
            }
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void ddlCampuses_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateGroups();
            PopulateLocations();
            PopulateInstances();
            GetAttended();
            BindGrid();
        }

        protected void ddlLocations_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if ( !String.IsNullOrEmpty( ddlLocations.SelectedValue ) )
            {
                _locationId = ddlLocations.SelectedValueAsId();
            }
            PopulateInstances();
            GetAttended();
            BindGrid();
        }

        protected void ddlInstances_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            GetAttended();
            BindGrid();
        }


        protected void ddlGroups_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateLocations();
            PopulateInstances();
            if (ddlInstances.Items.Count != 0 )
            {
                GetAttended();
                BindGrid();
            }
            else
            {
                DisplayBox( "No instances found for the selected group and location", NotificationBoxType.Warning );
            }
        }

        protected void ppAttendee_OnSelectPerson(object sender, EventArgs e)
        {
            if (ppAttendee.PersonId.HasValue && ppAttendee.PersonAliasId.HasValue)
            {
                AddAttendance(ppAttendee.PersonId, ppAttendee.PersonAliasId);
            }
            BindGrid();
            ppAttendee.SetValue(null);
        }

        protected void GListRemove(object sender, RowEventArgs e)
        {
            bool removed = false;
            var rowGuid = (Guid)e.RowKeyValue;
            if (_attendanceToAdd.Any(a => a.Guid == rowGuid))
            {
                var remove = _attendanceToAdd.Find(a => a.Guid == rowGuid);
                if (remove != null)
                {
                    _attendanceToAdd.Remove(remove);
                    removed = true;
                }
            }
            else
            {
                var remove = _attendance.Find( a => a.Guid == rowGuid );
                if ( remove != null )
                {
                    _attendanceToRemove.Add( remove.Id );
                    removed = true;
                }
            }

            if (!removed)
            {
                return;
            }

            _attendance.RemoveEntity( rowGuid );
            BindGrid();
            SetDirty();
        }

        protected void Save(object sender, EventArgs e)
        {
            if (hfIsDirty.Value.AsBoolean())
            {
                gList.Enabled = false;
                var attendanceService = new AttendanceService( _rockContext );
                foreach (var attendance in _attendanceToAdd)
                {
                    attendanceService.AddOrUpdate( attendance.PersonAliasId.Value, attendance.StartDateTime, _groupId, _locationId, _scheduleId, _campusId );
                }

                foreach (int id in _attendanceToChange)
                {
                    var attendance = attendanceService.Get(id);
                    if (attendance == null )
                    {
                        continue;
                    }
                    attendance.DidAttend = true;
                }

                foreach (int id in _attendanceToRemove)
                {
                    var attendance = attendanceService.Get(id);
                    attendanceService.Delete(attendance);
                }

                _rockContext.SaveChanges();
                _attendance.Clear();
                _attendanceToAdd.Clear();
                _attendanceToRemove.Clear();
                _attendanceToChange.Clear();
                gList.Enabled = true;
                DisplayBox( "Attendance Saved", NotificationBoxType.Info );
                hfIsDirty.Value = "false";
                GetAttended();
                BindGrid();
            }
        }

        #endregion

        #region Methods

        private void DisplayBox(string text, NotificationBoxType type )
        {
            nbDisplayBox.Text = text;
            nbDisplayBox.NotificationBoxType = type;
            nbDisplayBox.Visible = true;
        }
        

        private void RegisterScript()
        {
            string script = string.Format(@"
window.onbeforeunload = function (e) {{
if ( $('#{0}').val() == 'true' ) {{
            return 'You have not saved your changes. Are you sure you want to continue?';    
        }}
        return;
}};
    function isDirty() {{
        if ( $('#{0}').val() == 'true' ) {{
            if ( confirm('You have not saved your changes. Are you sure you want to continue?') ) {{
                return false;
            }}
            return true;
        }}
        return false;
    }}
    $('#{1}').click( function() {{
        if ( isDirty() ) {{
            return false;
        }}
    }});
    $('#{2}').click( function() {{
        if ( isDirty() ) {{
            return false;
        }}
    }});
    $('#{3}').click( function() {{
        if ( isDirty() ) {{
            return false;
        }}
    }});
    $('#{4}').click( function() {{
        if ( isDirty() ) {{
            return false;
        }}
    }});
", hfIsDirty.ClientID, ddlLocations.ClientID, ddlCampuses.ClientID, ddlGroups.ClientID, ddlInstances.ClientID);
            ScriptManager.RegisterStartupScript((Control) upnlContent, this.GetType(), "isDirty", script, true);

        }

        private void AddAttendance(int? personId, int? personAliasId)
        {
            if (_attendance != null)
            {
                var attended = _attendance.Where(a => a.PersonAlias.PersonId == personId).FirstOrDefault();
                            
                if (attended == null && _campusId.HasValue && _groupId.HasValue && _startDateTime.HasValue && _locationId.HasValue)
                {
                    var attendance = new Attendance
                    {
                        CampusId = _campusId.Value,
                        DidAttend = true,
                        PersonAliasId = personAliasId,
                        StartDateTime = _startDateTime.Value,
                        PersonAlias = _personAliasService.Get(personAliasId.Value)
                    };
                    _attendance.Add(attendance);
                    _attendanceToAdd.Add(attendance);
                    SetDirty();
                }
                else if (attended.DidAttend == null || attended.DidAttend.Value == false || attended.Occurrence.DidNotOccur == null || attended.Occurrence.DidNotOccur.Value == false)
                {
                    _attendanceToChange.Add(attended.Id);
                    SetDirty();
                }
            }
        }

        private void SetDirty()
        {
            hfIsDirty.Value = "true";
            DisplayBox( "You have unsaved changes.", NotificationBoxType.Warning );
        }

        private void GetAttended()
        {
            _scheduleId = GetScheduleIdandStart(ddlInstances.SelectedValue, out _startDateTime);
            if ( _attendanceToAdd != null )
            {
                _attendanceToAdd.Clear();
            }
            if ( _attendanceToChange != null )
            {

                _attendanceToChange.Clear();
            }
            if ( _attendanceToRemove != null )
            {
                _attendanceToRemove.Clear();
            }
            if (_scheduleId.HasValue &&_startDateTime.HasValue)
            {
                // Try to find the selected occurrence based on group's schedule

                _attendance = new AttendanceService(_rockContext).Queryable("PersonAlias").Where(a =>
                                a.Occurrence.GroupId == _groupId &&
                                a.Occurrence.LocationId == _locationId &&
                                a.Occurrence.ScheduleId == _scheduleId &&
                                a.StartDateTime == _startDateTime.Value
                                && (a.DidAttend == null || a.DidAttend.Value)
                                && (a.Occurrence.DidNotOccur == null || !a.Occurrence.DidNotOccur.Value))
                                .ToList();
            }
            else
            {
                DisplayBox( "Tried to fetch attendance but failed.", NotificationBoxType.Warning );
            }
        }

        private void LoadPickers()
        {
            ddlCampuses.Items.Clear();
            var campuses = CampusCache.All().OrderBy(a => a.Name);
            foreach (var campus in campuses)
            {
                var listItem = new ListItem();
                listItem.Text = campus.Name;
                listItem.Value = campus.Id.ToString();
                ddlCampuses.Items.Add(listItem);
            }
            if (campuses.Count() < 2)
            {
                _campusId = campuses.First().Id;
                ddlCampuses.Enabled = false;
                ddlCampuses.Visible = false;
                ExtensionMethods.AddCssClass((HtmlControl) campusContainer, "hidden");
            }

            PopulateGroups();
            PopulateLocations();
            PopulateInstances();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if (gList.DataSource == null)
            {
                gList.DataSource = _attendance;
            }
            gList.DataBind();
        }

        private void PopulateGroups(Dictionary<string, string> userPreferences = null)
        {
            string groupTypeTemplateGuid = this.GetAttributeValue("GroupTypeTemplate");
            if (!String.IsNullOrEmpty(groupTypeTemplateGuid))
            {
                var groupType = new GroupTypeService(_rockContext).Get(groupTypeTemplateGuid.AsGuid());
                var groups = new ChildCheckInGroupGenerator().Get(new List<GroupType> {groupType});
                ddlGroups.Items.Clear();
                //Find groups with schedules which folks can attend
                foreach (var group in groups.Where(g => g.GroupLocations.Any(l => l.Schedules.Count > 0)))
                {
                    var listItem = new ListItem();
                    listItem.Text = group.Name;
                    listItem.Value = group.Id.ToString();
                    ddlGroups.Items.Add(listItem);
                }
                if (ddlGroups.Items.Count < 1)
                {
                    DisplayBox( "No groups found for the selected Attendance Type", NotificationBoxType.Warning );
                }
            }
        }

        protected void PopulateLocations()
        {
            string groupId = ddlGroups.SelectedValue;
            if (!String.IsNullOrEmpty(groupId))
            {
                _groupId = int.Parse(groupId);
                _campusId = int.Parse( ddlCampuses.SelectedValue );
                var group = new GroupService(_rockContext).Get(int.Parse(groupId));
                _groupLocations =
                    group.GroupLocations.Where(gl => gl.Location.CampusId == _campusId || gl.Location.CampusId == 0)
                        .ToList();
                ddlLocations.Items.Clear();
                if (_groupLocations != null && _groupLocations.Count > 1)
                {
                    foreach (var groupLocation in _groupLocations)
                    {
                        var item = new ListItem();
                        item.Text = groupLocation.Location.Name;
                        item.Value = groupLocation.Id.ToString();
                        ddlLocations.Items.Add(item);
                    }

                    ddlLocations.Enabled = true;
                }
                else if (_groupLocations != null && _groupLocations.Count == 1)
                {
                    var item = new ListItem
                    {
                        Text = _groupLocations.First().Location.Name,
                        Value = _groupLocations.First().Location.Id.ToString()
                    };
                    ddlLocations.Items.Add(item);
                }
                else if ( _groupLocations != null && _groupLocations.Count == 0)
                {
                    DisplayBox( "Tried to fetch attendance but failed.", NotificationBoxType.Warning );
                }
                if (!String.IsNullOrEmpty(ddlLocations.SelectedValue))
                {
                    _locationId = ExtensionMethods.SelectedValueAsId(ddlLocations);
                }
            }
        }

        private void PopulateInstances()
        {
            ddlInstances.Items.Clear();
            int historicalWeeks = Convert.ToInt32(GetAttributeValue("historicalweeks"));
            var locationIds = new List<int>();
            var scheduleIds = new List<int>();
            if (!_groupId.HasValue)
            {
                _groupId = ExtensionMethods.SelectedValueAsId(ddlGroups);
            }
            if (_groupId.HasValue)
            {
                var group = new GroupService(_rockContext).Get(_groupId.Value);
                var groupLocations = group.GroupLocations.Where(l => l.Location.CampusId == null || l.Location.CampusId == _campusId);
                if (groupLocations != null)
                {
                    var occurances = new List<KeyValuePair<string, DateTime>>();
                    foreach (var groupLocation in groupLocations)
                    {
                        locationIds.Add(groupLocation.LocationId);
                        foreach (var schedule in groupLocation.Schedules.Where(s => s.HasSchedule()))
                        {
                            foreach (
                                var startDateTime in
                                    schedule.GetScheduledStartTimes(RockDateTime.Now.AddDays(-7 * historicalWeeks), RockDateTime.Now))
                            {
                                occurances.Add(new KeyValuePair<string, DateTime>(schedule.Id.ToString(), startDateTime));
                            }
                        }
                        var orderedOccurances = occurances.OrderByDescending(o => o.Value);
                        foreach (var occurance in orderedOccurances)
                        {
                            var item = new ListItem();
                            item.Text = occurance.Value.ToShortDateString() + " " + occurance.Value.ToShortTimeString();
                            item.Value = occurance.Value.ToString("o") + "," + occurance.Key;
                            ddlInstances.Items.Add(item);
                        }

                    }
                    if (ddlInstances.Items.Count > 1)
                    {
                        ddlInstances.Enabled = true;
                    }
                }
            }
        }


        private static int? GetScheduleIdandStart(string itemText, out DateTime? startTime)
        {
            var split = itemText.Split(',');
            startTime = split[0].AsDateTime();
            if (split.Length > 1)
            {
                return split[1].AsInteger();
            }
            else
            {
                return null;
            }
        }

        #endregion

        private class ChildCheckInGroupGenerator
        {
            /// Given a list of groupTypes this classes returns their child groups that are related to check-in via recursively expanding the tree avoiding loops

            private int _checkInFilterId;
            private readonly List<int> _seenGroupTypeIds = new List<int>();
            private readonly List<Group> _groups = new List<Group>();

            public List<Group> Get( IEnumerable<GroupType> groupTypes )
            {
                _checkInFilterId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER ).Id;
                GetValidGroups( groupTypes );
                return _groups;
            }

            public List<Group> Get( GroupType groupType )
            {
                _checkInFilterId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ).Id;
                GetValidGroup( groupType );
                return _groups;
            }

            public List<Group> Get( IEnumerable<int> groupTypeIds )
            {
                _checkInFilterId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ).Id;
                var groupTypeService = new GroupTypeService( new Rock.Data.RockContext() );
                var groupTypes = groupTypeService.GetByIds( groupTypeIds.ToList() );
                GetValidGroups( groupTypes );
                return _groups;
            }

            private void GetValidGroups( IEnumerable<GroupType> groupTypes )
            {
                foreach ( var groupType in groupTypes )
                {
                    GetValidGroup( groupType );
                }
            }

            private void GetValidGroup( GroupType groupType )
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
}