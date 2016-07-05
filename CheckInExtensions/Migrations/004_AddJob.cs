using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bricksandmortarstudio.checkinextensions.Migrations
{
    [MigrationNumber( 4, "1.4.0" )]
    class AddJob : Migration
    {
        public override void Up()
        {
            Sql( string.Format( @"
DECLARE @EntityTypeId int
SET @EntityTypeId = (SELECT TOP 1 [Id]
  FROM [EntityType]
  WHERE NAME = 'Rock.Model.MetricCategory');  
INSERT INTO [Category] ( [IsSystem],[EntityTypeId],[Name],[IconCssClass],[Description],[Order],[Guid] )
                            VALUES( 0, @EntityTypeId,'Headcount','fa fa-line-chart','Used to store check-in headcounts',0,{0} )", SystemGuid.Category.ROOT_HEADCOUNT_CATEGORY ) );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.CATEGORY, "Class", "com.bricksandmortarstudio.checkinextensions.SetupHeadcountMetric",
               "Headcount Category", "", "The category to store your metrics in.", 0, "", SystemGuid.Attribute.CALCULATE_HEADCOUNT_METRIC_CATEGORY );
            Sql( string.Format( @" 
        DECLARE @EntityId int
        INSERT INTO [ServiceJob]
        ([IsSystem]
        ,[IsActive]
        ,[Name]
        ,[Description]
        ,[Class]
        ,[CronExpression]
        ,[NotificationStatus]
        ,[Guid])
     VALUES
        (0
        ,1
        ,'Headcount Metric Sync Groups'
        ,'Creates metrics and metric categories as new check-in areas and groups are added.'
        ,'com.bricksandmortarstudio.checkinextensions.SetupHeadcountMetric'
        ,'0 0 2 1/1 * ? *'
        ,1
        ,{0))
        SET @EntityId = SCOPE_IDENTITY()
        SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')
        INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId],[Value],[Guid] )
        VALUES ( 1, @AttributeId, @EntityId, '{2}', NEWID() )
", SystemGuid.Job.CALCULATE_HEADCOUNT_METRIC, SystemGuid.Attribute.CALCULATE_HEADCOUNT_METRIC_CATEGORY, SystemGuid.Category.ROOT_HEADCOUNT_CATEGORY ) );
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteCategory( SystemGuid.Category.ROOT_HEADCOUNT_CATEGORY );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.CALCULATE_HEADCOUNT_METRIC_CATEGORY );
            Sql(string.Format( @"DELETE FROM [ServiceJob]
                WHERE [Guid]={0};", SystemGuid.Job.CALCULATE_HEADCOUNT_METRIC) 
            );
        }
    }
}
