USE [MomentaRecruitment.Phase3.Test]
GO
/****** Object:  StoredProcedure [TimeSheet].[GetExpensesReciept]    Script Date: 08/24/2015 22:34:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================

ALTER PROCEDURE [TimeSheet].[GetExpensesReciept]
    -- Add the parameters for the stored procedure here
    @RoleId INT,
    @AssociateId INT,
    @TimesheetId INT=0
AS
BEGIN
    -- SET NOCOUNT ON added to prevent extra result sets from
    -- interfering with SELECT statements.
    SET NOCOUNT ON;

        --SELECT 
        --@RoleId as RoleId,
        --ext.Description AS Expense, 
        --CASE 
        --	WHEN  Receipt = 1 
        --	THEN 1 
        --	ELSE 0 END AS Receipt
        --FROM ROLE Ro
        --	INNER JOIN RoleExpenseOption roe
        --	ON ro.RoleId = roe.RoleId
        --	INNER  JOIN ExpenseOption Exo
        --	ON  Exo.ExpenseOptionId = roe.ExpenseOptionId
        --	RIGHT OUTER  JOIN ExpenseType ext
        --	ON ext.ExpenseTypeId = Exo.ExpenseTypeId
        --	AND ro.RoleId = @RoleId
        --Order by ext.[Order]  
                   
        ;with roleInfo as(
        SELECT 
        @RoleId as RoleId,
        ext.Description AS Expense, 
        CASE 
            WHEN  Receipt = 1 
            THEN 1 
            ELSE 0 END AS Receipt			, 
            ext.[Order] as expOrder
        FROM dbo.Role Ro
            INNER JOIN RoleExpenseOption roe
            ON ro.RoleId = roe.RoleId
            INNER  JOIN ExpenseOption Exo
            ON  Exo.ExpenseOptionId = roe.ExpenseOptionId
			right outer JOIN ExpenseType ext
            ON ext.ExpenseTypeId = Exo.ExpenseTypeId
            AND ro.RoleId = @RoleId)
            
            
            SELECT 
            r.RoleId,
            R.Expense,
            ISNULL(i.Receipt, r.Receipt) AS Receipt
            FROM  roleInfo r
            INNER JOIN (		
            SELECT RoleId, Expense, Receipt
            FROM Individual	
            CROSS APPLY(			
            SELECT  'Accommodation',ExpenseAccomodationReceipt  
            UNION ALL
            SELECT  'Meal Allowance',  ExpenseSubsistenceReceipt
            UNION ALL
            SELECT  'Travel',ExpenseTravelReceipt 			
            UNION ALL			
            SELECT  'Parking',ExpenseParkingReceipt 			
            UNION ALL			
            SELECT  'Mileage (rate per mile)',ExpenseMileageReceipt AS df 			
            UNION ALL			
            SELECT  'Other (add note)',ExpenseOtherReceipt 
            )  ind(Expense,Receipt)	
            WHERE RoleId =@RoleId AND AssociateId = @AssociateId) i 
            ON r.RoleId =  i.RoleId
            AND r.Expense = i.Expense
            ORDER BY r.expOrder

END
