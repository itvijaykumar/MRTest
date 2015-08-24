
using System.Configuration;

namespace AssociatePortal.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web;
    using AssociatePortal.Services;
    using AssociatePortal.ViewModel;
    using MomentaRecruitment.Common.Helpers;
    using MomentaRecruitment.Common.Models;
    using MomentaRecruitment.Common.Services;
    using MomentaRecruitment.Common.ViewModel;    
    using MomentaRecruitment.Common.Repositories;
    using Models;
    using Newtonsoft.Json;
    using MR_DAL.Enumerations;

    using IAssociateService = AssociatePortal.Services.AssociateService;
    using AssociateModel = MomentaRecruitment.Common.Models.AssociateModel;

    [Authorize]
    public class TimeSheetController : Controller
    {
        private ITimesheetService timesheetService;
        private IAssociateService associateService;
        private IMembershipService membershipService;
        private IClientContactService clientContactService;
        private decimal vatRate;


        public TimeSheetController(ITimesheetService timesheetService, IAssociateService associateService, IMembershipService membershipService, IClientContactService clientContactService)
        {
            this.timesheetService = timesheetService;
            this.associateService = associateService;
            this.membershipService = membershipService;
            this.clientContactService = clientContactService;
            this.vatRate = Convert.ToDecimal(ConfigurationManager.AppSettings["VatRate"]);
        }

        [HttpPost]
        public JsonResult DeleteDocument(string id)
        {
            var documentId = new Guid(id);

            bool result = this.timesheetService.DeleteDocument(documentId);
            return this.Json(result);
        }

        [HttpPost]
        public JsonResult GetTimesheetEntries(int timesheetId)
        {
            var entry = timesheetService.GetTimeSheetEntry(timesheetId);

            return this.Json(entry);
        }

        public ActionResult GetTimesheetEntriesView(int timesheetId)
        {
            var entry = timesheetService.GetTimeSheetEntry(timesheetId);
            ViewBag.Data = JsonConvert.SerializeObject(entry);

            return PartialView("_EditTimeSheet");
        }

        [HttpPost]
        public JsonResult GetTimeSheetsForInvoicing()
        {
            int associateId = this.AssociateVerifiedDetails().AssociateId;

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

        public ActionResult ViewInvoiceReport(int id)
        {
            var invoice = this.timesheetService.GetInvoice(id);
            var associate = this.associateService.GetAssociate<AssociateModel>(invoice.AssociateId);

            if (associate.UmbrellaCompanyId > 0)
                associate.UmbrellaCompanyName = this.associateService.GetUmbrellaCompanyName(associate.UmbrellaCompanyId);
            
            string companyName = this.timesheetService.GetCompanyName(id);

            if (companyName == null)
            {
                companyName = associate.RegisteredCompanyName;

                if (associate.BusinessTypeId.GetValueOrDefault() == (int)MR_DAL.Enumerations.BusinessType.Umbrella)
                {
                    companyName = associate.UmbrellaCompanyName;
                }
            }

            //var companyName = associate.RegisteredCompanyName;

            //if (associate.BusinessTypeId.GetValueOrDefault() == (int)MR_DAL.Enumerations.BusinessType.Umbrella)
            //{
            //    companyName = associate.UmbrellaCompanyName;
            //}

            ViewBag.InvoiceId = id;
            ViewBag.ReportNo = invoice.InvoiceId.ToString("00000");
            ViewBag.Name = associate.FirstName + " " + associate.LastName;
            ViewBag.BusinessTypeId = associate.BusinessTypeId;
            ViewBag.Company = companyName;
            ViewBag.Code = invoice.AssociateId.ToString("00000");
            ViewBag.AssociateId = invoice.AssociateId;
            ViewBag.HasFile = invoice.HasFile;
            ViewBag.ShowProcessButton = (invoice.Status == "Invoiced");
            ViewBag.StartDate = invoice.StartDate;
            ViewBag.EndDate = invoice.EndDate;
            ViewBag.OptOutSelfBilling = associate.OptOutSelfBilling;  

            try
            {
                IEnumerable<TimeSheetsGridViewModel> invoicedTimeSheets = timesheetService.GetTimeSheetsByInvoiceId(id);
                ViewBag.InvoiceType = invoicedTimeSheets.First().TimesheetTypeId;

                if (ViewBag.InvoiceType == 2)
                {
                    var retainerEntries = this.timesheetService.GetInvoicesReportRetainerData(id);
                    ViewBag.AssociateRate = retainerEntries.First().AssociateRate;
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
        public JsonResult GetInvoicesFiles(int invoiceId)
        {
            return this.Json(this.timesheetService.GetInvoiceFiles(invoiceId, false));
        }

        [HttpPost]
        public JsonResult GetInvoiceHistory(int id)
        {
            return this.Json(this.timesheetService.GetInvoiceHistory(id));
        }

        [HttpPost]
        public JsonResult GetCurrentInvoices()
        {
            int associateId = this.AssociateVerifiedDetails().AssociateId;

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
        public JsonResult CreateInvoice(int[] timesheetIds)
        {
            int associateId = this.AssociateVerifiedDetails().AssociateId;

            try
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
            catch (Exception)
            {
                return this.Json("The was an error generating your invoice");
            }                                    
        }

        [HttpPost]
        public JsonResult CreateInvoiceForRetainer(int[] timesheetIds)
        {
            int associateId = this.AssociateVerifiedDetails().AssociateId;

            try
            {
                if (this.timesheetService.ValidateInvoiceRetainerTimesheets(associateId, timesheetIds))
                {
                    this.timesheetService.CreateInvoice(associateId, timesheetIds, vatRate);
                    return this.Json(string.Empty);
                }
                else
                {
                    return this.Json("Unable to generate the invoice, please ensure all submitted timesheets have been approved.");
                }
            }
            catch (Exception)
            {
                return this.Json("The was an error generating your invoice");
            }
        }

        [ChildActionOnly]
        public PartialViewResult GetTimeSheetDashBoard()
        {
            return PartialView("_TimeSheetDashBoard");
        }

        [HttpPost]
        public JsonResult GetTimeSheetEntry(int timeSheetId)
        {
            var entries  = timesheetService.GetTimeSheetEntry(timeSheetId);

            return Json(entries);
        }

        [HttpPost]
        public JsonResult GetDashBoardData(int associateId)
        {
            var model = timesheetService.GetDashBoardData(associateId);
             
            return Json(model);
        }

        [HttpPost]
        public JsonResult SaveTimesheetEntries(TimesheetViewModel model)
        {
            return Json(timesheetService.SaveTimeSheet(model));
        }

        [HttpPost]
        public JsonResult SaveTimesheetEntryNote(int id, string accomodation, string mealAllowance,string travel,string noOfMiles, string parkingNotes, string other)
        {
            var entry = this.timesheetService.GetEntry(id);
            if (string.IsNullOrWhiteSpace(accomodation))
            {
                entry.AccomodationNotes = accomodation;
            }
            if (string.IsNullOrWhiteSpace(mealAllowance))
            {
                entry.MealAllowanceNotes = mealAllowance;
            }
            if (string.IsNullOrWhiteSpace(travel))
            {
                entry.TravelNotes = travel;
            }
            if (string.IsNullOrWhiteSpace(noOfMiles))
            {
                entry.NumberOfMilesNotes = noOfMiles;
            }
            if (string.IsNullOrWhiteSpace(parkingNotes))
            {
                entry.ParkingNotes = parkingNotes;
            }
            if (string.IsNullOrWhiteSpace(other))
            {
                entry.OtherNotes = other;
            }
            this.timesheetService.UpdateEntry(entry);
            return Json(true);//timesheetService.SaveTimeSheet(model));
        }

        [HttpPost]
        public JsonResult SaveTimesheetEntriesWithoutStatus(TimesheetViewModel model)
        {
            return Json(timesheetService.SaveTimeSheetWithoutStatus(model));
        }

        [HttpPost]
        public JsonResult SubmitTimesheet(TimesheetViewModel model)
        {
            return Json(timesheetService.SubmitTimeSheet(model));
        }

        [HttpPost]
        public JsonResult ApproveTimesheet(TimesheetViewModel model, int? roleId = null)
        {
           bool result;
          AssociateModel currentUserId = null;
           try
           {
                currentUserId = associateService.GetAssociate(User.Identity.Name);
           }
           catch (Exception){}
      
           if (currentUserId == null || currentUserId.Id == 0)
            {
                model.CurrentUserId = this.ClientContactVerifiedDetails().ClientContactId;
                result = timesheetService.ApproveTimesheet(model,true);
            }else{
                result = timesheetService.ApproveTimesheet(model);
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult RejectTimesheet(TimesheetViewModel model)
        {
            string userName = this.membershipService.GetCurrentUserName();
            model.TimesheetChecker = userName;
            bool result;
            AssociateModel currentUserId = null;
            try
            {
                currentUserId = associateService.GetAssociate(User.Identity.Name);
            }
            catch (Exception) { }
            
            if (currentUserId == null || currentUserId.Id == 0)
            {
                model.CurrentUserId = this.ClientContactVerifiedDetails().ClientContactId;
                result = timesheetService.RejectTimesheet(model, true);
            }
            else
            {
                result = timesheetService.RejectTimesheet(model);
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult ReSubmitTimesheet(TimesheetViewModel model)
        {
            return Json(timesheetService.ReSubmitTimesheet(model));
        }

        [HttpPost]
        public JsonResult AssociateRejectTimesheet(TimesheetViewModel model)
        {
            return Json(timesheetService.AssociateRejectTimesheet(model));
        }

        [HttpPost]
        public JsonResult MarkAsPending(TimesheetViewModel model)
        {
            string userName = this.membershipService.GetCurrentUserName();
            model.TimesheetChecker = userName;
            return Json(timesheetService.MarkAsPending(model));
        }

        [HttpPost]
        public JsonResult MarkAsCompleted(TimesheetViewModel model)
        {
            return Json(timesheetService.MarkAsCompleted(model));
        }

        [Authorize]
        public ActionResult DownloadInvoice(int id)
        {
            var inv = this.associateService.GetInvoiceFile(id);

            return FileStreamHelper.FileDownloadModelResult(inv, this.HttpContext, true);
        }

        [Authorize]
        public ActionResult DownloadFile(Guid? id)
        {
            var inv = this.associateService.GetInvoiceFile(null, id);

            return FileStreamHelper.FileDownloadModelResult(inv, this.HttpContext, true);
        }

        public PartialViewResult GetDashBoard()
        {
            return PartialView("_TimeSheetDashBoard");
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

        private ClientContactServerVerifiedDetails ClientContactVerifiedDetails()
        {
            object providerUserKey = this.membershipService.GetUser().ProviderUserKey;

            if (providerUserKey == null)
            {
                throw new NullReferenceException("providerUserKey is null");
            }

            return this.clientContactService.GetClientContactServerVerifiedDetailsFromMembershipUserId(
                (Guid)providerUserKey);
        }

        private AssociateServerVerifiedDetails AssociateVerifiedDetails()
        {
            var providerUserKey = this.membershipService.GetUser().ProviderUserKey;

            if (providerUserKey == null)
            {
                throw new NullReferenceException("providerUserKey is null");
            }

            return this.associateService.GetAssociateServerVerifiedDetailsFromMembershipUserId(
                (Guid)providerUserKey);
        }
        [HttpPost]
        public JsonResult UpdateNotes(int TimeSheetEntryId,string ExpenseColumn,string ExpenseNotes)
        {
            return Json(timesheetService.UpdateNotes(TimeSheetEntryId,ExpenseColumn,ExpenseNotes));
            
        }
    }
}