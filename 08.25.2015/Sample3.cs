using System.Linq;

namespace MR_DAL.TimeSheet.Repository
{
    using System;
    using System.Collections.Generic;
    using Enumerations;

    // TODO: consider moving this to the common library
    public interface ITimeSheetRepository
    {
        void AddTimeSheet(TimeSheet timeSheet);

        IEnumerable<UserTimeSheet> GetUserTimeSheet(int userId);

        IEnumerable<TimeSheetEntry> GetTimeSheetEntryByTimeSheetId(int timeSheetId);

        AssignedRole GetTimeSheetEntryValidationDetails(int associateId, int roleId = 0);

        TimeSheet GetTimeSheetById(int timesheetId);

        IEnumerable<AssociateAttendanceOption> GetAttendenceOptions(int associateId);

        IEnumerable<Document> GetTimesheetDocuments(int timesheetId);

        void UpdateTimeSheetEntry(TimeSheetEntry timeSheetEntry);

        TimeSheetEntry GetTimeSheetEntry(int entryId);

        void UpdateStatus(int timesheetId, TimesheetStatus status, string comment = "", int? associateId = null, int? approverAssociateId = null, int? approverClientContactId = null, string timesheetChecker = null);

        IEnumerable<AwaitingApproval> GetAwaitingApproval(int approverAssociateId, int? approverClientContactId = null);

        TimesheetHistory[] GetTimesheetHistory(int timesheetId);

        bool DeleteDocument(Guid documentId, int associateId);

        bool AcceptDocument(Guid documentId, int associateId, string comment, string username);

        bool RejectDocument(Guid documentId, int associateId, string comment, string username);

        void SuspendAssociateTimesheets(int id, bool suspensionStatus);

        bool ValidateEmailsubmission(int timesheetId);
        bool UpdateNotes(int TimeSheetEntryId, string ExpenseColumn, string ExpenseNotes);

        string GetCompanyName(int invoiceId);

        string GetCompanyNameByIndividual(int IndividualId);

        string GetCompanyNameByTimesheet(int timesheetId);

        IEnumerable<TimeSheet> GetTimeSheetsByStatus(TimesheetStatus status);      

        IEnumerable<TimeSheet> GetAssociateTimeSheetsByStatus(int associateId, TimesheetStatus status);

        void CreateCommunicationsHistoryItem(int associateId, CommunicationType type, string description, string loggedInUser, string details);

        IEnumerable<Invoice> GetInvoicesForAssociate(int associateId);

        IEnumerable<InvoiceHistory> GetInvoiceHistory(int invoiceId);

        IEnumerable<GetTimeSheetsForInvoicing_Result> GetTimeSheetsForInvoicing(int associateId);

        IEnumerable<GetCurrentInvoices_Result> GetCurrentInvoices(int associateId);

        IEnumerable<GetInvoicesTimeSheets_Result> GetInvoicedTimeSheets(int associateid);

        int CreateInvoice(int associateId, DateTime start, DateTime end, InvoiceStatus status, string projectName, string roleName, bool selfBilling = false);

        void AddTimeSheetToInvoice(int invoiceId, int timesheetId);

        decimal RetainerDays(int timesheetId);

        void GenerateRetainerNow(int timeSheetId);

        decimal GetAssociateRate(int timesheetId, DateTime currentDate);

        UpComingRetainer GetRetainerTimesheetForInd(int associateId, int roleId);

        void UpdateTimeSheet(TimeSheet timeSheetEntry);

        void CreateInvoiceFile(int invoiceId, string filePath, InvoiceFileStatus status);

        void AddInvoiceHistory(InvoiceHistory invoiceHistory);

        IEnumerable<GetTimeSheetsByInvoiceId_Result> GetTimeSheetsByInvoiceId(int invoiceId);

        GetInvoice_Result GetInvoice(int invoiceId);

