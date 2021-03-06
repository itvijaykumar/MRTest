USE [MomentaRecruitment.Phase3.Test]
GO
/****** Object:  StoredProcedure [TimeSheet].[GetValidationDetailsForTimesheet]    Script Date: 08/24/2015 18:36:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER  PROCEDURE [TimeSheet].[GetValidationDetailsForTimesheet] --12960
	@timeSheetId INT

AS
BEGIN
	SET NOCOUNT ON;	

IF EXISTS(SELECT * 
          FROM   timesheet.timesheet 
          WHERE  timesheetid = @timeSheetId) 
  BEGIN 
      DECLARE @ModifiedTime DATETIME2; 
	  DECLARE @EndTime DATETIME2; 
      DECLARE @IndividualId INT; 
      DECLARE @TimesheetTypeid INT; 
	  DECLARE @AssociateId INT; 
	  DECLARE @AgencyId INT;
	  --Declared the below one to get the status of the given timesheet
	  DECLARE @TimesheetStatusId INT;
	  --Declared the below 2 variable to hold blans status and updated status which helps to get overtime details
	  DECLARE @TimesheetSubmittedStatusId INT;
	  DECLARE @TimesheetUpdateStatusId INT;
	  --Declare the below one to hold the actual modified time i.e. timesheet start date or the current date 
	  --which helps in getting overtime details
	  DECLARE @OverTimeModifiedTime DATETIME2;
	  DECLARE @AttendanceOptionsModifiedTime DATETIME2;

      SELECT @IndividualId = ind.individualid, 
             @ModifiedTime =case when ind.startdate between tms.startdate and tms.enddate then ind.startdate else tms.startdate end,
			 @EndTime = tms.EndDate,  
             @TimesheetTypeid = tms.timesheettypeid, 
			 @AssociateId = tms.associateid,
			 @TimesheetStatusId=TMS.TimeSheetStatusId
      FROM   timesheet.timesheet tms 
             INNER JOIN timesheet.assignedrole ind 
                     ON ind.roleid = tms.roleid 
                        AND ind.associateid = tms.associateid 
      WHERE  tms.timesheetid = @TimesheetId; 

	  SELECT top 1 @TimesheetSubmittedStatusId=ts.TimeSheetStatusId
	  from TimeSheet.TimeSheetStatus ts where ts.Description like '%submitted%'

	  --SELECT top 1 @TimesheetUpdateStatusId=ts.TimeSheetStatusId
	  --from TimeSheet.TimeSheetStatus ts where ts.Description like '%update%'
	  SELECT @AgencyId = AgencyId FROM Associate WHERE ID = @AssociateId

	  DECLARE @StartDate DATETIME2; 
      DECLARE @EndDate DATETIME2; 

	  SELECT @StartDate = StartDate, 
			 @EndDate = EndDate
	  FROM   TimeSheet.AssignedRole
	  WHERE IndividualId = @IndividualId
	
		/*
		In the below condition, checking whether the given timesheet is under blank status or updated status
		If it is blank or updated timesheet, in the variable "OverTimeModifiedTime", storing current time, 
		so that we can get latest data under overtime columns
		Otherwise, we are getting last update data which is updated before timesheet start date

		*/
		if (@TimesheetStatusId!=@TimesheetSubmittedStatusId)
			select @OverTimeModifiedTime=GETDATE()
		else
			select @OverTimeModifiedTime=@ModifiedTime

		set @AttendanceOptionsModifiedTime =GETDATE()


	
	SELECT *
	INTO #IC
	FROM x_audit.Get_dbo_Individual_Changes(@IndividualId)

	CREATE CLUSTERED INDEX IDX_C_IC_IndividualId ON #IC(IndividualId)
	CREATE INDEX IDX_IC_Col ON #IC(Col)
	CREATE INDEX IDX_IC_ModifiedTime ON #IC(ModifiedTime)
	
	
	
	/*
	Otherthan overtime columns, we are getting last update data which is updated 
	before timesheet start date, using @ModifiedTime
	For overtime columns, based on timesheet sstatus, getting latest data or last update data 
	which is updated before timesheet start date, using @@OverTimeModifiedTime
	*/
	;WITH ICA AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'AssociateId' 
		AND CAST([ModifiedTime] AS DATE)<= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICC AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ClientId' 
		AND CAST([ModifiedTime] AS DATE)<= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICP AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ProjectId' 
		AND CAST([ModifiedTime] AS DATE)<= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICR AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'RoleId' 
		AND CAST([ModifiedTime] AS DATE)<= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICSD AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'StartDate' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICED AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'EndDate'
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICMR AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'FullMomentaRate' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICAR AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'AssociateRate' 
		AND CAST([ModifiedTime] AS DATE) <= @EndTime
		ORDER BY ModifiedTime DESC
	),
	ICRT AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'Retention' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICRP AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'RetentionPeriodId' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICRC AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'RetentionCharge' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICRPA AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'RetentionPayAway' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICTSA AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'TimeSheetApproverId' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICTSAA AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'TimeSheetApproverAssociateId' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICTST AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'TimeSheetTypeId' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICM AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'Monday' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@OverTimeModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	
	ICTU AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'Tuesday' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@OverTimeModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	
	ICW AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'Wednesday' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@OverTimeModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICTH AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'Thursday' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@OverTimeModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICF AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'Friday' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@OverTimeModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICSA AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'Saturday' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@OverTimeModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICSU AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'Sunday' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@OverTimeModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICWH AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'WorkingHours' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICOTPA AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'OvertimePayAway' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICOTPR AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'OvertimePayRatio' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICIDMW AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'IncentiveDaysMaxWorked' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICIDCA AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'IncentiveDaysCountedAs' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICIDI AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'IncentiveDaysIn7' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICACR AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'AdditionalCaseRate' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICOOPA AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'OneOffPmtAmount' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICOOPD AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'OneofPaymentDate' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXA AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseAccomodation' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXS AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseSubsistence' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXT AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseTravel' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXP AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseParking' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXM AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseMileage' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXO AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseOther' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXARCPT AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseAccomodationReceipt' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXSRCPT AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseSubsistenceReceipt' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXTRCPT AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseTravelReceipt' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXPRCPT AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseParkingReceipt' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXMRCPT AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseMileageReceipt' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXORCPT AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseOtherReceipt' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXAREC AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseAccomodationRecharge' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXSREC AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseSubsistenceRecharge' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXTREC AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseTravelRecharge' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXPREC AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseParkingRecharge' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXMREC AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseMileageRecharge' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICEXOREC AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ExpenseOtherRecharge' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICS AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'Suspended' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICOPC AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'OverProductionCharge' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICOPPA AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'OverProductionPayAway' 
		AND CAST([ModifiedTime] AS DATE) <= @EndTime
		ORDER BY ModifiedTime DESC
	),
	ICIC AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'IncentiveCharge' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICIPA AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'IncentivePayAway' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICCAFD AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'CancelFullDay' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@AttendanceOptionsModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICCAHD AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'CancelHalfDay' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@AttendanceOptionsModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICTFD AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'TravelFullDay' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@AttendanceOptionsModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICTHD AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'TravelHalfDay' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@AttendanceOptionsModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICNI AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'NoticeIntervalId' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICNA AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'NoticeAmount' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICH AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'Hourly' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@AttendanceOptionsModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICOTC AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'OverTimeCharge' 
		AND CAST([ModifiedTime] AS DATE) <= CAST( @ModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC
	),
	ICMO AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'MondayOvertime' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@OverTimeModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC, Sequence DESC
	),
	ICTUO AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'TuesdayOvertime' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@OverTimeModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC, Sequence DESC
	),
	ICWO AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'WednesdayOvertime' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@OverTimeModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC, Sequence DESC
	),
	ICTHO AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'ThursdayOvertime' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@OverTimeModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC, Sequence DESC
	),
	ICFO AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'FridayOvertime' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@OverTimeModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC, Sequence DESC
	),
	ICSAO AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'SaturdayOvertime' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@OverTimeModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC, Sequence DESC
	),
	ICSUO AS (
		SELECT TOP 1 IndividualId, Val
		FROM #IC
		WHERE IndividualId = @IndividualId 
		AND Col = 'SundayOvertime' 
		AND CAST([ModifiedTime] AS DATE) <= CAST(@OverTimeModifiedTime AS DATE)
		ORDER BY ModifiedTime DESC, Sequence DESC
	)
	
	SELECT
			@IndividualId IndividualId, 
			A.Val AssociateId,
			C.Val ClientId,
			P.Val ProjectId,
			R.Val RoleId,
			SD.Val StartDate,
			ED.Val EndDate,      
			MR.Val FullMomentaRate,
			AR.Val AssociateRate,
			RT.Val [Retention],
			RP.Val RetentionPeriodId,
			RC.Val RetentionCharge,
			RPA.Val RetentionPayAway,
			TSA.Val TimeSheetApproverId,
			TSAA.Val TimeSheetApproverAssociateId,
			TST.Val TimeSheetTypeId,
			M.Val Monday,
			MO.Val MondayOvertime,
			TU.Val Tuesday,
			TUO.Val TuesdayOvertime,
			W.Val Wednesday,
			WO.Val WednesdayOvertime,
			TH.Val Thursday,
			THO.Val ThursdayOvertime,
			F.Val Friday,
			FO.Val FridayOvertime,
			SA.Val Saturday,
			SAO.Val SaturdayOvertime,
			SU.Val Sunday,
			SUO.Val SundayOvertime,
			WH.Val WorkingHours,
			OTPA.Val OvertimePayAway,
			OTPR.Val OvertimePayRatio,
			IDMW.Val IncentiveDaysMaxWorked,
			IDCA.Val IncentiveDaysCountedAs,
			IDI.Val IncentiveDaysIn7,
			ACR.Val AdditionalCaseRate,
			OOPA.Val OneOffPmtAmount,
			OOPD.Val OneofPaymentDate,
			EXA.Val ExpenseAccomodation,
			EXS.Val ExpenseSubsistence,
			EXT.Val ExpenseTravel,
			[EXP].Val ExpenseParking,
			EXM.Val ExpenseMileage,
			EXO.Val ExpenseOther,
			EXARCPT.Val ExpenseAccomodationReceipt,
			EXSRCPT.Val ExpenseSubsistenceReceipt,
			EXTRCPT.Val ExpenseTravelReceipt,
			[EXPRCPT].Val ExpenseParkingReceipt,
			EXMRCPT.Val ExpenseMileageReceipt,
			EXORCPT.Val ExpenseOtherReceipt,
			EXAREC.Val ExpenseAccomodationRecharge,
			EXSREC.Val ExpenseSubsistenceRecharge,
			EXTREC.Val ExpenseTravelRecharge,
			[EXPREC].Val ExpenseParkingRecharge,
			EXMREC.Val ExpenseMileageRecharge,
			EXOREC.Val ExpenseOtherRecharge,
			S.Val Suspended,
			OPC.Val OverProductionCharge,
			OPPA.Val OverProductionPayAway,
 			IC.Val IncentiveCharge,
			IPA.Val IncentivePayAway,
			CAFD.Val CancelFullDay,
			CAHD.Val CancelHalfDay,
			TFD.Val TravelFullDay,
			THD.Val TravelHalfDay,
			NI.Val NoticeIntervalId,
			NA.Val NoticeAmount,
			H.Val Hourly,
			OTC.Val OverTimeCharge
	INTO #IH
	FROM ICA AS A -- Associate
	LEFT OUTER JOIN ICC AS C ON A.IndividualId = C.IndividualId -- ClientId
	LEFT OUTER JOIN ICP AS P ON A.IndividualId = P.IndividualId  -- ProjectId
	LEFT OUTER JOIN ICR AS R ON A.IndividualId = R.IndividualId  -- RoleId
	LEFT OUTER JOIN ICSD AS SD ON A.IndividualId = SD.IndividualId  -- StartDate
	LEFT OUTER JOIN ICED AS ED ON A.IndividualId = ED.IndividualId  -- EndDate
	LEFT OUTER JOIN ICMR AS MR ON A.IndividualId = MR.IndividualId  -- FullMomentaRate
	LEFT OUTER JOIN ICAR AS AR ON A.IndividualId = AR.IndividualId  -- AssociateRate
	LEFT OUTER JOIN ICRT AS RT ON A.IndividualId = RT.IndividualId  -- Retension
	LEFT OUTER JOIN ICRP AS RP ON A.IndividualId = RP.IndividualId  -- RetentionPeriodId
	LEFT OUTER JOIN ICRC AS RC ON A.IndividualId = RC.IndividualId  -- RetentionCharge
	LEFT OUTER JOIN ICRPA AS RPA ON A.IndividualId = RPA.IndividualId  -- RetentionPayAway
	LEFT OUTER JOIN ICTSA AS TSA ON A.IndividualId = C.IndividualId  -- TimeSheetApproverId
	LEFT OUTER JOIN ICTSAA AS TSAA ON A.IndividualId = C.IndividualId  -- TimeSheetApproverAssociateId
	LEFT OUTER JOIN ICTST AS TST ON A.IndividualId = C.IndividualId  -- TimeSheetTypeId
	LEFT OUTER JOIN ICM AS M ON A.IndividualId = C.IndividualId  -- Monday
	LEFT OUTER JOIN ICMO AS MO ON A.IndividualId = C.IndividualId  -- MondayOvertime
	LEFT OUTER JOIN ICTU AS TU ON A.IndividualId = C.IndividualId  -- Tuesday
	LEFT OUTER JOIN ICTUO AS TUO ON A.IndividualId = C.IndividualId  -- TuesdayOvertime
	LEFT OUTER JOIN ICW AS W ON A.IndividualId = C.IndividualId  -- Wednesday
	LEFT OUTER JOIN ICWO AS WO ON A.IndividualId = C.IndividualId  -- WednesdayOvertime
	LEFT OUTER JOIN ICTH AS TH ON A.IndividualId = C.IndividualId  -- Thursday
	LEFT OUTER JOIN ICTHO AS THO ON A.IndividualId = C.IndividualId  -- ThursdayOvertime
	LEFT OUTER JOIN ICF AS F ON A.IndividualId = C.IndividualId  -- Friday
	LEFT OUTER JOIN ICFO AS FO ON A.IndividualId = C.IndividualId  -- FridayOvertime
	LEFT OUTER JOIN ICSA AS SA ON A.IndividualId = C.IndividualId  -- Saturday
	LEFT OUTER JOIN ICSAO AS SAO ON A.IndividualId = C.IndividualId  -- SaturdayOvertime
	LEFT OUTER JOIN ICSU AS SU ON A.IndividualId = C.IndividualId  -- Sunday
	LEFT OUTER JOIN ICSUO AS SUO ON A.IndividualId = C.IndividualId  -- SundayOvertime
	LEFT OUTER JOIN ICWH AS WH ON A.IndividualId = C.IndividualId  -- WorkingHours
	LEFT OUTER JOIN ICOTPA AS OTPA ON A.IndividualId = C.IndividualId  -- OvertimePayAway
	LEFT OUTER JOIN ICOTPR AS OTPR ON A.IndividualId = C.IndividualId  -- OvertimePayRatio
	LEFT OUTER JOIN ICIDMW AS IDMW ON A.IndividualId = C.IndividualId  -- IncentiveDaysMaxWorked
	LEFT OUTER JOIN ICIDCA AS IDCA ON A.IndividualId = C.IndividualId  -- IncentiveDaysCountedAs
	LEFT OUTER JOIN ICIDI AS IDI ON A.IndividualId = C.IndividualId  -- IncentiveDaysIn7
	LEFT OUTER JOIN ICACR AS ACR ON A.IndividualId = C.IndividualId  -- AdditionalCaseRate
	LEFT OUTER JOIN ICOOPA AS OOPA ON A.IndividualId = C.IndividualId  -- OneOffPmtAmount
	LEFT OUTER JOIN ICOOPD AS OOPD ON A.IndividualId = C.IndividualId  -- OneofPaymentDate
	LEFT OUTER JOIN ICEXA AS EXA ON A.IndividualId = C.IndividualId  -- ExpenseAccomodation
	LEFT OUTER JOIN ICEXS AS EXS ON A.IndividualId = C.IndividualId  -- ExpenseSubsistence
	LEFT OUTER JOIN ICEXT AS EXT ON A.IndividualId = C.IndividualId  -- ExpenseTravel
	LEFT OUTER JOIN ICEXP AS [EXP] ON A.IndividualId = C.IndividualId  -- ExpenseParking
	LEFT OUTER JOIN ICEXM AS EXM ON A.IndividualId = C.IndividualId  -- ExpenseMileage
	LEFT OUTER JOIN ICEXO AS EXO ON A.IndividualId = C.IndividualId  -- ExpenseOther
	LEFT OUTER JOIN ICEXARCPT AS EXARCPT ON A.IndividualId = C.IndividualId  -- ExpenseAccomodationReceipt
	LEFT OUTER JOIN ICEXSRCPT AS EXSRCPT ON A.IndividualId = C.IndividualId  -- ExpenseSubsistenceReceipt
	LEFT OUTER JOIN ICEXTRCPT AS EXTRCPT ON A.IndividualId = C.IndividualId  -- ExpenseTravelReceipt
	LEFT OUTER JOIN ICEXPRCPT AS [EXPRCPT] ON A.IndividualId = C.IndividualId  -- ExpenseParkingReceipt
	LEFT OUTER JOIN ICEXMRCPT AS EXMRCPT ON A.IndividualId = C.IndividualId  -- ExpenseMileageReceipt
	LEFT OUTER JOIN ICEXORCPT AS EXORCPT ON A.IndividualId = C.IndividualId  -- ExpenseOtherReceipt
	LEFT OUTER JOIN ICEXAREC AS EXAREC ON A.IndividualId = C.IndividualId  -- ExpenseAccomodationRecharge
	LEFT OUTER JOIN ICEXSREC AS EXSREC ON A.IndividualId = C.IndividualId  -- ExpenseSubsistenceRecharge
	LEFT OUTER JOIN ICEXTREC AS EXTREC ON A.IndividualId = C.IndividualId  -- ExpenseTravelRecharge
	LEFT OUTER JOIN ICEXPREC AS [EXPREC] ON A.IndividualId = C.IndividualId  -- ExpenseParkingRecharge
	LEFT OUTER JOIN ICEXMREC AS EXMREC ON A.IndividualId = C.IndividualId  -- ExpenseMileageRecharge
	LEFT OUTER JOIN ICEXOREC AS EXOREC ON A.IndividualId = C.IndividualId  -- ExpenseOtherRecharge
	LEFT OUTER JOIN ICS AS S ON A.IndividualId = C.IndividualId  -- Suspended
	LEFT OUTER JOIN ICOPC AS OPC ON A.IndividualId = C.IndividualId  -- OverProductionCharge
	LEFT OUTER JOIN ICOPPA AS OPPA ON A.IndividualId = C.IndividualId  -- OverProductionPayAway
	LEFT OUTER JOIN ICIC AS IC ON A.IndividualId = C.IndividualId  -- IncentiveCharge
	LEFT OUTER JOIN ICIPA AS IPA ON A.IndividualId = C.IndividualId  -- IncentivePayAway
	LEFT OUTER JOIN ICCAFD AS CAFD ON A.IndividualId = C.IndividualId  -- CancelFullDay
	LEFT OUTER JOIN ICCAHD AS CAHD ON A.IndividualId = C.IndividualId  -- CancelHalfDay
	LEFT OUTER JOIN ICTFD AS TFD ON A.IndividualId = C.IndividualId  -- TravelFullDay
	LEFT OUTER JOIN ICTHD AS THD ON A.IndividualId = C.IndividualId  -- TravelHalfDay
	LEFT OUTER JOIN ICNI AS NI ON A.IndividualId = C.IndividualId  -- NoticeIntervalId
	LEFT OUTER JOIN ICNA AS NA ON A.IndividualId = NA.IndividualId  -- NoticeAmount
	LEFT OUTER JOIN ICH AS H ON A.IndividualId = H.IndividualId  -- Hourly
	LEFT OUTER JOIN ICOTC AS OTC ON A.IndividualId = OTC.IndividualId  -- OverTimeCharge

		DROP TABLE #IC

		IF EXISTS(SELECT associateid
		FROM   #IH)	
	  BEGIN
      ;WITH i_cte 
           AS (SELECT 
		   *
			  
               FROM   #ih), 
           r_cte 
           AS (SELECT distinct individual.individualid, 
                      project.NAME    AS ProjectName, 
                      roletype.NAME   AS RoleName, 
                      client.NAME     AS ClientName, 
                      role.requirementid, 
                      timesheet.timesheet.timesheettypeid, 
                      individual.timesheetapproverid, 
                      CASE 
                        WHEN CTP.associateid IS NULL THEN CTP.NAME 
                        ELSE A.firstname + ' ' + A.lastname 
                      END             AS TimeSheetApprover, 
                      CTP.associateid AS TimeSheetApproverAssociateId,
					  A.TimesheetsSuspended,
					  A1.TimesheetsSuspended As AssociateTimesheetsSuspended
					
               FROM   timesheet.timesheet 
                      INNER JOIN role 
                              ON timesheet.timesheet.roleid = role.roleid 
                      INNER JOIN roletype 
                              ON role.roletypeid = roletype.roletypeid 
                      INNER JOIN project 
                              ON timesheet.timesheet.projectid = 
                                 project.projectid 
                      INNER JOIN client 
                              ON project.clientid = client.clientid 
                      INNER JOIN individual 
                              ON role.roleid = individual.roleid 
					  INNER JOIN Associate A1
                              ON timesheet.timesheet.AssociateId = A1.ID
                      LEFT OUTER JOIN clientcontact CTP 
                                   ON CTP.clientcontactid = 
                                      individual.timesheetapproverid 
                      LEFT OUTER JOIN dbo.associate A 
                                   ON A.id = CTP.associateid 


               WHERE  ( timesheet.timesheet.timesheetid = @TimesheetId )) 

			   ,
			   abs_cte as(
	
				SELECT 
					 AssociateId
					,AnticipatedReturn 
					,AbsenceDurationTypeId
					,AbsenceTypeId
					,AbsentFrom
					,AbsenceId
					,ROW_NUMBER() OVER(PARTITION BY AssociateId ORDER BY AnticipatedReturn ASC) AS Anticipated
				FROM dbo.Absence 
				WHERE AbsenceStatusId != 3 --not deleted
				AND AnticipatedReturn >= CAST (GETDATE() AS DATE)		
				--) ABS_ASO
				--ON ABS_ASO.AssociateId	= @AssociateId
				--AND ABS_ASO.Anticipated = 1	
			   )

      SELECT i_cte.individualid, 
             i_cte.associateid, 
             i_cte.clientid, 
             r_cte.clientname, 
             i_cte.projectid, 
             r_cte.projectname, 
             i_cte.roleid, 
             r_cte.rolename, 
             r_cte.requirementid, 
             Isnull(i_cte.startdate,@StartDate) AS startdate, 
             Isnull(i_cte.enddate,@EndDate) AS enddate, 
             i_cte.fullmomentarate, 
             i_cte.associaterate, 
             i_cte.[retention], 
             i_cte.retentionperiodid, 
             i_cte.retentioncharge, 
             i_cte.retentionpayaway, 
             Isnull(timesheetapprover, 'Currently not set') AS TimeSheetApprover,              
             i_cte.timesheetapproverid, 
             r_cte.timesheetapproverassociateid,
			 Isnull(i_cte.timesheettypeid, @TimesheetTypeid) AS timesheettypeid,
			 i_cte.monday,
			 i_cte.mondayovertime,
			 i_cte.tuesday,
			 i_cte.tuesdayovertime,
			 i_cte.wednesday,
			 i_cte.wednesdayovertime,
			 i_cte.thursday,
			 i_cte.thursdayovertime,
			 i_cte.friday,
			 i_cte.fridayovertime,
			 i_cte.saturday,
			 i_cte.saturdayovertime,
			 i_cte.sunday,
			 i_cte.sundayovertime,
			 i_cte.workinghours,
			 i_cte.overtimepayaway,
			 i_cte.overtimepayratio,
			 i_cte.incentivedaysmaxworked,
			 i_cte.incentivedayscountedas,
			 i_cte.incentivedaysin7,
			 i_cte.additionalcaserate,
			 i_cte.oneoffpmtamount,
			 i_cte.oneofpaymentdate,
			 i_cte.expenseaccomodation,
			 i_cte.expensesubsistence,
			 i_cte.expensetravel,
			 i_cte.ExpenseMileage,
			 i_cte.expenseparking,
			 i_cte.expenseother,
			  i_cte.expenseaccomodationreceipt,
			 i_cte.expensesubsistencereceipt,
			 i_cte.expensetravelreceipt,
			 i_cte.ExpenseMileagereceipt,
			 i_cte.expenseparkingreceipt,
			 i_cte.expenseotherreceipt,
			  i_cte.expenseaccomodationrecharge,
			 i_cte.expensesubsistencerecharge,
			 i_cte.expensetravelrecharge,
			 i_cte.ExpenseMileagerecharge,
			 i_cte.expenseparkingrecharge,
			 i_cte.expenseotherrecharge,
			-- isNull(r_cte.TimesheetsSuspended,0) AS suspended,
			 isNull(r_cte.AssociateTimesheetsSuspended,0) AS suspended,
			 abs_cte.AnticipatedReturn,
			 abs_cte.AbsenceDurationTypeId,
			 abs_cte.AbsentFrom,
			 abs_cte.AbsenceTypeId,
			 abs_cte.AbsenceId,
			i_cte.overproductioncharge,
			i_cte.overproductionpayaway,
			i_cte.incentivecharge,
			i_cte.incentivepayaway,
			i_cte.cancelfullday,
			i_cte.cancelhalfday,
			i_cte.travelfullday,
			i_cte.travelhalfday,
			CASE 
						WHEN @AgencyId IS NOT NULL
							--OR ASO.UmbrellaCompanyId IS NOT NULL
						THEN 1 ELSE 0 END AS IsAgencyOrUmbrella,
i_cte.NoticeIntervalId,
i_cte.NoticeAmount,
i_cte.Hourly,
i_cte.OverTimeCharge


      FROM   i_cte 
             INNER JOIN r_cte 
                     ON i_cte.individualid = r_cte.individualid 
			LEFT OUTER JOIN abs_cte
			ON i_cte.AssociateId = abs_cte.AssociateId	
				AND abs_cte.Anticipated = 1		
  END
  ELSE 
  BEGIN 
      SELECT [IndividualId]
      ,ind.[AssociateId]
      ,[ClientId]
      ,[ClientName]
      ,ind.[ProjectId]
      ,[ProjectName]
      ,ind.[RoleId]
      ,[RoleName]
      ,[RequirementId]
      ,ind.[StartDate]
      ,ind.[EndDate]
      ,[FullMomentaRate]
      ,[AssociateRate]
      ,[Retention]
      ,[RetentionPeriodId]
      ,[RetentionCharge]
      ,[RetentionPayAway]
      ,[TimeSheetApprover]
      ,[TimeSheetApproverId]
      ,[TimeSheetApproverAssociateId]
      ,[Monday]
      ,[MondayOvertime]
      ,[Tuesday]
      ,[TuesdayOvertime]
      ,[Wednesday]
      ,[WednesdayOvertime]
      ,[Thursday]
      ,[ThursdayOvertime]
      ,[Friday]
      ,[FridayOvertime]
      ,[Saturday]
      ,[SaturdayOvertime]
      ,[Sunday]
      ,[SundayOvertime]
      ,[WorkingHours]
      ,[OverTimePayAway]
      ,[OverTimePayRatio]
      ,[IncentiveDaysMaxWorked]
      ,[IncentiveDaysCountedAs]
      ,[IncentiveDaysIn7]
      ,[AdditionalCaseRate]
      ,[AssociateRateComment]
      ,[FullMomentaRateComment]
      ,[OneOffPmtAmount]
      ,[OneofPaymentDate]
      ,[ExpenseAccomodation] = NULL
      ,[ExpenseSubsistence] = NULL
      ,[ExpenseTravel] = NULL
      ,[ExpenseParking] = NULL
      ,[ExpenseMileage] = NULL
      ,[ExpenseOther] = NULL
     -- ,[Suspended]
      ,isnull([Associate].TimesheetsSuspended,0) Suspended
      ,[AnticipatedReturn]
      ,[AbsenceDurationTypeId]
      ,[AbsentFrom]
      ,[AbsenceTypeId]
      ,[AbsenceId]
      ,[OverProductionCharge]
      ,[OverProductionPayaway]
      ,[IncentiveCharge]
      ,[IncentivePayaway]
      ,[CancelFullDay]
      ,[CancelHalfDay]
      ,[TravelFullDay]
      ,[TravelHalfDay]
      ,[IsAgencyOrUmbrella]
      ,ind.NoticeIntervalId
      ,[NoticeAmount]
      ,[Hourly]
      ,[OverTimeCharge]
      ,[ExpenseAccomodationRecharge]
      ,[ExpenseAccomodationReceipt]
      ,[ExpenseSubsistenceRecharge]
      ,[ExpenseSubsistenceReceipt]
      ,[ExpenseTravelRecharge]
      ,[ExpenseTravelReceipt]
      ,[ExpenseParkingRecharge]
      ,[ExpenseParkingReceipt]
      ,[ExpenseMileageRecharge] 
      ,[ExpenseMileageReceipt] 
      ,[ExpenseOtherRecharge] 
      ,[ExpenseOtherReceipt] 
	   ,tms.TimesheetTypeId 			
				FROM TimeSheet.TimeSheet  tms 
				INNER JOIN TimeSheet.AssignedRole ind
				ON ind.RoleId =  tms.RoleId 
				AND ind.AssociateId = tms.AssociateId
				INNER JOIN Associate ON Associate.ID=  tms.AssociateId
				WHERE tms.TimeSheetId = @TimesheetId
  END 
  END
ELSE 
  BEGIN 
      SELECT [IndividualId]
      ,ind.[AssociateId]
      ,[ClientId]
      ,[ClientName]
      ,ind.[ProjectId]
      ,[ProjectName]
      ,ind.[RoleId]
      ,[RoleName]
      ,[RequirementId]
      ,ind.[StartDate]
      ,ind.[EndDate]
      ,[FullMomentaRate]
      ,[AssociateRate]
      ,[Retention]
      ,[RetentionPeriodId]
      ,[RetentionCharge]
      ,[RetentionPayAway]
      ,[TimeSheetApprover]
      ,[TimeSheetApproverId]
      ,[TimeSheetApproverAssociateId]
      ,[Monday]
      ,[MondayOvertime]
      ,[Tuesday]
      ,[TuesdayOvertime]
      ,[Wednesday]
      ,[WednesdayOvertime]
      ,[Thursday]
      ,[ThursdayOvertime]
      ,[Friday]
      ,[FridayOvertime]
      ,[Saturday]
      ,[SaturdayOvertime]
      ,[Sunday]
      ,[SundayOvertime]
      ,[WorkingHours]
      ,[OverTimePayAway]
      ,[OverTimePayRatio]
      ,[IncentiveDaysMaxWorked]
      ,[IncentiveDaysCountedAs]
      ,[IncentiveDaysIn7]
      ,[AdditionalCaseRate]
      ,[AssociateRateComment]
      ,[FullMomentaRateComment]
      ,[OneOffPmtAmount]
      ,[OneofPaymentDate]
      ,[ExpenseAccomodation] = NULL
      ,[ExpenseSubsistence] = NULL
      ,[ExpenseTravel] = NULL
      ,[ExpenseParking] = NULL
      ,[ExpenseMileage] = NULL
      ,[ExpenseOther] = NULL
    --  ,[Suspended]
      ,isnull([Associate].TimesheetsSuspended,0) Suspended
      ,[AnticipatedReturn]
      ,[AbsenceDurationTypeId]
      ,[AbsentFrom]
      ,[AbsenceTypeId]
      ,[AbsenceId]
      ,[OverProductionCharge]
      ,[OverProductionPayaway]
      ,[IncentiveCharge]
      ,[IncentivePayaway]
      ,[CancelFullDay]
      ,[CancelHalfDay]
      ,[TravelFullDay]
      ,[TravelHalfDay]
      ,[IsAgencyOrUmbrella]
      ,[ind].[NoticeIntervalId]
      ,[NoticeAmount]
      ,[Hourly]
      ,[OverTimeCharge]
      ,[ExpenseAccomodationRecharge]
      ,[ExpenseAccomodationReceipt]
      ,[ExpenseSubsistenceRecharge]
      ,[ExpenseSubsistenceReceipt]
      ,[ExpenseTravelRecharge]
      ,[ExpenseTravelReceipt]
      ,[ExpenseParkingRecharge]
      ,[ExpenseParkingReceipt]
      ,[ExpenseMileageRecharge] 
      ,[ExpenseMileageReceipt] 
      ,[ExpenseOtherRecharge] 
      ,[ExpenseOtherReceipt] 
	   ,tms.TimesheetTypeId 			
				FROM TimeSheet.TimeSheet  tms 
				INNER JOIN TimeSheet.AssignedRole ind
				ON ind.RoleId =  tms.RoleId 
				AND ind.AssociateId = tms.AssociateId
				INNER JOIN Associate ON Associate.ID=  tms.AssociateId
				WHERE tms.TimeSheetId = @TimesheetId
  END 
END
