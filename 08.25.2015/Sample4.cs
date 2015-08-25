namespace MR_DAL.TimeSheet.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Elmah;
    using Enumerations;

    // TODO: consider moving this to the common library
    public class TimeSheetRepository : ITimeSheetRepository
    {
        private readonly object obj = new object();
        private TimeSheetContext context;
        
        public TimeSheetRepository(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException("unitOfWork");
            }

            context = unitOfWork as TimeSheetContext;
        }

        public void AddTimeSheet(TimeSheet timeSheet)
        {
            if (timeSheet == null)
            {
                throw new ArgumentNullException("TimeSheet can not be null");
            }

            context.TimeSheets.AddObject(timeSheet);
        }

        public TimeSheetEntry GetTimeSheetEntry(int entryId)
        {
            return context.TimeSheetEntry.Where(x => x.TimeSheetEntryId == entryId).FirstOrDefault();
        }

        public bool DeleteDocument(Guid documentId, int associateId)
        {
            try
            {
                context.Context.DeleteDocument(documentId, associateId);
                return true;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }

        public IEnumerable<UserTimeSheet> GetUserTimeSheet(int userId)
        {
            if (userId == default(int))
            {
                throw new ArgumentException("Invalid user id");
            }

            return context.Context.FunctionUserTimeSheet(userId).AsEnumerable();
        }

        public TimeSheet GetTimeSheetById(int timesheetId)
        {
            if (timesheetId == default(int))
            {
                throw new ArgumentException("Invalid timesheetId");
            }

            try
            {
                return context.TimeSheets.Where(x => x.TimeSheetId == timesheetId).AsEnumerable().FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to retrieve timesheet", ex);
            }
        }

        public IEnumerable<TimeSheetEntry> GetTimeSheetEntryByTimeSheetId(int timeSheetId)
        {
            if (timeSheetId == default(int))
            {
                throw new ArgumentException("Invalid timeSheetId");
            }

            return context.TimeSheetEntry.Where(x => x.TimeSheetId == timeSheetId).AsEnumerable();
        }

        public IEnumerable<Document> GetTimesheetDocuments(int timeSheetId)
        {
            if (timeSheetId == default(int))
            {
                throw new ArgumentException("Invalid timeSheetId");
            }

            return context.Documents.Where(x => x.TimeSheetId == timeSheetId).AsEnumerable();
        }

        public AssignedRole GetTimeSheetEntryValidationDetails(int associateId, int roleId = 0)
        {
            if (associateId == default(int))
            {
                throw new ArgumentException("Invalid associateId");
            }

            if (roleId == default(int))
            {
                return context.Context.AssignedRole.Where((x => x.AssociateId == associateId)).AsEnumerable().FirstOrDefault();
            }

            return context.Context.AssignedRole.Where((x => x.AssociateId == associateId && x.RoleId == roleId)).OrderBy(y => y.AbsentFrom).AsEnumerable().FirstOrDefault();
        }

        public IEnumerable<AssociateAttendanceOption> GetAttendenceOptions(int associateId)
        {
            if (associateId == default(int))
            {
                throw new ArgumentException("Invalid associateId");
            }

            return context.Context.AssociateAttendanceOption(associateId).AsEnumerable();
        }

        public decimal GetAssociateRate(int timesheetId, DateTime currentDate)
        {
            var result = context.Context.GetAssociateRate(timesheetId, currentDate); 
            decimal rate = 0;
            var associateRateData = result.FirstOrDefault();
            if (result != null && associateRateData.HasValue)
            {
                rate = associateRateData.Value;
            }
            return rate;
        }

        public void UpdateTimeSheetEntry(TimeSheetEntry timeSheetEntry)
        {
            if (timeSheetEntry == null)
            {
                throw new ArgumentNullException("timeSheetEntry can not be null");
            }

            if(timeSheetEntry.TimeSheetEntryId > 0)
            {
                if (timeSheetEntry.EntityState == EntityState.Detached)
                    context.TimeSheetEntry.Attach(timeSheetEntry);
               //context.TimeSheetEntry.AddObject(timeSheetEntry);
                context.TimeSheetEntry.Context.ObjectStateManager.ChangeObjectState(timeSheetEntry, EntityState.Modified);            
            }
            else
            {
                context.TimeSheetEntry.AddObject(timeSheetEntry);
                context.TimeSheetEntry.Context.ObjectStateManager.ChangeObjectState(timeSheetEntry, EntityState.Added);            
            }
            context.SaveChanges();
        }

        public void UpdateTimeSheet(TimeSheet timeSheet)
        {
            if (timeSheet == null)
            {
                throw new ArgumentNullException("timeSheet can not be null");
            }

            var ts = context.TimeSheets.Where(x => x.TimeSheetId == timeSheet.TimeSheetId).First();
            ts.IncentiveDay = timeSheet.IncentiveDay;

            // context.TimeSheets.AddObject(ts);
            context.TimeSheets.Context.ObjectStateManager.ChangeObjectState(ts, EntityState.Modified);
            context.SaveChanges();
        }

        public void UpdateStatus(int timesheetId, TimesheetStatus status, string comment = "", int? associateId = null, int? approverAssociateId = null, int? approverClientContactId = null, string timesheetChecker = null)
        {
            if (timesheetId == default(int))
            {
                throw new ArgumentNullException("invalid timesheetId");
            }

            context.Context.UpdateTimeSheetStatus(timesheetId, (int)status, comment, associateId, approverAssociateId, approverClientContactId, timesheetChecker);
        }

        public IEnumerable<AwaitingApproval> GetAwaitingApproval(int approverAssociateId, int? approverClientContactId = null)
        {
            if (approverAssociateId == default(int))
            {
                throw new ArgumentException("Invalid Approver Associate Id");
            }

            return context.Context.GetAwaitingApproval(approverAssociateId, approverClientContactId).AsEnumerable();
        }

        public TimesheetHistory[] GetTimesheetHistory(int timesheetId)
        {
            if (timesheetId == default(int))
            {
                throw new ArgumentException("Invalid timesheet Id");
            }
                
            return context.Context.TimesheetHistories(timesheetId).ToArray();
        }

        public string GetCompanyNameByIndividual(int individualId)
        {
            return context.Context.GetCompanyNameByIndividual(individualId).FirstOrDefault();
        }

        public string GetCompanyNameByTimesheet(int timesheetId)
        {
            return context.Context.GetCompanyNameByTimesheet(timesheetId).FirstOrDefault();
        }
        public string GetCompanyName(int invoiceId)
        {
            return context.Context.GetCompanyName(invoiceId).FirstOrDefault();
        }

        public System.Data.Objects.ObjectResult<GetIndividual_Result> GetTimeSheetIndividual(int timeSheetId)
        {
            return context.Context.GetIndividual(timeSheetId);
        }

        public void SuspendAssociateTimesheets(int id, bool suspensionStatus)
        {
            if (id == default(int))
            {
                throw new ArgumentException("Invalid user Id");
            }

            context.Context.SuspendAssociateTimesheets(id, suspensionStatus);
        }

        public bool ValidateEmailsubmission(int timesheetId)
        {
            if (timesheetId == default(int))
            {
                throw new ArgumentException("Invalid associate Id");
            }

            var approverUnapprovedCount = this.context.Context.GetUnapprovedCountForApprover(timesheetId).FirstOrDefault().GetValueOrDefault();

            return approverUnapprovedCount > 0;
        }
        public bool UpdateNotes(int TimeSheetEntryId, string ExpenseColumn, string ExpenseNotes)
        {
            bool status=false;

            var result = this.context.Context.UpdateNotes(TimeSheetEntryId, ExpenseColumn, ExpenseNotes);

            if (result > 0)
                status = true;

            return status;

        }
        public bool AcceptDocument(Guid documentId, int associateId, string comment, string username)
        {
            try
            {
                context.Context.AcceptDocument(documentId, associateId, comment, username);
                return true;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }

        public bool RejectDocument(Guid documentId, int associateId, string comment, string username)
        {
            try
            {
                context.Context.RejectDocument(documentId, associateId, comment, username);
                return true;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }

        public IEnumerable<TimeSheet> GetTimeSheetsByStatus(TimesheetStatus status)
        {
            return context.Context.TimeSheets.Where(t => t.TimeSheetStatusId == (byte)status).ToList<TimeSheet>();
        }
       
        public IEnumerable<TimeSheet> GetUninvoicedWeeklyTimeSheetsForAssociate(int associateId, int roleId, int projectId)
        {
            return
                context.Context.TimeSheets.Where(
                    t =>
                        t.TimeSheetStatusId < (byte) TimesheetStatus.Pending && t.RoleId == roleId &&
                        t.AssociateId == associateId && t.ProjectId == projectId).ToList<TimeSheet>();
        }


        public IEnumerable<TimeSheet> GetAssociateTimeSheetsByStatus(int associateId, TimesheetStatus status)
        {
            return context.Context.TimeSheets.Where(t => t.AssociateId == associateId && t.TimeSheetStatusId == (byte)status).ToList<TimeSheet>();
        }

        public void CreateCommunicationsHistoryItem(int associateId, CommunicationType type, string description, string loggedInUser, string details)
        {
            context.Context.CreateCommunicationsHistoryItem(associateId, (byte)type, description, loggedInUser, details);
        }

        public RetainerEntry GetRetainerEntry(int timesheetId)
        {
            if (timesheetId == default(int))
            {
                throw new ArgumentException("Invalid timesheetId");
            }

            return context.RetainerEntry.Where(entry => entry.TimeSheetId == timesheetId).FirstOrDefault();
        }

        public decimal RetainerDays(int timesheetId)
        {
            if (timesheetId == default(int))
            {
                throw new ArgumentException("Invalid timesheetId");
            }

            return context.Context.RetainerDays(timesheetId).First().HasValue ? (int)context.Context.RetainerDays(timesheetId).First() : 0;
        }

        public void GenerateRetainerNow(int timeSheetId)
        {
            if (timeSheetId == default(int))
            {
                throw new ArgumentException("Invalid timesheetId");
            }

            var timesheet = context.TimeSheets.Where(ts => ts.TimeSheetId == timeSheetId).FirstOrDefault();
            timesheet.EndDate = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            context.TimeSheets.Context.ObjectStateManager.ChangeObjectState(timesheet, EntityState.Modified);
            context.SaveChanges();
        }

        public UpComingRetainer GetRetainerTimesheetForInd(int associateId, int roleId)
        {
            if (associateId == default(int))
            {
                throw new ArgumentException("Invalid associateId");
            }

            return context.Context.UpComingRetainer(associateId, roleId).FirstOrDefault();
        }

        public IEnumerable<Invoice> GetInvoicesForAssociate(int associateId)
        {
            return context.Context.Invoice.Where(i => i.AssociateId == associateId);
        }

        public IEnumerable<InvoiceHistory> GetInvoiceHistory(int invoiceId)
        {
            return context.Context.InvoiceHistories.Where(h => h.InvoiceId == invoiceId).OrderByDescending(h => h.Date);
        }

        public void AddInvoiceHistory(InvoiceHistory invoiceHistory)
        {
            if (invoiceHistory == null)
            {
                throw new ArgumentNullException("Invoice History can not be null");
            }

            context.Context.CreateInvoiceHistory(
                invoiceHistory.InvoiceId,
                invoiceHistory.InvoiceStatusId,
                invoiceHistory.AssociateId,
                invoiceHistory.LoggedInUser,
                invoiceHistory.Comments);
        }

        public IEnumerable<GetTimeSheetsForInvoicing_Result> GetTimeSheetsForInvoicing(int associateId)
        {
            return context.Context.GetTimeSheetsForInvoicing(associateId);
        }

        public IEnumerable<GetCurrentInvoices_Result> GetCurrentInvoices(int associateId)
        {
            return context.Context.GetCurrentInvoices(associateId);
        }

        public IEnumerable<GetInvoicesTimeSheets_Result> GetInvoicedTimeSheets(int associateid)
        {
            return context.Context.GetInvoicesTimeSheets(associateid);
        }

        public int CreateInvoice(int associateId, DateTime start, DateTime end, InvoiceStatus status, string projectName, string roleName, bool selfBilling = false)
        {
            DateTime? paymentDate = null;
            if (selfBilling)
            {                
                paymentDate = PaymentDateRules(DateTime.Now.AddDays(45));
            }
            int invoiceId = (int) context.Context.CreateInvoice(
                associateId,
                start,
                end,
                (byte) status,
                projectName,
                roleName,
                selfBilling,
                paymentDate
                ).FirstOrDefault();

            return invoiceId;
        }

        public void AddTimeSheetToInvoice(int invoiceId, int timesheetId)
        {
            context.Context.AddTimeSheetToInvoice(invoiceId, timesheetId);
        }

        public void CreateInvoiceFile(int invoiceId, string filePath, InvoiceFileStatus status)
        {
            context.Context.CreateInvoiceFile(invoiceId, filePath, (byte)status);
        }

        public IEnumerable<GetTimeSheetsByInvoiceId_Result> GetTimeSheetsByInvoiceId(int invoiceId)
        {
            return context.Context.GetTimeSheetsByInvoiceId(invoiceId);
        }

        public GetInvoice_Result GetInvoice(int invoiceId)
        {
            return context.Context.GetInvoice(invoiceId).FirstOrDefault();
        }

        public IEnumerable<GetInvoiceReportTimeSheetData_Result> GetInvoicesReportData(int invoiceId)
        {
            return context.Context.GetInvoiceReportTimeSheetData(invoiceId);
        }

        public IEnumerable<GetInvoiceReportRetainerTimeSheetData_Result> GetInvoiceReportRetainerData(int invoiceId)
        {
            return context.Context.GetInvoiceReportRetainerTimeSheetData(invoiceId);
        }

        public IEnumerable<GetInvoiceFiles_Result> GetInvoiceFiles(int invoiceId)
        {
            return context.Context.GetInvoiceFiles(invoiceId);
        }

        public void UpdateInvoiceFileStatus(Guid invoiceFileId, MR_DAL.Enumerations.InvoiceFileStatus fileStatus, string statusText)
        {
            context.Context.UpdateInvoiceFileStatus(invoiceFileId, (byte)fileStatus, statusText);
        }

        public IEnumerable<GetInvoiceComments_Result> GetInvoiceComments(int invoiceId)
        {
            return context.Context.GetInvoiceComments(invoiceId);
        }

        public void CreateInvoiceComment(int invoiceId, string userName, string comment)
        {
            context.Context.CreateInvoiceComment(invoiceId, comment, userName);
        }

        public void ProcessInvoice(int invoiceId, int timesheetStatus, int invoiceStatus, decimal totalAmount, decimal vatAmount)
        {
            context.Context.ProcessInvoice(invoiceId, timesheetStatus, invoiceStatus, totalAmount, vatAmount);
        }

        public GetInvoiceEmailViewModel_Result GetInvoiceEmailViewModel(int invoiceId)
        {
            return context.Context.GetInvoiceEmailViewModel(invoiceId).FirstOrDefault();
        }

        public IEnumerable<TimeSheet> GetTimeSheetsFromList(int[] timesheetIds)
        {
            return context.Context.TimeSheets.Where(t => timesheetIds.Contains(t.TimeSheetId));
        }

        public IEnumerable<TimeSheet> GetUnInvoicedTimeSheetsForAssociateRole(int associateId, int roleId)
        {
            return context.Context.TimeSheets.Where(t => t.AssociateId == associateId
                                                      && t.RoleId == roleId
                                                      && t.TimeSheetStatusId < 5);
        }

        public void CreateTimesheetHistory(int timesheetId, TimesheetStatus status, string comment = "", int? associateId = null, int? approverAssociateId = null, int? approverClientContactId = null)
        {
            if (timesheetId == default(int))
            {
                throw new ArgumentNullException("invalid timesheetId");
            }

            context.Context.CreateTimesheetHistory(timesheetId, (int)status, comment, associateId, approverAssociateId, approverClientContactId);
        }

        public Document GetDocument(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException("invalid documentId");
            }

            return context.Documents.FirstOrDefault(x => x.DocumentId == id);
        }

        public IEnumerable<InvoiceTimeSheet> GetInvoicedTimesheet(int invoiceId)
        {
            if (invoiceId == 0)
            {
                throw new ArgumentNullException("invalid invoiceId");
            }

            return context.Context.InvoiceTimeSheet.Where(x => x.InvoiceId == invoiceId).AsEnumerable();
        }

        public IEnumerable<ExpensesRecieptDetails> GetRecieptInfo(int roleId, int associateId, int timesheetId=0)
        {
            if (roleId == 0)
            {
                throw new ArgumentNullException("invalid roleId");
            }

            var result = context.Context.GetExpensesReciept(roleId, associateId, timesheetId).AsEnumerable();
            return result;
        }

        public void MarkReatinerEntry(int timesheetId)
        {
            if (timesheetId == 0)
            {
                throw new ArgumentNullException("invalid timesheetId");
            }

            var timesheet = context.RetainerEntry.Where(ts => ts.TimeSheetId == timesheetId).FirstOrDefault();

            timesheet.RetainerGenerationDate = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            context.RetainerEntry.Context.ObjectStateManager.ChangeObjectState(timesheet, EntityState.Modified);
            context.SaveChanges();
        }

        public IEnumerable<Absence> GetThisWeekAbsence(int associateId, DateTime startDate)
        {
            if (associateId == 0)
            {
                throw new ArgumentNullException("invalid associateId");
            }

            var thisSun = startDate.AddDays(6).Date;
            IEnumerable<Absence> result = null;
            lock (obj)
            {
                result = context.Absence.Where(x => x.AssociateId == associateId
                                                && x.AbsentFrom < thisSun
                                                && x.AnticipatedReturn > startDate
                                                && x.AbsenceStatusId != 3).AsEnumerable();
            }

            return result;
        }

        public IEnumerable<ScheduleRetainer> GenerateScheduleRetainer()
        {
            return this.context.Context.GenerateScheduleRetainer().AsEnumerable();
        }

        public TimesheetValidationDetail GetTimeSheetAssignRoleDetail(int timesheetId)
        {
            return this.context.Context.GetValidationDetailsForTimesheet1(timesheetId).First();
        }

        public int GetApproverIdForTimeSheet(int timeSheetId)
        {
            try
            {
                return context.Context.GetApproverIdForTimeSheet(timeSheetId).First().Value;
            }
            catch
            {
                return 0;
            }
        }

        public T GetHistoricAssociateValue<T>(int associateId, string field, DateTime date, T defaultValue, IEnumerable<AssociateChanx> indobj = null)
        {
            try
            {
                DateTime targetDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 0);

                var changes = (from i in indobj select i);
                if (indobj == null)
                {
                    changes = (from i in context.Context.AssociateChanges
                                   where i.ID == associateId
                                   && i.ModifiedTime < targetDate
                                   && (i.Action == "U" || i.Action == "I")
                                   select i);
                }
                else
                {
                    changes = (from i in indobj
                               where i.ID == associateId
                               && i.ModifiedTime < targetDate
                               && (i.Action == "U" || i.Action == "I")
                               select i);
                }

                if (changes == null || changes.Count() == 0)
                {
                    //// no changes found, return default value
                    return defaultValue;
                }
                else
                {
                    var change = changes.OrderByDescending(i => i.ModifiedTime).First();

                    object obj;

                    switch (change.DataType)
                    {
                        case "int":
                            if (defaultValue is decimal)
                            {
                                obj = decimal.Parse(change.Value);
                            }
                            else
                            {
                                obj = int.Parse(change.Value);
                            }
                            break;
                        case "bit":
                            obj = bool.Parse(change.Value);
                            break;
                        case "date":
                            obj = DateTime.Parse(change.Value);
                            break;
                        case "numeric":
                            obj = decimal.Parse(change.Value);
                            break;
                        default:
                            obj = change.Value;
                            break;
                    }

                    return (T)obj;
                }
            }
            catch (Exception)
            {
                // error retrieving historic value, return default
                return defaultValue;
            }
        }
        public T GetHistoricIndividualValue<T>(int individualId, string field, DateTime date, T defaultValue, IEnumerable<IndividualChanges> indobj=null)
        {
            try
            {
                DateTime targetDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 0);

                var changes = (from i in indobj select i);
                if (indobj == null)
                {
                    changes = (from i in context.Context.IndividualChanges
                               where i.IndividualId == individualId
                               && i.ModifiedTime < targetDate
                               && (i.Action == "U" || i.Action == "I")
                               select i);
                }
                else
                {
                    changes = (from i in indobj  
                               where i.IndividualId == individualId
                               && i.ModifiedTime < targetDate
                               && (i.Action == "U" || i.Action == "I")
                               select i);
                }

                if (changes == null || changes.Count() == 0)
                {
                    //// no changes found, return default value
                    return defaultValue;
                }
                else
                {
                    var change = changes.OrderByDescending(i => i.ModifiedTime).First();

                    object obj;

                    switch (change.DataType)
                    {
                        case "int":
                            if (defaultValue is decimal)
                            {
                                obj = decimal.Parse(change.Value);
                            }
                            else
                            {
                                obj = int.Parse(change.Value);
                            }
                            break;
                        case "bit":
                            obj = bool.Parse(change.Value);
                            break;
                        case "date":
                            obj = DateTime.Parse(change.Value);
                            break;
                        case "numeric":
                            obj = decimal.Parse(change.Value);
                            break;
                        default:
                            obj = change.Value;
                            break;
                    }

                    return (T)obj;
                }
            }
            catch (Exception)
            {
                // error retrieving historic value, return default
                return defaultValue;
            }
        }
        public IEnumerable<IndividualChanges> GetHistoricIndividualChanges(int individualId)
        {
            try
            {
                return context.Context.IndividualChanges.Where(i=>i.IndividualId==individualId);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<AssociateChanx> GetHistoricAssociateChanges(int associateId)
        {
            try
            {
                return context.Context.AssociateChanges.Where(i => i.ID == associateId);
            }
            catch
            {
                return null;
            }
        }
        public TimeSheet GetAllTimesheetData(int timeSheetId)
        {
            TimeSheet timesheetdata = null;

            lock (obj)
            {
                timesheetdata = context.TimeSheets
               .Include("Document")
               .Include("TimeSheetEntries")
               .Include("TimeSheetHistory")
               .Where(x => x.TimeSheetId == timeSheetId).AsEnumerable().FirstOrDefault();
            }

            return timesheetdata;
        }

        public InvoiceTimeSheet GetInvoiceByTimesheetId(int timeSheetId)
        {
            InvoiceTimeSheet invoicedata = null;

            lock (obj)
            {
                invoicedata = context.Context.InvoiceTimeSheet
               .Where(x => x.TimeSheetId == timeSheetId).AsEnumerable().FirstOrDefault();
            }

            return invoicedata;
        }
        public Associate GetAssociatebyId(int id)
        {
            Associate result = null;
            lock (obj)
            {
                result = context.Associate.FirstOrDefault(x => x.ID == id);
            }

            return result;
        }

        public void UpdateTimesheetStatusWithNoHistory(int timesheetId, TimesheetStatus timesheetStatus)
        {
            var timesheet = context.TimeSheets.FirstOrDefault(x => x.TimeSheetId == timesheetId);
            timesheet.TimeSheetStatusId = (byte)timesheetStatus;
            context.TimeSheets.Context.ObjectStateManager.ChangeObjectState(timesheet, EntityState.Modified);
            context.SaveChanges();
        }

        //public IEnumerable<AssociateRateChange> GetAssociateRateChangesForPeriod(GetInvoiceReportTimeSheetData_Result row)
        //{
        //    var results = context.Context.GetAssociateRateChangesForPeriod(row.IndividualId, row.StartDate.Value.AddDays(1), row.EndDate);
        //    return from r in results
        //           select new AssociateRateChange 
        //           {
        //               AssociateRate = (decimal)r.AssociateRate,
        //               ModifiedTime = (DateTime)r.ModifiedTime
        //           };
        //}

        public IQueryable<HoldingBaySearch> GetAllHoldingBayInvoices()
        {      
            return context.Context.HoldingBaySearches;           
        }

        public int UpdateInvoiceStatus(int invoiceId, InvoiceStatus status)
        {
            context.Context.UpdateInvoiceStatus(invoiceId, (byte)status);
            var invoice = context.Context.Invoice.FirstOrDefault(i => i.InvoiceId == invoiceId);
            return invoice != null ? Convert.ToInt32(invoice.SelfBillingInvoiceStatusId) : 0;
        }

        public void UpdateInvoiceStatusAndSageUrn(int invoiceId, int sageUrn, InvoiceStatus status)
        {
            context.Context.UpdateInvoiceStatusAndSageUrn(invoiceId, sageUrn, (byte)status);
        }

        public void UpdateInvoicePaymentDate(int invoiceId, string paymentDateString)
        {
            var paymentDateStringArray = paymentDateString.Split('/');

            var paymentDate = new DateTime(Convert.ToInt32(paymentDateStringArray[2]),
                Convert.ToInt32(paymentDateStringArray[1]), Convert.ToInt32(paymentDateStringArray[0]));
            context.Context.UpdateInvoicePaymentDate(invoiceId, paymentDate);
        }

        public int? GetInvoiceFileStatus(Guid id)
        {
            var invoiveFile = context.Context.GetInvoiceFile(id).FirstOrDefault();
            if (invoiveFile == null) return null;
            return invoiveFile.InvoiceFileStatusId;          
        }

        public IEnumerable<Invoice> GetInvoicesPendingPayment()
        {
            var cutOffDate = DateTime.Now.AddDays(10);
            return context.Context.Invoice.Where(
                i => i.InvoiceStatusId == (int)InvoiceStatus.Processed 
                && i.PaymentDate < cutOffDate).ToList<Invoice>();
        }
      
        private static DateTime PaymentDateRules(DateTime paymentDate)
        {
            switch (paymentDate.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                case DayOfWeek.Tuesday:
                    paymentDate = paymentDate.AddDays(-1);
                    break;
                case DayOfWeek.Sunday:
                case DayOfWeek.Wednesday:
                    paymentDate = paymentDate.AddDays(-2);
                    break;
            }
            return paymentDate;
        }
    }
}
