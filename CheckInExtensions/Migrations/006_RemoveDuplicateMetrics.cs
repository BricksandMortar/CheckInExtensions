using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bricksandmortarstudio.checkinextensions.Migrations
{
    [MigrationNumber( 6, "1.4.0" )]
    class RemoveDuplicateMetrics : Migration
    {
        public override void Up()
        {
            Sql( @"
            DECLARE @metricCategoryEntityId int;
            SET @metricCategoryEntityId = (SELECT TOP 1 [Id] 
						            FROM [EntityType]
						            WHERE [EntityType].[Name] = 'Rock.Model.MetricCategory');

            DELETE
            FROM [Category]
            WHERE [Category].Id NOT IN (SELECT [MetricCategory].CategoryId 
						            FROM [MetricCategory] 
						            WHERE (
							            SELECT Count(*) 
							            FROM [MetricValue] 
							            WHERE [MetricValue].MetricId = [MetricCategory].[MetricId]) > 0)
            AND [Category].EntityTypeId = @metricCategoryEntityId AND Category.[Guid] != 'A0832CDB-CB65-4DFD-9D1B-4F702D8BB64F';

            DELETE
            FROM [Metric]
            WHERE ((SELECT COUNT(*) FROM [MetricValue] WHERE [MetricValue].MetricId = [Metric].Id) = 0 AND [Metric].[Description] LIKE '%Headcount%')

            DECLARE @sourceTypeValueId nvarchar(max);
            SET @sourceTypeValueId = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '1D6511D6-B15D-4DED-B3C4-459CD2A7EC0E');

            UPDATE [Metric]
            SET SourceValueTypeId = @sourceTypeValueId
            WHERE [SourceValueTypeId] IS NULL
            " );
        }

        public override void Down()
        {
           
        }
    }
}
