using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bricksandmortarstudio.checkinextensions.Migrations
{
    [MigrationNumber( 10, "1.4.0" )]
    class EnsureIndividualAttendedGraphBadgeDidAttend : Migration
    {
        public override void Up()
        {
            Sql( @"
DROP PROCEDURE [dbo].[spBricksandMortarStudio_BadgeAttendanceWithGroupType]" );
            Sql( @"
/* spBricksandMortarStudio_BadgeAttendanceWithGroupType
*/
CREATE PROCEDURE [dbo].[spBricksandMortarStudio_BadgeAttendanceWithGroupType]
	@PersonId int 
	, @ReferenceDate datetime = null
	, @MonthCount int = 24
	, @IdList nvarchar(max)
	, @Groups bit
AS
BEGIN
	DECLARE @StartDay datetime
	DECLARE @LastDay datetime

	-- if start date null get today's date
	IF @ReferenceDate is null
		SET @ReferenceDate = getdate()

	-- set data boundaries
	SET @LastDay = dbo.ufnUtility_GetLastDayOfMonth(@ReferenceDate) -- last day is most recent day
	SET @StartDay = DATEADD(M, DATEDIFF(M, 0, DATEADD(month, ((@MonthCount -1) * -1), @LastDay)), 0) -- start day is the 1st of the first full month of the oldest day

	-- make sure last day is not in future (in case there are errant checkin data)
	IF (@LastDay > getdate())
	BEGIN
		SET @LastDay = getdate()
	END
	
	
    declare @groupIds table (groupId int);
	IF (@Groups = 1)
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

	-- query for attendance data
	BEGIN
		SELECT 
			COUNT([Attended]) AS [AttendanceCount]
			, (SELECT dbo.ufnUtility_GetNumberOfSundaysInMonth(DATEPART(year, [SundayDate]), DATEPART(month, [SundayDate]), 'True' )) AS [SundaysInMonth]
			, DATEPART(month, [SundayDate]) AS [Month]
			, DATEPART(year, [SundayDate]) AS [Year]
		FROM (

			SELECT s.[SundayDate], [Attended]
				FROM dbo.ufnUtility_GetSundaysBetweenDates(@StartDay, @LastDay) s
				LEFT OUTER JOIN (	
						SELECT 
							DISTINCT a.[SundayDate] AS [AttendedSunday],
							1 as [Attended]
						FROM
							[Attendance] a
							INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
						WHERE 
							[GroupId] IN (select groupId from @groupIds)
							AND pa.[PersonId] = @PersonId 
							AND a.[StartDateTime] BETWEEN @StartDay AND @LastDay
                            AND a.[DidAttend] = 1
						) a ON [AttendedSunday] = s.[SundayDate]

		) [CheckinDates]
		GROUP BY DATEPART(month, [SundayDate]), DATEPART(year, [SundayDate])
		OPTION (MAXRECURSION 1000)
	END
END
" );
        }

        public override void Down()
        {
            Sql( @"
DROP PROCEDURE [dbo].[spBricksandMortarStudio_BadgeAttendanceWithGroupType]" );
            Sql( @"
/* spBricksandMortarStudio_BadgeAttendanceWithGroupType
*/
CREATE PROCEDURE [dbo].[spBricksandMortarStudio_BadgeAttendanceWithGroupType]
	@PersonId int 
	, @ReferenceDate datetime = null
	, @MonthCount int = 24
	, @IdList nvarchar(max)
	, @Groups bit
AS
BEGIN
	DECLARE @StartDay datetime
	DECLARE @LastDay datetime

	-- if start date null get today's date
	IF @ReferenceDate is null
		SET @ReferenceDate = getdate()

	-- set data boundaries
	SET @LastDay = dbo.ufnUtility_GetLastDayOfMonth(@ReferenceDate) -- last day is most recent day
	SET @StartDay = DATEADD(M, DATEDIFF(M, 0, DATEADD(month, ((@MonthCount -1) * -1), @LastDay)), 0) -- start day is the 1st of the first full month of the oldest day

	-- make sure last day is not in future (in case there are errant checkin data)
	IF (@LastDay > getdate())
	BEGIN
		SET @LastDay = getdate()
	END
	
	
    declare @groupIds table (groupId int);
	IF (@Groups = 1)
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

	-- query for attendance data
	BEGIN
		SELECT 
			COUNT([Attended]) AS [AttendanceCount]
			, (SELECT dbo.ufnUtility_GetNumberOfSundaysInMonth(DATEPART(year, [SundayDate]), DATEPART(month, [SundayDate]), 'True' )) AS [SundaysInMonth]
			, DATEPART(month, [SundayDate]) AS [Month]
			, DATEPART(year, [SundayDate]) AS [Year]
		FROM (

			SELECT s.[SundayDate], [Attended]
				FROM dbo.ufnUtility_GetSundaysBetweenDates(@StartDay, @LastDay) s
				LEFT OUTER JOIN (	
						SELECT 
							DISTINCT a.[SundayDate] AS [AttendedSunday],
							1 as [Attended]
						FROM
							[Attendance] a
							INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
						WHERE 
							[GroupId] IN (select groupId from @groupIds)
							AND pa.[PersonId] = @PersonId 
							AND a.[StartDateTime] BETWEEN @StartDay AND @LastDay
						) a ON [AttendedSunday] = s.[SundayDate]

		) [CheckinDates]
		GROUP BY DATEPART(month, [SundayDate]), DATEPART(year, [SundayDate])
		OPTION (MAXRECURSION 1000)
	END
END
" );
        }
    }
}
