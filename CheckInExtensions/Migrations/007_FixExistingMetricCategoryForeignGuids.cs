using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bricksandmortarstudio.checkinextensions.Migrations
{
    [MigrationNumber( 7, "1.4.0" )]
    class FixExistingMetricCategoryForeignGuids : Migration
    {
        public override void Up()
        {
            Sql( @"
            DECLARE @metricCategoryEntityId int;
            SET @metricCategoryEntityId = (SELECT TOP 1 [Id] 
						            FROM [EntityType]
						            WHERE [EntityType].[Name] = 'Rock.Model.MetricCategory');

            UPDATE [Category]
            SET [Category].[ForeignGuid] = (SELECT TOP 1 [Guid]

            FROM (SELECT *
		            FROM [GroupType]
		            WHERE [Id] = (SELECT TOP 1 [GroupTypeId]
						            FROM [Group]
						            WHERE [Guid] = (SELECT TOP 1 [ForeignGuid]
										            FROM [Metric]
										            WHERE [Id] IN (SELECT [MetricId]
														            FROM [MetricCategory]
														            WHERE [CategoryId] = [Category].[Id]
														            )
										            )
						            )
		            ) AS f)
            WHERE [EntityTypeId] = @metricCategoryEntityId
            " );
        }

        public override void Down()
        {
           
        }
    }
}
