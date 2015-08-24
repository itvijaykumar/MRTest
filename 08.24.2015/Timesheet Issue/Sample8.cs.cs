using System.Configuration;
using MR_DAL.Enumerations;

namespace Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;

    using Admin.Helpers;
    using Admin.Models;
    using Admin.Properties;
    using Admin.Services;

    using AssociatePortal.Services;
    using AssociatePortal.ViewModel;

    using MomentaRecruitment.Common.Enumerations;

    using MomentaRecruitment.Common.Helpers;
    using MomentaRecruitment.Common.Models;
    using MomentaRecruitment.Common.Properties;
    using IMembershipService = MomentaRecruitment.Common.Services.IMembershipService;
    using MomentaRecruitment.Common.ViewModel;
    using MR_DAL.TimeSheet.Repository;
    using Newtonsoft.Json;

    using AssociateModel = Admin.Models.AssociateModel;
    using IAssociateService = Admin.Services.IAssociateService;

    [DefaultAuthorizeAttribute(Roles = RoleName.AdminSiteUser)]
    public class TimeSheetController : Controller
    {
        private ITimesheetService timesheetService;
        private IAssociateService associateService;
        private IMembershipService membershipService;
        private MomentaRecruitment.Common.Services.ITimesheetAssociateService timesheetAssociateService;
        private decimal vatRate;

        public TimeSheetController(ITimesheetService timesheetService, IAssociateService associateService, IMembershipService membershipService, MomentaRecruitment.Common.Services.ITimesheetAssociateService timesheetAssociateService)
        {
            this.timesheetService = timesheetService;
            this.associateService = associateService;
            this.membershipService = membershipService;
            this.timesheetAssociateService = timesheetAssociateService;
            this.vatRate = Convert.ToDecimal(ConfigurationManager.AppSettings["VatRate"]);
        }

        [HttpPost]
        public JsonResult GetTimesheetEntries(int timesheetId)
        {
            var entry = timesheetService.GetTimeSheetEntry(timesheetId);

            return this.Json(entry);
        }

        [HttpPost]
        public JsonResult GetTimeSheetsForInvoicing(int associateId)
        {
            try
            {
                var timesheets = timesheetService.GetTimeSheetsForInvoicing(associateId);
                return this.Json(timesheets);
            }
            catch (Exception ex)
            {
                return this.Json(ex);
            }
        }

        public ActionResult GetTimesheets()
        {
            return PartialView("_TimeSheets");
        }

        [HttpPost]
        public JsonResult GetInvoicesFiles(int invoiceId)
        {
            return this.Json(this.timesheetService.GetInvoiceFiles(invoiceId));
        }

        [HttpPost]
        public JsonResult UpdateInvoiceFileStatus(int associateId, string invoiceFileId, string status, int invoiceId, string comment, bool sendEmail = false, string attachments=null)        
        {
            try
            {
                MR_DAL.Enumerations.InvoiceFileStatus fileStatus = (status == "checked" ? MR_DAL.Enumerations.InvoiceFileStatus.Checked : MR_DAL.Enumerations.InvoiceFileStatus.Rejected);
                this.timesheetService.UpdateInvoiceFileStatus(associateId, invoiceFileId, fileStatus, invoiceId, comment, sendEmail,attachments);                

                return this.Json(true);
            }
            catch (Exception ex)
            {
                return this.Json(ex);
            }
        }

        [HttpPost]
        public JsonResult GetInvoiceHistory(int id)
        {
            return this.Json(this.timesheetService.GetInvoiceHistory(id));
        }

        [HttpPost]
        public JsonResult ProcessInvoice(int invoiceId)
        {
            try
            {
                var vatRate = Convert.ToDecimal(ConfigurationManager.AppSettings["VatRate"]);
                var invoicedTimeSheets = timesheetService.GetTimeSheetsByInvoiceId(invoiceId);
                ViewBag.InvoiceType = invoicedTimeSheets.First().TimesheetTypeId;

                var isRetainer = (ViewBag.InvoiceType == 2);
                this.timesheetService.ProcessInvoice(invoiceId, (int)TimesheetStatus.Processed, (int)InvoiceStatus.Processed, vatRate, isRetainer);

                var json = new
                {
                    success = true,
                    data = ""
                };

                return this.Json(json);
            }
            catch (Exception ex)
            {
                var json = new
                {
                    success = false,
                    data = ex.Message
                };
                return this.Json(json);
            }
        }

        [HttpPost]
        public JsonResult AddAdminComment(int invoiceId, string comment)
        {
            try
            {
                this.timesheetService.CreateInvoiceComment(invoiceId, User.Identity.Name, comment);
                return this.Json(true);
            }
            catch (Exception ex)
            {
                return this.Json(ex);
            }
        }

        public ActionResult ViewInvoiceReport(int id)
        {
            var invoice = this.timesheetService.GetInvoice(id);
            var associate = this.associateService.GetAssociate<AssociateModel>(invoice.AssociateId);

            string companyName = this.timesheetService.GetCompanyName(id);

            var varHasBeenVatRegistered = this.timesheetService.GetHistoricAssociateValue<bool>(invoice.AssociateId, "HasBeenVatRegistered", (DateTime)invoice.InvoiceCreatedDate, associate.HasBeenVatRegistered, null);

            if (companyName == null)
            {
                if (associate.UmbrellaCompanyId > 0)
                    associate.UmbrellaCompanyName = this.associateService.GetUmbrellaCompanyName(associate.UmbrellaCompanyId);

                companyName = associate.RegisteredCompanyName;

                if (associate.BusinessTypeId.GetValueOrDefault() == (int)MR_DAL.Enumerations.BusinessType.Umbrella)
                {
                    companyName = associate.UmbrellaCompanyName;
                }
            }
            
            ViewBag.InvoiceId = id;
            ViewBag.ReportNo = invoice.InvoiceId.ToString("00000");
            ViewBag.Name = associate.FirstName + " " + associate.LastName;
            ViewBag.Company = companyName;
            ViewBag.Code = invoice.AssociateId.ToString("00000");
            ViewBag.AssociateId = invoice.AssociateId;
            ViewBag.OptOutSelfBilling = associate.OptOutSelfBilling;
            ViewBag.HasFile = invoice.HasFile;
            ViewBag.ShowProcessButton = (invoice.Status == "Invoiced");
            ViewBag.StartDate = invoice.StartDate;
            ViewBag.EndDate = invoice.EndDate;
            if (varHasBeenVatRegistered==true)
                ViewBag.VATRegistration = "Yes";
            else
                ViewBag.VATRegistration = "No";

            try
            {
                var invoicedTimeSheets = timesheetService.GetTimeSheetsByInvoiceId(id);
                ViewBag.InvoiceType = invoicedTimeSheets.First().TimesheetTypeId;                                                

                if (ViewBag.InvoiceType == 2) 
                {
                    ViewBag.RetentionPayAway = invoice.RetainerRetentionPayAway;
                    //var retainerEntries = this.timesheetService.GetInvoicesReportRetainerData(id);
                    //if (retainerEntries.Any())
                    //{
                    //    ViewBag.RetentionPayAway = retainerEntries.First().RetentionPayAway;
                    //}
                }                

                this.SetUploader(id);
                return PartialView("_InvoiceReport");
            }
            catch (Exception)
            {
                throw new Exception("Invoice Report doesn't contain any timesheets");
            }
        }

        [HttpPost]
        public JsonResult GetInvoicesReportData(int invoiceId)
        {
          //  var results = this.timesheetService.GetInvoicesReportData(invoiceId);

           // foreach (var invoiceReportViewModel in results)
           // {
           //     invoiceReportViewModel.Hours = -5;

           // }
           //// return this.Json(results);
           return this.Json(this.timesheetService.GetInvoicesReportData(invoiceId));
        }

        [HttpPost]
        public JsonResult GetInvoicesReportRetainerData(int invoiceId)
        {
            return this.Json(this.timesheetService.GetInvoicesReportRetainerData(invoiceId));
        }

        public ActionResult GetInvoicedTimesheets(int invoiceId)
        {
            try
            {
                var timesheets = timesheetService.GetTimeSheetsByInvoiceId(invoiceId);
                return this.Json(timesheets);
            }
            catch (Exception ex)
            {
                return this.Json(ex);
            }
        }

        [HttpPost]
        public JsonResult GetCurrentInvoices(int associateId)
        {
            try
            {
                var invoices = timesheetService.GetCurrentInvoices(associateId);

                return this.Json(invoices);
            }
            catch (Exception ex)
            {
                return this.Json(ex);
            }
        }

        public ActionResult GetInvoices()
        {
            return PartialView("_Invoices");
        }

        [HttpPost]
        public JsonResult CreateInvoice(int associateId, int[] timesheetIds)
        {
            if (this.timesheetService.ValidateInvoiceTimeSheets(associateId, timesheetIds))
            {
                this.timesheetService.CreateInvoice(associateId, timesheetIds, vatRate);
                return this.Json(string.Empty);
            }
            else
            {
                return this.Json("Unable to generate the Invoice report. Please ensure you have selected the correct number of consecutive timesheets and that all timesheets have been approved.");
            }
        }

        [HttpPost]
        public JsonResult CreateInvoiceForRetainer(int associateId, int[] timesheetIds)
        {
            try
            {
                if (this.timesheetService.ValidateInvoiceRetainerTimesheets(associateId, timesheetIds))
                {
                    this.timesheetService.CreateInvoice(associateId, timesheetIds, vatRate);
                    return this.Json(string.Empty);
                }
                else
                {
                    return this.Json("Unable to generate the invoice report, please ensure you have submitted the correct number of timesheets and that all timesheets have been approved.");                    
                }
            }
            catch (Exception)
            {
                return this.Json("The was an error generating your invoice");
            }
        }

        [HttpPost]
        public JsonResult CreateInvoiceComment(int invoiceId, string comment)
        {
            try
            {
                this.timesheetService.CreateInvoiceComment(invoiceId, this.User.Identity.Name, comment);
                return this.Json(true);
            }
            catch (Exception ex)
            {
                return this.Json(new Exception("Error creating invoice comment", ex));
            }
        }

        [HttpPost]
        public JsonResult GetInvoiceComments(int invoiceId)
        {
            try
            {
                return this.Json(this.timesheetService.GetInvoiceComments(invoiceId));
            }
            catch (Exception ex)
            {
                return this.Json(new Exception("Error retrieving invoice comments", ex));
            }
        }

        [ChildActionOnly]
        public PartialViewResult GetTimeSheetDashBoard()
        {
            return PartialView("_TimeSheetDashBoard");
        }

        public JsonResult GetTimeSheetEntry(int timeSheetId)
        {
            var entries  = timesheetService.GetTimeSheetEntry(timeSheetId);
            return Json(entries, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDashBoardData(int currentUserId)
        {
            var model = timesheetService.GetDashBoardData(currentUserId);
             
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveTimesheetEntries(TimesheetViewModel model)
        {
            string userName = this.membershipService.GetCurrentUserName();
            model.TimesheetChecker = userName;
            return Json(timesheetService.SaveTimeSheet(model), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SubmitTimesheet(TimesheetViewModel model)
        {
            return Json(timesheetService.SubmitTimeSheet(model), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ApproveTimesheet(TimesheetViewModel model)
        {
            return Json(timesheetService.ApproveTimesheet(model), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = RoleName.TimesheetRejection)]
        public JsonResult RejectTimesheet(TimesheetViewModel model)
        {
            string userName = this.membershipService.GetCurrentUserName();
            model.TimesheetChecker = userName;
            return Json(timesheetService.RejectTimesheet(model), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult MarkAsPending(TimesheetViewModel model)
        {
            return Json(timesheetService.MarkAsPending(model));
        }

        [HttpPost]
        public JsonResult MarkAsCompleted(TimesheetViewModel model)
        {

            string userName = this.membershipService.GetCurrentUserName();
            model.TimesheetChecker = userName;
            return Json(timesheetService.MarkAsCompleted(model));
        }

        [HttpPost]
        public JsonResult ReSubmitTimesheet(TimesheetViewModel model)
        {
            return Json(timesheetService.ReSubmitTimesheet(model), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AssociateRejectTimesheet(TimesheetViewModel model)
        {
            return Json(timesheetService.AssociateRejectTimesheet(model), JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteDocument(string id, int associateId)
        {
            var documentId = new Guid(id);
            var result = this.timesheetService.DeleteDocument(documentId, associateId);
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RejectDocument(int timesheetId, string id, int associateId, string comment)
        {
            var documentId = new Guid(id);
            var result = this.timesheetService.RejectDocument(timesheetId, documentId, associateId, comment);
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RejectSelectedDocuments(int timesheetId, string[] documentIds, int associateId, string comment)
        {
            bool result = true;

            foreach (string id in documentIds)
            {
                var documentId = new Guid(id);
                result &= this.timesheetService.RejectDocument(timesheetId, documentId, associateId, comment);
            }
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AcceptDocument(string id, int associateId)
        {
            var documentId = new Guid(id);
            var result = this.timesheetService.AcceptDocument(documentId, associateId);
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AcceptSelectedDocuments(string[] documentIds, int associateId)
        {
            bool result = true;

            foreach (string id in documentIds)
            {
                var documentId = new Guid(id);
                result &= this.timesheetService.AcceptDocument(documentId, associateId);
            }
            
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult GetDashBoard()
        {
            return PartialView("_TimeSheetDashBoard");
        }

        [HttpPost]
        public JsonResult SuspendTimesheet(string currentUserId, string status, string comment)
        {
            return Json(timesheetService.SuspendTimesheet(Convert.ToInt32(currentUserId), Convert.ToBoolean(status), comment), JsonRequestBehavior.AllowGet);
        }

        [OverrideAuthorize(Roles = RoleName.TimesheetActions)]
        public ActionResult AssociatesBlankOrUpdated(string id)
        {
            try
            {
                var count = this.timesheetService.SendBlankOrUpdatedReminderEmail(id);
                var msg = string.Format("{0} associates emailed", count);

                return new HttpStatusCodeResult(200, msg);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        [OverrideAuthorize(Roles = RoleName.TimesheetActions)]
        public ActionResult AssociatesBlankOrUpdatedExtended(string id,int weeks)
        {
            try
            {
                var count = this.timesheetService.SendBlankOrUpdatedReminderEmailExtended(id,weeks);
                var msg = string.Format("{0} associates emailed", count);

                return new HttpStatusCodeResult(200, msg);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }


        [OverrideAuthorize(Roles = RoleName.TimesheetActions)]
        public ActionResult ApprovalsOutstanding()
        {
            try
            {
                var count = this.timesheetService.SendApproverReminderEmail();
                var msg = string.Format("{0} approvers emailed", count);

                return new HttpStatusCodeResult(200, msg);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

       // [OverrideAuthorize(Roles = RoleName.TimesheetActions)]
        public ActionResult PaymentDateReminder()
        {
            try
            {
                var count = this.timesheetService.SendPaymentDateReminderEmail();
                var msg = string.Format("{0} reminders emailed", count);

                return new HttpStatusCodeResult(200, msg);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }
        [Authorize]
        public ActionResult DownloadInvoice(int id)
        {
            FileDownloadModel inv = this.associateService.GetInvoiceFile(id);

            return FileStreamHelper.FileDownloadModelResult(inv, this.HttpContext, true);
        }

        [Authorize]
        public ActionResult DownloadFile(Guid? id)
        {
            FileDownloadModel inv = this.associateService.GetInvoiceFile(null, id);

            return FileStreamHelper.FileDownloadModelResult(inv, this.HttpContext, true);
        }

        protected void SetUploader(int invoiceId)
        {
            this.ViewBag.UploaderHtml = UploadHelper.GetHtml(
                "uploadButton",
                MomentaRecruitment.Common.Properties.Settings.Default.AllowedFileExtensions,
                "~/UploadHandler.axd?Mode=Invoice&Id=" + invoiceId.ToString(),
                "documentUploader",
                "Browse");
        }

        protected void SetUploader()
        {
            this.ViewBag.UploaderHtml = UploadHelper.GetHtml(
                "uploadButton",
                MomentaRecruitment.Common.Properties.Settings.Default.AllowedFileExtensions,
                "~/UploadHandler.axd?Mode=Default",
                "documentUploader",
                "Browse");
        }

        [OverrideAuthorize(Roles = RoleName.TimesheetActions)]
        public HttpStatusCodeResult GenerateScheduleRetainer()
        {
            try
            {
                this.timesheetService.GenerateScheduleRetainer();          
                return new HttpStatusCodeResult(200, "Success");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        public JsonResult BilledToClient(string[] timsheetIds)
        {
            return Json(timesheetService.BilledToClient(timsheetIds), JsonRequestBehavior.AllowGet);      
        }
        [HttpPost]
        public JsonResult UpdateNotes(int TimeSheetEntryId, string ExpenseColumn, string ExpenseNotes)
        {
            return Json(timesheetService.UpdateNotes(TimeSheetEntryId, ExpenseColumn, ExpenseNotes));

        }
    }
}