        IEnumerable<GetInvoiceReportTimeSheetData_Result> GetInvoicesReportData(int invoiceId);

        IEnumerable<GetInvoiceReportRetainerTimeSheetData_Result> GetInvoiceReportRetainerData(int invoiceId);

        IEnumerable<GetInvoiceFiles_Result> GetInvoiceFiles(int invoiceId);

        void UpdateInvoiceFileStatus(Guid invoiceFileId, MR_DAL.Enumerations.InvoiceFileStatus fileStatus, string statusText);

        IEnumerable<GetInvoiceComments_Result> GetInvoiceComments(int invoiceId);

        void CreateInvoiceComment(int invoiceId, string userName, string comment);

        void ProcessInvoice(int invoiceId, int timesheetStatus, int invoiceStatus, decimal totalAmount, decimal vatAmount);

        GetInvoiceEmailViewModel_Result GetInvoiceEmailViewModel(int invoiceId);

        IEnumerable<TimeSheet> GetTimeSheetsFromList(int[] timesheetIds);

        IEnumerable<TimeSheet> GetUnInvoicedTimeSheetsForAssociateRole(int associateId, int roleId);

        void CreateTimesheetHistory(int timesheetId, TimesheetStatus status, string comment = "", int? associateId = null, int? approverAssociateId = null, int? approverClientContactId = null);

        Document GetDocument(Guid id);

        IEnumerable<InvoiceTimeSheet> GetInvoicedTimesheet(int invoiceId);

        IEnumerable<ExpensesRecieptDetails> GetRecieptInfo(int roleId,  int associateId, int timesheetId=0);

        RetainerEntry GetRetainerEntry(int timesheetId);

        void MarkReatinerEntry(int timesheetId);

        IEnumerable<Absence> GetThisWeekAbsence(int associateId, DateTime startDate);

        IEnumerable<ScheduleRetainer> GenerateScheduleRetainer();

        int GetApproverIdForTimeSheet(int timeSheetId);

        T GetHistoricIndividualValue<T>(int individualId, string field, DateTime date, T defaultValue,IEnumerable<IndividualChanges> indobj=null);

        T GetHistoricAssociateValue<T>(int associateId, string field, DateTime date, T defaultValue, IEnumerable<AssociateChanx> indobj = null);

        IEnumerable<AssociateChanx> GetHistoricAssociateChanges(int associateId);
        IEnumerable<IndividualChanges> GetHistoricIndividualChanges(int individualId);

        TimesheetValidationDetail GetTimeSheetAssignRoleDetail(int timesheetId);

        TimeSheet GetAllTimesheetData(int timeSheetId);

        InvoiceTimeSheet GetInvoiceByTimesheetId(int timeSheetId);
        Associate GetAssociatebyId(int id);

        void UpdateTimesheetStatusWithNoHistory(int p, TimesheetStatus timesheetStatus);

        //IEnumerable<AssociateRateChange> GetAssociateRateChangesForPeriod(GetInvoiceReportTimeSheetData_Result row);

        IQueryable<HoldingBaySearch> GetAllHoldingBayInvoices();

        int UpdateInvoiceStatus(int invoiceId, InvoiceStatus status);

        void UpdateInvoiceStatusAndSageUrn(int invoiceId, int sageUrn, InvoiceStatus status);

        void UpdateInvoicePaymentDate(int invoiceId, string paymentDate);
        
        int? GetInvoiceFileStatus(Guid id);

        System.Data.Objects.ObjectResult<GetIndividual_Result> GetTimeSheetIndividual(int timeSheetId);

        IEnumerable<Invoice> GetInvoicesPendingPayment();

        IEnumerable<TimeSheet> GetUninvoicedWeeklyTimeSheetsForAssociate(int associateId, int roleId, int projectId);        
    }

    public class AssociateRateChange
    {
        public DateTime ModifiedTime { get; set; }

        public decimal AssociateRate { get; set; }
    }
}