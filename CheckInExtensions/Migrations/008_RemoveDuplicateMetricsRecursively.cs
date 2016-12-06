using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bricksandmortarstudio.checkinextensions.Migrations
{
    [MigrationNumber( 8, "1.4.0" )]
    class RemoveDuplicateMetricsRecursively : Migration
    {
        public override void Up()
        {
            Sql( @"
            DECLARE @metricCategoryEntityId int;
SET @metricCategoryEntityId = (SELECT TOP 1 [Id] 
						FROM [EntityType]
						WHERE [EntityType].[Name] = 'Rock.Model.MetricCategory');


WITH q AS
        (
        SELECT  id
        FROM    [Category]
        WHERE [Category].Id NOT IN (SELECT [MetricCategory].CategoryId 
						            FROM [MetricCategory] 
						            WHERE (
							            (SELECT Count(*) 
							            FROM [MetricValue] 
							            WHERE [MetricValue].MetricId = [MetricCategory].[MetricId]) > 0 OR MetricCategory.ForeignGuid IS NULL))
            AND [Category].EntityTypeId = @metricCategoryEntityId AND Category.[Guid] != 'A0832CDB-CB65-4DFD-9D1B-4F702D8BB64F'

        UNION ALL
        SELECT  c.id
        FROM    q
        JOIN    [Category] as c
        ON      c.ParentCategoryId = q.id
        )
DELETE
FROM    [Category]
WHERE   EXISTS
        (
        SELECT  id
        INTERSECT
        SELECT  id
        FROM    q
        )

		    DELETE FROM [Metric]
            WHERE ((SELECT COUNT(*) FROM [MetricValue] WHERE [MetricValue].MetricId = [Metric].Id) = 0 AND [Metric].[Description] LIKE '%Headcount for%' AND [Metric].YAxisLabel LIKE '%Headcount%' AND ([Metric].IconCssClass IS NULL OR [Metric].IconCssClass = '') AND ([Metric].Subtitle IS NULL OR [Metric].Subtitle = '') AND [Metric].ForeignGuid IS NOT NULL)            " );
        }

        public override void Down()
        {

        }
    }
}
