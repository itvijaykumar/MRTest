USE [MomentaRecruitment.Phase3.Test]
GO
/****** Object:  StoredProcedure [TimeSheet].[GetCompanyNameByTimesheet]    Script Date: 08/25/2015 16:16:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--exec [TimeSheet].[GetCompanyNameByTimesheet] 9317
ALTER PROCEDURE [TimeSheet].[GetCompanyNameByTimesheet] 
	@TimesheetId INT

	AS
	BEGIN
		DECLARE @BusinessTypeID INT; 
		DECLARE @AssociateId INT; 
		DECLARE @StartTime DATETIME2; 
DECLARE @timesheetstatusid int; 

		Select @StartTime = StartDate,
		@AssociateId = AssociateId,@timesheetstatusid=TimeSheetStatusId
		From [Timesheet].[Timesheet]
		where TimesheetId= @TimesheetId

	
		
if(@timesheetstatusid=0)
begin
set @StartTime=getdate()
end
else
begin
select top 1 @StartTime=[Date] from TimeSheet.TimeSheetHistory where TimeSheetId=@TimesheetId and TimeSheetStatusId<>0 order by [Date] 
end



	SELECT top 1 @BusinessTypeID = CASE Val 
		WHEN 3 THEN 3 ELSE 1 END 
		FROM x_audit.Get_dbo_Associate_Changes(@AssociateId)
		where Col = 'BusinessTypeID'
		and ModifiedTime < @StartTime
		order by ModifiedTime desc;
		IF @BusinessTypeID = 3 
		BEGIN 
		Select UmbrellaCompany.Name
		FROM x_audit.Get_dbo_Associate_Changes(@AssociateId)
		x_ass INNER JOIN UmbrellaCompany ON x_ass.Val = UmbrellaCompany.UmbrellaCompanyId
		where Col = 'UmbrellaCompanyId'
		and ModifiedTime < @StartTime
		order by ModifiedTime desc
		END 
		ELSE 
		Select top 1 Val
		FROM x_audit.Get_dbo_Associate_Changes(@AssociateId)
		where Col = 'RegisteredCompanyName'
		and ModifiedTime < @StartTime
		order by ModifiedTime desc 
	END
