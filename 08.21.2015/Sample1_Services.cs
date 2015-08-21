using System.Collections;
using MR_DAL.Enumerations;

namespace Admin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Transactions;
    using System.Web.Script.Serialization;

    using Admin.GBGroupService;
    using Admin.Helpers;
    using Admin.Models;
    using Admin.Properties;

    using Elmah;

    using MomentaRecruitment.Common.Exceptions;
    using MomentaRecruitment.Common.Models;
    using MomentaRecruitment.Common.ViewModel;
    using MomentaRecruitment.Common.Repositories;
    using SortDescription = MomentaRecruitment.Common.Helpers.SortDescription;

    using MoreLinq;

    using MR_DAL;

    using AssociateApprovalStatus = MR_DAL.Enumerations.AssociateApprovalStatus;
    using AssociateModel = Admin.Models.AssociateModel;
    using EmailTemplate = MomentaRecruitment.Common.Enumerations.EmailTemplate;
    using IAssociateRepository = Admin.Repositories.IAssociateRepository;
    using ISchedulerRepository = Admin.Repositories.ISchedulerRepository;
    using System.Reflection;
    using System.Web.Mvc;
    using System.Web;
    using MomentaRecruitment.Common.Services.Scheduler;

    public interface IAssociateService : MomentaRecruitment.Common.Services.IAssociateService
    {
        IEnumerable<int> GetIndividualIdsForProject(IEnumerable<int> associateIds, int projectId);

        IEnumerable<MomentaRecruitment.Common.Helpers.ListItem<byte>> GetBusinessUnitOptions();

        void AcceptAssociate(AssociateModel associate);

        int CreateAssociate(AssociateModel associate, Guid membershipUserId);

        void CreateAssociateDocumentRequired(int associateId, int docType);

        void SaveAssessmentDocument(Guid FileDocId, int assessmentTypeId);
        void ApproveAssociate(AssociateModel associate);

        void UpdateAssociateToITRIS(AssociateModel associate, Guid currentUserId);

        void UpdateReferenceAdminEventHistoryForAssociate(AssociateModel associate);

        AssociateModel GetAssociateAndCheckLock(int id);

        IEnumerable<AssociateHistorySaveOperation> GetAssociateCollapsedEditHistory(
            int associateId, out bool associateChangeAfterAdmin);

        AssociateModel GetAssociateVersion(int id, DateTime from, DateTime to);        

        FileDownloadModel GetCvVersion(Guid id);

        string GetItrisEmployeeIdFromMembershipId(Guid userId);

        IEnumerable<AssociateSummaryModel> GetPaginatedAssociates(AssociateSearchModel search, PaginationDetails paginationDetails);

        void ArchiveAssociate(int id, AssociateApprovalStatus status);

        void UnArchiveAssociate(int id);

        void ChangeToFullAssociate(int id);

        void CreateBusinessTask(int id, DateTime scheduledDate, string field, string value);

        //void UpdateBusinessTypeData(Dictionary<string, string> updates);
        void UpdateBusinessTypeData(string jsonArgument);
        bool SaveVisualInspection(Guid documentId, bool approved, string comments, string userName);

        void SaveComment(CommentModel comment, int clientId);

        void ApproveDocument(Guid documentId, string userName, DateTime approvalDate);

        void SaveCommentForReference(CommentModel comment, int referenceId);

        AssociateModel GetAssociateOnly(int id);

        //Admin.Models.AssociateSchedulerModel GetAssociateForScheduler(int id);
        List<CommunicationHistoryModel> GetPaginatedCommunicationsHistory(int associateId, int pageSize, int page, string sortColumn, string sortDirection, string searchText, out int resultsCount, int isAutomatic);

        string GetDetailsForAssociateCommunication(int associateCommunicationId);

        EmailModel GetAssociateSentEmail(int id);

        IEnumerable<AssociateEmailDocumentModel> GetAssociateEmailUploadedDocuments(int associateId, string newlyUploadedDocs);

        IEnumerable<AssociateEmailDocumentModel> GetAssociateEmailDocuments(int id);

        void SaveCommunication(CommunicationHistoryModel communicationHistoryModel, string attachments);

        void AddInvoiceHistory(int invoiceId, string user, string comments);

        List<GetAssociateProspects_Result> GetAssociateProspects(int associateId, int pageSize, int skip, List<SortDescription> sort, out int? resultsCount);

        IEnumerable<HistoricalRatesViewModel> GetHistoricalRates(int individualId);

        void CreateRateComment(int individualId, string rateName, string comment);

        string GetUmbrellaCompanyName(byte? umbrellaCompanyId);

        void CheckAssociateContractStatus();

        bool GetDocumentVATApprovalStatus(AssociateDocumentType vatCertificate, int id);

        List<AssociateExpiredSelfBillingAgreementModel> GetExpiredSelfBillingAgreements();

        void CheckAssociateDefaultDocument(int associateId);

        Associate GetAssociatesById(int associateId);

        List<BusinessType> GetBusinessTypes();
    }

    public class AssociateService : MomentaRecruitment.Common.Services.AssociateService, IAssociateService
    {
        private readonly List<AssociateApprovalStatus> nonArchiveStatuses;

        private readonly IAssociateRepository associateRepo;

        private readonly IAssociateReferenceRepository associateRefRepo;

        private readonly ISchedulerRepository schedulerRepo;

        private readonly IDataMapper dataMapper;

        private readonly IEmailService emailService;

        public AssociateService(IAssociateRepository repository, IDataMapper mapper, IEmailService emailService, IAssociateReferenceRepository associateRefRepo, ISchedulerRepository schedulerRepo)
            : base(repository, mapper)
        {
            this.associateRepo = repository;
            this.dataMapper = mapper;
            this.emailService = emailService;
            this.associateRefRepo = associateRefRepo;
            this.schedulerRepo = schedulerRepo;
            this.nonArchiveStatuses = new List<AssociateApprovalStatus>();

            this.nonArchiveStatuses.Add(AssociateApprovalStatus.Accepted);
            this.nonArchiveStatuses.Add(AssociateApprovalStatus.Approved);
            this.nonArchiveStatuses.Add(AssociateApprovalStatus.NotSet);
            this.nonArchiveStatuses.Add(AssociateApprovalStatus.PendingAcceptance);
            this.nonArchiveStatuses.Add(AssociateApprovalStatus.PendingApproval);
            this.nonArchiveStatuses.Add(AssociateApprovalStatus.Registered);
        }

        public IEnumerable<int> GetIndividualIdsForProject(IEnumerable<int> associateIds, int projectId)
        {
            return this.associateRepo.GetIndividualIdsForProject(associateIds, projectId);
        }

        public void ApproveAssociate(AssociateModel associate)
        {
            this.associateRepo.UpdateAssociateStatus(associate.Id, AssociateApprovalStatus.Approved);
        }

        public void AcceptAssociate(AssociateModel associate)
        {
            this.associateRepo.UpdateAssociateStatus(associate.Id, AssociateApprovalStatus.Accepted);

            // send associate approval notication
            EmailModel associateSubmission = this.emailService.GetAssociateEmail(associate.Id, EmailTemplate.AssociateAcceptedNotification);
            associateSubmission.ToAddress = associate.Email;
            this.emailService.SendEmail(associateSubmission);
        }

        public IEnumerable<MomentaRecruitment.Common.Helpers.ListItem<byte>> GetBusinessUnitOptions()
        {
            return
                MomentaRecruitment.Common.Helpers.CacheHelper.ForByteListItems.Cached("AssociateBusinessUnitOptions", this.associateRepo.GetBusinessUnitOptions);
        }

        public void UpdateAssociateToITRIS(AssociateModel associate, Guid currentUserId)
        {
            if (Settings.Default.UseTransactionScopeForItris)
            {
                using (var scope = new TransactionScope(
                    TransactionScopeOption.Required, 
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
                {
                    this.UpdateAssociateToItris(associate, currentUserId);
                    scope.Complete();
                }
            }
            else
            {
                this.UpdateAssociateToItris(associate, currentUserId);
            }
        }

        public int CreateAssociate(AssociateModel associate, Guid membershipUserId)
        {
            Associate associateEntity = this.dataMapper.MapAssociateM2E(associate);

            associateEntity.AssociateApprovalStatusId = (byte)AssociateApprovalStatus.Accepted;
            associateEntity.CreatedDate = DateTime.Now;
            associateEntity.AdminOwnerId = this.associateRepo.AssignAssociateOwner(associate.LastName);

            return this.associateRepo.CreateAssociate(
                associateEntity, membershipUserId);
        }

        public override void Dispose()
        {
            this.associateRepo.Dispose();
        }

        public AssociateModel GetAssociateOnly(int associateId)
        {
            var associate = this.GetAssociate<AssociateModel>(associateId);

            return associate;
        }
        //public Admin.Models.AssociateSchedulerModel GetAssociateForScheduler(int associateId)
        //{
        //    var associate = this.GetAssociateForScheduler<Admin.Models.AssociateSchedulerModel>(associateId);

        //    return associate;
        //}
       
        public Associate GetAssociatesById(int associateId)
        {
            var associate = this.associateRepo.GetAssociatesById(associateId);

            return associate;
        }

        public List<CommunicationHistoryModel> GetPaginatedCommunicationsHistory(int associateId, int pageSize, int page, string sortColumn, string sortDirection, string searchText, out int resultsCount, int isAutomatic=0)
        {
            return this.associateRepo.GetPaginatedCommunicationsHistory(
                associateId,
                pageSize,
                page,
                sortColumn,
                sortDirection,
                searchText,
                out resultsCount, isAutomatic);
        }

        public string GetDetailsForAssociateCommunication(int associateCommunicationId)
        {
            return this.associateRepo.GetDetailsForAssociateCommunication(associateCommunicationId);
        }

        public EmailModel GetAssociateSentEmail(int id)
        {
            AssociateSentEmail emailEntity = this.associateRepo.GetAssociateSentEmail(id);

            EmailModel objEmail = this.dataMapper.MapAssociateEmailAssociateSentEmailE2M(emailEntity);
            objEmail.EmailId = id;

            return objEmail;
        }

        public IEnumerable<AssociateEmailDocumentModel> GetAssociateEmailUploadedDocuments(int associateId, string newlyUploadedDocs)
        {
            var results = this.associateRepo.GetAssociateEmailUploadedDocuments(associateId, newlyUploadedDocs);

            return results;
        }
        public void SaveCommunication(CommunicationHistoryModel communicationHistoryModel, string attachments)
        {
            AssociateCommunication communicationEntity = this.dataMapper.MapAssociateCommunicationM2E(communicationHistoryModel);
            this.associateRepo.SaveCommunication(communicationEntity, attachments);
        }

        public AssociateModel GetAssociateAndCheckLock(int associateId)
        {
            var associate = this.GetAssociate<AssociateModel>(associateId);
            associate.LockingUser = this.associateRepo.GetLockingEmployeeUserId(associate.Id);

            this.UpdateReferenceAdminEventHistoryForAssociate(associate);

            return associate;
        }

        public void UpdateReferenceAdminEventHistoryForAssociate(AssociateModel associate)
        {
            foreach (ReferenceBaseModel reference in associate.References)
            {
                reference.AdministrativeEventsForReference = this.associateRefRepo.GetAdministrativeEventsForReference(reference.Id);

                var referenceWithAccountants = reference as IReferenceWithAccountants;
                if (referenceWithAccountants != null)
                {
                    foreach (ReferenceAccountantModel accountant in referenceWithAccountants.Accountants)
                    {
                        accountant.AdministrativeEventsForReference = this.associateRefRepo.GetAdministrativeEventsForReference(accountant.Id);
                    }
                }
            }

            associate.UpdateJson();
        }

        public IEnumerable<AssociateHistorySaveOperation> GetAssociateCollapsedEditHistory(
            int associateId, out bool associateChangeAfterAdmin)
        {
            IEnumerable<AssociateEditHistoryItem> editHistory =
                this.dataMapper.MapEditHistoryM2E(this.associateRepo.GetAssociateEditHistory(associateId));

            /* The list is ordered in ascending order of ModifiedTime. Where there are multiple consecutive edits 
               by the same user we only show the first and last save in each session. Slightly complicating matters
               is that each "save" as perceived by the user may actually be carried out by separate SQL statements with
               slightly different timestamps. If two consecutive saves have the same ModifiedSource, ModifiedTime and 
               are separated by <= one second we regard this as one operation.
               
              So timestamps of 10:00, 11:13:00,11:13:01,11:13:02, 14:00 would be treated as 3 saves (at 10:00, 11:13:00 and 14:00).
              
              We need to keep track internally of the fact that this save spans from 11:13:00 to 11:13:02 however. 
              
              Because if we compare the 10:00 version with the 11:13:00 version we should include the changes up until 11:13:02 in what is displayed.

             */
            AssociateEditHistoryItem lastHistoryItem = null;

            AssociateHistorySaveOperation currentSaveOperation = null;

            var saveOperations = new List<AssociateHistorySaveOperation>();

            foreach (AssociateEditHistoryItem historyItem in editHistory)
            {
                if (lastHistoryItem != null
                    &&
                    (lastHistoryItem.ModifiedBy != historyItem.ModifiedBy
                     || lastHistoryItem.ModifiedSource != historyItem.ModifiedSource
                     || historyItem.ModifiedTime.Subtract(lastHistoryItem.ModifiedTime).TotalMilliseconds > 1000))
                {
                    FinaliseCurrentSaveOperation(saveOperations, lastHistoryItem, currentSaveOperation);

                    currentSaveOperation = null;
                }

                if (currentSaveOperation == null)
                {
                    currentSaveOperation = new AssociateHistorySaveOperation
                                               {
                                                   FirstSaveTimeStamp = historyItem.ModifiedTime, 
                                                   ModifiedBy = historyItem.ModifiedBy, 
                                                   ModifiedSource = historyItem.ModifiedSource
                                               };
                }

                lastHistoryItem = historyItem;
            }

            if (currentSaveOperation != null)
            {
                FinaliseCurrentSaveOperation(saveOperations, lastHistoryItem, currentSaveOperation);
            }

            AssociateEditHistoryItem lastAdminEdit = editHistory.FirstOrDefault(e => e.ModifiedSource == "Admin");
            AssociateEditHistoryItem lastAssociateEdit =
                editHistory.FirstOrDefault(e => e.ModifiedSource == "AssociatePortal");

            associateChangeAfterAdmin = lastAdminEdit != null && lastAssociateEdit != null
                                        && lastAssociateEdit.ModifiedTime > lastAdminEdit.ModifiedTime;

            return saveOperations;
        }

        public AssociateModel GetAssociateVersion(int id, DateTime from, DateTime to)
        {
            Associate associate = this.associateRepo.GetAssociateVersion(id, to);

            IEnumerable<AssociateReferenceDetail> refs = this.associateRepo.GetAssociateReferenceVersions(id, from, to);

            IEnumerable<AssociateAddressHistory> addresses = this.associateRepo.GetAssociateAddressVersions(
                id, from, to);

            var associateVersion = this.dataMapper.MapAssociateE2M<AssociateModel>(associate, refs, addresses);

            AssociateItemHistory associateHistory = this.GetAssociateItemHistory(
                @from, to, id, ItemHistoryType.Associate);

            associateVersion.CategorizedItemHistory = new CategorizedItemHistory(associateHistory);

            foreach (ReferenceBaseModel reference in associateVersion.References)
            {
                var referenceWithAccountants = reference as IReferenceWithAccountants;
                if (referenceWithAccountants != null)
                {
                    foreach (ReferenceAccountantModel accountantModel
                        in (referenceWithAccountants).Accountants)
                    {
                        accountantModel.AssociateItemHistory = this.GetAssociateItemHistory(
                            @from, to, accountantModel.Id, ItemHistoryType.Reference, accountantModel);
                    }
                }

                reference.AssociateItemHistory = this.GetAssociateItemHistory(
                    @from, to, reference.Id, ItemHistoryType.Reference, reference);
            }

            foreach (AssociateAddressModel address in associateVersion.AddressHistory)
            {
                address.AssociateItemHistory = this.GetAssociateItemHistory(
                    @from, to, address.Id, ItemHistoryType.Address, address);
            }

            associateVersion.UpdateJson();
            associateVersion.LockingUser = this.associateRepo.GetLockingEmployeeUserId(associateVersion.Id);

            return associateVersion;
        }

        public FileDownloadModel GetCvVersion(Guid id)
        {
            return this.associateRepo.GetCvVersion(id);
        }

        public string GetItrisEmployeeIdFromMembershipId(Guid userId)
        {
            return this.associateRepo.GetItrisEmployeeIdFromMembershipId(userId);
        }

        public IEnumerable<AssociateSummaryModel> GetPaginatedAssociates(AssociateSearchModel search, PaginationDetails paginationDetails)
        {
            bool company = false;
            IQueryable<Associate> associates = this.associateRepo.GetAssociates();           

            if (search.SelectedStatuses != null)
            {
                if (search.SelectedStatuses.SelectedItems.Count > 0)
                {
                    // Add status filters to query
                    List<int> statusAsInts = search.SelectedStatuses.SelectedItems.Select(s => s.Id).ToList();
                    associates = associates.Where(a => statusAsInts.Contains(a.AssociateApprovalStatusId));
                }
            }

            if (!string.IsNullOrEmpty(search.FirstName))
            {
                associates = associates.Where(a => a.FirstName.ToLower().Contains(search.FirstName.Trim().ToLower()));
            }

            if (!string.IsNullOrEmpty(search.LastName))
            {
                associates = associates.Where(a => a.LastName.ToLower().Contains(search.LastName.Trim().ToLower()));
            }

            if (!string.IsNullOrEmpty(search.Email))
            {
                associates = associates.Where(a => a.Email.ToLower().Contains(search.Email.Trim().ToLower()));
            }

            paginationDetails.ItemsCount = associates.Count();

            if (paginationDetails.SortColumn == "Status")
            {
                associates = paginationDetails.AscendingSort
                                 ? associates.OrderBy(a => a.AssociateApprovalStatus.Description)
                                 : associates.OrderByDescending(a => a.AssociateApprovalStatus.Description);
            }
            else
            {
                if (paginationDetails.SortColumn != "Company")
                {
                    associates = paginationDetails.AscendingSort
                                     ? associates.OrderBy(paginationDetails.SortColumn)
                                     : associates.OrderByDescending(paginationDetails.SortColumn);
                }
                else { company = true; }
            }

            if (company)
            {
                var result = this.dataMapper.MapAssociateSummaryListE2M(associates);

                result = paginationDetails.AscendingSort
                     ? result.OrderBy(a => a.Company)
                     : result.OrderByDescending(a => a.Company);

                result =
                    result.Skip((paginationDetails.PageNumber - 1) * paginationDetails.PageSize).Take(
                        paginationDetails.PageSize);

                return result;
            }
            else
            {
                associates =
                    associates.Skip((paginationDetails.PageNumber - 1) * paginationDetails.PageSize).Take(
                        paginationDetails.PageSize);

                return this.dataMapper.MapAssociateSummaryListE2M(associates);
            }
        }

        public void ChangeToFullAssociate(int associateId)
        {
            this.associateRepo.ChangeToFullAssociate(associateId);
        }

        public void UnArchiveAssociate(int associateId)
        {
            this.associateRepo.UnArchiveAssociate(associateId);
        }

        public void ArchiveAssociate(int associateId, AssociateApprovalStatus status)
        {
            if (this.nonArchiveStatuses.Contains(status))
            {
                throw new ArgumentException("Only \"Archive: Active\" and \"Archive: Do not use\" can be archived");
            }

            this.associateRepo.ArchiveAssociate(associateId, status);
        }

        public bool SaveVisualInspection(Guid documentId, bool approved, string comments, string userName)
        {
            try
            {
                this.associateRepo.SaveVisualInspection(documentId, approved, comments, userName);
                return true;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }

        public void SaveComment(CommentModel comment, int associateId)
        {
            Comment commentEntity = this.dataMapper.MapCommentM2E(comment);
            this.associateRepo.SaveComment(commentEntity, associateId);
            comment.CommentId = commentEntity.CommentId;
        }

        public void SaveCommentForReference(CommentModel comment, int referenceId)
        {
            Comment commentEntity = this.dataMapper.MapCommentM2E(comment);

            this.associateRepo.SaveCommentForReference(commentEntity, referenceId);
            comment.CommentId = commentEntity.CommentId;
        }

        public void ApproveDocument(Guid documentId, string userName, DateTime approvalDate)
        {
            this.associateRepo.ApproveDocument(documentId, userName, approvalDate);
        }

        private static void FinaliseCurrentSaveOperation(
            IList<AssociateHistorySaveOperation> saveOperations, 
            AssociateEditHistoryItem lastHistoryItem, 
            AssociateHistorySaveOperation currentSaveOperation)
        {
            currentSaveOperation.LastSaveTimeStamp = lastHistoryItem.ModifiedTime;

            if (saveOperations.Count > 1)
            {
                // If the two most recent items in the list are save operations from the user
                // we are just about to add another record for then replace the middle one
                // rather than add a new one. (We only show the first and last save per 
                // contiguous edit session)
                AssociateHistorySaveOperation p1 = saveOperations[saveOperations.Count - 1];
                AssociateHistorySaveOperation p2 = saveOperations[saveOperations.Count - 2];

                if (p1.ModifiedBy == currentSaveOperation.ModifiedBy
                    && p1.ModifiedSource == currentSaveOperation.ModifiedSource && p1.ModifiedBy == p2.ModifiedBy
                    && p1.ModifiedSource == p2.ModifiedSource)
                {
                    saveOperations[saveOperations.Count - 1] = currentSaveOperation;
                    return;
                }
            }

            saveOperations.Add(currentSaveOperation);
        }

        private AssociateItemHistory GetAssociateItemHistory(
            DateTime @from, DateTime to, int itemId, ItemHistoryType itemType, AssociateDeletableItem item = null)
        {
            bool hasRestoredDelete;
            DateTime? createdDate;
            DateTime? deletedDate;
            IEnumerable<ItemHistory> h = this.associateRepo.GetItemHistory(
                itemId, itemType, @from, to, out createdDate, out deletedDate, out hasRestoredDelete);

            if (item != null)
            {
                item.IsDeletedOnServer = deletedDate.HasValue;
            }

            List<AssociateItemRestoredDeleteHistoryRecord> restoredDeleteHistory = null;

            if (hasRestoredDelete)
            {
                IEnumerable<ItemRestoredDeleteHistory> r = this.associateRepo.GetItemRestoredDeleteHistory(
                    itemId, itemType);
                restoredDeleteHistory = this.dataMapper.MapItemRestoredDeleteHistoryListE2M(r);
            }

            var associateItemHistory = new AssociateItemHistory
                                           {
                                               RestoredDeleteHistory = restoredDeleteHistory, 
                                               CreatedDate = createdDate, 
                                               DeletedDate = deletedDate, 
                                               CreatedDuringPeriod = (@from <= createdDate && createdDate <= to), 
                                               DeletedDuringPeriod = (@from <= deletedDate && deletedDate <= to), 
                                               ItemHistoryType = itemType
                                           };

            // format the address data column from json into a string
            h.Where(x => x.Col == "AddressData" && !string.IsNullOrEmpty(x.Val)).ForEach(this.FormatAddress);

            associateItemHistory.HistoryRecords.AddRange(this.dataMapper.MapItemHistoryListE2M(h));

            return associateItemHistory;
        }

        private void FormatAddress(ItemHistory item)
        {
            var address = new JavaScriptSerializer().Deserialize<GlobalAddress>(item.Val);

            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(address.Premise))
            {
                sb.AppendFormat("{0} ", address.Premise);
            }

            if (!string.IsNullOrEmpty(address.SubBuilding))
            {
                sb.AppendFormat("{0} ", address.SubBuilding);
            }

            if (!string.IsNullOrEmpty(address.Building))
            {
                sb.AppendFormat("{0} ", address.Building);
            }

            if (!string.IsNullOrEmpty(address.Street))
            {
                sb.AppendFormat("{0} ", address.Street);
            }

            if (!string.IsNullOrEmpty(address.City))
            {
                sb.AppendFormat("{0} ", address.City);
            }

            if (!string.IsNullOrEmpty(address.ZipPostcode))
            {
                sb.AppendFormat("{0} ", address.ZipPostcode);
            }

            item.Val = sb.ToString();
        }

        private void InsertAssociateToItris(AssociateModel associate, Guid userId)
        {
            // This is the 'Portal' user. Admin site password is 'Portal1'
            // ITRIS ID is 'HQ00000203'
            // For now all records inserted into ITRIS from Admin site will be make under
            // the employee name of 'Portal'. This is to avoid having to synched 2 different systems
            // users may exist in the admin site but not in ITRIS.
            string empId = this.GetItrisEmployeeIdFromMembershipId(new Guid("08B418E8-EDD9-452B-843D-ECC985048DD2"));

            // string empId = this.GetItrisEmployeeIdFromMembershipId(userId);
            if (empId == null)
            {
                throw new RecordNotFoundException(
                    string.Format("Cannot insert to ITRIS as could not find correct EMP_ID to use for user {0}", userId));
            }

            this.associateRepo.InsertAssociateToItris(this.dataMapper.MapAssociateM2E(associate), empId);
        }

        private void UpdateAssociateToItris(AssociateModel associate, Guid userId)
        {
            if (associate.ITRISAppId == null)
            {
                this.InsertAssociateToItris(associate, userId);
            }
            else
            {
                string empId = this.GetItrisEmployeeIdFromMembershipId(userId);

                if (empId == null)
                {
                    throw new RecordNotFoundException(
                        string.Format("Cannot update to ITRIS as could not find correct EMP_ID to use for user {0}", userId));
                }

                this.associateRepo.UpdateAssociateToItris(this.dataMapper.MapAssociateM2E(associate), empId);
            }
        }

        public void CreateBusinessTask(int id, DateTime scheduledDate, string field, string value)
        {
            //Type myType = am.GetType();
            //IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
            
            //string taskJson = "";
            //foreach (PropertyInfo prop in props)
            //{
            //    if (prop != null)
            //    {
                    
            //        string propName = prop.Name;
            //        if (prop.GetValue(am, null) != null)
            //        {
            //            string propValue = prop.GetValue(am, null).ToString();
            //            if (propName != null && propValue != null)
            //            {
            //                taskJson += "{\"id\":\"" + propName + "\",\"name\":\"" + propValue + "\"},";
            //            }
            //        }
            //    }
            //    // Do something with propValue
            //}
            //taskJson = taskJson.Remove(taskJson.Length - 1);
            //taskJson += "";
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            string url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority;
            url += urlHelper.Action("UpdateBusinessTypeData", "Associate");

            string authScheme = "Timesheets";
            this.schedulerRepo.CreateTask(ScheduledTaskPeriodType.OneTime, 0, scheduledDate, "BusinessTypes", url, value, authScheme);
        }

        public void UpdateBusinessTypeData(string jsonArgument)
        {
            //int associateId = int.Parse(updates["ID"]);
            //var associate = this.associateRepo.GetAssociatesById(associateId);
            ////update val

            //associate.BusinessTypeID = byte.Parse(updates["BusinessTypeId"]);
            //if (updates["VATRegistered"] == "1")
            //    associate.VATRegistered = bool.Parse("True");
            //else
            //    associate.VATRegistered = bool.Parse("False");
            //associate.VATRegistration = updates["VATRegistration"];
            //if (updates["DateVatRegistered"] == "")
            //    associate.DateVatRegistered = null;
            //else
            //    associate.DateVatRegistered = DateTime.Parse(updates["DateVatRegistered"]);
            //if (updates["DateVatDeRegistered"] == "")
            //    associate.DateVatDeRegistered = null;
            //else
            //    associate.DateVatDeRegistered = DateTime.Parse(updates["DateVatDeRegistered"]);
            //associate.SageId = updates["SageId"];
            //associate.RegisteredCompanyName = updates["RegisteredCompanyName"];
            //associate.RegisteredCompanyAddress = updates["RegisteredCompanyAddress"];
            //associate.LimitedCompanyNumber = updates["LimitedCompanyNumber"];
            //associate.RegistedCompanyBankAcctName = updates["RegistedCompanyBankAcctName"];
            //associate.RegistedCompanyBankAcctSort = updates["RegistedCompanyBankAcctSort"];
            //associate.RegistedCompanyBankAcctNumber = updates["RegistedCompanyBankAcctNumber"];
            //associate.OptOutSelfBilling = bool.Parse(updates["OptOutSelfBilling"]);
            //associate.UmbrellaCompanyId= byte.Parse(updates["UmbrellaCompanyId"]);
            //associate.OtherUmbrellaCompanyName= updates["OtherUmbrellaCompanyName"];
            //associate.OtherUmbrellaContactEmail = updates["OtherUmbrellaContactEmail"];
            //associate.OtherUmbrellaContactName = updates["OtherUmbrellaContactName"];
        
            ////pass to save
            //this.associateRepo.SaveUpdatedAssociate(associate);
            RunBulkScheduling(jsonArgument, ScheduleEntryType.Mutiple);
        }

        public void AddInvoiceHistory(int invoiceId, string user, string comment)
        {
            this.associateRepo.AddInvoiceHistory(invoiceId, user, comment);
        }

        public List<GetAssociateProspects_Result> GetAssociateProspects(int associateId, int pageSize, int skip, List<SortDescription> sort, out int? resultsCount)
        {
            return this.associateRepo.GetAssociateProspects(associateId, pageSize, skip, sort, out resultsCount);
        }
        public IEnumerable<HistoricalRatesViewModel> GetHistoricalRates(int individualId)
        {
            var tempOutput = new List<HistoricalRatesViewModel>();
            var output = new List<HistoricalRatesViewModel>();

            var tempResult = this.associateRepo.GetHistoricalRates(individualId).OrderBy(r => r.ModifiedTime);

            var result = from r in this.associateRepo.GetHistoricalRates(individualId).OrderBy(r => r.ModifiedTime)
                         select new HistoricalRatesViewModel
                         {
                             ModifiedTime = r.ModifiedTime,
                             FullMomentaRate = r.FullMomentaRate,
                             AssociateRate = r.AssociateRate,
                             OverTimeCharge = r.OverTimeCharge,
                             OverProductionCharge = r.OverProductionCharge,
                             OverTimePayAway = r.OverTimePayAway,
                             OverProductionPayAway = r.OverProductionPayaway
                         };

            var historicalRatesViewModels = result as HistoricalRatesViewModel[] ?? result.ToArray();
            var currentfmr = historicalRatesViewModels.ToList()[0].FullMomentaRate;
            var currentar = historicalRatesViewModels.ToList()[0].AssociateRate;
            var currentoc = historicalRatesViewModels.ToList()[0].OverTimeCharge;
            var currentopa = historicalRatesViewModels.ToList()[0].OverTimePayAway;
            var currentopc = historicalRatesViewModels.ToList()[0].OverProductionCharge;
            var currentoppa = historicalRatesViewModels.ToList()[0].OverProductionPayAway;

            foreach (var historicalRatesViewModel in historicalRatesViewModels)
            {
                if (currentfmr != historicalRatesViewModel.FullMomentaRate
                    || currentar != historicalRatesViewModel.AssociateRate
                    || currentoc != historicalRatesViewModel.OverTimeCharge
                    || currentopa != historicalRatesViewModel.OverTimePayAway
                    || currentopc != historicalRatesViewModel.OverProductionCharge
                    || currentoppa != historicalRatesViewModel.OverProductionPayAway)
                {
                    tempOutput.Add(historicalRatesViewModel);

                    currentfmr = historicalRatesViewModel.FullMomentaRate;
                    currentar = historicalRatesViewModel.AssociateRate;
                    currentoc = historicalRatesViewModel.OverTimeCharge;
                    currentopa = historicalRatesViewModel.OverTimePayAway;
                    currentopc = historicalRatesViewModel.OverProductionCharge;
                    currentoppa = historicalRatesViewModel.OverProductionPayAway;
                }
            }


            var date = string.Empty;

            foreach (var historicalRatesViewModel in tempOutput)
            {
                var tempDate = string.Format("{0}{1}{2}", historicalRatesViewModel.ModifiedTime.Year,
                    historicalRatesViewModel.ModifiedTime.Month, historicalRatesViewModel.ModifiedTime.Date);
                
                if (date != tempDate)
                {
                    
                    output.Add(historicalRatesViewModel);
                }
                else
                {
                    output[output.Count-1] = historicalRatesViewModel;
                }

                date = tempDate;
            }

            return output.OrderByDescending(r => r.ModifiedTime);
        }


        public void CreateRateComment(int individualId, string rateName, string comment)
        {
            this.associateRepo.CreateRateComment(individualId, rateName, comment);
        }

        public string GetUmbrellaCompanyName(byte? umbrellaCompanyId)
        {
            return associateRepo.GetUmbrellaCompany(umbrellaCompanyId).Name;
        }

        public void CheckAssociateContractStatus()
        {            
            // get associates on contract that have lapsed roles; which have end dates less than today
            var lapsedRoleAssociates = associateRepo.GetOnContractAssociatesWithElapsedRoles();

            // get associates on contract that have current roles; which have end dates less than today
            dynamic dynamicCurrentRoleAssociates = associateRepo.GetOnContractAssociatesWithCurrentRoles();

            var currentRoleAssociates = new List<int>();
            foreach (var currentRoleAssociate in dynamicCurrentRoleAssociates)
            {
                currentRoleAssociates.Add((int)currentRoleAssociate);
            }
         
            foreach (var lapsedRoleAssociate in lapsedRoleAssociates)
            {
                // check if associate has current role
                var lapsed = true;
                foreach (var currentRoleAssociate in currentRoleAssociates)
                {
                    if ((int) lapsedRoleAssociate != (int) currentRoleAssociate) continue;
                    lapsed = false;
                    break;
                }
                if (!lapsed) continue;

                // get audit chages for AssociateContractStatus
                List<GetAssociateContractStatusHistory_Result> associateContractStatusList = associateRepo.GetAssociateContractStatusHistory(lapsedRoleAssociate);

                // update associate to previous contract status      
                var auditRows = (from r in associateContractStatusList where r.AssociateApprovalStatusId != null select r).OrderByDescending(o => o.Sequence).ToList();
             
                if (auditRows.Count == 0) continue;
                int i;
                for(i = 0; i< auditRows.Count;i++)
                {
                    if (auditRows[i].AssociateApprovalStatusId == (int) AssociateApprovalStatus.OnContract) 
                    break;
                }

                if (i + 1 < auditRows.Count)
                {
                    associateRepo.UpdateAssociateContractStatus(lapsedRoleAssociate,
                               auditRows[i + 1].AssociateApprovalStatusId.GetValueOrDefault());
                }

            }         
        }

        public bool GetDocumentVATApprovalStatus(AssociateDocumentType vatCertificate, int id)
        {
            return associateRepo.GetDocumentVATApprovalStatus(vatCertificate, id);
        }

        public List<AssociateExpiredSelfBillingAgreementModel> GetExpiredSelfBillingAgreements()
        {
            var  associateList = associateRepo.GetExpiredSelfBillingAgreements();

            return associateList.Select(getExpiredSelfBillingAgreementsResult => new AssociateExpiredSelfBillingAgreementModel
            {
                AssociateId = getExpiredSelfBillingAgreementsResult.ID, Firstname = getExpiredSelfBillingAgreementsResult.FirstName, Email = getExpiredSelfBillingAgreementsResult.Email
            }).ToList();
        }

        public void SaveAssessmentDocument(Guid FileDocId, int assessmentTypeId)
        {

            this.associateRepo.SaveAssessmentDocument(FileDocId, assessmentTypeId);
        }

        public IEnumerable<AssociateEmailDocumentModel> GetAssociateEmailDocuments(int id)
        {
            var results = this.associateRepo.GetAssociateEmailDocuments(id);

            return results;
        }

        public List<MR_DAL.BusinessType> GetBusinessTypes()
        {
            return this.associateRepo.GetBusinessTypes();
        }

        public void CreateAssociateDocumentRequired(int associateId, int docType)
        {
            this.associateRepo.CreateAssociateDocumentRequired(associateId, docType);
        }

        public void CheckAssociateDefaultDocument(int associateId)
        {
            this.associateRepo.CheckAssociateDefaultDocument(associateId);
        }

        private void RunBulkScheduling(string jsonArgument, ScheduleEntryType type)
        {
            try
            {
                var handler = new ScheduleActionHandler(jsonArgument, ScheduleEntryType.Mutiple, null,associateRepo);
                handler.AssociateChangeHandle();
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}