using System.Configuration;

namespace Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.Principal;
    using System.Text;
    using System.Transactions;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using System.Web.Security;

    using Admin.Helpers;
    using Admin.Models;
    using Admin.Services;
    using Admin.Repositories;
    using Elmah;
    using MomentaRecruitment.Common;
    using MomentaRecruitment.Common.Enumerations;
    using MomentaRecruitment.Common.Exceptions;
    using MomentaRecruitment.Common.Helpers;
    using MomentaRecruitment.Common.Helpers.ExtensionMethods;
    using MomentaRecruitment.Common.Models;
    using MomentaRecruitment.Common.Services;
    using MR_DAL.Enumerations;
    using MvcContrib.Pagination;
    using MvcContrib.Sorting;
    using MvcContrib.UI.Grid;

    using ApplicationException = System.ApplicationException;
    using AssociateModel = Admin.Models.AssociateModel;
    using DateTime = System.DateTime;
    using IAssociateService = Admin.Services.IAssociateService;
    using Newtonsoft.Json;

    [DefaultAuthorize(Roles = RoleName.AdminSiteUser)]
    public class AssociateController : MomentaRecruitment.Common.Controllers.AssociateController
    {
        private readonly IAssociateService associateService;

        private readonly IDocumentService documentService;

        private readonly IReferenceService referenceService;

        private readonly IEmployeeService employeeService;

        private readonly IProspectService prospectService;

        private readonly IMembershipService associateMembershipService;

        private readonly IClientProjectSharedService clientProjectSharedService;

        private readonly IIndividualService individualService;

        private readonly IRoleService roleService;

        private readonly Admin.Services.IEmailService emailService;

        public AssociateController(
            IMembershipService membershipService,
            IMembershipService associateMembershipService,
            IAssociateService associateService,
            IDocumentService documentService,
            IReferenceService referenceService,
            IEmployeeService employeeService,
            IProspectService prospectService,
            IPrincipal user,
            IClientProjectSharedService clientProjectSharedService,
            IIndividualService individualService,
            IRoleService roleService,
            Admin.Services.IEmailService emailService)
            : base(membershipService, associateService, user)
        {
            this.associateService = associateService;
            this.documentService = documentService;
            this.referenceService = referenceService;
            this.employeeService = employeeService;
            this.prospectService = prospectService;
            this.associateMembershipService = associateMembershipService;
            this.clientProjectSharedService = clientProjectSharedService;
            this.individualService = individualService;
            this.roleService = roleService;
            this.emailService = emailService;
        }

        private IMembershipService AssociateMembership
        {
            get { return this.associateMembershipService; }
        }

        [Authorize]
        public ActionResult DownloadTimesheetDocument(int associateId, Guid? id)
        {
            if (id.HasValue)
            {
                var document = this.associateService.DownloadTimesheetDocument(id.Value, associateId);

                return FileStreamHelper.FileDownloadModelResult(document, this.HttpContext, true);
            }

            return null;
        }

        [HttpGet]
        public ActionResult CV(int? id, Guid? versionId)
        {
            try
            {
                if (id.HasValue == versionId.HasValue)
                {
                    throw new ArgumentException("Pass either id or versionId.");
                }

                return versionId.HasValue ? this.DownloadCvVersion(versionId.Value) : this.DownloadCV(id.Value);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);

                // TODO: return custom error page for CV not found.
                return this.View("Error");
            }
        }

        public JsonResult GetIndividualIdsForProject(List<int> associateIds, int projectId)
        {
            var ids = this.associateService.GetIndividualIdsForProject(associateIds, projectId);
            var list = ids.ToList();

            return Json(list);
        }

        [HttpGet]
        public ActionResult ScheduledChange()
        {
            return this.View("ScheduledChange");
        }
        [HttpGet]
        public ActionResult BusinessTypeChange()
        {
            //int associateId
            //var associate = this.associateService.GetAssociateForScheduler(associateId);
            
            //this.ViewBag.UmbrellaCompanys = ListItem.GetSelectListItems(
            //this.associateService.GetUmbrellaCompanyOptions(),
            //arg => associate.UmbrellaCompanyId.HasValue && associate.UmbrellaCompanyId == arg.Value,
            //true);
            //return this.View("BusinessTypeChange", associate);
            return this.View("BusinessTypeChange");
        }

        //[HttpPost]
        //public ActionResult CreateBusinessTask(Admin.Models.AssociateSchedulerModel am)
        //{
        //    DateTime scheduled = DateTime.Now;
        //    this.associateService.CreateBusinessTask(am, scheduled);
        //    var ass = this.associateService.GetAssociateOnly(am.Id);
        //    return Redirect("Details/"+ass.Id);
        //}

        [HttpPost]
        public JsonResult CreateBusinessTask(int id, DateTime scheduledDate, string field, string value)
        {
            this.associateService.CreateBusinessTask(id, scheduledDate, field, value);
            return this.Json(true);
        }

       // [OverrideAuthorize(Roles = RoleName.TimesheetActions)]
      //  [HttpPost]
        public HttpStatusCodeResult UpdateBusinessTypeData(string jsonArgument)
        {
            try
            {
                associateService.UpdateBusinessTypeData(jsonArgument);
                CreateSucessfullMessage();
                return new HttpStatusCodeResult(200, "Success");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        public JsonResult AssignToRole(int associateId, int roleId)
        {
            prospectService.AssignAssociate(associateId, roleId);

            return Json("assigned");
        }

        public JsonResult ProspectToRole(int associateId, int roleId)
        {
            try
            {
                var prospect = this.prospectService.GetProspectByAssociateAndRole(associateId, roleId);

                if (prospect == null)
                {
                    prospectService.ProspectAssociate(associateId, roleId);

                    return Json("prospected");
                }
                else
                {
                    // associate already prospected to role
                    return Json("already prospected");
                }
            }
            catch (Exception ex)
            {
                //throw new Exception("Unable to prospect to role", ex);
                return Json("ERROR :" + ex.InnerException.Message);
            }
        }

        public JsonResult ProspectBulkToRole(List<int> associateIds, int roleId)
        {
            try
            {
                foreach (var associateId in associateIds)
                {
                    prospectService.ProspectAssociate(associateId, roleId);
                }

                return Json("prospected");

            }
            catch (Exception ex)
            {
                return Json("ERROR :" + ex.InnerException.Message);
            }

        }

        public ActionResult Compare(int id, string versionA, string versionB)
        {
            long[] splitA = versionA.SplitToLongArray();
            long[] splitB = versionB.SplitToLongArray();

            var a = new AssociateHistorySaveOperation
            {
                FirstSaveTimeStamp = new DateTime(splitA[0]),
                LastSaveTimeStamp = new DateTime(splitA[1])
            };

            var b = new AssociateHistorySaveOperation
            {
                FirstSaveTimeStamp = new DateTime(splitB[0]),
                LastSaveTimeStamp = new DateTime(splitB[1])
            };

            return this.Details(id, true, a, b);
        }

        public ActionResult GetAssociateOnly(int id)
        {
            this.associateService.GetAssociateOnly(id);

            return this.View();
        }

        // GET /Associate/Create
        // create a new associate
        public ActionResult Create()
        {
            var associate = new AssociateCreateModel();

            SetCreateAssociateOptions(associate);

            return View(associate);
        }

        // POST /Associate/Create
        // save the new associate
        [HttpPost]
        public ActionResult Create(AssociateCreateModel associate)
        {
            @ViewBag.Title = "Create Associate";

            try
            {
                // Basic validation passed.
                if (!this.ModelState.IsValid)
                {
                    this.ViewBag.Feedback = "The associate has not been created";

                    this.ModelState.AddModelError(
                        string.Empty,
                        "You have entered some invalid data, please check all highlighted fields on all tabs.");
                }
                else
                {
                    // get the username of the current user
                    string userName = this.MembershipService.GetCurrentUserName();

                    // If leading whitespace is entered in the email field this then causes an error when verifying email address
                    // as it calls GetAssociate and passes the trimmed email address so can't find it. Just trim this off now.
                    associate.Email = associate.Email.TrimStart();

                    associate.AssociateRegistrationTypeId = (byte?) AssociateRegistrationType.Agency;

                    associate.ReferralSourceId = (int) ReferralSourceId.Other;
                    associate.ReferralName = "Agency";

                    MembershipCreateStatus createStatus;

                    using (var scope = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions
                        {
                            IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
                        }))
                    {
                        createStatus = this.RegisterNewAssociate(associate);

                        scope.Complete();
                    }

                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        associateService.AddAssociateToAssociateRole(associate.Id);

                        var email = GetEmailModel(associate.EmailModel, associate.Id, EmailTemplate.None);

                        // confirmation url
                        var user = this.AssociateMembership.GetUser(associate.Email);

                        var confirmationGuid = user.ProviderUserKey.ToString();
                        var verifyUrl = this.GetAssociatePortalUrl() + "v/" + confirmationGuid;

                        email.Body = email.Body.Replace("CONFIRMURL", verifyUrl);

                        this.emailService.SendEmail(email);

                        return this.RedirectToAction("Index");
                    }
                }
            }
            catch (DataException ex)
            {
                this.ViewBag.Feedback = "Unexpected error.";

                ErrorSignal.FromCurrentContext().Raise(ex);
                this.ModelState.AddModelError(
                    string.Empty,
                    "Unable to save associate details. Please try again or contact your system administrator.");
            }

            SetCreateAssociateOptions(associate);

            return View(associate);
        }

        [HttpPost]
        public JsonResult CreateRateComment(int individualId, string rateName, string comment)
        {
            try
            {
                this.associateService.CreateRateComment(individualId, rateName, comment);
                return this.Json(true);
            }
            catch (Exception ex)
            {
                return this.Json(new Exception("Error creating invoice comment", ex));
            }
        }

        private EmailModel GetEmailModel(string emailModel, int associateid, EmailTemplate etemplate, object viewmodeldata = null)
        {
            // send the agency associate confirmation email
            var jss = new JavaScriptSerializer();
            Dictionary<string, object> d = jss.Deserialize<dynamic>(emailModel);


            var email = this.emailService.GetAssociateEmail(associateid, etemplate);


            //var email = new EmailModel();

            // BCCAddress
            if (d["BCCAddress"] != null)
            {
                email.BCCAddress = d["BCCAddress"].ToString();
            }

            // IsHtml
            if (d["IsHtml"] != null)
            {
                email.IsHtml = Boolean.Parse(d["IsHtml"].ToString());
            }

            // CCAddress
            if (d["CCAddress"] != null)
            {
                email.CCAddress = d["CCAddress"].ToString();
            }

            // Body
            if (d["Body"] != null)
            {
                email.Body = d["Body"].ToString();
            }

            // Subject
            if (d["Subject"] != null)
            {
                email.Subject = d["Subject"].ToString();
            }

            // ToAddress
            if (d["ToAddress"] != null)
            {
                email.ToAddress = d["ToAddress"].ToString();
            }

            // FromAddress
            if (d["FromAddress"] != null)
            {
                email.FromAddress = d["FromAddress"].ToString();
            }
            return email;
        }

        // GET: /Associate/
        // Get Associate Details
        // GET: /Associate/
        // Get Associate Details
        public ActionResult Details(
            int id,
            bool? isCompare,
            AssociateHistorySaveOperation versionA,
            AssociateHistorySaveOperation versionB,
            bool holdingBay = false)
        {
            bool comparing = isCompare.GetValueOrDefault();

            AssociateModel associate;

            if (comparing && versionA != null && versionB != null)
            {
                if (versionA.LastSaveTimeStamp > versionB.LastSaveTimeStamp)
                {
                    var tmp = versionA;
                    versionA = versionB;
                    versionB = tmp;
                }

                associate = this.associateService.GetAssociateVersion(
                    id, versionA.LastSaveTimeStamp, versionB.LastSaveTimeStamp);
            }
            else
            {
                comparing = false;

                associate = this.associateService.GetAssociateAndCheckLock(id);
            }

            this.SetOptionsForDetailsView(associate, false, comparing, versionB);


            ViewBag.ArchiveAssociate = SetUpArchiveAssociateModel();
            ViewBag.FromHoldingBay = holdingBay;

            ViewBag.VatRate = associate.VATRegistered.GetValueOrDefault()
                ? Convert.ToDecimal(ConfigurationManager.AppSettings["VatRate"])
                : 0;

            return View("Details", associate);
        }

        [Authorize]
        public ActionResult DownloadDocument(Guid? id, AssociateDocumentType? documentType, int? associateId)
        {
            try
            {
                if (documentType == AssociateDocumentType.CV && associateId.HasValue)
                {
                    return this.DownloadCV(associateId.Value);
                }

                if (id.HasValue)
                {
                    return this.DownloadDocument(id.Value, null, documentType != AssociateDocumentType.Photo);
                }

                return null;
            }            
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                this.ModelState.AddModelError(
                    string.Empty,
                    "An expect error has occcured. Unable to load document.");

                return null;
            }
        }

        [HttpPost]
        public ActionResult Details(
            [ModelBinder(typeof (AssociateModelBinder))] AssociateModel associate,
            AssociateHistorySaveOperation versionB,
            string deletedAccountants,
            string deletedAddresses,
            string deletedReferences,
            string notes,
            string selectedBusinessAreas)
        {
            if (!associate.HasValidActionName())
            {
                // Typing $("#mainForm")[0].submit(); into the console can do this for example.
                // in which case all validation is bypassed
                throw new ApplicationException(string.Format(@"Unexpected ActionName:""{0}"" ", associate.ActionName));
            }

            try
            {
                // Basic validation passed.
                if (!this.ModelState.IsValid)
                {
                    this.ViewBag.Feedback = "Your changes have NOT been saved.";

                    this.ModelState.AddModelError(
                        string.Empty,
                        "You have entered some invalid data, please check all highlighted fields on all tabs.");
                }
                else
                {
                    var comment = this.CreateComment(notes);

                    if (comment != null)
                    {
                        associate.Comments.Add(comment);
                    }

                    IEnumerable<int> accountantsToDelete = deletedAccountants.SplitToIntArray();
                    IEnumerable<int> addressesToDelete = deletedAddresses.SplitToIntArray();
                    IEnumerable<int> referencesToDelete = deletedReferences.SplitToIntArray();

                    IEnumerable<int> areaIds = selectedBusinessAreas.SplitToIntArray();

                    //Clear VAT if needed
                    if (associate.VATRegistered.HasValue)
                    {
                        if (!associate.VATRegistered.Value)
                        {
                            associate.VATRegistration = "";
                        }
                    }

                    // Do the Save            
                    var initialOptOutSelfBillingSignedDate = associate.OptOutSelfBillingSignedDate; // this will change if associate has changed company details
                    AssociateModel originalAssociate=this.associateService.GetAssociateAndCheckLock(associate.Id);
                    var initialOptOutSelfBilling = originalAssociate.OptOutSelfBilling;
                    var updatedAssociate = this.associateService.UpdateAssociate(associate, accountantsToDelete,
                        addressesToDelete, referencesToDelete, areaIds, Site.Admin);

                    //if there is a change and the change is to opt in
                    if (associate.OptOutSelfBilling != initialOptOutSelfBilling && !associate.OptOutSelfBilling)
                    {
                        //if you opt in
                        if (!associate.OptOutSelfBilling && associate.AssociateRegistrationTypeId == (byte)AssociateRegistrationType.Contract)
                        {
                            // if you haven't signed send email regarding change
                            if (associate.OptOutSelfBillingSignedDate == initialOptOutSelfBillingSignedDate)
                            //|| 
                            //        associate.VATRegistered != originalAssociate.VATRegistered || associate.VATRegistration != originalAssociate.VATRegistration ||
                            //        associate.DateVatRegistered != originalAssociate.DateVatRegistered || associate.RegisteredCompanyName != originalAssociate.RegisteredCompanyName)
                            {
                                //if you have never signed
                                if (associate.OptOutSelfBillingSignedDate == null)
                                {
                                    //check if previously sent
                                    //if (!this.emailService.HasOptInEmailBeenSent(associate.Id))
                                    //{
                                    SendNewOptOutSelfBillingSignatureRequiredEmail(associate, originalAssociate);
                                    //}
                                }
                            }
                        }

                    }

                    this.associateService.CheckAssociateDefaultDocument(associate.Id);
                    //if (updatedAssociate.OptOutSelfBilling != originalAssociate.OptOutSelfBilling ||
                    //    updatedAssociate.VATRegistered!=originalAssociate.VATRegistered||
                    //    updatedAssociate.VATRegistration!=originalAssociate.VATRegistration||
                    //    updatedAssociate.DateVatRegistered!=originalAssociate.DateVatRegistered||
                    //    updatedAssociate.RegistedCompanyBankAcctName != originalAssociate.RegistedCompanyBankAcctName ||
                    //    updatedAssociate.RegistedCompanyBankAcctNumber != originalAssociate.RegistedCompanyBankAcctNumber ||
                    //    updatedAssociate.RegistedCompanyBankAcctNumber != originalAssociate.RegistedCompanyBankAcctNumber)
                    //{
                    //    this.ViewBag.originalassociate = originalAssociate;
                    //    var id = associate.Id;
                    //    var msg = this.emailService.GetAssociateEmail(id, EmailTemplate.AdminAssociateSaveDetails, originalAssociate);
                    //    msg.ToAddress = associate.Email;
                    //    msg.IsHtml = true;
                    //    this.emailService.SendEmail(msg);
                    //}

                    var warnings = this.associateService.ValidateBusinessRules(associate);

                    // Preserve the locking user since this value is not stored in the Associate entity
                    // so the update will always return null.
                    //updatedAssociate.LockingUser = associate.LockingUser;
                    associate = updatedAssociate;
                    this.associateService.UpdateReferenceAdminEventHistoryForAssociate(associate);

                    /*
                     * Clear model state for these so that when the page is re-displayed it 
                     * passes new values for these. Not the values that were originally posted.
                     */
                    this.ModelState.Remove("PWRId");
                    this.ModelState.Remove("ReferencesJson");
                    this.ModelState.Remove("AddressHistoryJson");
                    this.ModelState.Remove("DateVatDeRegisteredRequired");
                    this.ModelState.Remove("HasBeenVatRegistered");

                    this.ViewBag.Feedback = "Your changes have been saved.";

                    // Do Accept if that was the requested action and the Model is fully valid
                    if (associate.ActionName == associate.ApproveAssociate)
                    {
                        this.associateService.ApproveAssociate(associate);

                        var filters = new StringBuilder();

                        filters.AppendFormat("{0}.", Convert.ToInt32(EmailTemplate.AssociateApprovedNotification));
                        filters.AppendFormat("{0}.",
                            Convert.ToInt32(EmailTemplate.AssociateRejectionInsufficientExperience));
                        filters.AppendFormat("{0}.", Convert.ToInt32(EmailTemplate.ExploringExperience));
                        filters.AppendFormat("{0}", Convert.ToInt32(EmailTemplate.AssociateRejectionVisa));

                        var routeValues =
                            new
                            {
                                associate.Id,
//                                AutoUnlocked = true,
                                filter = filters.ToString(),
                                sendEmail = true
                            };

                        return this.RedirectToAction("Details", routeValues);
                    }

                    if (associate.ActionName == associate.AcceptAssociate)
                    {
                        if (warnings == null)
                        {
                            this.associateService.AcceptAssociate(associate);

                            // return this.RedirectToAction("Index");
                            return this.RedirectToAction("Details", new {associate.Id, AutoUnlocked = true});
                        }

                        this.ViewBag.Feedback =
                            "Your changes have been saved but it was not possible to accept the associate.";

                        foreach (string warning in warnings)
                        {
                            this.ModelState.AddModelError(string.Empty, warning);
                        }

                        this.ModelState.AddModelError(
                            string.Empty, "Incomplete applications cannot be accepted");
                    }
                    else if (associate.ActionName == associate.SaveToITRIS)
                    {
                        this.associateService.UpdateAssociateToITRIS(associate,
                            this.MembershipService.GetCurrentUserId());

                        return this.RedirectToAction("Index");
                    }
                }
            }
            catch (DataException ex)
            {
                this.ViewBag.Feedback = "Unexpected error.";

                ErrorSignal.FromCurrentContext().Raise(ex);
                this.ModelState.AddModelError(
                    string.Empty,
                    "Unable to save associate details. Please try again or contact your system administrator.");
            }

            this.SetOptionsForDetailsView(associate, true, false, versionB);

            SetUpArchiveAssociateModel();

            // update model - as VAT not being updated
            ModelState.Clear();
            return View(associate);
        }

        public ActionResult Index(AssociateSearchModel search, GridSortOptions sort, string btnSearch, int page = 1,
            int pageSize = 10)
        {
            if (sort.Column == null)
            {
                if (string.IsNullOrEmpty(btnSearch))
                {
                    sort.Column = "CreatedDate";
                }
                else
                {
                    sort.Column = "LastName";
                }
            }

            this.ViewBag.CurrentUserId = this.MembershipService.GetCurrentUserId();

            var paginationDetails = new PaginationDetails
            {
                SortColumn = sort.Column,
                AscendingSort = sort.Direction == SortDirection.Ascending,
                PageNumber = page,
                PageSize = pageSize
            };

            var vm = new AssociateIndexModel {Search = search};

            var all = AssociateStatuses.GetAllAssociateApprovalStatuses(this.associateService);

            var statuses = AssociateStatuses.GetMainAssociateApprovalStatuses(all);

            var archive = AssociateStatuses.GetArchiveAssociateApprovalStatuses(all);

            var doNotUse = AssociateStatuses.GetDoNotUseAssociateApprovalStatuses(archive);

            var archiveActive = AssociateStatuses.GetArchiveActiveAssociateApprovalStatuses(archive);

            vm.Search.Statuses = statuses.ToList();

            // order the statuses by the Order property then the name
            vm.Search.ArchiveDoNotUseStatuses = doNotUse.ToList();
            vm.Search.ArchiveActiveStatuses = archiveActive.ToList();

            if (btnSearch == "Search")
            {
                GetStatuses(search, vm.Search.Statuses, "AssociateStatuses");
                GetStatuses(search, vm.Search.ArchiveDoNotUseStatuses, "ArchiveDoNotUse");
                GetStatuses(search, vm.Search.ArchiveActiveStatuses, "ArchiveActive");
            }
            else
            {
                foreach (var checkBoxItem in vm.Search.Statuses)
                {
                    search.SelectedStatuses.SelectedItems.Add(checkBoxItem);
                }
                foreach (var checkBoxItem in vm.Search.ArchiveDoNotUseStatuses)
                {
                    search.SelectedStatuses.SelectedItems.Add(checkBoxItem);
                }
            }

            var associates = this.associateService.GetPaginatedAssociates(search, paginationDetails);

            vm.PageSize = pageSize;

            vm.Associates = new CustomPagination<AssociateSummaryModel>(
                associates, page, pageSize, paginationDetails.ItemsCount);

            var statusCount = search.Statuses.Count;
            statusCount += search.ArchiveActiveStatuses.Count;
            statusCount += search.ArchiveDoNotUseStatuses.Count;

            this.ViewBag.FilterApplied = !(string.IsNullOrEmpty(search.FirstName) &&
                                           string.IsNullOrEmpty(search.LastName) &&
                                           string.IsNullOrEmpty(search.Email) &&
                                           search.SelectedStatuses.SelectedItems.Count() == statusCount);

            this.ViewData["sort"] = sort;
            this.ViewBag.FilterURL = this.Request.QueryString.ToString();

            return View(vm);
        }
        public JsonResult GetActivityUploadedDocuments(int associateId, string uploadedDocuments)
        {
   


            return Json(this.associateService.GetAssociateEmailUploadedDocuments(associateId, uploadedDocuments));
        }

        [HttpPost]
        public JsonResult SaveCommunication(int id, CommunicationType communicationType, string communicationDetails,
            string communicationSummary, string attachments)
        {
            this.associateService.SaveCommunication(
                new CommunicationHistoryModel
                {
                    AssociateId = id,
                    CommunicationType = communicationType,
                    Created = DateTime.Now,
                    Description = communicationSummary,
                    LoggedInUser = this.User.Identity.Name,
                    Details = communicationDetails
                }, attachments);

            return this.Json(true);
        }

        [HttpPost]
        public string CommunicationDetails(int id, CommunicationSource source)
        {
            return source == CommunicationSource.AssociateCommunication
                ? this.associateService.GetDetailsForAssociateCommunication(id)
                : this.RenderRazorViewToString("_SentEmail", this.associateService.GetAssociateSentEmail(id));
        }

        public JsonResult GetSentEmailAttachments(int id)
        {
            return Json(this.associateService.GetAssociateEmailDocuments(id));
        }
        public string RenderRazorViewToString(string viewName, object model)
        {
            this.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(this.ControllerContext, viewName);
                var viewContext = new ViewContext(this.ControllerContext, viewResult.View, this.ViewData, this.TempData,
                    sw);

                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(this.ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }

        [HttpPost]
        public JsonResult GetCommunicationsHistory(int id, int pageSize, int page, string sortColumn,
            string sortDirection, string searchText, bool activityType)
        {
            if (!new[] {"asc", "desc"}.Contains(sortDirection))
            {
                throw new ArgumentOutOfRangeException("sortDirection");
            }

            var validColumns = typeof (CommunicationHistoryModel).GetProperties();

            // ReSharper disable SimplifyLinqExpression
            if (!validColumns.Any(propertyInfo => propertyInfo.Name == sortColumn))
            {
                // ReSharper restore SimplifyLinqExpression
                throw new ArgumentOutOfRangeException("sortColumn");
            }

            int resultsCount;

            int associateId = id;
            int isAutomatic = 0;
            if (activityType)
                isAutomatic = 1;

            var result = this.associateService.GetPaginatedCommunicationsHistory(
                associateId,
                pageSize,
                page,
                sortColumn,
                sortDirection,
                searchText,
                out resultsCount, isAutomatic);

            return this.Json(new {TotalServerItems = resultsCount, Items = result});
        }

        [HttpPost]
        public JsonResult SaveVisualInspection(Guid id, bool approved, string comments)
        {
            var currentUserId = this.MembershipService.GetCurrentUserId();
            var currentUserName = this.MembershipService.GetUserNameById(currentUserId);

            var result = this.associateService.SaveVisualInspection(id, approved, comments, currentUserName);

            return this.Json(result);
        }

        public ActionResult HandleUploadedReference(int associateId, int referenceId, string fileGuid)
        {
            if (referenceId != 0)
            {
                var documentId = Guid.Parse(fileGuid);

                // Add reference details to mapping table if this is a 
                // reference that already exists on the server. Otherwise
                // the mapping isn't added until they save the reference.
                this.AddReferenceDocumentMapping(associateId, referenceId, documentId);
            }

            return UploadHelper.HandleUploadedDocument(fileGuid);
        }

        public ActionResult HandleUploadedTimesheetDocument(string fileGuid)
        {
            return UploadHelper.HandleUploadedDocument(fileGuid);
        }

        public ActionResult HandleUploadedInvoice(string fileGuid, int associateId)
        {
            return UploadHelper.HandleUploadedDocument(fileGuid);
        }

        [HttpPost]
        public JsonResult ArchiveAssociate(int id, AssociateApprovalStatus status, string reason)
        {
            this.associateService.ArchiveAssociate(id, status);

            if (reason != null && reason.Length > 0)
            {
                reason = status.ToString() + " - " + reason;
                this.SaveNotes(id, reason);
            }

            return Json("archived");
        }

        [HttpPost]
        public JsonResult ChangeToFullAssociate(int id, string model)
        {
            this.associateService.ChangeToFullAssociate(id);

            var m = this.associateService.GetAssociateOnly(id);
           
            var agency = ListItem.GetSelectListItems(
                this.associateService.GetAgencyOptions(),
                arg => m.AgencyId == arg.Value).First().Text;

            m.AgencyId = null;
            m.AssociateRegistrationTypeId = null;

            IEnumerable<int> accountantsToDelete = string.Empty.SplitToIntArray();
            IEnumerable<int> addressesToDelete = string.Empty.SplitToIntArray();
            IEnumerable<int> referencesToDelete = string.Empty.SplitToIntArray();
            IEnumerable<int> businessAreas = string.Empty.SplitToIntArray();


            // Do the Save            
           var initialOptOutSelfBillingSignedDate = m.OptOutSelfBillingSignedDate; // this will change if associate has changed company details
           var associate = this.associateService.UpdateAssociate(m, accountantsToDelete, addressesToDelete, referencesToDelete,
                 businessAreas, Site.Admin);

            if (associate.OptOutSelfBillingSignedDate != initialOptOutSelfBillingSignedDate)
            {
                // send email regarding change
                SendNewOptOutSelfBillingSignatureRequiredEmail(associate, (AssociateModel)m);

            }

            var email = GetEmailModel(model, id, EmailTemplate.FullAssociateConfirmation);

            var url = this.GetAssociatePortalUrl();

            email.Body = email.Body.Replace("CONFIRMURL", url);

            email.Body = email.Body.Replace("XXXXX", agency);

            this.emailService.SendEmail(email);

            return Json("full associate");
        }

        [HttpPost]
        public JsonResult UnArchiveAssociate(int id)
        {
            this.associateService.UnArchiveAssociate(id);

            return Json("unarchived");
        }

        [HttpPost]
        public bool MarkReferenceAsReceived(int id)
        {
            this.referenceService.MarkReferenceAsReceived(id);

            return true;
        }

        [HttpPost]
        public JsonResult SendReferenceCheck(int id)
        {
            var refCheckResult = this.referenceService.SendReferenceCheck(id);

            return this.Json(
                new
                {
                    ReferenceCheckBaseModel = refCheckResult.Item1,
                    AdministrativeEventForReference = refCheckResult.Item2
                });
        }

        [HttpPost]
        public bool ApproveReferenceCheck(int id, bool approve)
        {
            try
            {
                return this.referenceService.ApproveReferenceCheck(id, approve);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }

        [HttpPost]
        public bool SaveCommentForReference(int referenceId, string comment)
        {
            this.associateService.SaveCommentForReference(this.CreateComment(comment), referenceId);

            return true;
        }

        [HttpPost]
        public JsonResult SaveNotes(int id, string notes)
        {
            var comment = this.CreateComment(notes);

            this.associateService.SaveComment(comment, id);

            return this.Json(
                new
                {
                    comment.CommentId,
                    CreatedTime = comment.CreatedTime.ToString("dd/MM/yyyy"),
                    comment.Text,
                    comment.User
                });
        }

        [HttpPost]
        public void SaveProjectEmail(int id, string email)
        {
            this.associateService.SaveProjectEmail(id, email);
        }

        public ActionResult HandleUploadedDocument(string fileGuid)
        {
            return UploadHelper.HandleUploadedDocument(fileGuid);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public JsonResult CheckEmailAddressValidAndNotInUse(string email)
        {
            return !EmailValidator.IsValidEmail(email)
                ? this.Json("Please enter a valid email address.", JsonRequestBehavior.AllowGet)
                : (this.AssociateMembership.GetUserNameByEmail(email) != null)
                    ? this.Json("This email address is already registered.", JsonRequestBehavior.AllowGet)
                    : (IsEmailAdddressOnBlackList(email))
                        ? this.Json("This email address is blacklisted.", JsonRequestBehavior.AllowGet) 
                        : this.Json(true, JsonRequestBehavior.AllowGet);
        }

        private bool IsEmailAdddressOnBlackList(string domain)
        {
            return this.emailService.IsEmailAdddressOnBlackList(domain);
        }

        public JsonResult CheckEmailAddressIsEmailAdddressOnBlackList(string email)
        {
            return !EmailValidator.IsValidEmail(email)
                    ? this.Json("Please enter a valid email address.", JsonRequestBehavior.AllowGet)
                        : (IsEmailAdddressOnBlackList(email))
                            ? this.Json("This email address is blacklisted.", JsonRequestBehavior.AllowGet) 
                            : this.Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddEmailToBlackList(string domain)
        {
            try
            {
                this.emailService.AddEmailToBlackList(domain);
                return this.Json(true);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return this.Json(false);
            }
        }

        [HttpPost]
        public JsonResult RemoveEmailToBlackList(string domain)
        {
            try
            {
                this.emailService.RemoveEmailToBlackList(domain);
                return this.Json(true);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return this.Json(false);
            }
        }

        [HttpPost]
        public JsonResult GetBlackList()
        {
            var result = this.emailService.GetBlackList();
            return this.Json(result);
        }

        [HttpPost]
        public JsonResult ApproveDocument(string documentId)
        {
            try
            {
                var approvalDate = DateTime.Now;

                this.associateService.ApproveDocument(new Guid(documentId), this.User.Identity.Name, approvalDate);

                return this.Json(new {date = approvalDate.ToShortDateString(), user = this.User.Identity.Name});
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return null;
            }
        }

        [HttpPost]
        public JsonResult GetApprovers(int? clientId = null, int? individualId = null)
        {
            try
            {
                ClientContactRole[] rolesOfInterest =
                {
                    ClientContactRole.TimesheetApprover
                };

                if (clientId != null)
                {
                    var clientContacts = this.clientProjectSharedService.GetClientContactsInRoles(clientId.Value,
                        rolesOfInterest);
                    var timesheetApproverContacts = this.clientProjectSharedService
                        .FilterClientContactsByRoleForSelectList(
                            clientContacts,
                            ClientContactRole.TimesheetApprover);

                    return this.Json(timesheetApproverContacts);
                }

                if (individualId != null)
                {
                    var clientContacts =
                        this.clientProjectSharedService.GetClientContactsInRolesForIndividual(individualId.Value,
                            rolesOfInterest);
                    var timesheetApproverContacts = this.clientProjectSharedService
                        .FilterClientContactsByRoleForSelectList(
                            clientContacts,
                            ClientContactRole.TimesheetApprover);

                    return this.Json(timesheetApproverContacts);
                }

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return null;
            }
        }

        #region Individual Tab Methods

        [HttpPost]
        public JsonResult GetIndividual(int id)
        {
            IndividualModel individual = this.individualService.GetIndividual(id);

            RoleModel role = this.roleService.GetRole(individual.RoleId);
            var data = new
            {
                individual = individual,
                roleEndDate = ((DateTime) role.EndDate).ToString("dd/MM/yyyy")
            };

            return this.Json(data);
        }

        [HttpPost]
        public JsonResult GetScheduledChangeFields(int id)
        {
            return this.Json(this.individualService.GetScheduledChangeFields(id));
        }

        [HttpPost]
        public JsonResult GetIndividualRecordsForAssociate(int id, int pageSize, int skip, List<SortDescription> sort)
        {
            int? resultsCount;

            int associateId = id;

            var individuals = this.individualService.GetIndividualsByAssociateId(
                associateId,
                pageSize,
                skip,
                sort,
                false,
                out resultsCount);

            return this.Json(new {total = resultsCount, data = individuals});
        }

        [HttpPost]
        public JsonResult GetBusinessTypeChangeFields(int id)
        {
            return this.Json(true);
        }


        [HttpPost]
        public JsonResult GetPastIndividualRecordsForAssociate(int id, int pageSize, int skip,
            List<SortDescription> sort)
        {
            int? resultsCount;

            int associateId = id;

            var individuals = this.individualService.GetIndividualsByAssociateId(
                associateId,
                pageSize,
                skip,
                sort,
                true,
                out resultsCount);

            return this.Json(new {total = resultsCount, data = individuals});
        }

        [HttpPost]
        public JsonResult GetAssociateProspects(int id, int pageSize, int skip, List<SortDescription> sort)
        {
            int? resultsCount;

            int associateId = id;

            var prospects = this.associateService.GetAssociateProspects(
                associateId,
                pageSize,
                skip,
                sort,
                out resultsCount);

            return this.Json(new {total = resultsCount, data = prospects});
        }

        [HttpPost]
        public JsonResult GetRetentionPeriods()
        {
            return this.Json(this.roleService.GetRoleRetentionPeriodList());
        }

        [HttpPost]
        public JsonResult UpdateIndividual(IndividualModel item, string clauses)
        {
            try
            {
                item = SavePerformanceClauses(item, clauses);

                this.individualService.UpdateIndividual(item);

                return this.Json(true);
            }
            catch (Exception ex)
            {
                string Message = ex.InnerException.Message;
                if (Message.Contains("Retainer timesheet"))
                {
                    ErrorSignal.FromCurrentContext().Raise(ex);
                    Message = "Data not saved, valid retainer timesheet already exist with-in this date range.";
                    return this.Json(Message);
                    ;
                }
                ErrorSignal.FromCurrentContext().Raise(ex);
                return this.Json(Message);
            }
        }

        [HttpPost]
        public JsonResult GetBusinessAreas(int associateId)
        {
            var data = this.associateService.GetBusinessAreaOptions();
            var value = this.associateService.GetAssociateBusinessAreas(associateId);

            if (value != null)
            {
                var result = new {data = data, value = value};
                return this.Json(result);
            }
            else
            {
                var result = new {data = data};
                return this.Json(result);
            }
        }

        [HttpPost]
        public JsonResult GetPerformanceClauses(int id)
        {
            var data = this.clientProjectSharedService.GetPerformanceClauses();
            var value =
                (from c in this.individualService.GetPerformanceClauses(id) select c.PerformanceClauseId).ToArray();

            var result = new {data = data, value = value};
            return this.Json(result);
        }

        [HttpPost]
        public JsonResult GetPerformanceClause(int id)
        {
            try
            {
                return this.Json(this.individualService.GetPerformanceClause(id));
            }
            catch
            {
                return null;
            }
        }

        [HttpPost]
        public JsonResult GetHistoricalRates(int individualId)
        {
            return this.Json(this.associateService.GetHistoricalRates(individualId));
        }

        #endregion

        #region Scheduler actions

        [OverrideAuthorize(Roles = RoleName.TimesheetActions)]
        [HttpPost]
        public HttpStatusCodeResult UpdateIndividualFieldSingle(string jsonArgument)
        {
            try
            {
                individualService.UpdateIndividualFieldSingle(jsonArgument);
                CreateSucessfullMessage();
                return new HttpStatusCodeResult(200, "Success");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        //[OverrideAuthorize(Roles = RoleName.TimesheetActions)]
        //[HttpPost]
        public HttpStatusCodeResult UpdateIndividualExpense(string jsonArgument)
        {
            try
            {
                individualService.UpdateIndividualExpense(jsonArgument);
                CreateSucessfullMessage();
                return new HttpStatusCodeResult(200, "Success");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        public HttpStatusCodeResult UpdateIndividualIncentiveTest()
        {
            try
            {
                var jsonArgument =
                    "[{'field':'IncentiveDaysCountedAs','value':4,'taskId':'423'},{'field':'IncentiveDaysIn7','value':0,'taskId':'423'},{'field':'IncentiveDaysMaxWorked','value':'','taskId':'423'},{'field':'IncentiveCharge','value':'','taskId':'423'},{'field':'IncentivePayAway','value':'','taskId':'423'},{'field':'OverTimeCharge','value':'','taskId':'423'},{'field':'OverTimePayAway','value':'','taskId':'423'},{'field':'OverproductionCharge','value':'','taskId':'423'},{'field':'OverproductionPayAway','value':'','taskId':'423'},{'field':'OneOffPmtAmount','value':'','taskId':'423'},{'field':'OneofPaymentDate','value':'','taskId':'423'}]";

                individualService.UpdateIndividualIncentive(jsonArgument);
                CreateSucessfullMessage();
                return new HttpStatusCodeResult(200, "Success");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        [OverrideAuthorize(Roles = RoleName.TimesheetActions)]
        [HttpPost]
        public HttpStatusCodeResult UpdateIndividualIncentive(string jsonArgument)
        {
            try
            {
                individualService.UpdateIndividualIncentive(jsonArgument);
                CreateSucessfullMessage();
                return new HttpStatusCodeResult(200, "Success");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }


        [OverrideAuthorize(Roles = RoleName.TimesheetActions)]
        [HttpPost]
        public HttpStatusCodeResult UpdateIndividualRates(string jsonArgument)
        {
            try
            {
                individualService.UpdateIndividualRates(jsonArgument);
                CreateSucessfullMessage();
                return new HttpStatusCodeResult(200, "Success");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        [OverrideAuthorize(Roles = RoleName.TimesheetActions)]
        public ActionResult CheckSelfBillingAgreementExpired()
        {
            try
            {
                var associates = this.associateService.GetExpiredSelfBillingAgreements();

                SendSelfBillingAgreementExpiredEmail(associates);

                return new HttpStatusCodeResult(200, "Success");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        [OverrideAuthorize(Roles = RoleName.TimesheetActions)]
        public HttpStatusCodeResult ReviewAssociateContractStatus()
        {
            try
            {
                this.associateService.CheckAssociateContractStatus();
                var msg = "contract status checked";

                return new HttpStatusCodeResult(200, msg);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }


        private void SendNewOptOutSelfBillingSignatureRequiredEmail(AssociateModel associate, AssociateModel originalassociate)
        {
            var id = associate.Id;
            var msg = this.emailService.GetAssociateEmail(id, EmailTemplate.NewOptOutSelfBillingSignatureRequired, originalassociate);
            msg.ToAddress = associate.Email;
            this.emailService.SendEmail(msg);
        }

        private void SendSelfBillingAgreementExpiredEmail(List<AssociateExpiredSelfBillingAgreementModel> associates)
        {
            foreach (var associate in associates)
            {
                var id = associate.AssociateId;
                var msg = this.emailService.GetAssociateEmail(id, EmailTemplate.SelfBillingAgreementExpired);
                msg.ToAddress = associate.Email;
                this.emailService.SendEmail(msg);   
            }          
        }

        private void CreateSucessfullMessage()
        {
            string message = "Successfull";
            UTF8Encoding encoding = new UTF8Encoding();
            var bytes = encoding.GetBytes(message);
            Response.OutputStream.Write(bytes, 0, bytes.Length);
        }       

        [HttpPost]
        public HttpStatusCodeResult UpdateIndividualFieldSingleTEst(string jsonArgument)
        {
            string message = "Successfull";
            UTF8Encoding encoding = new UTF8Encoding();
            var bytes = encoding.GetBytes(message);
            Response.OutputStream.Write(bytes, 0, bytes.Length);
            return new HttpStatusCodeResult(200, "val");
        }

        public HttpStatusCodeResult UpdateIndividualFieldSingleTEst2()
        {
            // {"field":"EndDate","value":"20141003","taskId":"422"}
            var obj = new { taskId = 422, field = "EndDate", value = "20141003" };
            var jsonArgument = new JavaScriptSerializer().Serialize(obj);

            individualService.UpdateIndividualFieldSingle(jsonArgument);
            CreateSucessfullMessage();
            return new HttpStatusCodeResult(200, "Success");
        }

 #endregion

        #region Assessments actions

        [HttpPost]
        public JsonResult GetAssessments(int associateId) 
        {
            return this.Json(this.associateService.GetAssessments(associateId));
        }


        [HttpPost]
        public JsonResult UpdateAssessment(AssessmentModel assessment)
        {
            try
            {
                this.associateService.UpdateAssessment(assessment);
                return this.Json(true);
            }
            catch
            {
                return this.Json(false);
            }
        }

        [HttpPost]
        public JsonResult CreateAssessment(AssessmentModel assessment)
        {
            try
            {                
                if (assessment.AssessmentId != 0)
                {
                    return UpdateAssessment(assessment);
                }
                else
                {

                byte score = (assessment.Score == null ? (byte)0 : (byte)assessment.Score);
                    this.associateService.CreateAssessment(assessment.AssociateId, assessment.AssessmentTypeId, assessment.AssessmentDate, assessment.Pass, score, assessment.Comment, assessment.Documents);
                }
                return this.Json(true);
            }
            catch (Exception)
            {
                return this.Json("There was an error adding the assessment.");
            }
        }

        [HttpPost]
        public JsonResult UpdateDocumentMain(AssociateDocumentModel doc, string userName)
        {
            try
            {
                this.documentService.UpdateDocumentMain(doc, userName);
                return this.Json(true);
            }
            catch
            {
                return this.Json(false);
            }
        }

        [HttpPost]
        public JsonResult CreateAssociateDocumentRequirment(int associateId, int docTypeId)
        {
            try
            {
                this.associateService.CreateAssociateDocumentRequired(associateId, docTypeId);
                return this.Json(true);
            }
            catch (Exception)
            {
                return this.Json("There was an error adding the document.");
            }
            return this.Json(false);
        }

        [HttpPost]
        public JsonResult CreateDocument(AssociateDocumentModel doc)
        {
            try
            {
                string userName = this.MembershipService.GetCurrentUserName();
                if (doc.DocumentId != null && doc.DocumentId != Guid.Empty)
                {
                    return UpdateDocumentMain(doc, userName);
                }
                else
                {

                    this.documentService.CreateDocument(doc, userName);
                }
                return this.Json(true);
            }
            catch (Exception)
            {
                return this.Json("There was an error adding the document.");
            }
            return this.Json(false);
        }

        [HttpPost]
        public JsonResult UpdateDocumentGrid(string id, bool qCval, bool iVal)
        {
            try
            {
                string userName = this.MembershipService.GetCurrentUserName();
                Guid docId = new Guid(id);
                this.documentService.UpdateDocumentGrid(docId, qCval, iVal, userName);
                return this.Json(true);
            }
            catch (Exception e)
            {
                return this.Json("There was an error updating the document. "+ e.InnerException + " "+ e.StackTrace);
            }

        }

        [HttpPost]
        public JsonResult GetAssessmentTypes()
        {
            return this.Json(this.associateService.GetAssessmentTypes());
        }

        [HttpPost]
        public JsonResult GetDocumentTypes()
        {
            return this.Json(this.documentService.GetDocumentTypes().OrderBy(a => a.Description));
        }

        [HttpPost]
        public JsonResult GetBusinessTypes()
        {
            var list = this.associateService.GetBusinessTypes();
            return this.Json(list);
        }

        [HttpGet]
        public ActionResult Assessments()
        {
            
           //IEnumerable<AssessmentDocumentModel> assessmentDocuments= this.assessmentService.GetAssessmentDocuments(associateId, assessmentId);
           //this.ViewBag.AssessmentDocuments = assessmentDocuments;
            this.ViewBag.UploaderHtml = UploadHelper.GetHtml(
                "uploadButton",
                MomentaRecruitment.Common.Properties.Settings.Default.AllowedFileExtensions,
                "~/UploadHandler.axd?Mode=Default",
                "assessmentDocumentUploader",
                "Browse");
            return this.PartialView("Assessments");
        }

        [HttpGet]
        public ActionResult Documents(int associateId)
        {
            ViewBag.CurrentAssociateModel = this.associateService.GetAssociate<AssociateModel>(associateId);
            this.ViewBag.UploaderHtml = UploadHelper.GetHtml(
                "uploadButton",
                MomentaRecruitment.Common.Properties.Settings.Default.AllowedFileExtensions,
                "~/UploadHandler.axd?Mode=Default",
                "associateDocumentUploader",
                "Browse");
            return this.PartialView("Documents");
        }

        #endregion

        #region Private Methods

        private static string GetCompareToolMessage(
            bool lockedByCurrentUser,
            bool isPostBack,
            bool isCompare,
            bool mostCurrentVersion,
            bool associateChangeAfterAdmin)
        {
            if (isCompare)
            {
                if (mostCurrentVersion)
                {
                    return "The versions you are comparing include the most recent.";
                }

                return lockedByCurrentUser
                    ? "The versions you are comparing do not include the most recent changes therefore this record is not editable."
                    : // Don't mention editability as they couldn't edit this anyway.
                    "The versions you are comparing do not show the most recent changes.";
            }

            // If we are not doing a compare there is no message displayed, except to alert them changes have been made that they might
            // wish to review.
            return isPostBack || !associateChangeAfterAdmin
                ? null
                : "This record has been edited by the associate since the last time it was saved by an administrator. Use the compare tool to review their changes";
        }

        private string GetAssociatePortalUrl()
        {
            return new Uri(Admin.Properties.Settings.Default.AssociatePortalUrl).AbsoluteUri;
        }

        private MembershipCreateStatus RegisterNewAssociate(AssociateModel associate)
        {
            var password = "Password";

            // Attempt to register the user
            MembershipCreateStatus createStatus;
            var user = this.AssociateMembership.CreateUser(
                associate.Email, password, associate.Email, null, null, false, null, out createStatus);

            if (createStatus == MembershipCreateStatus.Success)
            {
                // put the associate into the associate role

                Debug.Assert(user.ProviderUserKey != null, "user.ProviderUserKey != null");

                associate.Id = this.associateService.CreateAssociate(associate, (Guid)user.ProviderUserKey);
            }

            return createStatus;
        }

        private void SetCreateAssociateOptions(AssociateCreateModel associate)
        {
            // titles
            var titles = ListItem.GetSelectListItems(
                this.associateService.GetAssociateTitleOptions(),
                arg => associate != null && associate.PersonTitleId == arg.Value);

            var selectTitleOption = new SelectListItem { Selected = true, Text = "-- Select --", Value = string.Empty };
            titles = titles.Insert(selectTitleOption, 0);

            this.ViewBag.Titles = titles;

            // agency
            var agency = ListItem.GetSelectListItems(
                this.associateService.GetAgencyOptions(),
                arg => associate.AgencyId.HasValue && associate.AgencyId == arg.Value);

            var selectAgencyOption = new SelectListItem { Selected = true, Text = "-- Select --", Value = string.Empty };
            agency = agency.Insert(selectAgencyOption, 0);
            this.ViewBag.Agency = agency;

            this.ViewBag.Title = "Create Associate";
        }

        private void GetStatuses(AssociateSearchModel search, IList<AdminListItem> statusList, string name)
        {
            var statuses = this.Request.QueryString.GetValues(name);

            if (statuses != null)
            {
                foreach (string status in statuses)
                {
                    if (status.Contains(','))
                    {
                        foreach (string t in status.Split(','))
                        {
                            search.SelectedStatuses.SelectedItems.Add(statusList.First(s => s.Id == byte.Parse(t)));
                        }
                    }
                    else
                    {
                        search.SelectedStatuses.SelectedItems.Add(statusList.First(s => s.Id == byte.Parse(status)));
                    }
                }
            }
        }

        private AssociateArchiveModel SetUpArchiveAssociateModel()
        {
            var all = AssociateStatuses.GetAllAssociateApprovalStatuses(this.associateService);

            var archive = AssociateStatuses.GetArchiveAssociateApprovalStatuses(all);

            var doNotUse = AssociateStatuses.GetDoNotUseAssociateApprovalStatuses(archive);

            var archiveActive = AssociateStatuses.GetArchiveActiveAssociateApprovalStatuses(archive);

            var archiveModel = new AssociateArchiveModel();

            archiveModel.ArchiveActiveStatuses = archiveActive.OrderBy(x => x.Order).ThenBy(x => x.Name).ToList();
            archiveModel.ArchiveDoNotUseStatuses = doNotUse.OrderBy(x => x.Order).ThenBy(x => x.Name).ToList();

            return archiveModel;
        }

        private ActionResult DownloadCvVersion(Guid id)
        {
            var cv = this.associateService.GetCvVersion(id);

            return FileStreamHelper.FileDownloadModelResult(cv, this.HttpContext, true);
        }

        private void SetOptionsForDetailsView(
            AssociateModel associate, 
            bool isPostBack, 
            bool isCompare, 
            AssociateHistorySaveOperation versionB)
        {
            bool associateChangeAfterAdmin;

            int associateId = associate.Id;
            var saveOperations =
                this.associateService.GetAssociateCollapsedEditHistory(associateId, out associateChangeAfterAdmin);

            this.ViewBag.AssociateHistorySaveOperations = saveOperations;

            this.ViewBag.EditedByAssociateAfterAdmin = associateChangeAfterAdmin;

            var mostCurrentVersion = !saveOperations.Any()
                ? true
                : (versionB.LastSaveTimeStamp == saveOperations.Last().LastSaveTimeStamp);

            var currentUserId = this.MembershipService.GetCurrentUserId();

            var lockedByCurrentUser = associate.LockingUser.HasValue && currentUserId == associate.LockingUser;

            this.ViewBag.CompareToolMessage = GetCompareToolMessage(
                lockedByCurrentUser, isPostBack, isCompare, mostCurrentVersion, associateChangeAfterAdmin);

            this.ViewBag.Readonly = !lockedByCurrentUser || (isCompare && !mostCurrentVersion);

            this.ViewBag.IsCompare = isCompare;

            if (associate.LockingUser.HasValue && currentUserId != associate.LockingUser)
            {
                this.ViewBag.LockMessage = "This record is currently locked by "
                                           + this.MembershipService.GetUserNameById(associate.LockingUser.Value);
            }

            this.ViewBag.AdminUserFoundInItris =
                !string.IsNullOrEmpty(this.associateService.GetItrisEmployeeIdFromMembershipId(currentUserId));

            if (associate.ApprovalStatus != AssociateApprovalStatus.PendingApproval)
            {
                this.ViewBag.warnings = this.associateService.ValidateBusinessRules(associate);
            }

            this.ViewBag.MomentaEmployeeList = this.employeeService.GetMomentaEmployeeList();

            this.ViewBag.AssociateOwnerList = this.employeeService.GetMomentaEmployeeListForRole(RoleName.AssociateOwner);

            this.ViewBag.VettingContactList = this.employeeService.GetMomentaEmployeeListForRole(RoleName.Vetting);

            this.ViewBag.BusinessUnits = ListItem.GetSelectListItems(
                this.associateService.GetBusinessUnitOptions(),
                arg => associate.BusinessUnitId.HasValue && (byte)associate.BusinessUnitId == arg.Value,
                true);

            this.ViewBag.ReferralSources = this.associateService.GetReferralSourceOptions();

            this.ViewBag.DocumentationWarning = this.SetContractingDocumentationStatus(associate);

            this.ViewBag.IsApprover = this.associateService.AssociateIsApprover(associateId);

            if (associate.AssociateRegistrationTypeId == (byte)AssociateRegistrationType.Agency)
            {
                // agency
                var agency = ListItem.GetSelectListItems(
                    this.associateService.GetAgencyOptions(),
                    arg => associate.AgencyId.HasValue && associate.AgencyId == arg.Value);

                this.ViewBag.Agency = agency;
            }

            this.ViewBag.Roles = this.roleService.GetRoleTypeList();

            this.SetOptionsForDetailsView(associate);
        }

        private CommentModel CreateComment(string notes)
        {
            return string.IsNullOrEmpty(notes) || notes.Length > 500
                ? null
                : new CommentModel
                {
                    Text = notes,
                    CreatedTime = DateTime.Now,
                    User = this.MembershipService.GetCurrentUserId()
                };
        }

        private string SetContractingDocumentationStatus(MomentaRecruitment.Common.Models.AssociateModel associate)
        {
            var warning = new StringBuilder();

            switch ((BusinessType?)associate.BusinessTypeId)
            {
                case BusinessType.ContractLtdCompany:
                    {
                        var document = associate.Documents.Where(d => d.DocumentType == AssociateDocumentType.CertificateOfIncorporation).OrderByDescending(d => d.CreatedDate).FirstOrDefault();
                        
                        if (document == null)
                        {
                            warning.Append("<li>Certificate Of Incorporation for Ltd/LLP required.</li>");
                        }
                        else if (document.ApprovedDate == null)
                        {
                            warning.Append("<li>Certificate Of Incorporation review for Ltd/LLP needs review and approval.</li>");
                        }

                        document = associate.Documents.Where(d => d.DocumentType == AssociateDocumentType.ProfessionalIndemnity).OrderByDescending(d => d.CreatedDate).FirstOrDefault();
                        if (document == null)
                        {
                            warning.Append("<li>Professional Indemnity document for Ltd/LLP required.</li>");
                        }
                        else if (document.ApprovedDate == null)
                        {
                            warning.Append("<li>Professional Indemnity document for Ltd/LLP needs review and approval.</li>");
                        }
                    }

                    break;
                case BusinessType.Umbrella:
                    {
                        var document = associate.Documents.Where(d => d.DocumentType == AssociateDocumentType.UmbrellaConfirmation).OrderByDescending(d => d.CreatedDate).FirstOrDefault();
                        
                        if (document == null)
                        {
                            warning.Append("<li>Umbrella Company Confirmation required.</li>");
                        }
                        else if (document.ApprovedDate == null)
                        {
                            warning.Append("<li>Umbrella Company Confirmation needs review and approval.</li>");
                        }
                    }

                    break;
            }

            return warning.ToString();
        }

        private IndividualModel SavePerformanceClauses(IndividualModel item, string clauses)
        {
            try
            {
                item.PerformanceClauses = new List<MomentaRecruitment.Common.Models.PerformanceClauseModel>();

                if (clauses.Trim().Length > 0)
                {
                    var clauseIds = (clauses ?? "").Split(',').Select<string, int>(int.Parse).ToList<int>();

                    foreach (int id in clauseIds)
                    {
                        item.PerformanceClauses.Add(this.individualService.GetPerformanceClause(id));
                    }
                }
            }
            catch (Exception)
            {
                //do nothing - invalid clause data
            }

            return item;
        }       

        #endregion

        protected override void Dispose(bool disposing)
        {
            this.associateService.Dispose();
            base.Dispose(disposing);
        }
        //[HttpPost]
        //public JsonResult GetAssessmentDocuments(int associateId, int assessmentId)
        //{
        //    MomentaRecruitment.Admin.Repositories.AssessmentRepository assessmentRepository=new assess
        //    IAssessmentService associateService = new AssessmentService();

        //    return this.Json(associateService.GetAssessmentDocuments(associateId, assessmentId));
        //}
        [HttpPost]
        public JsonResult SaveAssessmentDocument(Guid FileDocId, int AssessmentTypeId)
        {
            try
            {

                this.associateService.SaveAssessmentDocument(FileDocId, AssessmentTypeId);
                return this.Json(true);
            }
            catch (Exception)
            {
                return this.Json("There was an error adding the assessment.");
            }
        }
    }


}