using Rock.Plugin;

namespace com.bricksandmortarstudio.checkinextensions.Migrations
{
    [MigrationNumber(1, "1.4.0")]
    class CreateDb : Migration
    {
        public override void Up()
        {
            // Create Tables
            Sql(@"CREATE FUNCTION dbo.com_bricksandmortarstudio_SplitInts
(
   @List      VARCHAR(MAX),
   @Delimiter VARCHAR(255)
)
RETURNS TABLE
AS
  RETURN ( SELECT Item = CONVERT(INT, Item) FROM
      ( SELECT Item = x.i.value('(./text())[1]', 'varchar(max)')
        FROM ( SELECT [XML] = CONVERT(XML, '<i>'
        + REPLACE(@List, @Delimiter, '</i><i>') + '</i>').query('.')
          ) AS a CROSS APPLY [XML].nodes('i') AS x(i) ) AS y
      WHERE Item IS NOT NULL
  )
");

            Sql(@"
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
");
            Sql(@"
/* spBricksandMortarStudio_Checkin_WeeksAttendedInDurationWithGroupType
*/
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
			END
");

        }

        public override void Down()
        {
            Sql(@"
DROP FUNCTION dbo.com_bricksandmortarstudio_SplitInts
DROP PROCEDURE [dbo].[spBricksandMortarStudio_Checkin_WeeksAttendedInDurationWithGroupType]
DROP PROCEDURE [dbo].[spBricksandMortarStudio_BadgeAttendanceWithGroupType]   
");
        }
    }
}