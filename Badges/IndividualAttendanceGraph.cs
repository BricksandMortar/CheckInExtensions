using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Text;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.PersonProfile;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;

namespace com.bricksandmortarstudio.checkinextensions
{
    
    [Description( "Shows a chart of the attendance history of a person to a specific group type with each bar representing one month." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Individual Attendance Grouptype Graph" )]
    
    [IntegerField("Months To Display", "The number of months to show on the chart (default 24.)", false, 24)]
    [IntegerField("Minimum Bar Height", "The minimum height of a bar (in pixels). Useful for showing hint of bar when attendance was 0. (default 2.)", false, 2)]
    [BooleanField("Animate Bars", "Determine whether bars should animate when displayed.", true)]
    [GroupTypesField("Group Types", "", true, key:"groupTypes")]
    [BooleanField("Include child groups", "If selected any attendance from child grouptypes and groups of those groups types will be included on the graph", key:"recursive")]
    public class IndividualAttendanceGraph : BadgeComponent
    {
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, HtmlTextWriter writer )
        {
            int minBarHeight = GetAttributeValue(badge, "MinimumBarHeight").AsIntegerOrNull() ?? 2;
            int monthsToDisplay = GetAttributeValue(badge, "MonthsToDisplay").AsIntegerOrNull() ?? 24;
            var groupTypeGuids = GetAttributeValue(badge, "groupTypes").Split(',').AsGuidList();
            var ids = new List<int>();
            var sb = new StringBuilder();
            bool recursive = GetAttributeValue(badge, "recursive").AsBoolean();
            if (!recursive)
            {
                foreach (Guid groupTypeGuid in groupTypeGuids)
                {
                    var groupType = GroupTypeCache.Read(groupTypeGuid);
                    ids.Add(groupType.Id);
                    sb.Append(groupType.Name);
                }
            }
            else
            {
                var groupTypes = new GroupTypeService(new RockContext()).GetByGuids(groupTypeGuids);
                foreach (var groupType in groupTypes)
                {
                    sb.Append(groupType.Name);
                }
                ids = new ChildCheckInGroupGenerator().Get(groupTypes);
            }


            string groupTypeNames = sb.ToString();
            string animateClass = string.Empty;

            if (GetAttributeValue(badge, "AnimateBars") == null || GetAttributeValue(badge, "AnimateBars").AsBoolean())
            {
                animateClass = " animate";
            }

            writer.Write(String.Format( "<div class='badge badge-attendance{0} badge-id-{1}' data-toggle='tooltip' data-original-title='Individual attendance for the last 24 months to {2}. Each bar is a month.'>", animateClass, badge.Id, groupTypeNames.Truncate(100)));

            writer.Write("</div>");

            writer.Write(String.Format(@"
                <script>
                    Sys.Application.add_load(function () {{
                        
                        var monthNames = [ 'January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December' ];
                        
                        
                        $.ajax({{
                                type: 'GET',
                                url: Rock.settings.get('baseUrl') + 'api/PersonBadges/IndividualAttendanceGraph/{0}/{1}/{2}/{5}' ,
                                statusCode: {{
                                    200: function (data, status, xhr) {{
                                            var chartHtml = '<ul class=\'badge-attendance-chart list-unstyled\'>';
                                            $.each(data, function() {{
                                                var barHeight = (this.AttendanceCount / this.SundaysInMonth) * 100;
                                                if (barHeight < {3}) {{
                                                    barHeight = {3};
                                                }}
                                
                                                chartHtml += '<li title=\'' + monthNames[this.Month -1] + ' ' + this.Year +'\'><span style=\'height: ' + barHeight + '%\'></span></li>';                
                                            }});
                                            chartHtml += '</ul>';
                                            
                                            $('.badge-attendance.badge-id-{4}').html(chartHtml);

                                        }}
                                }},
                        }});
                    }});
                </script>
                
            ", Person.Id, monthsToDisplay , string.Join(",", ids), minBarHeight, badge.Id, recursive ));

        }
    }
}
