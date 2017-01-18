using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Web.UI.WebControls;
using com.bricksandmortarstudio.checkinextensions.Utils;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace Plugins.com_bricksandmortarstudio.CheckInExtensions
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( @"Year Overview Headcount Comparison" )]
    [Category( "Bricks and Mortar Studio > Check-In Extensions" )]
    [Description( "Provides a statistical overview of two years of headcounts" )]
    [TextField( "Title", "The title the block should display", true, "Headcount Overview for the Year", order:0 )]
    [IntegerField( "Historic Years", "Years to display for comparison", true, 2, order:1 )]
    public partial class YearOverviewHeadcountComparison : RockBlock
    {
        #region Fields

        private readonly HeadCountStatisticCalculator _statisticCalculator = new HeadCountStatisticCalculator();
//        private YearGroupHeadCountStatisticCalculator _yearGroupHeadCountStatisticCalculator = new YearGroupHeadCountStatisticCalculator();
//        private FilteredGroupHeadCountStatisticCalculator _filteredGroupHeadCountStatisticCalculator = new FilteredGroupHeadCountStatisticCalculator();

        private DateTime _startDateTime;
        private DateTime _endDateTime;
        private DateTime _comparisonStartDate;
        private DateTime _comparisonEndDate;
        #endregion

        #region Properties
        

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
            gStatistics.GridRebind += gStatistics_GridRebind;
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                lBlockName.Text = GetAttributeValue( "Title" );
                SetupSettings();
                UpdateDates();
                BindGrid();
            }
            else
            {
                UpdateDates();
            }
        }

        private void UpdateDates()
        {
            if ( cbDateRangeEnabled.Checked && mdStart.SelectedDate.HasValue &&
                mdEnd.SelectedDate.HasValue )
            {
                int currentYear = ddlCurrentYear.SelectedValue.AsIntegerOrNull() != null ? ddlCurrentYear.SelectedValue.AsInteger() : RockDateTime.Now.Year;
                _startDateTime = new DateTime( currentYear, mdStart.SelectedDate.Value.Month,
                    mdStart.SelectedDate.Value.Day );
                _endDateTime = new DateTime( currentYear, mdEnd.SelectedDate.Value.Month,
                    mdEnd.SelectedDate.Value.Day );
                _comparisonStartDate = new DateTime( ddlComparisonYear.SelectedValue.AsInteger(), mdStart.SelectedDate.Value.Month, mdStart.SelectedDate.Value.Day );
                _comparisonEndDate = new DateTime( ddlComparisonYear.SelectedValue.AsInteger(), mdEnd.SelectedDate.Value.Month, mdEnd.SelectedDate.Value.Day );
            }
            else
            {
                int currentYear = ddlCurrentYear.SelectedValue.AsIntegerOrNull() != null ? ddlCurrentYear.SelectedValue.AsInteger() : RockDateTime.Now.Year;
                _startDateTime = new DateTime( currentYear, 1, 1 );
                _endDateTime = new DateTime( currentYear, 12, 31 );
                _comparisonStartDate = new DateTime( ddlComparisonYear.SelectedValue.AsInteger(), 1, 1 );
                _comparisonEndDate = new DateTime( ddlComparisonYear.SelectedValue.AsInteger(), 12, 31 );
            }
        }

        #endregion

        #region Events

        protected void cbDateRangeEnabled_OnCheckedChanged( object sender, EventArgs e )
        {
            mdStart.Enabled = cbDateRangeEnabled.Checked;
            mdEnd.Enabled = cbDateRangeEnabled.Checked;

            // When filter turned off, bind again
            if ( !cbDateRangeEnabled.Checked )
            {
                BindGrid();
            }
        }


        protected void mdStart_OnSelectedMonthDayChanged( object sender, EventArgs e )
        {
            if ( !cbDateRangeEnabled.Checked || !mdStart.SelectedDate.HasValue ||
                !mdEnd.SelectedDate.HasValue )
            {
                return;
            }
            // Only need to change state if filter is enabled and both ends of the range are set
            UpdateDates();
            BindGrid();
        }

        protected void mdEnd_OnSelectedMonthDayChanged( object sender, EventArgs e )
        {
            if ( !cbDateRangeEnabled.Checked || !mdStart.SelectedDate.HasValue ||
                !mdEnd.SelectedDate.HasValue )
            {
                return;
            }
            // Only need to change state if filter is enabled and both ends of the range are set
            UpdateDates();
            BindGrid();
        }


        protected void gList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.DataItem == null )
            {
                return;
            }
            var dataItem = ( YearHeadcountComparisonRow ) e.Row.DataItem;
            if ( dataItem != null && dataItem.IsGroupTypeRow )
            {
                e.Row.Font.Bold = true;
            }
            else if ( dataItem != null && dataItem.IsTotalRow )
            {
                e.Row.Font.Underline = true;
                e.Row.Font.Italic = true;
            }
        }

        // Known to not be working
        protected void cblCheckInTemplates_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetupSettings();
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gStatistics_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        private void SetupSettings()
        {
            // Create checkboxes for each possible group template
            var groupTypes = CheckInGroupsHelper.GetCheckInTemplatesGroupTypes().ToList();
            foreach ( var groupType in groupTypes )
            {
                var listItem = new ListItem
                {
                    Value = groupType.Id.ToString(),
                    Text = groupType.Name
                };
                cblCheckInTemplates.Items.Add( listItem );
            }
            cblCheckInTemplates.SetValues( groupTypes.Select( g => g.Id ) );

            // Ensure first item in current year dropdown is current year
            ddlCurrentYear.Items.Add( new ListItem { Value = "", Text = RockDateTime.Now.Year.ToString() } );

            // Create dropdown of number of historical years in block settings
            int currentYear = RockDateTime.Now.Year;
            int maxYears = GetAttributeValue( "HistoricYears" ).AsInteger();
            for ( int i = 1; i <= maxYears; i++ )
            {
                var listItem = new ListItem();
                listItem.Value = ( currentYear - i ).ToString();
                listItem.Text = ( currentYear - i ).ToString();
                ddlComparisonYear.Items.Add( listItem );
                ddlCurrentYear.Items.Add(listItem);
            }

            ddlCurrentYear.SelectedIndex = 0;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var checkInGroupTypes = GetSelectedGroupTypes();

            var rows = new List<YearHeadcountComparisonRow>();
            foreach ( var groupType in checkInGroupTypes )
            {
                CreateRowsForGroupType( groupType, rows );
            }

            gStatistics.DataSource = rows.ToList();
            gStatistics.DataBind();
        }

        private IEnumerable<GroupType> GetSelectedGroupTypes()
        {
            var rockContext = new RockContext();
            var groupTypeService = new GroupTypeService(rockContext);

            // Get group types from checkboxes, default to all if none selected
            var checkInGroupTypes = cblCheckInTemplates.SelectedValuesAsInt.Any()
                ? cblCheckInTemplates.SelectedValuesAsInt.Select(i => groupTypeService.Get(i)).ToList()
                : CheckInGroupsHelper.GetCheckInTemplatesGroupTypes();
            return checkInGroupTypes;
        }

        private void CreateRowsForGroupType( GroupType groupType, List<YearHeadcountComparisonRow> rows )
        {
            var groups = new ChildCheckInGroupGenerator().Get( groupType );

            // Add row to mark group type 
            var groupTypeTitleRow = new YearHeadcountComparisonRow( groupType.Name );
            rows.Add( groupTypeTitleRow );

            // Add row for each group in groupType
            var groupTypeRows = new List<YearHeadcountComparisonRow>();
            groupTypeRows.AddRange( groups.Select( CreateStatisticsRow ) );

            // Calculate group type specific totals
            var totalRow = new YearHeadcountComparisonRow( groupTypeRows.Sum( r => r.TotalMonthly ) ?? 0, groupTypeRows.Sum( r => r.AverageMonthlyAttendance ) ?? 0.0m, groupTypeRows.Sum( r => r.YearToDateTotal ) ?? 0, groupTypeRows.Sum( r => r.YearToDateAverage ) ?? 0.0m,
                groupTypeRows.Sum( r => r.ComparisonYearTotalMonthly ) ?? 0, groupTypeRows.Sum( r => r.ComparisonYearAverageMonthlyAttendance ) ?? 0.0m, groupTypeRows.Sum( r => r.ComparisonYearToDateTotal ) ?? 0, groupTypeRows.Sum( r => r.ComparisonYearToDateAverage ) ?? 0.0m );

            // Add rows to the list of rows gStatistics will bind
            rows.AddRange( groupTypeRows );
            rows.Add( totalRow );
        }

        private YearHeadcountComparisonRow CreateStatisticsRow( Group group )
        {
            decimal monthlyTotal = _statisticCalculator.CalculateAttendanceForMonth( group,
                _startDateTime, _endDateTime );
            decimal averageMonthlyAttendance = _statisticCalculator.CalculateAverageAttendanceForMonth( _startDateTime, _endDateTime );
            decimal yearToDateTotal = _statisticCalculator.CalculateYearToDate(_startDateTime, _endDateTime, group );
            decimal yearToDateAverage = _statisticCalculator.CalculateYearToDateAverage( _startDateTime, _endDateTime );
            decimal comparisonMonthlyTotal = _statisticCalculator.CalculateAttendanceForMonth( group,
                _comparisonStartDate, _comparisonEndDate );
            decimal comparisonAverageMonthlyAttendance = _statisticCalculator.CalculateAttendanceForMonth( group, _comparisonStartDate,
                _comparisonEndDate );
            decimal comparisonYearToDateTotal = _statisticCalculator.CalculateYearToDate( _startDateTime, _endDateTime,  group);
            decimal comparisonYearToDateAverage = _statisticCalculator.CalculateYearToDateAverage( _startDateTime, _endDateTime );
            return new YearHeadcountComparisonRow( group.Name, monthlyTotal, averageMonthlyAttendance, yearToDateTotal, yearToDateAverage, comparisonMonthlyTotal, comparisonAverageMonthlyAttendance, comparisonYearToDateTotal, comparisonYearToDateAverage );
        }

        #endregion

        protected void bbRefresh_OnClick(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void ddlYear_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDates();
        }
    }

    public class YearHeadcountComparisonRow
    {
        public YearHeadcountComparisonRow( string groupTypeName )
        {
            Service = groupTypeName;
            IsGroupTypeRow = true;
        }

        public YearHeadcountComparisonRow( string service, decimal totalMonthly, decimal averageMonthlyAttendance, decimal yearToDateTotal, decimal yearToDateAverage, decimal comparisonYearTotalMonthly, decimal comparisonYearAverageMonthlyAttendance, decimal comparisonYearToDateTotal, decimal comparisonYearToDateAverage )
        {
            Service = service;
            TotalMonthly = decimal.ToInt32( totalMonthly );
            AverageMonthlyAttendance = averageMonthlyAttendance;
            YearToDateTotal = decimal.ToInt32( yearToDateTotal );
            YearToDateAverage = yearToDateAverage;
            ComparisonYearTotalMonthly = decimal.ToInt32( comparisonYearTotalMonthly );
            ComparisonYearAverageMonthlyAttendance = comparisonYearAverageMonthlyAttendance;
            ComparisonYearToDateTotal = decimal.ToInt32( comparisonYearToDateTotal );
            ComparisonYearToDateAverage = comparisonYearToDateAverage;
        }

        public YearHeadcountComparisonRow( int sumTotalMonthly, decimal sumAverageMonthlyAttendance, int sumYearToDateTotal, decimal sumYearToDateAverage, int sumComparisonYearTotalMonthly, decimal sumComparisonYearAverageMonthlyAttendance, int sumComparisonYearToDateTotal, decimal sumComparisonYearToDateAverage )
        {
            Service = "Total";
            TotalMonthly = decimal.ToInt32( sumTotalMonthly );
            AverageMonthlyAttendance = sumAverageMonthlyAttendance;
            YearToDateTotal = decimal.ToInt32( sumYearToDateTotal );
            YearToDateAverage = sumYearToDateAverage;
            ComparisonYearTotalMonthly = decimal.ToInt32( sumComparisonYearTotalMonthly );
            ComparisonYearAverageMonthlyAttendance = sumComparisonYearAverageMonthlyAttendance;
            ComparisonYearToDateTotal = decimal.ToInt32( sumComparisonYearToDateTotal );
            ComparisonYearToDateAverage = sumComparisonYearToDateAverage;
            IsTotalRow = true;
        }


        public string Service { get; set; }
        public int? TotalMonthly { get; set; }
        public decimal? AverageMonthlyAttendance { get; set; }
        public int? YearToDateTotal { get; set; }
        public decimal? YearToDateAverage { get; set; }
        public int? ComparisonYearTotalMonthly { get; set; }
        public decimal? ComparisonYearAverageMonthlyAttendance { get; set; }
        public int? ComparisonYearToDateTotal { get; set; }
        public decimal? ComparisonYearToDateAverage { get; set; }
        public bool IsGroupTypeRow { get; set; }
        public bool IsTotalRow { get; set; }
    }

