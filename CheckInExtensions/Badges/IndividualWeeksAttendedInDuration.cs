using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Web.UI;
using com.bricksandmortarstudio.checkinextensions.Utils;
using Rock;
using Rock.Attribute;
using Rock.PersonProfile;
using Rock.Web.Cache;

namespace com.bricksandmortarstudio.checkinextensions.Badges
{

    [Description("Shows the number of times a individual attended a given set of grouptypes in a duration of weeks.")]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Individual Weeks Attended In Duration for Group Types")]

    [IntegerField("Duration", "The number of weeks to use for the duration (default 16.)", false, 16)]
    [GroupTypesField("Group Types", "", true, key:"groupTypes")]
    [BooleanField("Include Child Groups?", "If selected any attendance from child grouptypes and groups of those groups types will be included in the result", key:"recursive")]
    [TextField("Badge Colour", "The colour of the badge (#ffffff).", true, "#ecc759", key:"badgeColour")]
    public class IndividualWeeksAttendedInDuration : BadgeComponent
    {
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( PersonBadgeCache badge, HtmlTextWriter writer )
        {
            string badgeColour = GetAttributeValue(badge, "badgeColour");
            if (String.IsNullOrEmpty(badgeColour))
            {
                badgeColour = "#ECC759";
            }

            int duration = GetAttributeValue(badge, "Duration").AsIntegerOrNull() ?? 16;
            var groupTypeGuids = GetAttributeValue(badge, "groupTypes").Split(',').AsGuidList();
            bool recursive = GetAttributeValue(badge, "recursive").AsBoolean();

            var groupTypes = groupTypeGuids.Select(groupTypeGuid => GroupTypeCache.Get(groupTypeGuid)).Distinct().ToList();

            var ids = groupTypes.Select( gt => gt.Id ).ToList();
            string groupTypeNames = CheckInGroupsHelper.CreateGroupListString( groupTypes );

            writer.Write(string.Format("<style>#badge-id-{0}::before {{ color: {1};}}</style>", badge.Id, badgeColour));
            writer.Write(string.Format("<div class='badge badge-weeksattendanceduration badge-id-{0}' data-toggle='tooltip' data-original-title='Individual attendance for the last {1} weeks to {2}.'>", badge.Id, duration, groupTypeNames.Truncate(150)));

                writer.Write("</div>");

                writer.Write(string.Format(@"
                <script>
                    Sys.Application.add_load(function () {{
                                                
                        $.ajax({{
                                type: 'GET',
                                url: Rock.settings.get('baseUrl') + 'api/PersonBadges/IndividualWeeksAttendedInDuration/{1}/{0}/{3}/{4}' ,
                                statusCode: {{
                                    200: function (data, status, xhr) {{
                                            var badgeHtml = '<div class=\'weeks-metric\' id=\'badge-id-{2}\'>';
                                            
                                            badgeHtml += '<span class=\'weeks-attended\' >' + data + '</span><span class=\'week-duration\'>/{0}</span>';                
                                            badgeHtml += '</div>';
                                            
                                            $('.badge-weeksattendanceduration.badge-id-{2}').html(badgeHtml);

                                        }}
                                }},
                        }});
                    }});
                </script>
                
            ", duration, Person.Id.ToString(), badge.Id, string.Join(",", ids), recursive));
        }
    }
}
