USE [MomentaRecruitment.Phase3.Test]
GO
/****** Object:  StoredProcedure [dbo].[BackDateAssociateChanges]    Script Date: 08/21/2015 21:18:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--exec [dbo].[BackDateAssociateChanges] 416,'BusinessTypeId','1','20/08/2015 00:00:00.000'
ALTER PROC [dbo].[BackDateAssociateChanges]
    @associateId int,
    @col varchar(500),
	@ColValue varchar(500),
    @taskId int
AS

    DECLARE @SQLQuery AS NVARCHAR(500),
	 @changeDate DATETIME
	SELECT @changeDate=StartDate
	FROM SCHEDULER.TASK  WHERE TASKID=@taskId
    IF @ColValue = '' OR @ColValue IS NULL
    BEGIN
    SELECT @SQLQuery = 
    'UPDATE x_Audit.dbo_Associate
    SET ModifiedTime = ''' + CONVERT(varchar(20), @changeDate) + '''
    WHERE
        Sequence IN (
            SELECT Max(Sequence)
            FROM x_Audit.dbo_Associate
            WHERE ID = ' + CONVERT(varchar(6), @associateId) + ' AND ' + @col + ' IS NULL AND changed_' + @col + ' = 1 AND Action <> ''I'')'
      end
      else
      begin
      SELECT @SQLQuery = 
    'UPDATE x_Audit.dbo_Associate
    SET ModifiedTime = ''' + CONVERT(varchar(20), @changeDate) + '''
    WHERE
        Sequence IN (
            SELECT Max(Sequence)
            FROM x_Audit.dbo_Associate
            WHERE ID = ' + CONVERT(varchar(6), @associateId) + ' AND ' + @col + ' = ''' + @ColValue + ''' AND  changed_' + @col + ' = 1 AND Action <> ''I'')'
     
      end  
    
--      PRINT         @SQLQuery 
    EXECUTE(@SQLQuery)
