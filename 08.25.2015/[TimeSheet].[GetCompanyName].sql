USE [MomentaRecruitment.Phase3.Test]
GO
/****** Object:  StoredProcedure [TimeSheet].[GetCompanyName]    Script Date: 08/25/2015 16:38:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER  PROCEDURE [TimeSheet].[GetCompanyName] 
	@invoiceId INT

	AS
	BEGIN
		DECLARE @BusinessTypeID INT; 
		DECLARE @AssociateId INT; 
		DECLARE @EndTime DATETIME2; 


		Select @AssociateId = AssociateId
		From [TimeSheet].[Invoice]
		where InvoiceId = @invoiceId

		Select top 1 @EndTime = [date]
		From [TimeSheet].[InvoiceHistory]
		where InvoiceId = @invoiceId order by [date]

		SELECT top 1 @BusinessTypeID = CASE Val 
		WHEN 3 THEN 3 ELSE 1 END 
		FROM x_audit.Get_dbo_Associate_Changes(@AssociateId)
		where Col = 'BusinessTypeID'
		and ModifiedTime < @EndTime
		order by ModifiedTime desc;

		IF @BusinessTypeID = 3 
		BEGIN 
		Select UmbrellaCompany.Name
		FROM x_audit.Get_dbo_Associate_Changes(@AssociateId)
		x_ass INNER JOIN UmbrellaCompany ON x_ass.Val = UmbrellaCompany.UmbrellaCompanyId
		where Col = 'UmbrellaCompanyId'
		and ModifiedTime < @EndTime
		order by ModifiedTime desc
		END 
		ELSE 
		Select top 1 Val
		FROM x_audit.Get_dbo_Associate_Changes(@AssociateId)
		where Col = 'RegisteredCompanyName'
		and ModifiedTime < @EndTime
		order by ModifiedTime desc 
	END
