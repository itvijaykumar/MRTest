namespace MomentaRecruitment.Common.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Objects;
    using System.Linq;
    using System.Text;
    using System.Transactions;
    using System.Web.Helpers;
    using System.Web.Security;

    using ITRIS_DAL.Enumerations;

    using MomentaRecruitment.Common.Enumerations;
    using MomentaRecruitment.Common.Helpers;
    using MomentaRecruitment.Common.Models;
    using MomentaRecruitment.Common.Properties;
    using MomentaRecruitment.Common.Repositories;

    using MR_DAL;
    using MR_DAL.Enumerations;

    using AddressType = MR_DAL.Enumerations.AddressType;
    using Qualification = MomentaRecruitment.Common.Models.Qualification;
    using QualificationCategory = MomentaRecruitment.Common.Models.QualificationCategory;
    using ReferenceType = MR_DAL.Enumerations.ReferenceType;

    public interface IAssociateService : IDisposable
    {

        ChangeUsername ChangeUsername(string oldUsername, string newUsername);

        IEnumerable<ListItem<byte>> GetAddressTypeOptions();

        T GetAssociate<T>(int id) where T : AssociateModel, new();

        T GetAssociateWithNoIncludes<T>(int id) where T : AssociateModel, new();

        IEnumerable<ListItem<byte>> GetAssociateTitleOptions();

        IEnumerable<ListItem<byte>> GetAssociateVisaTypeOptions();

        IEnumerable<ListItem<byte>> GetAssociateStatusOptions();

        IEnumerable<ListItem<byte>> GetAvailabilityTypeOptions();

        IEnumerable<ListItem<byte>> GetBusinessTypeOptions();

        IEnumerable<ListItem<byte>> GetBusinessAreaOptions();  
      
        IEnumerable<int> GetAssociateBusinessAreas(int associateId);

        IEnumerable<ListItem<byte>> GetAgencyOptions();

        IEnumerable<ListItem<byte>> GetUmbrellaCompanyOptions();

        IEnumerable<ListItem<byte>> GetRegionOptions();

        FileDownloadModel GetCV(int associateId);

        IEnumerable<ListItem<int>> GetCountryOptions();

        IEnumerable<ListItem<int>> GetFilteredKeywords(ItrisKeywordCategory filterCategory);

        IEnumerable<ListItem<int>> GetNationalityOptions();

        IEnumerable<ListItem<int>> GetCountyOptions();

        IEnumerable<ListItem<byte>> GetNoticeIntervalOptions();

        IEnumerable<ListItem<byte>> GetReferenceGapTypeOptions();

        IEnumerable<ListItem<byte>> GetReferenceQualificationTypeOptions();

        IEnumerable<ListItem<byte>> GetReferenceTypeOptions();

        IEnumerable<ListItem<byte>> GetReferenceWorkTypeOptions();

        IEnumerable<ListItem<byte>> GetReferenceReasonForLeavingOptions(bool forRefereePortal);

        IEnumerable<ListItem<int>> GetReferralSourceOptions();

        T UpdateAssociate<T>(
            T associate,
            IEnumerable<int> accountantsToDelete,
            IEnumerable<int> addressesToDelete,
            IEnumerable<int> referencesToDelete,
            IEnumerable<int> businessAreas,
            Site sourceSite) where T : AssociateModel, new();

        IEnumerable<string> ValidateBusinessRules(AssociateModel associate);

        void SaveCV(int associateId, string filePath, string edit);

        string SaveTimesheetExpense(int associateId, string title, DateTime date, string filePath);

        string SaveInvoiceFile(int invoiceId, string filePath);

        void SaveReferenceDocument(AssociateDocumentType documentType, Guid documentId, int associateId, string filePath, int fileSize);

        void SaveAssociateDocument(AssociateDocumentType documentType, string documentTitle, Guid fileGuid, int associateId, string filePath, int fileSize, string edit);

        void SaveAssociatePDFDocument(AssociateDocumentType documentType, string documentTitle, Guid fileGuid, int associateId, byte[] fileData);

        void AddReferenceDocumentMapping(int associateId, int referenceId, Guid documentId);

        FileDownloadModel GetDocument(Guid documentId, int? associateId);

        FileDownloadModel DownloadTimesheetDocument(Guid id, int associateId);

        Guid? GetLatestDocumentIdOfType(AssociateDocumentType value, int associateId);

        void SaveProjectEmail(int id, string email);

        BusinessAreaModel GetBusinessArea(int id);

        IEnumerable<ListItem<byte>> GetTemporaryAddressTypeOptions();

        string GetRolesForExperienceRegistrationTypeAndSectorJson();

        Dictionary<string, Dictionary<string, IEnumerable<TopLevelRole>>> GetRolesForExperienceRegistrationTypeAndSector();

        IEnumerable<ListItem<byte>> GetExperienceMarketSectors();

        IEnumerable<ListItem<byte>> GetExperienceRegistrationTypes();

        Dictionary<string, QualificationCategory[]> GetQualificationsByCategoryAndSector();

        void CreateCommunicationsHistoryItem(int associateId, CommunicationType type, string description, string loggedInUser, string details);

        AssociateModel GetAssociateByTimeSheetId(int timesheetId);

        FileDownloadModel GetInvoiceFile(int? invoiceId = null, Guid? fileId = null);

        InvoiceModel GetInvoice(int invoiceId);

        bool AssociateIsApprover(int associateId);

        IEnumerable<AssessmentModel> GetAssessments(int associateId);

        void UpdateAssessment(AssessmentModel assessment);

        AssessmentModel CreateAssessment(int associateId, int assessmentTypeId, DateTime assessmentDate, string pass, byte score, string Comment, string documentIds);
        IEnumerable<AssessmentType> GetAssessmentTypes();

        void AddAssociateToAssociateRole(int associateId);

        List<string> GetUmbrellaCompanyEmails(byte? umbrellaCompanyId);

        List<string> GetUmbrellaAgencyContactFirstNames(byte? umbrellaCompanyId);

        List<UmbrellaUser> GetUmbrellaUsers(byte? umbrellaCompanyId);

        string  GetUmbrellaEmailList(byte? umbrellaCompanyId);
    }

    public struct DatePeriod
    {
        public DateTime EndDate { get; set; }

        public DateTime StartDate { get; set; }
    }

    public abstract class AssociateService : IAssociateService
    {
        private readonly IAssociateRepository associateRepo;

        private readonly IDataMapper dataMapper;

        protected AssociateService(IAssociateRepository repository, IDataMapper mapper)
        {
            this.associateRepo = repository;
            this.dataMapper = mapper;
        }

        public ChangeUsername ChangeUsername(string oldUsername, string newUsername)
        {
            var application = Membership.ApplicationName;
            int result = this.associateRepo.ChangeUsername(oldUsername, newUsername, application);

            return (ChangeUsername)result;
        }

        public virtual void Dispose()
        {
            this.associateRepo.Dispose();
        }

        public IEnumerable<ListItem<byte>> GetAddressTypeOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached("AddressTypeOptions", this.associateRepo.GetAddressTypeOptions);
        }

        public IEnumerable<ListItem<byte>> GetTemporaryAddressTypeOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached("TemporaryAddressTypeOptions", this.associateRepo.GetTemporaryAddressTypeOptions);
        }

        public virtual T GetAssociateWithNoIncludes<T>(int id) where T : AssociateModel, new()
        {
            var associate = this.associateRepo.GetAssociateWithNoIncludes(id);

            return this.dataMapper.MapAssociateE2M<T>(associate);
        }

        public virtual T GetAssociate<T>(int id) where T : AssociateModel, new()
        {
            var associate = this.associateRepo.GetAssociate(id);
         
            return this.dataMapper.MapAssociateE2M<T>(associate);
        }

       
        public IEnumerable<ListItem<byte>> GetAssociateTitleOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached("AssociateTitleOptions", this.associateRepo.GetAssociateTitleOptions);
        }

        public IEnumerable<ListItem<byte>> GetAssociateVisaTypeOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached("AssociateVisaTypeOptions", this.associateRepo.GetAssociateVisaTypeOptions);
        }

        public IEnumerable<ListItem<byte>> GetAssociateStatusOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached("AssociateStatusOptions", this.associateRepo.GetAssociateStatusOptions);
        }

        public IEnumerable<ListItem<byte>> GetAvailabilityTypeOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached("AvailabilityTypeOptions", this.associateRepo.GetAvailabilityTypeOptions);
        }

        public IEnumerable<ListItem<byte>> GetBusinessTypeOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached("BusinessTypeOptions", this.associateRepo.GetBusinessTypeOptions);
        }

        public IEnumerable<ListItem<byte>> GetBusinessAreaOptions()
        {
            return this.associateRepo.GetBusinessAreaOptions();
                //CacheHelper.ForByteListItems.Cached("BusinessAreaOptions", this.associateRepo.GetBusinessAreaOptions);
        }

        public IEnumerable<ListItem<byte>> GetAgencyOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached("AgencyOptions", this.associateRepo.GetAgencyOptions);
        }

        public IEnumerable<ListItem<byte>> GetUmbrellaCompanyOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached("UmbrellaCompanyOptions", this.associateRepo.GetUmbrellaCompanyOptions);
        }

        public IEnumerable<ListItem<byte>> GetRegionOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached("RegionOptions", this.associateRepo.GetRegionOptions);
        }

        public FileDownloadModel GetDocument(Guid documentId, int? associateId)
        {
            return this.associateRepo.GetDocument(documentId, associateId);
        }

        public Guid? GetLatestDocumentIdOfType(AssociateDocumentType value, int associateId)
        {
            return this.associateRepo.GetLatestDocumentIdOfType(value, associateId);
        }

        public FileDownloadModel GetCV(int associateId)
        {
            return this.associateRepo.GetCV(associateId);
        }

        public BusinessAreaModel GetBusinessArea(int id)
        {
            var result = this.associateRepo.GetBusinessArea(id);
            return new BusinessAreaModel
                         {
                             Description = result.Description,
                             BusinessAreaId = id
                         };
            
        }

        public void SaveProjectEmail(int id, string email)
        {
            this.associateRepo.SaveProjectEmail(id, email);
        }

        public FileDownloadModel GetInvoiceFile(int? invoiceId = null, Guid? fileId = null)
        {
            return this.associateRepo.GetInvoiceFile(invoiceId, fileId);
        }

        public InvoiceModel GetInvoice(int invoiceId)
        {
            return this.associateRepo.GetInvoice(invoiceId);
        }

        public IEnumerable<ListItem<int>> GetCountryOptions()
        {
            return
                CacheHelper.ForIntListItems.Cached("CountryOptions", this.associateRepo.GetCountryOptions);
        }

        public IEnumerable<ListItem<int>> GetFilteredKeywords(ItrisKeywordCategory filterCategory)
        {
            return this.associateRepo.GetFilteredKeywords(filterCategory);
        }

        public IEnumerable<ListItem<int>> GetNationalityOptions()
        {
            return
                CacheHelper.ForIntListItems.Cached("NationalityOptions", this.associateRepo.GetNationalityOptions);
        }

        public IEnumerable<ListItem<int>> GetCountyOptions()
        {
            return
                CacheHelper.ForIntListItems.Cached("CountyOptions", this.associateRepo.GetCountyOptions);
        }

        public IEnumerable<ListItem<byte>> GetNoticeIntervalOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached("NoticeIntervalOptions", this.associateRepo.GetNoticeIntervalOptions);
        }

        public IEnumerable<ListItem<byte>> GetReferenceGapTypeOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached("ReferenceGapTypeOptions", this.associateRepo.GetReferenceGapTypeOptions);
        }

        public IEnumerable<ListItem<byte>> GetReferenceQualificationTypeOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached(
                    "ReferenceQualificationTypeOptions", this.associateRepo.GetReferenceQualificationTypeOptions);
        }

        public IEnumerable<ListItem<byte>> GetReferenceTypeOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached("ReferenceTypeOptions", this.associateRepo.GetReferenceTypeOptions);
        }

        public IEnumerable<ListItem<byte>> GetReferenceWorkTypeOptions()
        {
            return
                CacheHelper.ForByteListItems.Cached("ReferenceWorkTypeOptions", this.associateRepo.GetReferenceWorkTypeOptions);
        }

        public IEnumerable<ListItem<byte>> GetReferenceReasonForLeavingOptions(bool forRefereePortal)
        {
            var cacheKey = "ReferenceReasonForLeavingOptions" + forRefereePortal;
            return
                CacheHelper.ForByteListItems.Cached(cacheKey, () => this.associateRepo.GetReferenceReasonForLeavingOptions(forRefereePortal));
        }

        public IEnumerable<ListItem<int>> GetReferralSourceOptions()
        {
            return
                CacheHelper.ForIntListItems.Cached("ReferralSourceOptions", this.associateRepo.GetReferralSourceOptions);
        }

        public IEnumerable<ListItem<byte>> GetExperienceMarketSectors()
        {
            return
                CacheHelper.ForByteListItems.Cached("ExperienceMarketSectorOptions", this.associateRepo.GetExperienceMarketSectorOptions);
        }

        public IEnumerable<ListItem<byte>> GetExperienceRegistrationTypes()
        {
            return
                CacheHelper.ForByteListItems.Cached("ExperienceRegistrationTypeOptions", this.associateRepo.GetExperienceRegistrationTypeOptions);
        }

        public string GetRolesForExperienceRegistrationTypeAndSectorJson()
        {
            var experienceRegistrationTypes = this.GetRolesForExperienceRegistrationTypeAndSector();

            return Json.Encode(experienceRegistrationTypes);
        }

        public Dictionary<string, QualificationCategory[]> GetQualificationsByCategoryAndSector()
        {
            var cacheHelper = new CacheHelper<Dictionary<string, QualificationCategory[]>>();

            return cacheHelper.Cached(
                "QualificationsByCategoryAndSector",
                () =>
                {
                    var qualificationCategoriesGroupedBySector =
                        from q in this.associateRepo.GetCategorizedQualifications()
                        group q by q.MarketSectorId;

                    return qualificationCategoriesGroupedBySector.ToDictionary(
                        kp => kp.Key.ToString(),
                        kp => kp.Select(r => new QualificationCategory
                        {
                            DisplayOrder = r.DisplayOrder,
                            Description = r.Description,
                            MarketSectorId = r.MarketSectorId,
                            Name = r.Name,
                            QualificationCategoryId = r.QualificationCategoryId,
                            Qualifications = r.Qualifications
                            .OrderBy(q => q.DisplayOrder)
                            .Select(q => new Qualification
                            {
                                DisplayOrder = q.DisplayOrder,
                                Name = q.Name,
                                QualificationCategoryId = q.QualificationCategoryId,
                                QualificationId = q.QualificationId
                            }).ToList()
                        }).OrderBy(r => r.DisplayOrder).ToArray());
                });
        }

        public Dictionary<string, Dictionary<string, IEnumerable<TopLevelRole>>> GetRolesForExperienceRegistrationTypeAndSector()
        {
            var cacheHelper = new CacheHelper<Dictionary<string, Dictionary<string, IEnumerable<TopLevelRole>>>>();

            return cacheHelper.Cached(
                "RolesForExperienceRegistrationTypeAndSector",
                () =>
                {
                    var experienceRegistrationTypes = new Dictionary<string, Dictionary<string, IEnumerable<TopLevelRole>>>();

                    var registrationTypes = this.associateRepo.GetExperienceRegistrationTypes();

                    foreach (var registrationType in registrationTypes)
                    {
                        var sectorDictionary = new Dictionary<string, IEnumerable<TopLevelRole>>();

                        var rolesGroupedBySector = registrationType
                            .MarketSectorRoles
                            .Select(r => new
                            {
                                r.DisplayOrder,
                                r.MarketSectorRoleId,
                                r.ParentMarketSectorRoleId,
                                r.MarketSectorId,
                                r.RoleName
                            }).GroupBy(r => r.MarketSectorId);

                        foreach (var sectorRoles in rolesGroupedBySector)
                        {
                            var topLevelRoles = sectorRoles
                                .Where(r => r.ParentMarketSectorRoleId == null)
                                .Select(r => new TopLevelRole
                                {
                                    DisplayOrder = r.DisplayOrder,
                                    MarketSectorRoleId = r.MarketSectorRoleId,
                                    RoleName = r.RoleName,
                                    ChildRoles = new List<ChildRole>()
                                }).OrderBy(r => r.DisplayOrder).ToArray();

                            foreach (var topLevelRole in topLevelRoles)
                            {
                                topLevelRole.ChildRoles.AddRange(sectorRoles
                                    .Where(r => r.ParentMarketSectorRoleId == topLevelRole.MarketSectorRoleId)
                                    .Select(r => new ChildRole
                                    {
                                        DisplayOrder = r.DisplayOrder,
                                        MarketSectorRoleId = r.MarketSectorRoleId,
                                        ParentMarketSectorRoleId = r.ParentMarketSectorRoleId,
                                        RoleName = r.RoleName
                                    }).OrderBy(r => r.DisplayOrder).ToList());
                            }

                            sectorDictionary[sectorRoles.Key.ToString()] = topLevelRoles;
                        }

                        experienceRegistrationTypes[registrationType.RegistrationTypeId.ToString()] = sectorDictionary;
                    }

                    return experienceRegistrationTypes;
                });
        }

        public virtual T UpdateAssociate<T>(
            T associate,
            IEnumerable<int> accountantsToDelete,
            IEnumerable<int> addressesToDelete,
            IEnumerable<int> referencesToDelete,
            IEnumerable<int> businessAreas,
            Site sourceSite) where T : AssociateModel, new()
        {
            foreach (var qualificationFreeTextModel in associate.QualificationFreeTexts)
            {
                qualificationFreeTextModel.AssociateId = associate.Id;
            }

            var accountantsToRestore = new List<int>();
            var referencesToRestore = new List<int>();

            var addressesToRestore =
                (from address in associate.AddressHistory where address.IsRestoredOnClient select address.Id).ToList();

            foreach (var reference in associate.References)
            {
                if (reference.EndDate != null && reference is IReferenceWithReasonForLeaving)
                {
                    var referenceWithReasonForLeaving = (IReferenceWithReasonForLeaving)reference;

                    if (reference.EndDate.Value.Year == 2999)
                    {
                        referenceWithReasonForLeaving.ReasonForLeaving = string.Empty;
                        referenceWithReasonForLeaving.ReasonForLeavingId = null;
                    }
                    else
                    {
                        byte? reasonForLeavingId = referenceWithReasonForLeaving.ReasonForLeavingId;

                        if (reasonForLeavingId != 21 && reasonForLeavingId != 5)
                        {
                            referenceWithReasonForLeaving.ReasonForLeaving = string.Empty;
                        }
                    }
                }

                if (reference.IsRestoredOnClient)
                {
                    referencesToRestore.Add(reference.Id);
                }

                var referenceWithAccountants = reference as IReferenceWithAccountants;

                if (referenceWithAccountants != null)
                {
                    accountantsToRestore.AddRange(
                        from accountant in (referenceWithAccountants).Accountants
                        where accountant.IsRestoredOnClient
                        select accountant.Id);
                }
            }

            T updatedAssociate;

            using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                updatedAssociate = this.UpdateAssociate(
                    associate,
                    accountantsToDelete,
                    addressesToDelete,
                    referencesToDelete,
                    addressesToRestore,
                    accountantsToRestore,
                    referencesToRestore,
                    businessAreas,
                    sourceSite);

                updatedAssociate.ActionName = associate.ActionName;

                scope.Complete();
            }

            return updatedAssociate;
        }

        public void ValidateAddresses(StringBuilder warnings, IEnumerable<AssociateAddressModel> addresses)
        {
            var registeredAddresses = addresses.Where(a => a.AddressTypeID.HasValue && !a.IsDeleted).ToList();

            if (registeredAddresses.Count == 0)
            {
                // Each associate must have at least 1 address
                warnings.Append("At least one address is required.|");
            }
            else
            {
                var earliestDate = (from r in registeredAddresses orderby r.StartDate select r.StartDate).FirstOrDefault();

                var latestDate =
                    (from r in registeredAddresses where r.EndDate.HasValue orderby r.EndDate descending select r.EndDate).FirstOrDefault();

                if (!earliestDate.HasValue || !latestDate.HasValue)
                {
                    warnings.Append("A full 5 year history of addresses up until the present is required.|");
                    return;
                }

                var now = DateTime.Now;
                if (now.AddYears(-5) < earliestDate || now > latestDate.Value.AddDays(1))
                {
                    warnings.Append("A full 5 year history of addresses up until the present is required.|");
                }

                var prevPeriod = new List<DatePeriod>();
                var currentPeriod = new List<DatePeriod>();

                prevPeriod.Add(new DatePeriod { StartDate = earliestDate.Value, EndDate = latestDate.Value });

                foreach (var address in registeredAddresses)
                {
                    if (address.StartDate.HasValue && address.EndDate.HasValue)
                    {
                        currentPeriod = this.CheckPeriodCoverage(prevPeriod, address.StartDate.Value, address.EndDate.Value, 1);

                        prevPeriod = currentPeriod;
                    }
                }

                foreach (var period in currentPeriod)
                {
                    if (period.EndDate.Subtract(period.StartDate).Days > 0)
                    {
                        warnings.AppendFormat(
                            "There is an address gap between {0} and {1}.|",
                            period.StartDate.ToShortDateString(),
                            period.EndDate.ToShortDateString());
                    }
                }
            }
        }

        public IEnumerable<string> ValidateBusinessRules(AssociateModel associate)
        {
            var warnings = new StringBuilder();

            this.ValidateReferences(warnings, associate.References);
            this.ValidateAddresses(warnings, associate.AddressHistory);

            return warnings.Length == 0
                ? null
                : warnings.ToString().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public string SaveTimesheetExpense(int timesheetId, string title, DateTime date, string filePath)
        {
            var documentId = this.associateRepo.SaveTimesheetExpense(timesheetId, title, date, filePath);
            return documentId;
        }

        public string SaveInvoiceFile(int invoiceId, string filePath)
        {
            var fileId = this.associateRepo.SaveInvoiceFile(invoiceId, filePath);
            return fileId;
        }

        public void SaveCV(int associateId, string filePath, string edit)
        {
            this.associateRepo.SaveCV(associateId, filePath, edit);
        }

        public void SaveReferenceDocument(AssociateDocumentType documentType, Guid documentId, int associateId, string filePath, int fileSize)
        {
            this.associateRepo.SaveReferenceDocument(documentId, associateId, filePath, (int)documentType, fileSize);
        }

        public void SaveAssociateDocument(AssociateDocumentType documentType, string documentTitle, Guid fileGuid, int associateId, string filePath, int fileSize, string edit)
        {
            this.associateRepo.SaveAssociateDocument(documentType, documentTitle, fileGuid, associateId, filePath, fileSize, edit);
        }

        public void SaveAssociatePDFDocument(
            AssociateDocumentType documentType,
            string documentTitle,
            Guid fileGuid,
            int associateId,
            byte[] fileData)
        {
            this.associateRepo.SaveAssociatePDFDocument(documentType, documentTitle, fileGuid, associateId, fileData);
        }

        public void AddReferenceDocumentMapping(int associateId, int referenceId, Guid documentId)
        {
            this.associateRepo.AddReferenceDocumentMapping(associateId, referenceId, documentId);
        }

        public void ValidateReferences(StringBuilder warnings, IEnumerable<ReferenceBaseModel> references)
        {
            var referencesToValidate =
                references.Where(r =>
                    !r.IsDeleted &&
                    r.StartDate.HasValue &&
                    r.EndDate.HasValue &&
                    r.ReferenceTypeID != ReferenceType.ProfessionalWorkReference).ToList();

            if (referencesToValidate.Count == 0)
            {
                // Each associate must have at least 1 reference
                warnings.Append("At least one completed reference is required.|");
            }
            else
            {
                var earliestDate =
                    (from r in referencesToValidate orderby r.StartDate select r.StartDate).First().Value;

                var latestDate =
                    (from r in referencesToValidate orderby r.EndDate descending select r.EndDate).First().Value;

                var now = DateTime.Now;
                latestDate = latestDate.Year == 2999 ? now.Date : latestDate;

                if (now.AddYears(-5) < earliestDate || now.AddDays(-90) > latestDate)
                {
                    warnings.Append("A full 5 year reference history up until the present is required.|");
                }

                var prevPeriod = new List<DatePeriod>();
                var currentPeriod = new List<DatePeriod>();

                prevPeriod.Add(new DatePeriod { StartDate = earliestDate, EndDate = latestDate });

                foreach (var reference in referencesToValidate)
                {
                    currentPeriod = this.CheckPeriodCoverage(
                        prevPeriod, reference.StartDate.Value, reference.EndDate.Value, 91);

                    prevPeriod = currentPeriod;
                }

                // Validate that there is no more than 90 days between each reference gap
                foreach (var period in currentPeriod)
                {
                    if (period.EndDate.Subtract(period.StartDate).Days > 90)
                    {
                        warnings.AppendFormat(
                            Settings.Default.ReferenceGapMessage,
                            period.StartDate.ToShortDateString(),
                            period.EndDate.ToShortDateString());
                    }
                }
            }
        }

        public void AddAssociateToAssociateRole(int associateId)
        {
            this.associateRepo.AddAssociateToAssociateRole(associateId);
        }

        private static void GetReferenceDocumentsToDeleteOrRestore(
            AssociateModel associate,
            List<Guid> documentsToRestore,
            List<Guid> documentsToDelete)
        {
            foreach (var reference in associate.References)
            {
                if (reference is IReferenceWithDocuments)
                {
                    ProcessReferenceDocuments(documentsToDelete, documentsToRestore, reference);
                }

                var referenceWithAccountants = reference as IReferenceWithAccountants;
                if (referenceWithAccountants != null)
                {
                    foreach (var accountant in referenceWithAccountants.Accountants)
                    {
                        ProcessReferenceDocuments(documentsToDelete, documentsToRestore, accountant);
                    }
                }
            }
        }

        private static void ProcessReferenceDocuments(
            List<Guid> documentsToDelete,
            List<Guid> documentsToRestore,
            ReferenceBaseModel reference)
        {
            foreach (var document in ((IReferenceWithDocuments)reference).Documents)
            {
                switch (document.State)
                {
                    case ReferenceDocumentState.Deleted:
                        documentsToDelete.Add(document.DocumentId);
                        break;
                    case ReferenceDocumentState.Restored:
                        documentsToRestore.Add(document.DocumentId);
                        break;
                }
            }
        }

        private List<DatePeriod> CheckPeriodCoverage(
            IEnumerable<DatePeriod> prevPeriod,
            DateTime startDate,
            DateTime endDate,
            int allowedDaysGap)
        {
            var currentPeriod = new List<DatePeriod>();

            foreach (var period in prevPeriod)
            {
                bool preserve = true;

                if (startDate < period.StartDate && endDate > period.EndDate)
                {
                    preserve = false;
                }
                else
                {
                    if (startDate >= period.StartDate && startDate <= period.EndDate)
                    {
                        if (startDate.Subtract(period.StartDate).Days > allowedDaysGap)
                        {
                            currentPeriod.Add(new DatePeriod { StartDate = period.StartDate, EndDate = startDate });
                        }

                        preserve = false;
                    }

                    if (endDate >= period.StartDate && endDate <= period.EndDate)
                    {
                        if (period.EndDate.Subtract(endDate).Days > allowedDaysGap)
                        {
                            currentPeriod.Add(new DatePeriod { StartDate = endDate, EndDate = period.EndDate });
                        }

                        preserve = false;
                    }
                }

                if (preserve)
                {
                    currentPeriod.Add(new DatePeriod { StartDate = period.StartDate, EndDate = period.EndDate });
                }
            }

            return currentPeriod;
        }

        private T UpdateAssociate<T>(
            T associate,
            IEnumerable<int> accountantsToDelete,
            IEnumerable<int> addressesToDelete,
            IEnumerable<int> referencesToDelete,
            IEnumerable<int> addressesToRestore,
            IEnumerable<int> accountantsToRestore,
            IEnumerable<int> referencesToRestore,
            IEnumerable<int> businessAreas,
            Site sourceSite) where T : AssociateModel, new()
        {
            var documentsToDelete = new List<Guid>();
            var documentsToRestore = new List<Guid>();

            if (sourceSite == Site.Admin)
            {
                // Neither deleting documents or restoring items should be possible except from the admin site
                GetReferenceDocumentsToDeleteOrRestore(associate, documentsToRestore, documentsToDelete);

                this.associateRepo.RestoreDeletedItems(
                    associate.Id, addressesToRestore, accountantsToRestore, referencesToRestore, documentsToRestore);
            }

            this.associateRepo.DeleteAssociateItems(
                associate.Id, addressesToDelete, accountantsToDelete, referencesToDelete, documentsToDelete, sourceSite);

            if (associate.InsuranceApplication != null)
            {
                associate.InsuranceApplication.AssociateId = associate.Id;
            }

            var businessAreaList = new List<BusinessAreaModel>();

            foreach (int id in businessAreas)
            {
                businessAreaList.Add(new BusinessAreaModel { BusinessAreaId = id, Description = string.Empty });
            }

            associate.BusinessAreas = businessAreaList;

            var associateEntity = this.dataMapper.MapAssociateM2E(associate);            

            var updatedAssociateEntity = this.associateRepo.UpdateAssociate(associateEntity, sourceSite);

            associate = this.dataMapper.MapAssociateE2M<T>(updatedAssociateEntity);

            associate.UpdateJson();

            return associate;
        }

        public FileDownloadModel DownloadTimesheetDocument(Guid documentId, int associateId)
        {
            return this.associateRepo.GetTimesheetDocument(documentId, associateId);
        }

        public void CreateCommunicationsHistoryItem(int associateId, CommunicationType type, string description, string loggedInUser, string details)
        {
            this.associateRepo.CreateCommunicationsHistoryItem(associateId, type, description, loggedInUser, details);
        }

        public AssociateModel GetAssociateByTimeSheetId(int timesheetId)
        {
            var associate = this.associateRepo.GetAssociateByTimeSheetId(timesheetId);

            return this.dataMapper.MapAssociateE2M<AssociateModel>(associate);
        }

        public bool AssociateIsApprover(int associateId)
        {
            return this.associateRepo.AssociateIsApprover(associateId);
        }

        public IEnumerable<AssessmentModel> GetAssessments(int associateId)
        {
            var results = this.associateRepo.GetAssessments(associateId);

            return results.Select(a => new AssessmentModel
            {
                AssessmentDate = (DateTime)a.AssessmentDate,
                AssessmentId = (int)a.AssessmentId,
                AssessmentTypeName = a.Name,
                AssessmentTypeDescription = a.Description,
                AssessmentTypeId = (int)a.AssessmentTypeId,
                Pass = a.Pass,
                Scorable = a.Scorable,
                Score = a.Score,
                Comment=a.Comment,
                AssociateId = (int)a.AssociateId

            }).ToList<AssessmentModel>();
        }
       
        public IEnumerable<AssessmentModel> GetAssessmentHistory(int associateId, int assessmentTypeId)
        {
            return this.associateRepo.GetAssessmentHistory(associateId, assessmentTypeId);
        }

        public void UpdateAssessment(AssessmentModel assessment)
        {
            var entity = this.dataMapper.MapAssessmentM2E(assessment);
            this.associateRepo.UpdateAssessment(entity, assessment.Documents);
        }

        public AssessmentModel CreateAssessment(int associateId, int assessmentTypeId, DateTime assessmentDate, string pass, byte score, string Comment, string documentIds)
        {
            var result = this.associateRepo.CreateAssessment(associateId, assessmentTypeId, assessmentDate, pass, score, Comment,documentIds);
            return this.dataMapper.MapAssessmentE2M(result);
        }

        public IEnumerable<AssessmentType> GetAssessmentTypes()
        {
            return this.associateRepo.GetAssessmentTypes();
        }

        public IEnumerable<int> GetAssociateBusinessAreas(int associateId)
        {
            return this.associateRepo.GetAssociateBusinessAreas(associateId);
        }

        public List<string> GetUmbrellaCompanyEmails(byte? umbrellaCompanyId)
        {
            return this.associateRepo.GetUmbrellaCompanyEmails(umbrellaCompanyId);
        }

        public List<string> GetUmbrellaAgencyContactFirstNames(byte? umbrellaCompanyId)
        {
            return this.associateRepo.GetUmbrellaAgencyContactFirstNames(umbrellaCompanyId);
        }

        public List<UmbrellaUser> GetUmbrellaUsers(byte? umbrellaCompanyId)
        {
            return this.associateRepo.GetUmbrellaUsers(umbrellaCompanyId);;
        }

        public string GetUmbrellaEmailList(byte? umbrellaCompanyId)
        {
            var companyId = umbrellaCompanyId.GetValueOrDefault();
            if (companyId == 0) return "";
           
            var users = this.associateRepo.GetUmbrellaEmailList(companyId);
            
            var rtnUserList = users.Select(umbrellaUser => umbrellaUser.EMail).ToList();
            return rtnUserList.Aggregate((x, y) => x + "," + y);
        }
    }
}