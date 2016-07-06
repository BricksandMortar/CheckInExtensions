using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bricksandmortarstudio.checkinextensions.Migrations
{
    [MigrationNumber( 5, "1.4.0" )]
    class FixIndividualAttendedDurationBadge : Migration
    {
        public override void Up()
        {
            Sql( @"
DROP PROCEDURE [dbo].[spBricksandMortarStudio_Checkin_WeeksAttendedInDurationWithGroupType]");
            Sql(@"
    CREATE PROCEDURE [dbo].[spBricksandMortarStudio_Checkin_WeeksAttendedInDurationWithGroupType]
	    @PersonId int
	    ,@WeekDuration int = 16
		,@IdList nvarchar(max)
		,@Recursive bit
    AS
    BEGIN
	
        DECLARE @LastSunday datetime 
        SET @LastSunday = [dbo].[ufnUtility_GetPreviousSundayDate]()

	 declare @groupIds table (groupId int);
	IF (@Recursive = 1)
		BEGIN
			insert into @groupIds select i.Item from [dbo].com_bricksandmortarstudio_SplitInts(@IdList, ',') as i
		END
	ELSE
		BEGIN
			insert into @groupIds 
			SELECT [Id] 
			FROM [Group] 
			WHERE [GroupTypeId] IN (select i.Item from [dbo].com_bricksandmortarstudio_SplitInts(@IdList, ',') as i)
		END

        SELECT 
	        COUNT(DISTINCT a.SundayDate )
        FROM
	        [Attendance] a
	        INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
        WHERE 
	        [GroupId] IN (select groupId from @groupIds)
	        AND pa.[PersonId] = @PersonId 
	        AND a.[StartDateTime] BETWEEN DATEADD(WEEK, ((@WeekDuration -1) * -1), @LastSunday) AND DATEADD(DAY, 1, @LastSunday)    
			END
" );
        }

        public override void Down()
        {
            Sql( @"DROP PROCEDURE [dbo].[spBricksandMortarStudio_Checkin_WeeksAttendedInDurationWithGroupType]");
            Sql(@")
 CREATE PROCEDURE [dbo].[spBricksandMortarStudio_Checkin_WeeksAttendedInDurationWithGroupType]
	    @PersonId int
	    ,@WeekDuration int = 16
		,@IdList nvarchar(max)
		,@Recursive bit
    AS
    BEGIN
	
        DECLARE @LastSunday datetime 
        SET @LastSunday = [dbo].[ufnUtility_GetPreviousSundayDate]()

	 declare @groupIds table (groupId int);
	IF (@Recursive = 1)
		BEGIN
			insert into @groupIds select i.Item from [dbo].com_bricksandmortarstudio_SplitInts(@IdList, ',') as i
		END
	ELSE
		BEGIN
			insert into @groupIds 
			SELECT [Id] 
			FROM [Group] 
			WHERE [GroupTypeId] IN (select i.Item from [dbo].com_bricksandmortarstudio_SplitInts(@IdList, ',') as i)
		END

        SELECT 
	        COUNT(DISTINCT a.SundayDate )
        FROM
	        [Attendance] a
	        INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
        WHERE 
	        [GroupId] IN (select groupId from @groupIds)
	        AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](@PersonId))
	        AND a.[StartDateTime] BETWEEN DATEADD(WEEK, ((@WeekDuration -1) * -1), @LastSunday) AND DATEADD(DAY, 1, @LastSunday)    
			END" );
        }
    }
}
