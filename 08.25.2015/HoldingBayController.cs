
using System.Configuration;
using System.Drawing;
using System.Runtime.Remoting.Messaging;
using Admin.Helpers;
using System.IO;
using noocyte.Utility.ExcelIO;
using MR_DAL.TimeSheet.Repository;
using AssociatePortal.ViewModel;

namespace Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using Admin.Models;
    using Admin.Services;
    using AssociatePortal.Services;
    using MomentaRecruitment.Common.Helpers;
    using MR_DAL;
    using MR_DAL.TimeSheet;
    using Admin.Properties;

    public class HoldingBayController : Controller
    {
        private IFinanceService financeService;
        private ITimesheetService timesheetService;
        private IRoleService roleService;
        private IAssociateService associateService;
        private IEmailService emailService;
        private decimal vatRate;

        public HoldingBayController(
            IFinanceService financeService, 
            ITimesheetService timesheetService, 
            IRoleService roleService, 
            IAssociateService associateService,
            IEmailService emailService)
        {
            this.financeService = financeService;
            this.timesheetService = timesheetService;
            this.roleService = roleService;
            this.associateService = associateService;
            this.emailService = emailService;
            this.vatRate = Convert.ToDecimal(ConfigurationManager.AppSettings["VatRate"]);
        }

        //public ActionResult Test(string id)
        //{
        //    this.financeService.TestPdfGenerator(id);
        //    return null;
        //}

        //// GET: /HoldingBay/
        public ActionResult Index()
        {
            
            ViewBag.VatRate = Convert.ToDecimal(ConfigurationManager.AppSettings["VatRate"]);

            this.SetUploader();
            return View(new HoldingBayModel()
            {
                PaymentDate = DateTime.Now
            });
        }

        public ActionResult HandleUploadedTimesheetDocument(string fileGuid)
        {
            return UploadHelper.HandleUploadedDocument(fileGuid);
        }

     //  [OverrideAuthorize(Roles = RoleName.TimesheetActions)]
        public HttpStatusCodeResult GenerateHoldingBayInvoices()
        {
            try
            {
                // Create an invoice for any Retainer invoices
                AddRetainerInvoices();

                // Create invoice based on an associates and 4 approved weekly invoices for the their role in a given project
                AddWeeklyAndAdjustmentInvoices();

                AddOnlyAdjustmentInvoices();

                return new HttpStatusCodeResult(200, "Success");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }      

        [HttpPost]
        public JsonResult SetInvoicesToHold(string reason, string invoices, string associates, string status)
        {
            financeService.SetInvoicesToHold(reason, invoices.Split(','), associates.Split(','));

            this.financeService.SendInvoicesChangeEmail(reason, associates.Split(','), invoices.Split(','), status.Split(','));

            return Json("invoices held");
        }        

         [HttpPost]
        public JsonResult SetInvoicesPaymentDate(string reason, string paymentDate, string invoices, string associates)
         {
            paymentDate = PaymentDate.PaymentDateRules(paymentDate);
            financeService.SetInvoicesPaymentDate(reason, paymentDate, invoices.Split(','), associates.Split(','));
            this.financeService.SendInvoicesChangeEmail(reason, associates.Split(','), invoices.Split(','), null, paymentDate);
            return Json("payment date set");
        }
      
        [HttpPost]
        public JsonResult Invoices(int pageSize, int skip, List<SortDescription> sort)
        {           
            return GetInvoices(pageSize, skip, sort);
        }

        protected void SetUploader()
        {
            this.ViewBag.UploaderHtml = UploadHelper.GetHtml(
                "uploadButton",
                MomentaRecruitment.Common.Properties.Settings.Default.AllowedFileExtensions,
                "~/UploadHandler.axd?Mode=Invoice",
                "documentUploader",
                "Browse");
        }

        private JsonResult GetInvoices(int pageSize, int skip, List<SortDescription> sort)
        {
            int? resultsCount;
           
            var invoices = this.financeService.GetHoldingBayInvoices(
                pageSize,
                skip,
                sort,
                out resultsCount);

            var result = from i in invoices
                select new
                {
                    Checked = false,
                    AssociateId = i.AssociateId,
                    InvoiceId = i.InvoiceId,
                    BusinessUnit = i.BusinessUnit,
                    ProjectName = i.ProjectName,
                    Name = i.Name,
                    CompanyName = i.CompanyName,
                    ReportDate = i.ReportDateFormatted,
                    InvoicereportNumber = i.InvoicereportNumber,
                    Status = i.Status == "OnHold" ? "On Hold" : i.Status,
                    PaymentDate = i.PaymentDateFormatted,
                    TimesheetStatus=i.TimeSheetStatus,
                    HasUpload=i.HasUpload
                    //// NetPayment = this.timesheetService.GetInvoicesReportData(i.InvoiceId)
                };

            return this.Json(new { total = resultsCount, data = result });
        }        

        private int[] AddAdjustmentTimeSheets(int uniqueAssociate, int[] associateTimeSheetArray)
        {
            var timesheets = this.financeService.GetApprovedAdjustmentTimeSheetsByAssociateId(uniqueAssociate);
            var timeSheetList = timesheets as IList<TimeSheet> ?? timesheets.ToList();

            if (!timeSheetList.Any())
            {
                return associateTimeSheetArray;
            }

            var associateTimeSheetList = associateTimeSheetArray.ToList();
            associateTimeSheetList.AddRange(timeSheetList.Select(t => t.TimeSheetId).Distinct().ToList());
            return associateTimeSheetList.ToArray();
        }

        private bool ValidateRoleTimeSheets(ref IList<TimeSheet> roleTimesheets)
        {
            var roleExpires = false;
            // check if role expires in a month
            var role = this.roleService.GetRole(roleTimesheets[0].RoleId);

            // check if timesheets are an entire batch of 4 XXX0000 would fail, XXXX0000 would pass XXXXX would pass XXX0X would fail
           var uninvoicedTimeSheetsAll = this.financeService.GetUninvoicedWeeklyTimeSheetsForAssociate(roleTimesheets[0].AssociateId, roleTimesheets[0].RoleId, roleTimesheets[0].ProjectId);
           var uninvoicedTimeSheets = uninvoicedTimeSheetsAll.Where(t => t.TimeSheetTypeId.Equals((int)TimesheetTypeId.Weekly)).ToList<TimeSheet>();
           //var uninvoicedTimeSheets = uninvoicedTimeSheets1.Where(p => p.TimeSheetStatusId.Equals(TimesheetStatus.Approved));
            // lets go through these in multiples of 4 and see if we can get four approved in a row but starting from position 0,4,8,etc. If so process them
            var existingTimesheets = new List<TimeSheet>();
            var currentBatchCounter = 0;
            var currentNotApprovedCounter = 0;

            existingTimesheets = uninvoicedTimeSheets.Where(i => i.TimeSheetStatusId == (int)TimesheetStatus.Approved).ToList();
            currentNotApprovedCounter = uninvoicedTimeSheets.Count() - existingTimesheets.Count;
            roleTimesheets = existingTimesheets;

            if (roleTimesheets.Count == 0) return false;

            if (DateTime.Compare(DateTime.Now.AddDays(28), role.EndDate.GetValueOrDefault()) > 0)
            {
                roleExpires = true;
            }
          
            // if existing timesheets exist but are multiples of 4 thats ok unless role has expired then any expired is not ok
            if (roleExpires && roleTimesheets.Count > 0 && roleTimesheets.Count < 4)
            {
                if (currentNotApprovedCounter == 0)
                    return true;
                else
                    return false;
            } 

            else if (roleExpires == false && roleTimesheets.Count > 0 && roleTimesheets.Count<4)
            {
                return false;
            }

            var orderedUnInvTimeSheets = uninvoicedTimeSheets.OrderBy(o => o.StartDate).ToList<TimeSheet>();

            for (int intRow = 0; intRow < orderedUnInvTimeSheets.Count(); intRow = intRow + 4)
            {
                var firstStartDate = orderedUnInvTimeSheets[intRow].StartDate;
                var lastStartDate = orderedUnInvTimeSheets[intRow].StartDate; 
                if ((intRow+3) > (orderedUnInvTimeSheets.Count()-1))
                    lastStartDate = orderedUnInvTimeSheets[orderedUnInvTimeSheets.Count()-1].StartDate;
                else
                    lastStartDate = orderedUnInvTimeSheets[intRow + 3].StartDate;
                var approvedTimesheetsPacket = uninvoicedTimeSheets.Where(o => o.StartDate >= firstStartDate && o.StartDate <= lastStartDate && o.TimeSheetStatusId == (int)TimesheetStatus.Approved).ToList<TimeSheet>();
                var currentWeekTimesheetPacket = uninvoicedTimeSheets.Where(o => o.StartDate >= firstStartDate && o.StartDate <= lastStartDate).ToList<TimeSheet>();
                if (approvedTimesheetsPacket.Count == currentWeekTimesheetPacket.Count)
                {
                    roleTimesheets = approvedTimesheetsPacket;
                    return true;
                }


            }
            if (roleExpires && roleTimesheets.Count > 0 && roleTimesheets.Count == 4)
            {
                if (currentNotApprovedCounter == 0)
                    return true;
                else
                    return false;
            } 
            return false;
            
        }

        private bool ValidateRoleAdjTimeSheets(IList<TimeSheet> roleTimesheets)
        {
            var roleExpires = false;
            // check if role expires in a month
            var role = this.roleService.GetRole(roleTimesheets[0].RoleId);

            if (DateTime.Compare(DateTime.Now, role.EndDate.GetValueOrDefault()) > 0)
            {
                roleExpires = true;
            }

            if (roleExpires==false)
            { return false; }

            // check if timesheets are an entire batch of 4 XXX0000 would fail, XXXX0000 would pass XXXXX would pass XXX0X would fail
            var uninvoicedTimeSheetsAll = this.financeService.GetUninvoicedWeeklyTimeSheetsForAssociate(roleTimesheets[0].AssociateId, roleTimesheets[0].RoleId, roleTimesheets[0].ProjectId);
            var uninvoicedTimeSheets = uninvoicedTimeSheetsAll.Where(t => t.TimeSheetTypeId.Equals((int)TimesheetTypeId.Weekly)).ToList<TimeSheet>();

            if (uninvoicedTimeSheets.Count() > 0)
                return false;
            else
                return true;

        }
        private bool IsAssociateSelfBilling(int associateId)
        {
            var associate = associateService.GetAssociateOnly(associateId);
            if (associate.OptOutSelfBilling) return false;

            if ((DateTime.Now - associate.OptOutSelfBillingSignedDate.GetValueOrDefault()).TotalDays > 365) return false;
            return associate.IsSelfBilling ||
            associate.BusinessTypeId == (int) MR_DAL.Enumerations.BusinessType.Umbrella;
        }

        private void AddWeeklyAndAdjustmentInvoices()
        {
            var vatRate = Convert.ToDecimal(ConfigurationManager.AppSettings["VatRate"]);
            var timeSheets = this.financeService.GetApprovedWeeklyTimeSheets();           

            var timeSheetList = timeSheets as IList<TimeSheet> ?? timeSheets.ToList();
            var uniqueProjects = timeSheetList.Select(p => p.ProjectId).Distinct();

            foreach (var uniqueProject in uniqueProjects)
            {
                var projectTimeSheets = timeSheetList.Where(p => p.ProjectId.Equals(uniqueProject));
                var projectTimeSheetList = projectTimeSheets as IList<TimeSheet> ?? projectTimeSheets.ToList();
                var uniqueAssociates = projectTimeSheetList.Select(a => a.AssociateId).Distinct();

                foreach (var uniqueAssociate in uniqueAssociates)
                {
                    // check if assocaite is self-billing
                    if (!IsAssociateSelfBilling(uniqueAssociate))
                    {
                        continue;
                    }

                    var associateTimesheets =
                        projectTimeSheetList.Where(p => p.AssociateId.Equals(uniqueAssociate))
                            .OrderBy(p => p.TimeSheetId);
                    var associateRoles = associateTimesheets.Select(r => r.RoleId).Distinct();

                    foreach (var associateRole in associateRoles)
                    {
                        var associateRoleTimesheets = associateTimesheets.Where(r => r.RoleId.Equals(associateRole));
                        var roleTimesheets = associateRoleTimesheets as IList<TimeSheet> ??
                                             associateRoleTimesheets.OrderBy(x => x.StartDate).ToList();


                        if (!ValidateRoleTimeSheets(ref roleTimesheets))
                        {
                            continue;
                        }

                        // get first 4 timeSheetIds or remaining timesheets
                        var associateTimeSheetList =
                            roleTimesheets.Count >= 4
                                ? roleTimesheets.Select(t => t.TimeSheetId).Distinct().Take(4).ToArray()
                                : roleTimesheets.Select(t => t.TimeSheetId).Distinct().ToArray();

                        associateTimeSheetList = AddAdjustmentTimeSheets(uniqueAssociate, associateTimeSheetList);

                        var invoiceId = this.timesheetService.CreateInvoice(uniqueAssociate, associateTimeSheetList, vatRate, true);
                        
                        // create eMail
                        financeService.SendSelfBillingInvoiceCreatedEmail(invoiceId,false);
                        financeService.SendUmbrellaSelfBillingInvoiceCreatedEmail(invoiceId, false);
                    }
                }
            }
        }

        private void AddOnlyAdjustmentInvoices()
        {
            var vatRate = Convert.ToDecimal(ConfigurationManager.AppSettings["VatRate"]);
            var timeSheets = this.financeService.GetApprovedAdjTimeSheets();

            var timeSheetList = timeSheets as IList<TimeSheet> ?? timeSheets.ToList();
            var uniqueProjects = timeSheetList.Select(p => p.ProjectId).Distinct();

            foreach (var uniqueProject in uniqueProjects)
            {
                var projectTimeSheets = timeSheetList.Where(p => p.ProjectId.Equals(uniqueProject));
                var projectTimeSheetList = projectTimeSheets as IList<TimeSheet> ?? projectTimeSheets.ToList();
                var uniqueAssociates = projectTimeSheetList.Select(a => a.AssociateId).Distinct();

                foreach (var uniqueAssociate in uniqueAssociates)
                {
                    // check if assocaite is self-billing
                    if (!IsAssociateSelfBilling(uniqueAssociate))
                    {
                        continue;
                    }

                    var associateTimesheets =
                        projectTimeSheetList.Where(p => p.AssociateId.Equals(uniqueAssociate))
                            .OrderBy(p => p.TimeSheetId);
                    var associateRoles = associateTimesheets.Select(r => r.RoleId).Distinct();

                    foreach (var associateRole in associateRoles)
                    {
                        var associateRoleTimesheets = associateTimesheets.Where(r => r.RoleId.Equals(associateRole));
                        var roleTimesheets = associateRoleTimesheets as IList<TimeSheet> ??
                                             associateRoleTimesheets.OrderBy(x => x.StartDate).ToList();


                        if (!ValidateRoleAdjTimeSheets(roleTimesheets))
                        {
                            continue;
                        }

                        // get first 4 timeSheetIds or remaining timesheets
                        var associateTimeSheetList =
                           roleTimesheets.Select(t => t.TimeSheetId).Distinct().ToArray();

                        //associateTimeSheetList = AddAdjustmentTimeSheets(uniqueAssociate, associateTimeSheetList);

                        var invoiceId = this.timesheetService.CreateInvoice(uniqueAssociate, associateTimeSheetList, vatRate, true);

                        // create eMail
                        financeService.SendSelfBillingInvoiceCreatedEmail(invoiceId, false);
                        financeService.SendUmbrellaSelfBillingInvoiceCreatedEmail(invoiceId, false);
                    }
                }
            }
        }
        private void AddRetainerInvoices()
        {
            var timeSheets = this.financeService.GetApprovedRetainerSheets();

            var timeSheetList = timeSheets as IList<TimeSheet> ?? timeSheets.ToList();

            foreach (var timeSheet in timeSheetList)
            {
                // check if assocaite is self-billing
                if (!IsAssociateSelfBilling(timeSheet.AssociateId))
                {
                    continue;
                }

                var invoiceId = this.timesheetService.CreateInvoice(timeSheet.AssociateId, new[]
                {
                    timeSheet.TimeSheetId
                }, vatRate, true);

                // create eMail
                if (invoiceId > 0)
                {
                    financeService.SendSelfBillingInvoiceCreatedEmail(invoiceId, true);
                    financeService.SendUmbrellaSelfBillingInvoiceCreatedEmail(invoiceId, true);
                }
            }       
        }

        public ActionResult ExportHoldingBay()
        {
            try
            {
                int? resultsCount;

                var invoices = this.financeService.GetHoldingBayInvoices(
                    0,
                    0,
                    null,
                    out resultsCount);

                var result = from i in invoices
                             select new
                             {
                                 //Checked = false,
                                 AssociateId = i.AssociateId,
                                 InvoiceId = i.InvoiceId,
                                 BusinessUnit = i.BusinessUnit,
                                 ProjectName = i.ProjectName,
                                 Name = i.Name,
                                 CompanyName = i.CompanyName,
                                 ReportDate = i.ReportDateFormatted,
                                 InvoicereportNumber = i.InvoicereportNumber,
                                 Status = i.Status == "OnHold" ? "On Hold" : i.Status,
                                 PaymentDate = i.PaymentDateFormatted,
                                 NetAmount = decimal.Round(i.NetAmount, 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                 VAT = decimal.Round(i.VAT, 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                 TotalAmount = decimal.Round(i.TotalAmount, 2, MidpointRounding.AwayFromZero).ToString("0.00"),
                                 //// NetPayment = this.timesheetService.GetInvoicesReportData(i.InvoiceId)
                             };

               
                var type = "HoldingBayInvoices";

                return ExportSearch(result, type);
            }
            catch (Exception ex)
            {
                // todo: if you are throwing custom exceptions, instead of checking for strings use an exception type
                if (ex.Message.Contains("Invoice Search Error:") || (ex.InnerException != null && (ex.InnerException.Message.Contains("Invoice Search Error:"))))
                {
                    var start = ex.InnerException.Message.LastIndexOf(":") + 1;
                    var status = ex.InnerException.Message.Substring(start).Trim();

                    return this.Json(new { File = string.Empty, Type = string.Empty, Status = "Error", Message = status });
                }
                else
                {
                    //ErrorSignal.FromCurrentContext().Raise(ex);

                    return this.Json(new { File = string.Empty, Type = string.Empty, Status = "Error", Message = "An error has occured" });
                }
            }
        }

        private ActionResult ExportSearch<T>(IEnumerable<T> result, string type) where T : class
        {
            var timestamp = DateTime.Now.ToString(".dd.MM.yyyy.HH.mm.ss");
            var excel = new GenericWriteExcel();
            var excelFile = excel.CreateExcelFile(result);
            var fileName = type + timestamp + ".xlsx";

            // contruct the path
            var filePath = Path.Combine(Settings.Default.SearchExportFolder, fileName);

            // save the file
            using (var file = System.IO.File.Create(filePath))
            {
                excelFile.Seek(0, SeekOrigin.Begin);
                excelFile.CopyTo(file);
            }

            // serve the reponse
            return this.Json(new { File = timestamp, Type = type, Status = "OK" });
        }

        public FileResult Export(string type, string timestamp)
        {
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // validate the type
            //if (!permittedExport.Contains(type))
            //{
            //    throw new Exception("un supported export type: " + type);
            //}

            var fileName = type + timestamp + ".xlsx";

            var path = Path.Combine(Settings.Default.SearchExportFolder, fileName);

            return File(path, contentType, fileName);
        }
    }
}