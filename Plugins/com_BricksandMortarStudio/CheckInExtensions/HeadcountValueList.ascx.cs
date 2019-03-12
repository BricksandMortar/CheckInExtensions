using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;
using com.bricksandmortarstudio.checkinextensions.Utils;
using Newtonsoft.Json;
using RestSharp.Extensions;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Plugins.com_bricksandmortarstudio.CheckInExtensions
{
    [DisplayName( "Headcount Metric Value List" )]
    [Category( "Bricks and Mortar Studio > Check-In Extensions" )]
    [Description( "Displays a list of metric values." )]
    [BooleanField("Strict Campus Filter", "Should the Campus Filter filter out groups without a campus?")]
    public partial class HeadcountMetricValueList : Rock.Web.UI.RockBlock
    {
        #region fields
        private RockContext _rockContext;
        private int? _campusId;
        private int? _groupId;
        private PersonAliasService _personAliasService;
        private GroupService _groupService;

        #endregion

        #region Control Methods

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
        }

        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            if ( _campusId != null )
            {
                ViewState["CampusId"] = _campusId;
            }
            if ( _groupId != null )
            {
                ViewState["GroupId"] = _groupId;
            }


            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _rockContext = new RockContext();
            if ( _personAliasService == null )
            {
                _personAliasService = new PersonAliasService( _rockContext );
            }

            gfMetricValues.ApplyFilterClick += gfMetricValues_ApplyFilterClick;
            gfMetricValues.DisplayFilterValue += gfMetricValues_DisplayFilterValue;

            gMetricValues.GridRebind += gMetricValues_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gMetricValues.Actions.ShowAdd = canAddEditDelete;
            gMetricValues.IsDeleteEnabled = canAddEditDelete;

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {

            LoadPreferences();
            if ( !Page.IsPostBack )
            {
                LoadPickers();
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Filter

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            drpDates.DelimitedValues = gfMetricValues.GetUserPreference( "Date Range" );
        }

        /// <summary>
        /// Gfs the metric values_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfMetricValues_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == "Date Range" )
            {
                e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
            }
            else
            {
                e.Value = null;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfMetricValues_ApplyFilterClick( object sender, EventArgs e )
        {
            gfMetricValues.SaveUserPreference( "Date Range", drpDates.DelimitedValues );
            BindGrid();
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Delete event of the gMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMetricValues_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var metricValueService = new MetricValueService( rockContext );
            var metricValuePartitionService = new MetricValuePartitionService( rockContext );
            int? metricValueId = e.RowKeyValues["MetricValueId"] as int?;

            if ( metricValueId.HasValue )
            {
                var metricValue = metricValueService.Get( metricValueId.Value );
                if ( metricValue != null )
                {
                    string errorMessage;
                    if ( !metricValueService.CanDelete( metricValue, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }
                    if ( !metricValueService.CanDelete( metricValue, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    rockContext.WrapTransaction( () =>
                    {
                        metricValuePartitionService.DeleteRange( metricValue.MetricValuePartitions );
                        metricValueService.Delete( metricValue );
                        rockContext.SaveChanges();

                    } );
                }

                BindGrid();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gMetricValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMetricValues_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }



        #endregion

        #region Internal Methods

        private void LoadPreferences()
        {
            var settingPrefix = string.Format( "com_bricksandmortarstudio-checkinextension-{0}-", this.BlockId );
            var userPreferences = GetUserPreferences( settingPrefix ).ToDictionary( userPreference => userPreference.Key, userPreference => userPreference.Value );
            _campusId = userPreferences.GetValueOrNull( "campus" ).AsIntegerOrNull();
            _groupId = userPreferences.GetValueOrNull( "group" ).AsIntegerOrNull();
        }

        private void GetMetricId()
        {
            int? groupId = ddlGroups.SelectedValue.AsIntegerOrNull();
            if ( groupId.HasValue )
            {
                if ( _rockContext == null )
                {
                    _rockContext = new RockContext();
                }
                var groupGuid = new GroupService( _rockContext ).Get( groupId.Value ).Guid;
                var metric = new MetricService( _rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .FirstOrDefault( m => m.ForeignGuid == groupGuid );

                if ( metric != null )
                {
                    hfMetricId.Value = metric.Id.ToString();
                }
            }
        }

        private IQueryable<MetricValue> GetMetricData( int metricId )
        {

            var rockContext = new RockContext();

            var metricValueService = new MetricValueService( rockContext );
            var qry = metricValueService.Queryable( "Metric" ).AsNoTracking();

            qry = qry.Where( a => a.MetricId == metricId );

            var drp = new DateRangePicker();
            drp.DelimitedValues = gfMetricValues.GetUserPreference( "Date Range" );
            if ( drp.LowerValue.HasValue )
            {
                qry = qry.Where( a => a.MetricValueDateTime >= drp.LowerValue.Value );
            }

            if ( drp.UpperValue.HasValue )
            {
                var upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                qry = qry.Where( a => a.MetricValueDateTime < upperDate );
            }

            var campusId = ddlCampuses.SelectedValueAsId();
            if ( ddlCampuses.SelectedValue.HasValue() )
            {
                qry = qry.Where( mv => mv.MetricValuePartitions.Any(mvp => mvp.EntityId == campusId || mvp.EntityId == null));
            }

            return qry;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            GetMetricId();
            int? metricId = hfMetricId.Value.AsIntegerOrNull();

            if ( !metricId.HasValue )
            {

                var blankSummaries = new List<Summary>();
                gMetricValues.DataSource = blankSummaries;

                gMetricValues.DataBind();
                return;
            }

            int? scheduleId = null;
            var ddlInstanceSplit = ddlInstanceTwo.SelectedValue.Split( ',' );
            var scheduleInstanceStart = ddlInstanceSplit[0].AsDateTime();
            if ( ddlInstanceSplit.Length > 1 )
            {
                scheduleId = ddlInstanceSplit[1].AsIntegerOrNull();
            }

            if ( !scheduleInstanceStart.HasValue )
            {
                return;
            }

            var groupId = ddlGroups.SelectedValue.AsIntegerOrNull();

            if ( !groupId.HasValue )
            {
                return;
            }

            var qry = GetMetricData( metricId.Value );
            var attendanceService = new AttendanceService( _rockContext );
            
            var summaries = new List<Summary>();
            var groupService = new GroupService( _rockContext );
            foreach ( var metricValue in qry.Where(m => m.MetricValueDateTime.HasValue) )
            {
                var summary = new Summary();

                var group = groupService.Get( groupId.Value );
                Schedule schedule = null;
                if ( scheduleId.HasValue )
                {
                    schedule = new ScheduleService( _rockContext ).Get( scheduleId.Value );
                }

                int checkInCount;
                if ( schedule != null )
                {
                    var checkInStart = metricValue.MetricValueDateTime.Value.AddMinutes( schedule.CheckInStartOffsetMinutes ?? 0 );
                    var checkInEnd = metricValue.MetricValueDateTime.Value.AddMinutes( schedule.CheckInEndOffsetMinutes ?? 0 );
                    checkInCount = attendanceService
                        .Queryable()
                        .Count( a => a.Occurrence.GroupId == groupId.Value && ( a.DidAttend == null || a.DidAttend.Value ) && a.StartDateTime >= checkInStart && a.StartDateTime < checkInEnd );
                    
                }
                else
                {
                    checkInCount = attendanceService
                    .Queryable(
                    )
                    .Count( a => a.Occurrence.GroupId == groupId.Value && ( a.DidAttend == null || a.DidAttend.Value ) &&
                                 a.StartDateTime == metricValue.MetricValueDateTime.Value );
                }

                summary.Headcount = metricValue.YValue.HasValue ? decimal.ToInt32( metricValue.YValue.Value ) : 0;

                summary.Group = group;
                summary.MetricId = metricValue.MetricId;
                summary.MetricValueId = metricValue.Id;
                summary.StartDateTime = metricValue.MetricValueDateTime.Value;
                summary.CheckInCount = checkInCount;
                summaries.Add( summary );
            }


            var sortProperty = gMetricValues.SortProperty;
            if ( sortProperty != null )
            {
                summaries = summaries.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                summaries = summaries.OrderByDescending( s => s.StartDateTime ).ToList();
            }
            gMetricValues.DataSource = summaries;

            gMetricValues.DataBind();
        }

        #endregion

        #region Methods

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
                campusContainer.AddCssClass( "hidden" );
            }
            else
            {
                if ( _campusId.HasValue && campuses.Any( c => c.Id == _campusId.Value ) )
                {
                    ddlCampuses.SelectedValue = _campusId.ToString();
                }
            }

            PopulateGroups();
            PopulateInstances();
        }

        private void PopulateGroups()
        {
            int checkInTemplateId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ).Id;
            var checkInTemplates = new GroupTypeService( new RockContext() ).Queryable().AsNoTracking().Where( g => g.GroupTypePurposeValueId == checkInTemplateId );
            var groups = new ChildCheckInGroupGenerator().Get( checkInTemplates ).AsEnumerable();
            if (GetAttributeValue("StrictCampusFilter").AsBoolean())
            {
                groups = groups.Where(g => g.CampusId == _campusId);
            }
            else
            {
                groups = groups.Where( g => g.CampusId == null || g.CampusId == _campusId );
            }
            ddlGroups.Items.Clear();
            var enumeratedGroups = groups as IList<Group> ?? groups.ToList();
            foreach ( var group in enumeratedGroups )
            {
                var listItem = new ListItem();
                listItem.Text = group.Name;
                listItem.Value = group.Id.ToString();
                ddlGroups.Items.Add( listItem );
            }
            if ( ddlGroups.Items.Count < 1 )
            {
                nbWarning.Heading = "No checkin groups found.";
            }
            else
            {
                if ( _groupId.HasValue && enumeratedGroups.Any( g => g.Id == _groupId.Value ) )
                {
                    ddlGroups.SelectedValue = _groupId.ToString();
                }
            }
        }

        private void PopulateInstances()
        {
            ddlInstanceTwo.Items.Clear();
            var locationIds = new List<int>();
            _groupId = ddlGroups.SelectedValueAsId();
            if ( _groupId.HasValue )
            {
                var group = new GroupService( _rockContext ).Get( _groupId.Value );
                var groupLocations = group.GroupLocations;
                if ( groupLocations != null )
                {
                    var occurances = new List<KeyValuePair<string, DateTime>>();
                    foreach ( var location in groupLocations.Where( gl => gl.Group.CampusId == null || gl.Group.CampusId == _campusId ) )
                    {
                        locationIds.Add( location.LocationId );
                        foreach ( var schedule in location.Schedules.Where( s => s.HasSchedule() ) )
                        {
                            foreach (
                                var startDateTime in
                                    schedule.GetScheduledStartTimes( RockDateTime.Now.AddDays( -7 * 52 ), RockDateTime.Now ) )
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
                            ddlInstanceTwo.Items.Add( item );
                        }

                    }
                    if ( ddlInstanceTwo.Items.Count > 1 )
                    {
                        ddlInstanceTwo.Enabled = true;
                    }
                }
            }
        }

        #endregion

        protected void ddlCampuses_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            SetUserPreference( string.Format( "com_bricksandmortarstudio-checkinextension-{0}-campus", this.BlockId ), ddlGroups.SelectedValue );
            PopulateGroups();
            PopulateInstances();
            BindGrid();
        }

        protected void ddlGroups_OnSelectedIndexChanged( object sender, EventArgs e )
        {
            SetUserPreference( string.Format( "com_bricksandmortarstudio-checkinextension-{0}-group", this.BlockId ), ddlGroups.SelectedValue );
            PopulateInstances();
            BindGrid();
        }

        protected void gAttendeesAttendance_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var dataItem = e.Row.DataItem;
            if ( dataItem != null )
            {
                var lGroup = e.Row.FindControl( "lGroup" ) as Literal;
                var groupGuid = dataItem.GetPropertyValue( "ForeignGuid" ) as Guid?;
                if ( groupGuid != null )
                {
                    if ( _groupService == null )
                    {
                        _groupService = new GroupService( _rockContext );
                    }
                    var group = _groupService.Get( groupGuid.Value );
                    lGroup.Text = group.Name;
                }
            }
        }

        protected void bbAdd_OnClick( object sender, EventArgs e )
        {
            int? metricId = hfMetricId.ValueAsInt();
            var dateTimeArray = ddlInstanceTwo.SelectedValue.Split( ',' );
            var dateTime = dateTimeArray[0].AsDateTime();
            var value = nbValue.Text.AsIntegerOrNull();

            if ( metricId == 0 || !dateTime.HasValue || !value.HasValue )
            {
                nbWarning.Text = "Unable to save. Please check you have selected an instance and input a value. Also ask your administrator to double check your Headcount Metric Sync Groups job is running.";
                nbWarning.Visible = true;
                return;
            }

            var rockContext = new RockContext();
            var metricValueService = new MetricValueService( rockContext );
            var existingMetricValue = metricValueService.Queryable().FirstOrDefault( v => v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value == dateTime.Value && v.MetricId == metricId );
            if ( existingMetricValue != null && !string.IsNullOrWhiteSpace( existingMetricValue.MetricValuePartitionEntityIds) && existingMetricValue.MetricValuePartitionEntityIds.Split( ',' ).Any( partition => partition.Split( '|' )[0].AsInteger() == EntityTypeCache.Get( typeof( Campus ) ).Id && partition.Split( '|' )[1].AsInteger() == ddlCampuses.SelectedValueAsId() ) )
            {  
                    nbWarning.Text =
                        String.Format(
                            "A metric value already existed for the {0}, the old value of {1} has been changed to {2}",
                            dateTime.Value, Decimal.ToInt32( existingMetricValue.YValue.Value ), value );
                    nbWarning.Visible = true;
                    existingMetricValue.YValue = value;
            }
            else
            {
                var metric = new MetricService(rockContext).Get(metricId.Value);
                var metricValue = new MetricValue();
                metricValue.MetricValueDateTime = dateTime;
                metricValue.YValue = value;
                metricValue.MetricValuePartitions = new List<MetricValuePartition>();
                var metricPartitionsByPosition = metric.MetricPartitions.OrderBy( a => a.Order ).ToList();
                foreach (var metricPartition in metricPartitionsByPosition)
                {
                    var metricValuePartition = new MetricValuePartition();
                    metricValuePartition.MetricPartition = metricPartition;
                    metricValuePartition.MetricPartitionId = metricPartition.Id;
                    metricValuePartition.MetricValue = metricValue;
                    metricValuePartition.EntityId = ddlCampuses.SelectedValueAsId();
                    metricValue.MetricValuePartitions.Add( metricValuePartition );
                }
                metricValue.MetricId = metricId.Value;
                metricValue.Note = "Input as a headcount metric value";

                metricValueService.Add( metricValue );
                nbWarning.Text = "";
                nbWarning.Visible = false;
            }

            nbValue.Text = "";
            rockContext.SaveChanges();
            BindGrid();
        }
    }

    internal class Summary
    {
        public int Headcount { get; set; }
        public int CheckInCount { get; set; }
        public int MetricId { get; set; }
        public int MetricValueId { get; set; }
        public Group Group { get; set; }
        public DateTime StartDateTime { get; set; }
    }
}