//    internal interface IGroupHeadCountStatisticCalculator
//    {
//        decimal CalculateAttendanceForMonth( Group group, DateTime startDate, DateTime endDate );
//        decimal CalculateAverageAttendanceForMonth( DateTime startDate, DateTime? endDate );
//        decimal CalculateYearToDate( DateTime startDate, DateTime endDate, Group group );
//        decimal CalculateYearToDateAverage( DateTime startDate, DateTime endDate );
//    }
//
//    internal class FilteredGroupHeadCountStatisticCalculator : IGroupHeadCountStatisticCalculator
//    {
//
//        private decimal _headcountTotal;
//
//        public decimal CalculateAttendanceForMonth( Group group, DateTime startDate, DateTime endDate )
//        {
//            _headcountTotal = HeadCountHelper.GetHeadCountForGroup( group.Guid, startDate, new DateTime( startDate.Year, startDate.Month,
//                                    DateTime.DaysInMonth( startDate.Year, startDate.Month) ) ) ?? 0.0m;
//            return _headcountTotal;
//        }
//
//        public decimal CalculateAverageAttendanceForMonth( DateTime startDate, DateTime? endDate )
//        {
//            int numberOfSameDayOfWeekLeft = startDate.NumberOfThisDayOfWeekLeftInMonth();
//            return _headcountTotal / numberOfSameDayOfWeekLeft;
//        }
//
//        public decimal CalculateYearToDate(DateTime startDate, DateTime? endDate)
//        {
//            throw new NotImplementedException();
//        }
//
//        public decimal CalculateYearToDateAverage(DateTime startDate, DateTime? endDate)
//        {
//            throw new NotImplementedException();
//        }
//    }

    internal class HeadCountStatisticCalculator
    {
        private decimal _headcountTotal;
        private decimal _headcountTotalToDate;

        public decimal CalculateAttendanceForMonth( Group group, DateTime startDate, DateTime endDate )
        {
            var endOfMonth = new DateTime(startDate.Year, startDate.Month,
                DateTime.DaysInMonth(startDate.Year, startDate.Month));

            // If filtered date is before end of month use that instead of end of month
            endDate = endDate < endOfMonth ? endDate : endOfMonth;

            _headcountTotal = HeadCountHelper.GetHeadCountForGroup( group.Guid, startDate, endDate ) ?? 0.0m;
            return _headcountTotal;
        }

        public decimal CalculateAverageAttendanceForMonth( DateTime startDate, DateTime endDate)
        {

            int daysOfWeekTillEndOfMonth = startDate.NumberOfThisDayOfWeekLeftInMonth();
            int daysOfWeekTillEndDate = startDate.NumberOfThisDayOfWeekLeftBeforeDate(endDate);
            int divisor = daysOfWeekTillEndOfMonth < daysOfWeekTillEndDate
                ? daysOfWeekTillEndOfMonth
                : daysOfWeekTillEndDate;
            return _headcountTotal / divisor;
        }

        public decimal CalculateYearToDate( DateTime startDate, DateTime endDate, Group group )
        {
            _headcountTotalToDate = HeadCountHelper.GetHeadCountForGroup( group.Guid, startDate, endDate ) ?? 0.0m;
            return _headcountTotalToDate;
        }

        public decimal CalculateYearToDateAverage( DateTime startDate, DateTime endDate )
        {
            int totalMonths = endDate.Month - startDate.Month + 1;
            return _headcountTotalToDate / totalMonths;
        }
    }
}