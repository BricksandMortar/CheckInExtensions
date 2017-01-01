﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Web.UI.WebControls;
using com.bricksandmortarstudio.checkinextensions.Utils;
using CSScriptLibrary;
using Mono.CSharp;
using OpenXmlPowerTools;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Plugins.com_BricksandMortarStudio.CheckInExtensions
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( @"Quarterly Headcount Comparison" )]
    [Category( "Bricks and Mortar Studio > Check-In Extensions" )]
    [Description( "Provides a statistical overview of two years of headcounts split into quarters" )]
    [TextField( "Title", "The title the block should display", true, "Quarterly Headcount Comparison", order: 0 )]
    [IntegerField( "Historic Years", "Years to display for comparison", true, 2, order: 1 )]
    public partial class QuarterlyHeadcountComparison : RockBlock
    {
        #region Fields

        private IQuarterlyHeadCountStatisticCalculator _statisticCalculator;

        private YearQuarterlyHeadCountStatisticCalculator _yearQuarterlyHeadCountStatisticCalculator =
            new YearQuarterlyHeadCountStatisticCalculator();

        private FilteredQuarterlyHeadCountStatisticCalculator _filteredQuarterlyHeadCountStatisticCalculator =
            new FilteredQuarterlyHeadCountStatisticCalculator();

        private DateTime _startDateTime;
        private DateTime _endDateTime;

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
            gInformation.GridRebind += gInformation_GridRebind;
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
                ChangeCalculatorState();
                BindGrid();
            }
            else
            {
                ChangeCalculatorState();
            }
        }

        private void ChangeCalculatorState()
        {
            if ( cbDateRangeEnabled.Checked && mdStart.SelectedDate.HasValue &&
                mdEnd.SelectedDate.HasValue )
            {
                _statisticCalculator = _filteredQuarterlyHeadCountStatisticCalculator;
                int currentYear = RockDateTime.Now.Year;
                _startDateTime = new DateTime( currentYear, mdStart.SelectedDate.Value.Month,
                    mdStart.SelectedDate.Value.Day );
                _endDateTime = new DateTime( currentYear, mdEnd.SelectedDate.Value.Month,
                    mdEnd.SelectedDate.Value.Day );
            }
            else
            {
                _statisticCalculator = _yearQuarterlyHeadCountStatisticCalculator;
                _startDateTime = new DateTime( RockDateTime.Today.Year, 1, 1 );
                _endDateTime = RockDateTime.Now;
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
            ChangeCalculatorState();
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
            ChangeCalculatorState();
            BindGrid();
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
        private void gInformation_GridRebind( object sender, EventArgs e )
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

            // Create dropdown of number of historical years in block settings
            int currentYear = RockDateTime.Now.Year;
            int maxYears = GetAttributeValue( "HistoricYears" ).AsInteger();
            for ( int i = 1; i <= maxYears; i++ )
            {
                var listItem = new ListItem();
                listItem.Value = ( currentYear - i ).ToString();
                listItem.Text = ( currentYear - i ).ToString();
                ddlComparisonYear.Items.Add( listItem );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var groupTypes = GetSelectedGroupTypes();
            var rows = new List<QuarterlyHeadcountComparisonRow>();
            foreach ( var groupType in groupTypes )
            {
                var groups = new ChildCheckInGroupGenerator().Get( groupType );

                // Add row for each group in groupType
                rows.AddRange( groups.Select( g => _statisticCalculator.Calculate( g, _startDateTime, _endDateTime, ddlComparisonYear.SelectedValue.AsInteger() ) ) );
            }

            gInformation.DataSource = rows.ToList();
            gInformation.DataBind();
        }

        private IList<GroupType> GetSelectedGroupTypes()
        {
            var rockContext = new RockContext();
            var groupTypeService = new GroupTypeService( rockContext );

            // Get group types from checkboxes, default to all if none selected
            var checkInGroupTypes = cblCheckInTemplates.SelectedValuesAsInt.Any()
                ? cblCheckInTemplates.SelectedValuesAsInt.Select( i => groupTypeService.Get( i ) ).ToList()
                : CheckInGroupsHelper.GetCheckInTemplatesGroupTypes();
            var inGroupTypes = checkInGroupTypes as IList<GroupType> ?? checkInGroupTypes.ToList();
            return inGroupTypes;
        }

        #endregion

        protected void bbRefresh_OnClick( object sender, EventArgs e )
        {
            BindGrid();
        }
    }

    public class QuarterInformation
    {
        public QuarterInformation( decimal yearTotal, decimal q1Total, decimal q2Total, decimal q3Total, decimal q4Total, decimal average )
        {
            YearTotal = decimal.ToInt32( yearTotal );
            Q1Total = decimal.ToInt32( q1Total );
            Q2Total = decimal.ToInt32( q2Total );
            Q3Total = decimal.ToInt32( q3Total );
            Q4Total = decimal.ToInt32( q4Total );
            Average = average;
        }

        public int YearTotal { get; set; }
        public int Q1Total { get; set; }
        public int Q2Total { get; set; }
        public int Q3Total { get; set; }
        public int Q4Total { get; set; }
        public decimal Average { get; set; }
    }

    public class QuarterlyHeadcountComparisonRow
    {

        public QuarterlyHeadcountComparisonRow( string service, QuarterInformation quarter, QuarterInformation comparisonQuarter, int comparisonYear )
        {
            Service = service;
            Quarter = quarter;
            ComparisonQuarter = comparisonQuarter;
            ComparisonYear = ComparisonYear;
        }


        public QuarterInformation Quarter { get; set; }
        public QuarterInformation ComparisonQuarter { get; set; }

        public string Service { get; set; }

        public int ComparisonYear { get; set; }

        public static QuarterInformation GetQuarteredStatistics( Group @group, DateTime startDateTime, DateTime? endDateTime, Dictionary<int, Quarter> quarters )
        {
            decimal q1 = quarters[1].StartDateTime == null
                ? 0.0m
                : HeadCountHelper.GetHeadCountForGroup( @group.Guid, quarters[1].StartDateTime.Value,
                      quarters[1].EndDateTime.Value ) ?? 0.0m;
            decimal q2 = quarters[2].StartDateTime == null
                ? 0.0m
                : HeadCountHelper.GetHeadCountForGroup( @group.Guid, quarters[2].StartDateTime.Value,
                      quarters[2].EndDateTime.Value ) ?? 0.0m;
            decimal q3 = quarters[3].StartDateTime == null
                ? 0.0m
                : HeadCountHelper.GetHeadCountForGroup( @group.Guid, quarters[3].StartDateTime.Value,
                      quarters[3].EndDateTime.Value ) ?? 0.0m;
            decimal q4 = quarters[4].StartDateTime == null
                ? 0.0m
                : HeadCountHelper.GetHeadCountForGroup( @group.Guid, quarters[4].StartDateTime.Value,
                      quarters[4].EndDateTime.Value ) ?? 0.0m;
            decimal total = q1 + q2 + q3 + q4;
            decimal average = total / ( endDateTime.Value.Month - startDateTime.Month + 1 );
            var quarterResults = new QuarterInformation( total, q1, q2, q3, q4, average );
            return quarterResults;
        }
    }

    internal interface IQuarterlyHeadCountStatisticCalculator
    {
        QuarterlyHeadcountComparisonRow Calculate( Group group, DateTime startDateTime, DateTime endDateTime, int comparisonYear );
    }

    internal class FilteredQuarterlyHeadCountStatisticCalculator : IQuarterlyHeadCountStatisticCalculator
    {
        public QuarterlyHeadcountComparisonRow Calculate( Group group, DateTime startDateTime, DateTime endDateTime, int comparisonYear )
        {
            var quarters = QuartersHelper.GetQuarterStartEndForYear( startDateTime.Year ).FilterQuartersToRange( startDateTime, endDateTime );
            var quarterResults = QuarterlyHeadcountComparisonRow.GetQuarteredStatistics( @group, startDateTime, endDateTime, quarters );

            var comparisonStartDateTime = new DateTime( comparisonYear, startDateTime.Month, startDateTime.Day );
            var comparisonEndDateTime = new DateTime( comparisonYear, endDateTime.Month, endDateTime.Day );
            var comparisonQuarters = QuartersHelper.GetQuarterStartEndForYear( comparisonYear ).FilterQuartersToRange( comparisonStartDateTime, comparisonEndDateTime );
            var comparisonQuartersResults = QuarterlyHeadcountComparisonRow.GetQuarteredStatistics( group, comparisonStartDateTime, comparisonEndDateTime, comparisonQuarters );

            return new QuarterlyHeadcountComparisonRow( group.Name, quarterResults, comparisonQuartersResults, comparisonYear );
        }


    }

    internal class YearQuarterlyHeadCountStatisticCalculator : IQuarterlyHeadCountStatisticCalculator
    {
        public QuarterlyHeadcountComparisonRow Calculate( Group group, DateTime startDateTime, DateTime endDateTime, int comparisonYear )
        {
            var quarters = QuartersHelper.GetQuarterStartEndForYear( startDateTime.Year ).FilterQuartersToRange( startDateTime, endDateTime );
            var quarterResults = QuarterlyHeadcountComparisonRow.GetQuarteredStatistics( @group, startDateTime, endDateTime, quarters );

            var comparisonStartDateTime = new DateTime( comparisonYear, startDateTime.Month, startDateTime.Day );
            var comparisonEndDateTime = new DateTime( comparisonYear, endDateTime.Month, endDateTime.Day );
            var comparisonQuarters = QuartersHelper.GetQuarterStartEndForYear( comparisonYear ).FilterQuartersToRange( comparisonStartDateTime, comparisonEndDateTime );
            var comparisonQuartersResults = QuarterlyHeadcountComparisonRow.GetQuarteredStatistics( group, comparisonStartDateTime, comparisonEndDateTime, comparisonQuarters );

            return new QuarterlyHeadcountComparisonRow( group.Name, quarterResults, comparisonQuartersResults, comparisonYear );
        }
    }
}