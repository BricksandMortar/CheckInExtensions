using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Plugins.com_bricksandmortarstudio.CheckInExtensions
{
    [DisplayName("Attended Groups")]
    [Category("Bricks and Mortar Studio > Check-In Extensions")]
    [Description("Used to find the groups checked into by people.")]
    [GroupTypesField("Excluded Group Types", "Group types that should not have their groups included in the results", false, key:"excludedgroups" )]
    [LinkedPage("Person Profile Page", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", true, Rock.SystemGuid.Page.PERSON_PROFILE_PERSON_PAGES, key: "personprofilepage")]
    public partial class FindAttendedClasses : Rock.Web.UI.RockBlock
    {
        #region Fields

        private RockContext _rockContext;
        private List<Attendance> _attendance;
        private PersonAliasService _personAliasService;
        private List<int> _seenGroupTypeIds = new List<int>();
        private List<int> _validGroupIds = new List<int>();
        private int _checkInTemplateId;

        #endregion

        #region Properties

        // used for public / protected properties

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                string sdrpTimeUnit = GetBlockUserPreference( "sdrp-time-unit" );
                string sdrpTimeValue = GetBlockUserPreference( "sdrp-time-value" );
                if ( !string.IsNullOrWhiteSpace( sdrpTimeUnit ) && sdrpTimeValue.AsIntegerOrNull().HasValue )
                {
                    sdrpAttendedBetween.TimeUnit = sdrpTimeUnit.ConvertToEnum<SlidingDateRangePicker.TimeUnitType>();
                }
                if ( !string.IsNullOrWhiteSpace( sdrpTimeValue ) && sdrpTimeValue.AsIntegerOrNull().HasValue )
                {
                    sdrpAttendedBetween.NumberOfTimeUnits = sdrpTimeValue.AsInteger();
                }
                if (String.IsNullOrEmpty(GetAttributeValue("personprofilepage")))
                {
                    nbWarning.Text = "Person profile page not set in block settings";
                }
                BindGrid();
            }
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
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
        }

        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            if (_attendance != null)
            {
                string json = JsonConvert.SerializeObject(_attendance, Formatting.None, jsonSetting);
                ViewState["Attendance"] = json;
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
            if (String.IsNullOrEmpty(GetAttributeValue("personprofilepage")))
            {
                nbWarning.Text = "Person profile page not set in block settings";
            }
            else
            {
                nbWarning.Visible = false;
            }
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

        #endregion

        #region Methods

        private void GetValidGroups(IEnumerable<GroupType> groupTypes)
        {
            foreach (var groupType in groupTypes)
            {
                _seenGroupTypeIds.Add(groupType.Id);
                var groups = groupType.Groups.Where(g => !_validGroupIds.Contains(g.Id));
                foreach (var group in groups)
                {
                    _validGroupIds.Add(group.Id);
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

        private List<AttendanceSummary> GetAttended()
        {
            _checkInTemplateId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE).Id;
            var excludedGroupTypes = GetAttributeValue("excludedgroups").SplitDelimitedValues();
            var groupTypeService = new GroupTypeService(_rockContext);
            foreach (string groupTypeStrinGuid in excludedGroupTypes)
            {
                _seenGroupTypeIds.Add(groupTypeService.Get(groupTypeStrinGuid.AsGuid()).Id);
            }
            var validGroupTypes = new GroupTypeService(_rockContext).Queryable().Where(g => g.GroupTypePurposeValueId == _checkInTemplateId).ToList();
            GetValidGroups(validGroupTypes);
            DateTime startDateTime;
            switch (sdrpAttendedBetween.TimeUnit)
            {
                case (SlidingDateRangePicker.TimeUnitType.Day):
                    startDateTime = RockDateTime.Now.AddDays(-sdrpAttendedBetween.NumberOfTimeUnits);
                    break;
                case (SlidingDateRangePicker.TimeUnitType.Hour):
                    startDateTime = RockDateTime.Now.AddHours(-sdrpAttendedBetween.NumberOfTimeUnits);
                    break;
                case (SlidingDateRangePicker.TimeUnitType.Month):
                    startDateTime = RockDateTime.Now.AddMonths(-sdrpAttendedBetween.NumberOfTimeUnits);
                    break;
                case (SlidingDateRangePicker.TimeUnitType.Week):
                    startDateTime = RockDateTime.Now.AddDays(sdrpAttendedBetween.NumberOfTimeUnits * -7);
                    break;
                case (SlidingDateRangePicker.TimeUnitType.Year):
                    startDateTime = RockDateTime.Now.AddYears(sdrpAttendedBetween.NumberOfTimeUnits * -7);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var attendenceSummaries = new List<AttendanceSummary>();
            var attendence = new AttendanceService(_rockContext).Queryable().Where(a => a.StartDateTime > startDateTime && a.GroupId.HasValue && _validGroupIds.Contains(a.GroupId.Value)).ToList();
            foreach (var instance in attendence)
            {
                if (!attendenceSummaries.Any(s => s.Person == instance.PersonAlias.Person))
                {
                    var summary = new AttendanceSummary();
                    summary.Id = instance.PersonAlias.PersonId;
                    summary.Person = instance.PersonAlias.Person;
                    summary.AttendedGroups = new List<Group> { instance.Group };
                    summary.LastAttended = instance.StartDateTime;
                    summary.AttendedCount++;
                    attendenceSummaries.Add(summary);
                }
                else
                {
                    var existingSummary =
                        attendenceSummaries.FirstOrDefault(s => s.Person == instance.PersonAlias.Person);
                    if (existingSummary == null)
                    {
                        continue;
                    }
                    if (instance.StartDateTime < existingSummary.LastAttended)
                    {
                        existingSummary.LastAttended = instance.StartDateTime;
                    }
                    if (!existingSummary.AttendedGroups.Contains(instance.Group))
                    {
                        existingSummary.AttendedGroups.Add(instance.Group);
                    }
                    existingSummary.AttendedCount++;
                }

            }
            return attendenceSummaries;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            SortProperty sortProperty = gList.SortProperty;
            if (gList.DataSource == null)
            {
                var attendanceSummaries = GetAttended();
                if (attendanceSummaries != null)
                {
                    if (sortProperty != null)
                    {
                        attendanceSummaries.AsQueryable().Sort(sortProperty);
                    }
                    gList.DataSource = attendanceSummaries;
                }
                gList.DataBind();
            }
        }

        #endregion

        protected void sdrpAttendedBetween_OnSelectedDateRangeChanged(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void Refresh(object sender, EventArgs e)
        {
            SetBlockUserPreference( "sdrp-time-unit", sdrpAttendedBetween.TimeUnit.ToString() );
            SetBlockUserPreference( "sdrp-time-value", sdrpAttendedBetween.NumberOfTimeUnits.ToString() );
            BindGrid();
        }

        protected void gClick(object sender, RowEventArgs e)
        {
            var personId = e.RowKeyValue as int?;
            NavigateToLinkedPage("personprofilepage", new Dictionary<string, string> { { "PersonId", personId.ToString() } });
        }
    }

    internal class AttendanceSummary
    {
        public Person Person { get; set; }
        public DateTime LastAttended { get; set; }

        public List<Group> AttendedGroups { get; set; }

        public int AttendedCount { get; set; }

        public int Id { get; set; }
    }
}