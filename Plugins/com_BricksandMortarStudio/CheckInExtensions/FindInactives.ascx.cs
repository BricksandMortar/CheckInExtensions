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
    [DisplayName("Find Inactive Attendees")]
    [Category("Bricks and Mortar Studio > Check-In Extensions")]
    [Description("Used to find people who have a recently checked into a group with a given record status.")]
    [DefinedValueField(Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, "Inactive Record Statuses", "Record statuses to search for", true, true, Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE, key:"inactivestatus")]
    [LinkedPage("Person Profile Page", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", true, Rock.SystemGuid.Page.PERSON_PROFILE_PERSON_PAGES, key:"personprofilepage")]
    public partial class FindInactives : Rock.Web.UI.RockBlock
    {
        #region Fields

        private RockContext _rockContext;
        private List<Attendance> _attendance;
        private PersonAliasService _personAliasService;

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
                if (!string.IsNullOrWhiteSpace( sdrpTimeValue ) && sdrpTimeValue.AsIntegerOrNull().HasValue )
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

        private List<Person> GetAttended()
        {
            DateTime startDateTime;
            var recordIds = new List<int>();
            var recordGuids = GetAttributeValues("inactivestatus");
            if (recordGuids != null)
            {
                foreach (string recordGuid in recordGuids)
                {
                    recordIds.Add(DefinedValueCache.Read(recordGuid.AsGuid()).Id);
                }
            }
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
                    startDateTime = RockDateTime.Now.AddYears(sdrpAttendedBetween.NumberOfTimeUnits);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var persons = new List<Person>();
            var seenpersonAliasId = new List<int>();
            var attendence = new AttendanceService(_rockContext).Queryable().Where(a => a.StartDateTime > startDateTime && a.ScheduleId.HasValue).ToList();
            foreach (var instance in attendence)
            {
                if (instance.PersonAlias.Person.RecordStatusValueId.HasValue &&
                    recordIds.Contains(instance.PersonAlias.Person.RecordStatusValueId.Value) &&
                    instance.PersonAlias.Person.RecordStatusValue.Guid.ToString() !=
                    Rock.SystemGuid.DefinedValue.PERSON_REVIEW_REASON_SELF_INACTIVATED &&
                    !seenpersonAliasId.Contains(instance.PersonAlias.Id))
                {
                    persons.Add(instance.PersonAlias.Person);
                    seenpersonAliasId.Add(instance.PersonAlias.Id);
                }
            }

            return persons.ToList();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if (gList.DataSource == null)
            {
                var notAttended = GetAttended();
                if (notAttended != null)
                {
                    gList.DataSource = notAttended;
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
            NavigateToLinkedPage("personprofilepage", new Dictionary<string, string> {{"PersonId", personId.ToString()}});
        }
    }
}