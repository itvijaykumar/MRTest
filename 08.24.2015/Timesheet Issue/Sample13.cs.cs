using MR_DAL;

namespace AssociatePortal.Services
{
    using AssociatePortal.ViewModel;
    using MomentaRecruitment.Common.Services;
    using MomentaRecruitment.Common.ViewModel;
    using MR_DAL.Enumerations;
    using MR_DAL.TimeSheet;
    using System;
    using System.Collections.Generic;

    public interface ITimesheetService
    {
        TimesheetViewModel GetTimeSheetEntry(int timesheetId);

        DashBoardTimeSheetModel GetDashBoardData(int currentUserId = 0);

        bool SaveTimeSheet(TimesheetViewModel model);

        bool SubmitTimeSheet(TimesheetViewModel model);

        bool ApproveTimesheet(TimesheetViewModel model, bool isClientContact = false);

        bool RejectTimesheet(TimesheetViewModel model, bool isClientContact = false);

        bool ReSubmitTimesheet(TimesheetViewModel model);
        bool UpdateNotes(int TimeSheetEntryId, string ExpenseColumn, string ExpenseNotes);
        bool AssociateRejectTimesheet(TimesheetViewModel model);

        bool DeleteDocument(Guid id, int? associateId = null);

        bool AcceptDocument(Guid id, int associateId);

        bool RejectDocument(int timesheetId, Guid documentId, int associateId, string comment);

        bool SuspendTimesheet(int id, bool toBoolean, string comment);

        int SendBlankOrUpdatedReminderEmail(string action);
        int SendBlankOrUpdatedReminderEmailExtended(string action,int weeks);

        int SendApproverReminderEmail();

        int SendPaymentDateReminderEmail();

        void CreateCommunicationsHistoryItem(int associateId, CommunicationType type, string description, string loggedInUser, string details);

        IEnumerable<TimeSheetsGridViewModel> GetTimeSheetsForInvoicing(int associateId);

        IEnumerable<TimeSheetsGridViewModel> GetInvoicedTimeSheets(int associateId);

        IEnumerable<InvoiceGridViewModel> GetCurrentInvoices(int associateId);

        int CreateInvoice(int associateId, int[] timesheetIds, decimal vatRate, bool selfBilling = false);

        void CreateInvoiceFile(int invoiceId, string filePath, InvoiceFileStatus status);

        IEnumerable<TimeSheetsGridViewModel> GetTimeSheetsByInvoiceId(int invoiceId);

        InvoiceGridViewModel GetInvoice(int invoiceId);

        TimeSheetEntry GetEntry(int entryId);

        void UpdateEntry(TimeSheetEntry entry);

        IEnumerable<InvoiceReportViewModel> GetInvoicesReportData(int invoiceId);

        IEnumerable<InvoiceReportRetainerViewModel> GetInvoicesReportRetainerData(int invoiceId);

        IEnumerable<InvoiceFileViewModel> GetInvoiceFiles(int invoiceId, bool includeUserName = true);

        void UpdateInvoiceFileStatus(int associateId, string invoiceFileId, MR_DAL.Enumerations.InvoiceFileStatus fileStatus, int invoiceId, string comment, bool sendEmail,string attachments);        

        IEnumerable<InvoiceCommentViewModel> GetInvoiceComments(int invoiceId);

        string GetCompanyName(int invoiceId);

        void CreateInvoiceComment(int invoiceId, string userName, string comment);

        void ProcessInvoice(int invoiceId, int timesheetStatus, int invoiceStatus, decimal vatRate, bool isRetainer = false);

        bool ValidateInvoiceRetainerTimesheets(int associateid, int[] timesheetIds);

        bool ValidateInvoiceTimeSheets(int associateId, int[] timesheetIds);

        bool MarkAsPending(TimesheetViewModel model);

        bool MarkAsCompleted(TimesheetViewModel model);

        void GenerateScheduleRetainer();

        IEnumerable<InvoiceHistory> GetInvoiceHistory(int invoiceId);

        IEnumerable<IndividualChanges> GetHistoricIndividualChanges(int individualId);
        T GetHistoricIndividualValue<T>(int individualId, string field, DateTime date, T defaultValue, IEnumerable<IndividualChanges> indobj = null);

        IEnumerable<AssociateChanx> GetHistoricAssociateChanges(int associateId);
        T GetHistoricAssociateValue<T>(int associateId, string field, DateTime date, T defaultValue, IEnumerable<AssociateChanx> indobj = null);
        bool SaveTimeSheetWithoutStatus(TimesheetViewModel model);

        bool BilledToClient(string[] timsheetIds);
        
        int? GetInvoiceFileStatus(Guid id);

        void UpdateInvoiceListStatus(int[] ids, InvoiceStatus status);        
    }
}