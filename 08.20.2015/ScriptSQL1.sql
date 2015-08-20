CREATE PROC [dbo].[BackDateAssociateChanges]
    @associateId int,
    @col varchar(500),
	@ColValue varchar(500),
    @changeDate datetime
AS
    DECLARE @SQLQuery AS NVARCHAR(500)
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
