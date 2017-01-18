using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;
using com.bricksandmortarstudio.checkinextensions.Utils;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Plugins.com_bricksandmortarstudio.CheckInExtensions
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName("Attendance Overview")]
    [Category("Bricks and Mortar Studio > Check-In Extensions")]
    [Description("A summary of attendance.")]
    [TextField("Headcount Expression", "The wording used to define a headcount", true, "Head Count")]
    [TextField( "Check In Expression", "The wording used to define a kiosk based check-in count", true, "Check-In Count" )]
    [BooleanField("Calculate Percentage Difference", "Display the difference as a percentage", true)]
    public partial class AttendanceOverview : Rock.Web.UI.RockBlock
    {
        #region Fields

        private RockContext _rockContext;
        private GroupTypeService _groupTypeService;
        private bool _calculatePercentageDifference;

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
            if (_groupTypeService == null)
            {
                _groupTypeService = new GroupTypeService(_rockContext);
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
                SetupSettings();
                BindGrid();
            }
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            //if (ViewState["Attendance"] != null)
            //{
            //    string json = ViewState["Attendance"] as string;
            //    _attendance = Attendance.FromJsonAsList(json) ?? new List<Attendance>();
            //    foreach (var attendee in _attendance)
            //    {
            //        attendee.PersonAlias = _personAliasService.Get(attendee.PersonAliasId.Value);
            //    }
            //}
            //else
            //{
            //    _attendance = new List<Attendance>();
            //}
        }

        protected override object SaveViewState()
        {
            //var jsonSetting = new JsonSerializerSettings
            //{
            //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            //};

            //if (_attendance != null)
            //{
            //    string json = JsonConvert.SerializeObject(_attendance, Formatting.None, jsonSetting);
            //    ViewState["Attendance"] = json;
            //}
            return base.SaveViewState();
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        protected void sdrpAttendedBetween_OnSelectedDateRangeChanged(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void Refresh(object sender, EventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {

            nbWarning.Visible = false;
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind(object sender, EventArgs e)
        {
            BindGrid();
        }

        public void OnFormatDataValue(object sender, CallbackField.CallbackEventArgs e)
        {
            var callbackField = sender as CallbackField;
            var groupMemberList = e.DataValue as IEnumerable<GroupTypeAttendanceSummary>;
            if (callbackField != null)
            {
                var values = callbackField.DataFormatString.Split(',');
                var type = (ColumnType)Enum.Parse(typeof(ColumnType), values[1]);
                var attendanceSummary = groupMemberList.FirstOrDefault(
                    a => a.GroupTypeId == values[0].AsInteger());

                if (attendanceSummary == null)
                {
                    e.FormattedValue = "";
                    return;
                }

                switch (type)
                {
                    case ColumnType.CheckinCount:
                        e.FormattedValue = attendanceSummary.CheckInCount.ToString();
                        break;
                    case ColumnType.HeadCount:
                        e.FormattedValue = attendanceSummary.HeadCount.ToString();
                        break;
                    case ColumnType.Difference:
                        if (!(attendanceSummary.CheckInCount == 0 || attendanceSummary.HeadCount == 0))
                        {
                            //Either provide percentage difference or absolute difference
                            if ( _calculatePercentageDifference )
                            {
                                var difference = ( Math.Abs( attendanceSummary.HeadCount - attendanceSummary.CheckInCount ) /
                                             Convert.ToDecimal( attendanceSummary.HeadCount ) * 100 );
                                e.FormattedValue = difference.ToString( "#.##" ) + "%";
                            }
                            else
                            {
                                e.FormattedValue = Math.Abs( attendanceSummary.HeadCount - attendanceSummary.CheckInCount ).ToString();
                            }
                            
                        }
                        else
                        {
                            e.FormattedValue = "N/A";
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                e.FormattedValue = "";
            }
        }

        #endregion

        #region Methods

        private string GetColumnHeader(ColumnType column)
        {
            switch ( column )
            {
                case ColumnType.CheckinCount: return GetAttributeValue( "CheckInExpression" );
                case ColumnType.HeadCount: return GetAttributeValue( "HeadcountExpression" );
                case ColumnType.Difference: return "Difference";
                default: return "";
            }

        }

        private IEnumerable<GroupType> GetCheckInTemplatesGroupTypes()
        {
            if (_groupTypeService == null)
            {
                _groupTypeService = new GroupTypeService(_rockContext);
            }
            int checkInTemplateId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE).Id;
            var checkInTemplates = _groupTypeService.Queryable().AsNoTracking().Where(g => g.GroupTypePurposeValueId == checkInTemplateId);

            return checkInTemplates;
        }

        private IEnumerable<int> GetCheckInTemplatesIds()
        {
            if (_groupTypeService == null)
            {
                _groupTypeService = new GroupTypeService(_rockContext);
            }
            int checkInTemplateId = DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE).Id;
            var checkInTemplates = _groupTypeService.Queryable().AsNoTracking().Where(g => g.GroupTypePurposeValueId == checkInTemplateId);
            return checkInTemplates.Select(g => g.Id);
        }

        private void SetupSettings()
        {
            var groupTypes = GetCheckInTemplatesGroupTypes().ToList();
            foreach (var groupType in groupTypes)
            {
                var listItem = new ListItem
                {
                    Value = groupType.Id.ToString(),
                    Text = groupType.Name,
                };
                cblCheckInTemplates.Items.Add(listItem);
            }
            cblCheckInTemplates.SetValues(groupTypes.Select(g => g.Id));
            sdrpAttendedBetween.DateRangeModeStart = RockDateTime.Now.PreviousOfWeek(DayOfWeek.Monday);
            sdrpAttendedBetween.DateRangeModeEnd = RockDateTime.Now;
        }

        private IEnumerable<Week> GetWeekSpans()
        {
            var startDateTime = sdrpAttendedBetween.DateRangeModeStart;
            var endDateTime = sdrpAttendedBetween.DateRangeModeEnd ?? RockDateTime.Now;

            if (startDateTime == null)
            {
                switch (sdrpAttendedBetween.TimeUnit)
                {
                    case SlidingDateRangePicker.TimeUnitType.Day:
                        startDateTime = RockDateTime.Now.AddDays(-sdrpAttendedBetween.NumberOfTimeUnits);
                        break;
                    case SlidingDateRangePicker.TimeUnitType.Hour:
                        startDateTime = RockDateTime.Now.AddHours(-sdrpAttendedBetween.NumberOfTimeUnits);
                        break;
                    case SlidingDateRangePicker.TimeUnitType.Month:
                        startDateTime = RockDateTime.Now.AddMonths(-sdrpAttendedBetween.NumberOfTimeUnits);
                        break;
                    case SlidingDateRangePicker.TimeUnitType.Week:
                        startDateTime = RockDateTime.Now.AddDays(sdrpAttendedBetween.NumberOfTimeUnits * -7);
                        break;
                    case SlidingDateRangePicker.TimeUnitType.Year:
                        startDateTime = RockDateTime.Now.AddYears(sdrpAttendedBetween.NumberOfTimeUnits * -7);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return GetDates(startDateTime.Value, endDateTime);
        }

        private static IEnumerable<Week> GetDates(DateTime startDateTime, DateTime endDateTime)
        {
            var allDates = new List<DateTime>();
            for (var date = startDateTime; date <= endDateTime;)
            {
                allDates.Add(date);
                date = date.NextDayOfWeek(DayOfWeek.Monday);
            }
            var dates = allDates.Select(d => new Week(d.Date, d.NextDayOfWeek(DayOfWeek.Sunday, true).OneSecondToMidnight())).Distinct().ToList();
            if (dates.Any() && dates.Last().End > endDateTime)
            {
                dates.Last().End = endDateTime;
            }
            return dates;
        }

        private List<TemplateContainer> GetCheckInTemplateGroups()
        {
            var checkInGroupTypes = cblCheckInTemplates.SelectedValuesAsInt.Any() ? cblCheckInTemplates.SelectedValuesAsInt.Select(i => _groupTypeService.Get(i)).ToList() : GetCheckInTemplatesGroupTypes();
            var result = new List<TemplateContainer>();
            foreach (var groupType in checkInGroupTypes)
            {
                var results = new ChildCheckInGroupGenerator().Get(new List<GroupType> {groupType});
                result.Add(new TemplateContainer
                {
                    GroupType = groupType,
                    GroupTypeId = groupType.Id,
                    GroupGuids = results.Select(g => g.Guid).ToList(),
                    GroupIds = results.Select(g => g.Id).ToList()
                });
            }
            return result;
        }

        private List<AttendanceResult> GetAttendance()
        {
            var weekSpans = GetWeekSpans();
            var checkInTemplateHeirachy = GetCheckInTemplateGroups();

            var attendanceService = new AttendanceService(_rockContext);
            var metricService = new MetricService(_rockContext);
            var attendanceResultSummary = new List<AttendanceResult>();

            foreach (var weekSpan in weekSpans)
            {
                var weekResult = new AttendanceResult(weekSpan);
                foreach (var template in checkInTemplateHeirachy)
                {
                    int checkInCount = attendanceService.Queryable().AsNoTracking().Where(a => a.StartDateTime >= weekSpan.Start && a.StartDateTime < weekSpan.End && template.GroupIds.Contains(a.GroupId.Value)).Distinct().Count();
                    var headCount = metricService.Queryable("MetricValues").AsNoTracking().Where(m => m.ForeignGuid.HasValue && template.GroupGuids.Contains(m.ForeignGuid.Value)).Select(m => m.MetricValues.Where(v => v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value >= weekSpan.Start && v.MetricValueDateTime.Value < weekSpan.End)).SelectMany(i => i).Sum(v => v.YValue);
                    weekResult.AddInstance(decimal.ToInt32(headCount ?? 0), checkInCount, template.GroupType);
                }
                attendanceResultSummary.Add(weekResult);
            }
            return attendanceResultSummary;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var sortProperty = gList.SortProperty;
            if (gList.DataSource == null)
            {
                _calculatePercentageDifference = GetAttributeValue( "CalculatePercentageDifference" ).AsBoolean(true);
                var attendanceSummaries = GetAttendance();
                if (attendanceSummaries != null)
                {
                    if (sortProperty != null)
                    {
                        attendanceSummaries.AsQueryable().Sort(sortProperty);
                    }
                    var groupTypes = attendanceSummaries.SelectMany(a => a.GroupTypeAttendanceSummaries.Select(g => g.GroupType).Distinct()).Distinct().ToList();
                    foreach (var column in gList.Columns.OfType<CallbackField>().ToList())
                    {
                        gList.Columns.Remove(column);
                    }
                    foreach (var groupType in groupTypes)
                    {
                        foreach (var type in Enum.GetValues(typeof(ColumnType)).Cast<ColumnType>())
                        {
                            var field = new CallbackField
                            {
                                ConvertEmptyStringToNull = true,
                                DataFormatString = groupType.Id + "," + type,
                                HeaderText = groupType.Name + " " + GetColumnHeader(type),
                                DataField = "GroupTypeAttendanceSummaries",
                            };
                            if (type == ColumnType.Difference)
                            {
                                field.ColumnPriority = ColumnPriority.DesktopSmall;
                            }
                            gList.Columns.Add(field);
                            field.OnFormatDataValue += OnFormatDataValue;
                        }
                    }
                    gList.DataSource = attendanceSummaries;
                }
            }
            gList.DataBind();
        }

        #endregion

        protected void cblCheckInTemplates_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            BindGrid();
        }
    }

    internal enum ColumnType
    {
        HeadCount,
        CheckinCount,
        Difference
    }

    internal class Week
    {
        public Week(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    internal class TemplateContainer
    {
        public GroupType GroupType { get; set; }
        public int GroupTypeId { get; set; }
        public List<Guid> GroupGuids { get; set; }
        public List<int> GroupIds { get; set; }
    }

    internal class AttendanceResult
    {
        public AttendanceResult(DateTime start, DateTime end)
        {
            Week = new Week(start, end);
            GroupTypeAttendanceSummaries = new List<GroupTypeAttendanceSummary>();
        }

        public AttendanceResult(Week week)
        {
            Week = week;
            GroupTypeAttendanceSummaries = new List<GroupTypeAttendanceSummary>();
        }

        public Week Week { get; set; }
        public List<GroupTypeAttendanceSummary> GroupTypeAttendanceSummaries { get; set; }

        public void AddInstance(int headCount, int checkInCount, GroupType groupType)
        {
            GroupTypeAttendanceSummaries.Add(new GroupTypeAttendanceSummary(headCount, checkInCount, groupType));
        }
    }

    internal class GroupTypeAttendanceSummary
    {
        public GroupTypeAttendanceSummary(int headCount, int checkInCount, GroupType groupType)
        {
            HeadCount = headCount;
            CheckInCount = checkInCount;
            GroupType = groupType;
            GroupTypeId = groupType.Id;
        }

        public int GroupTypeId { get; set; }
        public int CheckInCount { get; set; }
        public int HeadCount { get; set; }
        public GroupType GroupType { get; set; }
    }
}