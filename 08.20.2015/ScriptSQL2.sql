CREATE PROCEDURE  [Scheduler].[UpdateAssociateFieldSingle] --'AssociateRate', '60', 10
	-- Add the parameters for the stored procedure here
		 @ColName varchar(500),
		 @ColValue varchar(500),
		 @AssId varchar(100) 
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	--Declare @ColName varchar(max) = 'AssociateRate';
	--Declare @ColValue varchar(max) = '30';
	--Declare @TaskId int  = 56; 
	
	Declare @Table varchar(max) = 'dbo.Associate';		
	--Declare @IndId varchar(100);
   
	 --SELECT TOP(1) @IndId =  IndividualId
	 --FROM Scheduler.IndividualTask
	 --WHERE TaskId = @TaskId 


	IF EXISTS(SELECT * FROM sys.columns 
	WHERE [name] = @ColName AND [object_id] = OBJECT_ID(@table))        
		BEGIN 				
		
		IF @ColValue = '' OR @ColValue IS NULL
		BEGIN
			DECLARE @sql1 nvarchar(max) = 'update dbo.Associate SET ' + @ColName + ' = NULL WHERE ID = ' + @AssId ;
			EXEC sp_executesql @sql1, N'' 			
		END
		ELSE
		BEGIN
			DECLARE @sql2 nvarchar(max) = 'update dbo.Associate SET ' + @ColName + ' = ''' + @ColValue + ''' WHERE IndividualId = ' + @AssId ;
			EXEC sp_executesql @sql2, N'' 			
		END
	END 
END
