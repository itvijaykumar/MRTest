namespace MomentaRecruitment.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Objects;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Transactions;
    using System.Text.RegularExpressions;

    using Elmah;

    using ITRIS_DAL;
    using ITRIS_DAL.Enumerations;

    using MomentaRecruitment.Common.Enumerations;
    using MomentaRecruitment.Common.Helpers;
    using MomentaRecruitment.Common.Helpers.ExtensionMethods;
    using MomentaRecruitment.Common.Models;

    using MR_DAL;
    using MR_DAL.Enumerations;

    using AddressType = MR_DAL.AddressType;
    using ApplicationException = System.ApplicationException;
    using AssociateApprovalStatus = MR_DAL.Enumerations.AssociateApprovalStatus;
    using AvailabilityType = MR_DAL.AvailabilityType;
    using BusinessType = MR_DAL.BusinessType;
    using DateTime = System.DateTime;
    using IsolationLevel = System.Transactions.IsolationLevel;
    using ObjectContext = MomentaRecruitment.Common.Helpers.ExtensionMethods.ObjectContext;
    using Qualification = MR_DAL.Qualification;
    using QualificationCategory = MR_DAL.QualificationCategory;
    using Region = MR_DAL.Region;
    using VisaType = MR_DAL.VisaType;
    using System.Web.Script.Serialization;
    using System.Xml;
    using MomentaRecruitment.Common.ViewModel;

    public interface IAssociateRepository : IDisposable
    {
        int ChangeUsername(string oldUsername, string newUsername, string application);

        IEnumerable<ListItem<byte>> GetAddressTypeOptions();

        IEnumerable<ListItem<byte>> GetTemporaryAddressTypeOptions();

        Associate GetAssociate(int id);

        Associate GetFullAssociateForEmail(int id);

        IEnumerable<ListItem<byte>> GetAssociateTitleOptions();

        IEnumerable<ListItem<byte>> GetAssociateVisaTypeOptions();

        IEnumerable<ListItem<byte>> GetAssociateStatusOptions();

        IEnumerable<ListItem<byte>> GetAvailabilityTypeOptions();

        IEnumerable<ListItem<byte>> GetBusinessTypeOptions();

        IEnumerable<ListItem<byte>> GetBusinessAreaOptions();

        BusinessArea GetBusinessArea(int id);

        IEnumerable<int> GetAssociateBusinessAreas(int associateId);

        IEnumerable<ListItem<byte>> GetAgencyOptions();

        IEnumerable<ListItem<byte>> GetUmbrellaCompanyOptions();

        IEnumerable<ListItem<byte>> GetRegionOptions();

        FileDownloadModel GetCV(int? associateId = null, Guid? versionId = null);

        FileDownloadModel GetInvoiceFile(int? invoiceId = null, Guid? fileId = null);

        InvoiceModel GetInvoice(int invoiceId);

        IEnumerable<ListItem<int>> GetCountryOptions();

        IEnumerable<ListItem<int>> GetFilteredKeywords(ItrisKeywordCategory filterCategory);

        IEnumerable<ListItem<int>> GetNationalityOptions();

        IEnumerable<ListItem<int>> GetCountyOptions();

        IEnumerable<ListItem<byte>> GetNoticeIntervalOptions();

        void SaveCV(int associateId, string filePath, string edit);

        IEnumerable<ListItem<byte>> GetReferenceGapTypeOptions();

        IEnumerable<ListItem<byte>> GetReferenceQualificationTypeOptions();

        IEnumerable<ListItem<byte>> GetReferenceTypeOptions();

        IEnumerable<ListItem<byte>> GetReferenceWorkTypeOptions();

        IEnumerable<ListItem<byte>> GetReferenceReasonForLeavingOptions(bool forRefereePortal);

        IEnumerable<ListItem<int>> GetReferralSourceOptions();

        void CheckAssociateDefaultDocument(int associateId);

        void SaveProjectEmail(int id, string email);

        void DeleteAssociateItems(int associateId, IEnumerable<int> addresses, IEnumerable<int> accountants, IEnumerable<int> references, IEnumerable<Guid> referenceDocuments, Site sourceSite);

        void RestoreDeletedItems(
            int associateId, 
            IEnumerable<int> addresses, 
            IEnumerable<int> accountants, 
            IEnumerable<int> references, 
            IEnumerable<Guid> referenceDocuments);

        Associate UpdateAssociate(Associate associate, Site sourceSite);

        void UpdateAssociateStatus(int associateId, AssociateApprovalStatus status);

        void SaveReferenceDocument(Guid documentId, int associateId, string filePath, int documentTypeId, int fileSize);

        void AddReferenceDocumentMapping(int associateId, int referenceId, Guid documentId);

        FileDownloadModel GetDocument(Guid documentId, int? associateId);

        FileDownloadModel GetTimesheetDocument(Guid documentId, int associateId);

        void SaveAssociateDocument(AssociateDocumentType documentType, string documentTitle, Guid documentId, int associateId, string filePath, int fileSize, string edit);

        void SaveAssociatePDFDocument(AssociateDocumentType documentType, string documentTitle, Guid documentId, int associateId, byte[] fileData);

        Guid? GetLatestDocumentIdOfType(AssociateDocumentType documentType, int associateId);

        Associate GetAssociateForEmail(int id);

        bool CheckAssociateEmail(string email);

        Associate GetAssociateForEmailByEmail(string email);

        Associate GetAssociateWithNoIncludes(int id);

        ObjectQuery<RegistrationType> GetExperienceRegistrationTypes();

        IEnumerable<ListItem<byte>> GetExperienceMarketSectorOptions();

        IEnumerable<ListItem<byte>> GetExperienceRegistrationTypeOptions();

        QualificationCategory[] GetCategorizedQualifications();

        List<string> GetUmbrellaAgencyContactFirstNames(byte? umbrellaCompanyId);

        List<string> GetUmbrellaCompanyEmails(byte? umbrellaCompanyId);

        List<UmbrellaUser> GetUmbrellaUsers(byte? umbrellaCompanyId);

        string SaveTimesheetExpense(int timesheetId, string title, DateTime date, string filePath);

        string SaveInvoiceFile(int invoiceId, string filePath);

        void CreateCommunicationsHistoryItem(int associateId, CommunicationType type, string description, string loggedInUser, string details);

        Associate GetAssociateByTimeSheetId(int timesheetId);

        bool AssociateIsApprover(int associateId);

        void AddAssociateToAssociateRole(int associateId);

        IEnumerable<Associate> GetAssociatesByMobileNumber(string number);

        #region Assessments

        IEnumerable<GetAssessments_Result> GetAssessments(int associateId);
        
        IEnumerable<AssessmentModel> GetAssessmentHistory(int associateId, int assessmentTypeId);
        void UpdateAssessment(Assessment assessment, string documentIds);
        Assessment CreateAssessment(int associateId, int assessmentTypeId, DateTime assessmentDate, string pass, byte score, string Comment, string documentIds);
        IEnumerable<AssessmentType> GetAssessmentTypes();

        #endregion

        AssociateSageId GetSageIdByAssociateId(int id, int projectId);

        AssociateSageId CreateSageIdForAssociate(int id, int projectId);
     
        List<UmbrellaUser> GetUmbrellaEmailList(byte companyId);

        UmbrellaCompany GetUmbrellaCompany(byte? umbrellaCompanyId);

        string GetBusinessUnit(int businessUnitId);

        string GetSageCompanyIdForBusinessUnit(int businessUnitId);
        List<int> GetSageCompanyIds();
        List<string> GetPurchaseSageIds(int companyid);

        ScheduledTaskViewModel GetTask(int taskId);
        void UpdateScheduleField(int AssId, string PropertyName, string value);
        
        void BackDateAssociateChanges(int associateId, string field, string value,int taskId);
    }

    public abstract class AssociateRepository : BaseRepository, IAssociateRepository
    {
        private static readonly Func<MomentaRecruitmentEntities, int, Associate> AssociateForEmailByIdQuery =
            CompiledQuery.Compile(
            (MomentaRecruitmentEntities db, int id) => db.Associates
                .Include("VisaType")
                .Single(a => a.ID == id));

        private static readonly Func<MomentaRecruitmentEntities, int, Associate> FullAssociateForEmailByIdQuery =
            CompiledQuery.Compile(
            (MomentaRecruitmentEntities db, int id) => db.Associates
                .Include("PersonTitle")
                .Include("Nationality")
                .Include("VisaType")
                .Include("County")
                .Include("Country")
                .Include("BusinessType")
                .Include("UmbrellaCompany")
                .Include("AvailabilityType")
                .Include("NoticeInterval")
                .Include("Regions")
                .Include("RegistrationType")
                .Include("MarketSectorRoles")
                .Include("MarketSectorRoles.MarketSector")
                .Include("Qualifications")
                .Include("Qualifications.QualificationCategory")
                .Include("Qualifications.QualificationCategory.MarketSector")
                .Include("AssociateQualificationFreeTexts")
                .Include("AssociateQualificationFreeTexts.MarketSector")
                .Include("Specialism")
                .Single(a => a.ID == id));

        private readonly ITRISEntities itrisDb = new ITRISEntities();

        private bool disposed;

        protected ITRISEntities ItrisDb
        {
            get
            {
                return this.itrisDb;
            }
        }

        protected abstract Func<MomentaRecruitmentEntities, int, Associate> CompiledQueryForGetAssociateById { get; }

        private Func<MomentaRecruitmentEntities, int, Associate> CompiledQueryForGetFullAssociateForEmailById
        {
            get
            {
                return FullAssociateForEmailByIdQuery;
            }
        }

        private Func<MomentaRecruitmentEntities, int, Associate> CompiledQueryForGetAssociateForEmailById
        {
            get
            {
                return AssociateForEmailByIdQuery;
            }
        }

        public UmbrellaCompany GetUmbrellaCompany(byte? umbrellaCompanyId)
        {
            return this.MomentaDb.UmbrellaCompanies.Single(u => u.UmbrellaCompanyId == umbrellaCompanyId);
        }

        public List<string> GetUmbrellaAgencyContactFirstNames(byte? umbrellaCompanyId)
        {
            return this.MomentaDb.UmbrellaUsers.Where(u => u.UmbrellaCompanyId == umbrellaCompanyId).Select(u => u.Name).ToList();
        }

        public List<string> GetUmbrellaCompanyEmails(byte? umbrellaCompanyId)
        {
            return this.MomentaDb.UmbrellaUsers.Where(u => u.UmbrellaCompanyId == umbrellaCompanyId).Select(u => u.EMail).ToList();
        }

        public void SaveProjectEmail(int id, string email)
        {
            var assoicate = this.MomentaDb.Associates.FirstOrDefault(e => e.ID == id);
            assoicate.ProjectEmail = email;
            //this.MomentaDb.Associates.ApplyCurrentValues(assoicate);
            this.MomentaDb.SaveChanges();
        }

        public List<UmbrellaUser> GetUmbrellaUsers(byte? umbrellaCompanyId)
        {
            return this.MomentaDb.UmbrellaUsers.Where(u => u.UmbrellaCompanyId == umbrellaCompanyId).ToList();
        }

        public int ChangeUsername(string oldUsername, string newUsername, string application)
        {
            int? result = this.MomentaDb.ChangeUserName(application, oldUsername, newUsername).Single();

            return result ?? 0;
        }

        public void DeleteAssociateItems(int associateId, IEnumerable<int> addresses, IEnumerable<int> accountants, IEnumerable<int> references, IEnumerable<Guid> referenceDocuments, Site sourceSite)
        {
            DataTable dt = GetAssociateItemsDataTable(
                addresses.Where(a => a != 0), 
                accountants.Where(a => a != 0), 
                references.Where(a => a != 0), 
                referenceDocuments);

            if (dt.Rows.Count > 0)
            {
                this.MomentaDb.ExecuteStoreCommand(
                    "EXEC dbo.DeleteAssociateItems @AssociateId = @AssociateId, @SourceSite = @SourceSite, @DeletedItems = @AssociateItems;",
                    new SqlParameter("@AssociateId", SqlDbType.Int) { Value = associateId }, 
                    new SqlParameter("@SourceSite", SqlDbType.Int) { Value = (int)sourceSite }, 
                    new SqlParameter("@AssociateItems", SqlDbType.Structured)
                    {
                        Value = dt, 
                        TypeName = "dbo.AssociateItems"
                    });
            }
        }

        public void AddReferenceDocumentMapping(int associateId, int referenceId, Guid documentId)
        {
            if (!this.MomentaDb.AssociateReferenceDetails.Any(r => r.ID == referenceId && r.AssociateID == associateId))
            {
                throw new ApplicationException(
                    string.Format(
                    "referenceId {0} passed does not map with a reference for associate {1}",
                    referenceId,
                    associateId));
            }

            this.MomentaDb.ReferenceDocuments.AddObject(
                new ReferenceDocument { ReferenceId = referenceId, DocumentId = documentId });
            this.MomentaDb.SaveChanges();
        }

        public void CreateCommunicationsHistoryItem(int associateId, CommunicationType type, string description, string loggedInUser, string details)
        {
            this.MomentaDb.CreateCommunicationsHistoryItem(associateId, (byte)type, description, loggedInUser, details);
        }

        public Associate GetAssociateByTimeSheetId(int timesheetId)
        {
            ObjectResult<GetTimeSheetEmailModel_Result> objectResult = this.MomentaDb.GetTimeSheetEmailModel(timesheetId);
            GetTimeSheetEmailModel_Result result = objectResult.First();
            Associate associate = this.MomentaDb.Associates.First(a => a.ID == result.AssociateId);
            return associate;
        }

        public IEnumerable<ListItem<byte>> GetAddressTypeOptions()
        {
            var addressTypes = new List<ListItem<byte>>();

            foreach (AddressType addressType in this.MomentaDb.AddressTypes.OrderBy(a => a.Order))
            {
                addressTypes.Add(ListItem.Create(addressType.ID, addressType.Description));
            }

            return addressTypes;
        }

        public IEnumerable<ListItem<byte>> GetBusinessAreaOptions()
        {
            var areas = new List<ListItem<byte>>();

            foreach (BusinessArea area in this.MomentaDb.BusinessArea)
            {
                areas.Add(ListItem.Create((byte)area.BusinessAreaId, area.Description));
            }

            return areas;
        }

        public BusinessArea GetBusinessArea(int id)
        {
            return this.MomentaDb.BusinessArea.FirstOrDefault(b => b.BusinessAreaId == id);
        }

        public IEnumerable<ListItem<byte>> GetTemporaryAddressTypeOptions()
        {
            var temporaryAddressTypes = new List<ListItem<byte>>();

            foreach (TemporaryAddressType temporaryAddressType in this.MomentaDb.TemporaryAddressTypes.OrderBy(a => a.Order))
            {
                temporaryAddressTypes.Add(ListItem.Create(temporaryAddressType.ID, temporaryAddressType.Description));
            }

            return temporaryAddressTypes;
        }

        public Associate GetAssociateWithNoIncludes(int id)
        {
            return this.MomentaDb.Associates.Single(a => a.ID == id);
        }

        public Associate GetAssociate(int id)
        {
            return this.CompiledQueryForGetAssociateById(this.MomentaDb, id);
        }

        public Associate GetFullAssociateForEmail(int id)
        {
            return this.CompiledQueryForGetFullAssociateForEmailById(this.MomentaDb, id);
        }

        public Associate GetAssociateForEmail(int id)
        {
            return this.CompiledQueryForGetAssociateForEmailById(this.MomentaDb, id);
        }

        public Associate GetAssociateForEmailByEmail(string email)
        {
            if (this.MomentaDb.Associates.Where(a => a.Email == email).Any())
            {
                return this.MomentaDb.Associates.Single(a => a.Email == email);
            }
            else
            {
                return this.MomentaDb.Associates.Single(a => a.ProjectEmail == email);
            }
        }

        public bool CheckAssociateEmail(string email)
        {
            return this.MomentaDb.Associates.Where(a => a.Email == email || a.ProjectEmail == email).Any();
        }

        public IEnumerable<ListItem<byte>> GetAssociateTitleOptions()
        {
            var titles = new List<ListItem<byte>>();

            foreach (PersonTitle personTitle in this.MomentaDb.PersonTitles)
            {
                titles.Add(ListItem.Create(personTitle.PersonTitleId, personTitle.Description));
            }

            return titles;
        }

        public IEnumerable<ListItem<byte>> GetAssociateVisaTypeOptions()
        {
            var titles = new List<ListItem<byte>>();

            foreach (VisaType visaType in this.MomentaDb.VisaTypes.OrderBy(v => v.Order))
            {
                titles.Add(ListItem.Create(visaType.ID, visaType.Name));
            }

            return titles;
        }

        public IEnumerable<ListItem<byte>> GetAssociateStatusOptions()
        {
            var statuses = new List<ListItem<byte>>();

            foreach (MR_DAL.AssociateApprovalStatus status in this.MomentaDb.AssociateApprovalStatuses)
            {
                statuses.Add(ListItem.Create(status.AssociateApprovalStatusId, status.Description));
            }

            return statuses;
        }

        public IEnumerable<ListItem<byte>> GetAvailabilityTypeOptions()
        {
            var availabilityTypes = new List<ListItem<byte>>();

            foreach (AvailabilityType availabilityType in this.MomentaDb.AvailabilityTypes)
            {
                availabilityTypes.Add(ListItem.Create(availabilityType.AvailabilityTypeId, availabilityType.Description));
            }

            return availabilityTypes;
        }

        public IEnumerable<ListItem<byte>> GetBusinessTypeOptions()
        {
            var businessTypes = new List<ListItem<byte>>();

            foreach (BusinessType businessType in this.MomentaDb.BusinessTypes)
            {
                businessTypes.Add(ListItem.Create(businessType.ID, businessType.Type));
            }

            return businessTypes;
        }

        public IEnumerable<ListItem<byte>> GetAgencyOptions()
        {
            var agencies = new List<ListItem<byte>>();

            foreach (Agency agency in this.MomentaDb.Agency.OrderBy(u => u.Order))
            {
                agencies.Add(ListItem.Create(agency.AgencyId, agency.Name));
            }

            return agencies;
        }

        public IEnumerable<ListItem<byte>> GetUmbrellaCompanyOptions()
        {
            var umbrellaCompanys = new List<ListItem<byte>>();

            foreach (UmbrellaCompany umbrellaCompany in this.MomentaDb.UmbrellaCompanies.OrderBy(u => u.Order))
            {
                umbrellaCompanys.Add(ListItem.Create(umbrellaCompany.UmbrellaCompanyId, umbrellaCompany.Name));
            }

            return umbrellaCompanys;
        }

        public IEnumerable<ListItem<byte>> GetRegionOptions()
        {
            var regions = new List<ListItem<byte>>();

            foreach (Region region in this.MomentaDb.Regions)
            {
                regions.Add(ListItem.Create(region.Id, region.Name));
            }

            return regions;
        }

        public FileDownloadModel GetDocument(Guid documentId, int? associateId)
        {
            SqlCommand cmd = this.GetCommandForDocumentSelectForFileStreaming(documentId, associateId);

            return FileStreamHelper.GetFileDownloadModel(cmd);
        }

        public FileDownloadModel GetTimesheetDocument(Guid documentId, int associateId)
        {
            SqlCommand cmd = this.GetCommandForTimesheetDocumentSelectForFileStreaming(documentId, associateId);

            return FileStreamHelper.GetFileDownloadModel(cmd);
        }
        
        public FileDownloadModel GetCV(int? associateId = null, Guid? versionId = null)
        {
            if (associateId.HasValue == versionId.HasValue)
            {
                throw new ArgumentException("Pass either associateId or versionId.");
            }

            SqlCommand cmd = GetCommandForCVSelectWithoutFileStreaming(associateId, versionId);

            return FileStreamHelper.GetFileDownloadModel(cmd);
        }

        public FileDownloadModel GetInvoiceFile(int? invoiceId = null, Guid? fileId = null)
        {
            if (invoiceId.HasValue == fileId.HasValue)
            {
                throw new ArgumentException("Pass either invoiceId or fileId.");
            }

            SqlCommand cmd = GetCommandForInvoiceSelectWithoutFileStreaming(invoiceId, fileId);

            return FileStreamHelper.GetFileDownloadModel(cmd);
        }

        public InvoiceModel GetInvoice(int invoiceId)
        {
            var invoice = this.MomentaDb.Invoice.FirstOrDefault(i => i.InvoiceId == invoiceId);

            if (invoice != null) 
            {
                return new InvoiceModel
                {
                    InvoiceId = invoiceId,
                    StartDate = invoice.StartDate,
                    EndDate = invoice.EndDate,
                    ProjectName = invoice.ProjectName,
                    RoleName = invoice.RoleName,
                    AssociateId = invoice.AssociateId,
                    PaymentDate = invoice.PaymentDate.GetValueOrDefault(),
                    TotalAmount = invoice.TotalAmount.GetValueOrDefault(),
                    VATAmount = invoice.VATAmount.GetValueOrDefault(),
                    InvoiceStatusId = invoice.InvoiceStatusId
                };
            }
            else {
                return null;
            }
            
        }

        public IEnumerable<ListItem<int>> GetCountryOptions()
        {
            var countries = new List<ListItem<int>>();

            IOrderedQueryable<Country> ordered = this.MomentaDb.Countries.OrderBy(o => o.Order);

            foreach (Country country in ordered)
            {
                countries.Add(ListItem.Create(country.ID, country.Name));
            }

            return countries;
        }

        public IEnumerable<ListItem<int>> GetFilteredKeywords(ItrisKeywordCategory filterCategory)
        {
            var keywords = new List<ListItem<int>>();

            foreach (Keyword keyword in this.ItrisDb.Keywords.Where(k => k.TYPE == (int)filterCategory))
            {
                keywords.Add(ListItem.Create(keyword.DICT_ID, keyword.KEYWORD1));
            }

            return keywords;
        }

        public IEnumerable<ListItem<int>> GetNationalityOptions()
        {
            var nationalities = new List<ListItem<int>>();

            IOrderedQueryable<Nationality> ordered = this.MomentaDb.Nationalities.OrderBy(o => o.Order);

            foreach (Nationality nationality in ordered)
            {
                nationalities.Add(ListItem.Create(nationality.ID, nationality.Name));
            }

            return nationalities;
        }

        public IEnumerable<ListItem<int>> GetCountyOptions()
        {
            var counties = new List<ListItem<int>>();

            IOrderedQueryable<County> ordered = this.MomentaDb.Counties.OrderBy(o => o.Name);

            foreach (County county in ordered)
            {
                counties.Add(ListItem.Create(county.ID, county.Name));
            }

            return counties;
        }

        public IEnumerable<ListItem<byte>> GetNoticeIntervalOptions()
        {
            var noticeIntervals = new List<ListItem<byte>>();

            foreach (var noticeInterval in this.MomentaDb.NoticeIntervals)
            {
                noticeIntervals.Add(ListItem.Create(noticeInterval.NoticeIntervalId, noticeInterval.Description));
            }

            return noticeIntervals;
        }

        public string SaveTimesheetExpense(int timesheetId, string title, DateTime date, string filePath)
        {
            SqlCommand command = GetCommandForTimesheetExpenseUpload(timesheetId, title, date, filePath);

            var fsh = new FileStreamHelper();
            var conn = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            fsh.SaveDocumentWithoutFileStream(command, conn, filePath);

            return command.Parameters["@DocumentId"].Value.ToString();
        }

        public string SaveInvoiceFile(int invoiceId, string filePath)
        {
            SqlCommand command = GetCommandForInvoiceUpload(invoiceId, filePath);

            var fsh = new FileStreamHelper();
            var conn = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            fsh.SaveDocumentWithoutFileStream(command, conn, filePath);

            return command.Parameters["@DocumentId"].Value.ToString();
        }

        public void SaveCV(int associateId, string filePath, string edit)
        {
            using (var scope = new TransactionScope(
                TransactionScopeOption.Required, 
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {

                SqlCommand command = GetCommandForCVUpload(associateId, filePath);
                new FileStreamHelper().SaveDocumentWithoutFileStream(
                    command, ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString, filePath);

                //add associate document
                var documentId = this.MomentaDb.CVs.Where(cv => cv.AssociateId == associateId).SingleOrDefault().Id;

                if (edit == null)
                {
                    edit = "";
                }

                if (!this.MomentaDb.AssociateDocuments.Where(cv => cv.DocumentId == documentId && cv.DocumentTypeId == (int)AssociateDocumentType.CV).Any() && !edit.Equals("Edit"))
                {
                    var associateDocumentLinkEntry = new AssociateDocument
                    {
                        AssociateId = associateId,
                        DocumentId = documentId,
                        DocumentTypeId = (int)AssociateDocumentType.CV,
                        CreatedDate = DateTime.Now
                    };

                    this.MomentaDb.AssociateDocuments.AddObject(associateDocumentLinkEntry);
                    this.MomentaDb.SaveChanges();
                }
                else 
                {
                    var aDocs = this.MomentaDb.AssociateDocuments.Where(x => x.AssociateId == associateId && x.DocumentTypeId == (int)AssociateDocumentType.CV).ToList();
                    AssociateDocument old = aDocs.OrderByDescending(p => p.CreatedDate).FirstOrDefault();
                    //.GroupBy(p => p.DocumentTypeId)
                    //.Select(g => g.OrderByDescending(p => p.CreatedDate).FirstOrDefault())
                    //.OrderByDescending(d => d.CreatedDate).SingleOrDefault();

                    Guid oldId = old.DocumentId;

                    //update associate document
                    var associateDocumentLinkEntry = new AssociateDocument();
                    associateDocumentLinkEntry.DocumentId = documentId;
                    associateDocumentLinkEntry.AssociateId = old.AssociateId;
                    associateDocumentLinkEntry.DocumentTypeId = old.DocumentTypeId;
                    associateDocumentLinkEntry.CreatedDate = DateTime.Now;

                    this.MomentaDb.AssociateDocuments.AddObject(associateDocumentLinkEntry);
                    this.MomentaDb.AssociateDocuments.DeleteObject(old);

                    if (this.MomentaDb.AssociateDocumentComments.Where(x => x.DocumentId == oldId).Any())
                    {
                        var oldComment = this.MomentaDb.AssociateDocumentComments.Where(x => x.DocumentId == oldId).SingleOrDefault();
                        this.MomentaDb.AssociateDocumentComments.DeleteObject(oldComment);
                    }
                    this.MomentaDb.SaveChanges();

                    //Delete Orphan
                    //this.MomentaDb.DeleteDocument(oldId);
                }
                scope.Complete();

            }

        }

        public IEnumerable<ListItem<byte>> GetReferenceGapTypeOptions()
        {
            var referenceGapTypes = new List<ListItem<byte>>();

            foreach (ReferenceGapType referenceGapType in this.MomentaDb.ReferenceGapTypes.OrderBy(rg => rg.Order))
            {
                referenceGapTypes.Add(ListItem.Create(referenceGapType.ID, referenceGapType.Description));
            }

            return referenceGapTypes;
        }

        public IEnumerable<ListItem<byte>> GetReferenceQualificationTypeOptions()
        {
            var referenceQualificationTypes = new List<ListItem<byte>>();

            ObjectSet<ReferenceQualificationType> qualificationTypes = this.MomentaDb.ReferenceQualificationTypes;
            foreach (ReferenceQualificationType referenceQualificationType in qualificationTypes.OrderBy(q => q.Order))
            {
                referenceQualificationTypes.Add(
                    ListItem.Create(referenceQualificationType.ID, referenceQualificationType.Description));
            }

            return referenceQualificationTypes;
        }

        public abstract IEnumerable<ListItem<byte>> GetReferenceTypeOptions();

        public IEnumerable<ListItem<byte>> GetReferenceWorkTypeOptions()
        {
            var referenceWorkTypes = new List<ListItem<byte>>();

            foreach (ReferenceWorkType referenceWorkType in this.MomentaDb.ReferenceWorkTypes)
            {
                referenceWorkTypes.Add(ListItem.Create(referenceWorkType.ID, referenceWorkType.Description));
            }

            return referenceWorkTypes;
        }

        public IEnumerable<ListItem<byte>> GetReferenceReasonForLeavingOptions(bool forRefereePortal)
        {
            var referenceReasonsForLeaving = new List<ListItem<byte>>();

            IQueryable<ReferenceReasonForLeaving> reasons = this.MomentaDb.ReferenceReasonForLeavings.Where(r => forRefereePortal || r.ShowOnAssociatePortal);

            foreach (ReferenceReasonForLeaving referenceReasonForLeaving in reasons)
            {
                referenceReasonsForLeaving.Add(
                    ListItem.Create(referenceReasonForLeaving.ID, referenceReasonForLeaving.Reason));
            }

            return referenceReasonsForLeaving;
        }

        public IEnumerable<ListItem<int>> GetReferralSourceOptions()
        {
            var referralSources = new List<ListItem<int>>();

            foreach (ReferralSource referralSource in this.MomentaDb.ReferralSources.OrderBy(r => r.DisplayOrder))
            {
                referralSources.Add(ListItem.Create(referralSource.ID, referralSource.Source));
            }

            return referralSources;
        }

        public ObjectQuery<RegistrationType> GetExperienceRegistrationTypes()
        {
            ObjectSet<RegistrationType> r = this.MomentaDb.RegistrationTypes;

            foreach (RegistrationType registrationType in r)
            {
                registrationType.MarketSectorRoles.Load(MergeOption.OverwriteChanges);
            }

            return r;
        }

        public IEnumerable<ListItem<byte>> GetExperienceMarketSectorOptions()
        {
            var marketSectors = new List<ListItem<byte>>();

            foreach (MarketSector marketSector in this.MomentaDb.MarketSectors.OrderBy(r => r.DisplayOrder))
            {
                marketSectors.Add(ListItem.Create(marketSector.MarketSectorId, marketSector.Description));
            }

            return marketSectors;
        }

        public IEnumerable<ListItem<byte>> GetExperienceRegistrationTypeOptions()
        {
            var registrationTypes = new List<ListItem<byte>>();

            foreach (RegistrationType registrationType in this.MomentaDb.RegistrationTypes.OrderBy(r => r.RegistrationTypeId))
            {
                registrationTypes.Add(ListItem.Create(registrationType.RegistrationTypeId, registrationType.Description));
            }

            return registrationTypes;
        }

        public QualificationCategory[] GetCategorizedQualifications()
        {
            ObjectSet<QualificationCategory> qualificationCategories = this.MomentaDb.QualificationCategories;

            foreach (QualificationCategory qualificationCategory in qualificationCategories)
            {
                qualificationCategory.Qualifications.Load(MergeOption.OverwriteChanges);
            }

            return qualificationCategories.ToArray();
        }

        public void RestoreDeletedItems(
            int associateId, 
            IEnumerable<int> addresses, 
            IEnumerable<int> accountants, 
            IEnumerable<int> references, 
            IEnumerable<Guid> referenceDocuments)
        {
            DataTable dt = GetAssociateItemsDataTable(addresses, accountants, references, referenceDocuments);

            if (dt.Rows.Count > 0)
            {
                this.MomentaDb.ExecuteStoreCommand(
                    "EXEC dbo.RestoreDeletedItems @AssociateId, @AssociateItems;", 
                    new SqlParameter("AssociateId", SqlDbType.Int) { Value = associateId }, 
                    new SqlParameter("@AssociateItems", SqlDbType.Structured)
                    {
                        Value = dt, 
                        TypeName = "dbo.AssociateItems"
                    });
            }
        }

        public Associate UpdateAssociate(Associate associate, Site sourceSite)
        {
            Associate originalAssociate = this.GetAssociate(associate.ID);

            // check if Company Name or VAT registration number has changed
            if (originalAssociate.VATRegistration != associate.VATRegistration ||
                originalAssociate.RegisteredCompanyName != associate.RegisteredCompanyName ||
                originalAssociate.VATRegistered != associate.VATRegistered)
            {
                // reset the self billing date to be invalid for the document
                associate.OptOutSelfBillingSignedDate = null;
            }

            // Not round tripped
            associate.CreatedDate = originalAssociate.CreatedDate;

            associate.ITRISAppId = originalAssociate.ITRISAppId;

            associate.ReferralSourceID = originalAssociate.ReferralSourceID;

            associate.AssociateApprovalStatusId = originalAssociate.AssociateApprovalStatusId;
            associate.DeclarationSignedDate = originalAssociate.DeclarationSignedDate;

            associate.HasBeenVatRegistered = originalAssociate.HasBeenVatRegistered || (associate.VATRegistered == true);

            bool insuranceApplicationEditable = sourceSite == Site.AssociatePortal && 
                associate.AssociateApprovalStatusId == (byte)AssociateApprovalStatus.Registered;

            if (sourceSite != Site.Admin)
            {
                associate.VettingContactId = originalAssociate.VettingContactId;
                associate.ReferralName = originalAssociate.ReferralName;
                associate.ReferralByFriendName = originalAssociate.ReferralByFriendName;
                associate.ReferralByFriendEmail = originalAssociate.ReferralByFriendEmail;
                associate.IdentityDocumentsQualityChecked = originalAssociate.IdentityDocumentsQualityChecked;
            }

            if (!insuranceApplicationEditable)
            {
                associate.RequiresHiscoxInsurance = originalAssociate.RequiresHiscoxInsurance;
            }

            // Check if a new self billing document is required due to changes in LTD company details
            CheckNewBillingDocumentRequirement(associate, originalAssociate);

            // longitude and latitude
            if (originalAssociate.Postcode != associate.Postcode || originalAssociate.AddressData != associate.AddressData)
            {
                var postcode = associate.Postcode;
                if (postcode == null)
                {
                    var jss = new JavaScriptSerializer();
                    Dictionary<string, object> d = jss.Deserialize<dynamic>(associate.AddressData);

                    // fill in the to address
                    if (d["ZipPostcode"] != null)
                    {
                        postcode = d["ZipPostcode"].ToString();
                    }
                }

                var postcodes = this.MomentaDb.PostCode;

                postcode = postcode.Replace(" ", string.Empty).ToLower();

                var match = from p in postcodes
                            where p.postcode.Replace(" ", string.Empty).ToLower() == postcode
                            select new { p.longitude, p.latitude };

                var coords = match.FirstOrDefault();

                if (coords != null)
                {
                    associate.Latitude = coords.latitude;
                    associate.Longitude = coords.longitude;
                }
            }

            // update the membership email address if need be
            if (originalAssociate.Email != associate.Email)
            {
                var userId = originalAssociate.aspnet_Users.UserId;

                var user = this.MomentaDb.aspnet_Users.Where(x => x.UserId == userId).First();
                var member = this.MomentaDb.aspnet_Membership.Where(x => x.UserId == userId).First();

                user.UserName = associate.Email;
                user.LoweredUserName = associate.Email.ToLower();

                member.Email = associate.Email;
                member.LoweredEmail = associate.Email.ToLower();
            }

            // Update Associate changes
            this.MomentaDb.Associates.ApplyCurrentValues(associate);

            if (insuranceApplicationEditable && associate.RequiresHiscoxInsurance == true)
            {
                if (originalAssociate.InsuranceApplication == null)
                {
                    originalAssociate.InsuranceApplication = associate.InsuranceApplication;
                    originalAssociate.InsuranceApplication.InsuranceApplicationStatusId = 1;
                }
                else
                {
                    associate.InsuranceApplication.AssociateId = originalAssociate.ID;
                    associate.InsuranceApplication.InsuranceApplicationStatusId = originalAssociate.InsuranceApplication.InsuranceApplicationStatusId;
                    this.MomentaDb.InsuranceApplications.ApplyCurrentValues(associate.InsuranceApplication);
                }
            }

            while (associate.Comments.Any())
            {
                originalAssociate.Comments.Add(associate.Comments.First());
            }

            var originalMarketSectorRoles = originalAssociate.MarketSectorRoles.ToList();
            var currentMarketSectorRoles = associate.MarketSectorRoles.ToList();

            var addedMarketSectorRoles = IEnumerableT.AntiJoin(currentMarketSectorRoles, originalMarketSectorRoles, (a, b) => a.MarketSectorRoleId == b.MarketSectorRoleId);
            var deletedMarketSectorRoles = IEnumerableT.AntiJoin(originalMarketSectorRoles, currentMarketSectorRoles, (a, b) => a.MarketSectorRoleId == b.MarketSectorRoleId);

            foreach (var deletedMarketSectorRole in deletedMarketSectorRoles)
            {
                originalAssociate.MarketSectorRoles.Remove(deletedMarketSectorRole);
            }

            foreach (var addedMarketSectorRole in addedMarketSectorRoles)
            {
                var marketSectorRole = new MarketSectorRole { MarketSectorRoleId = addedMarketSectorRole.MarketSectorRoleId };
                this.MomentaDb.MarketSectorRoles.Attach(marketSectorRole);
                originalAssociate.MarketSectorRoles.Add(marketSectorRole);
            }

            var originalQualifications = originalAssociate.Qualifications.ToList();
            var currentQualifications = associate.Qualifications.ToList();

            var addedQualifications = IEnumerableT.AntiJoin(currentQualifications, originalQualifications, (a, b) => a.QualificationId == b.QualificationId);
            var deletedQualifications = IEnumerableT.AntiJoin(originalQualifications, currentQualifications, (a, b) => a.QualificationId == b.QualificationId);

            foreach (var deletedQualification in deletedQualifications)
            {
                originalAssociate.Qualifications.Remove(deletedQualification);
            }

            foreach (var addedQualification in addedQualifications)
            {
                var qualification = new Qualification { QualificationId = addedQualification.QualificationId };
                this.MomentaDb.Qualifications.Attach(qualification);
                originalAssociate.Qualifications.Add(qualification);
            }

            var originalQualificationFreeTexts = originalAssociate.AssociateQualificationFreeTexts.ToList();
            var currentQualificationFreeTexts = associate.AssociateQualificationFreeTexts.ToList();

            var addedQualificationFreeTexts = IEnumerableT.AntiJoin(currentQualificationFreeTexts, originalQualificationFreeTexts, (a, b) => a.AssociateId == b.AssociateId && a.MarketSectorId == b.MarketSectorId);

            foreach (var associateQualificationFreeText in addedQualificationFreeTexts)
            {
                originalAssociate.AssociateQualificationFreeTexts.Add(associateQualificationFreeText);
            }

            foreach (var qft in associate.AssociateQualificationFreeTexts)
            {
                this.MomentaDb.AssociateQualificationFreeTexts.ApplyCurrentValues(qft);
            }

            foreach (var associateQualificationFreeText in originalAssociate.AssociateQualificationFreeTexts.ToList())
            {
                if (string.IsNullOrWhiteSpace(associateQualificationFreeText.FreeText))
                {
                    originalAssociate.AssociateQualificationFreeTexts.Remove(associateQualificationFreeText);
                }
            }

            var originalSpecialisms = originalAssociate.Specialism.ToList();
            var currentSpecialisms = associate.Specialism.ToList();

            var addedSpecialisms = IEnumerableT.AntiJoin(currentSpecialisms, originalSpecialisms, (a, b) => a.SpecialismId == b.SpecialismId);
            var deletedSpecialisms = IEnumerableT.AntiJoin(originalSpecialisms, currentSpecialisms, (a, b) => a.SpecialismId == b.SpecialismId);

            foreach (var deletedSpecialism in deletedSpecialisms)
            {
                originalAssociate.Specialism.Remove(deletedSpecialism);
            }

            foreach (var addedSpecialism in addedSpecialisms)
            {
                var specialism = new Specialism { SpecialismId = addedSpecialism.SpecialismId };
                this.MomentaDb.Specialism.Attach(specialism);
                originalAssociate.Specialism.Add(specialism);
            }

            // Compare with original regions
            var regionsOld = originalAssociate.Regions.ToList();
            foreach (var regionOld in regionsOld)
            {
                // Delete old regions that aren't in the new associate
                if (!associate.Regions.Select(s => s.Id).Contains(regionOld.Id))
                {
                    originalAssociate.Regions.Remove(regionOld);
                }
            }

            foreach (var regionNew in associate.Regions.ToList())
            {
                // Add new regions that aren't in the old associate
                if (!regionsOld.Select(s => s.Id).Contains(regionNew.Id))
                {
                    // TODO: Optimise so that we dont have to query the child
                    var region = this.MomentaDb.Regions.Single(s => s.Id == regionNew.Id);
                    originalAssociate.Regions.Add(region);
                }
            }

            this.UpdateReferences(associate, sourceSite, originalAssociate);

            // Add any new addresses
            // We cannot use a foreach loop because when you call originalAssociate.AssociateAddressHistories.Add(address), 
            // the entity gets removed from associate.AssociateAddressHistories, thus rendering the existing enumerator 
            // invalid, causing an exception
            // http://stackoverflow.com/questions/5538974/the-relationship-could-not-be-changed-because-one-or-more-of-the-foreign-key-pro
            while (associate.AssociateAddressHistories.Any(r => r.ID <= 0))
            {
                var associateAddressHistory = associate.AssociateAddressHistories.FirstOrDefault(r => r.ID <= 0);
                associateAddressHistory.HasHadEndDate = associateAddressHistory.EndDate.HasValue;
                originalAssociate.AssociateAddressHistories.Add(associateAddressHistory);
            }

            // Update any changes in existing addresses
            // After all the new addresses have been added to originalAssociate.AssociateAddressHistories
            // and thus removed from associate.AssociateAddressHistories (see above), the remainder addresses
            // must be the updates.
            foreach (var address in associate.AssociateAddressHistories)
            {
                /* Check we aren't dealing with an address that existed when the page was first loaded but since deleted in another tab / session. */
                var qualifiedEntitySetName =
                    this.MomentaDb.DefaultContainerName + "." + ObjectContext.GetEntitySetName(this.MomentaDb, address);

                // "MomentaRecruitmentEntities.AssociateAddressHistories";
                var entityKey = address.EntityKey ?? new EntityKey(qualifiedEntitySetName, "ID", address.ID);

                object a;
                this.MomentaDb.TryGetObjectByKey(entityKey, out a);

                if (a != null)
                {
                    address.HasHadEndDate = (((AssociateAddressHistory)a).HasHadEndDate == true) || address.EndDate.HasValue;
                    this.MomentaDb.AssociateAddressHistories.ApplyCurrentValues(address);
                }
            }

            this.MomentaDb.SaveChanges();

            this.MomentaDb.ClearAssociateBusinessAreas(associate.ID);

            foreach (var area in associate.BusinessArea)
            {
                this.MomentaDb.InsertAssociateBusinessArea(associate.ID, area.BusinessAreaId);
            }

            return originalAssociate;
        }

        public void UpdateAssociateStatus(int associateId, AssociateApprovalStatus status)
        {
            this.MomentaDb.UpdateAssociateApprovalStatus(associateId, (byte)status);
        }

        public void SaveAssociateDocument(AssociateDocumentType documentType, string documentTitle, Guid documentId, int associateId, string filePath, int fileSize, string edit)
        {
            using (var scope = new TransactionScope(
                TransactionScopeOption.Required, 
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                SqlCommand command = FileStreamHelper.GetCommandForDocumentUpsert(filePath, true, documentId);

                if (documentTitle != null)
                {
                    command.Parameters.Add(new SqlParameter("@Title", SqlDbType.VarChar, 256) { Value = documentTitle });
                }

                if (documentType == AssociateDocumentType.Photo)
                {
                    Image newImage;
                    using (Image image = Image.FromFile(filePath))
                    {
                        newImage = ScaleImage(image, 110, 138);
                    }

                    byte[] photo;
                    using (var ms = new MemoryStream())
                    {
                        newImage.Save(ms, ImageFormat.Jpeg);
                        photo = ms.ToArray();
                    }

                    new FileStreamHelper().SaveDocumentWithoutFileStream(
                        command, ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString, null, photo);
                }
                else
                {
                    new FileStreamHelper().SaveDocumentWithoutFileStream(
                        command, ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString, filePath);
                }

                if (edit == null)
                {
                    edit = "";
                }
                
                if (!edit.Equals("Edit"))
                {
                    var associateDocumentLinkEntry = new AssociateDocument
                    {
                        AssociateId = associateId,
                        DocumentId = documentId,
                        DocumentTypeId = (int)documentType,
                        CreatedDate = DateTime.Now,
                        FileSizes = fileSize
                    };
                    var placeHolder = this.MomentaDb.AssociateDocumentRequired.Where(x => x.AssociateId == associateId && x.DocumentTypeId == (int)documentType).SingleOrDefault();
                    if (placeHolder != null)
                    {
                        this.MomentaDb.AssociateDocumentRequired.DeleteObject(placeHolder);
                    }

                    this.MomentaDb.AssociateDocuments.AddObject(associateDocumentLinkEntry);
                    this.MomentaDb.SaveChanges();
                }
                else {
                    var aDocs = this.MomentaDb.AssociateDocuments.Where(x => x.AssociateId == associateId && x.DocumentTypeId == (int)documentType).ToList();
                    AssociateDocument old = aDocs
                    .GroupBy(p => p.DocumentTypeId)
                    .Select(g => g.OrderByDescending(p => p.CreatedDate).FirstOrDefault())
                    .OrderByDescending(d => d.CreatedDate).SingleOrDefault();
                    
                    Guid oldId = old.DocumentId;

                    //update associate document
                    var associateDocumentLinkEntry = new AssociateDocument();
                    associateDocumentLinkEntry.DocumentId = documentId;
                    associateDocumentLinkEntry.AssociateId = old.AssociateId;
                    associateDocumentLinkEntry.DocumentTypeId = old.DocumentTypeId;
                    associateDocumentLinkEntry.FileSizes = old.FileSizes;
                    associateDocumentLinkEntry.CreatedDate = old.CreatedDate;

                    this.MomentaDb.AssociateDocuments.AddObject(associateDocumentLinkEntry);
                    this.MomentaDb.AssociateDocuments.DeleteObject(old);
                    //old.DocumentId = documentId;
                    this.MomentaDb.SaveChanges();

                    //Delete Orphan
                    this.MomentaDb.DeleteDocument(oldId);
                }
                

                scope.Complete();
            }

            //// Declaration forms should always be obtained from DocuSign as a PDF so the following
            //// code should never be run.
            // if (documentType == AssociateDocumentType.DeclarationForm && !Path.GetExtension(filePath).IsEqualToCaseInsensitive(".pdf"))
            // {
            // string pdfPath = Path.ChangeExtension(filePath, ".pdf");
            // ImageToPDF.CreatePDFFromImage(pdfPath, filePath);
            // this.SaveAssociateDocument(AssociateDocumentType.AutoConvertedDeclarationForm, documentId.ToString(), Guid.NewGuid(), associateId, pdfPath);
            // }
        }

        public void SaveAssociatePDFDocument(AssociateDocumentType documentType, string documentTitle, Guid documentId, int associateId, byte[] fileData)
        {
            string fileName;
            switch (documentType)
            {
                case AssociateDocumentType.DeclarationForm:
                    fileName = "Declaration.pdf";
                    break;

                case AssociateDocumentType.InsuranceQuote:
                    fileName = "InsuranceQuote.pdf";
                    break;

                case AssociateDocumentType.SelfBillingForm:
                    fileName = "SelfBilling.pdf";
                    break;

                default:
                    fileName = documentTitle ?? "Document.pdf";
                    break;
            }

            using (var scope = new TransactionScope(
                TransactionScopeOption.Required, 
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                string userName;
                string applicationName;
                Audit.GetUserAndApplicationName(out applicationName, out userName);

                var command =
                    new SqlCommand(@"dbo.InsertDocument_WithoutFileStreaming")
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                command.Parameters.Add(new SqlParameter("@FileType", SqlDbType.VarChar, 10) { Value = ".pdf" });
                command.Parameters.Add(new SqlParameter("@ContentType", SqlDbType.VarChar, 256) { Value = "application/pdf" });
                command.Parameters.Add(new SqlParameter("@FileName", SqlDbType.VarChar, 256) { Value = fileName });
                command.Parameters.Add(new SqlParameter("@UserName", SqlDbType.Char, 64) { Value = userName });
                command.Parameters.Add(new SqlParameter("@ApplicationName", SqlDbType.Char, 64) { Value = applicationName });
                command.Parameters.Add(new SqlParameter("@DocumentId", SqlDbType.UniqueIdentifier) { Value = documentId });

                if (documentTitle != null)
                {
                    command.Parameters.Add(new SqlParameter("@Title", SqlDbType.VarChar, 256) { Value = documentTitle });
                }

                new FileStreamHelper().SaveDocumentWithoutFileStream(
                    command, ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString, fileData: fileData);

                var associateDocumentLinkEntry = new AssociateDocument
                {
                    AssociateId = associateId,
                    DocumentId = documentId,
                    DocumentTypeId = (int)documentType
                };

                this.MomentaDb.AssociateDocuments.AddObject(associateDocumentLinkEntry);
                this.MomentaDb.SaveChanges();

                scope.Complete();
            }
        }

        public Guid? GetLatestDocumentIdOfType(AssociateDocumentType documentType, int associateId)
        {
            AssociateDocument doc = this.MomentaDb.AssociateDocuments
                .OrderByDescending(d => d.CreatedDate)
                .FirstOrDefault(d => d.AssociateId == associateId && d.DocumentTypeId == (int)documentType);

            return doc == null ? (Guid?)null : doc.DocumentId;
        }

        public bool GetDocumentVATApprovalStatus(AssociateDocumentType documentType, int associateId)
        {
            var doc = this.MomentaDb.AssociateDocuments
               .OrderByDescending(d => d.CreatedDate)
               .FirstOrDefault(d => d.AssociateId == associateId && d.DocumentTypeId == (int)documentType);

            return doc != null && doc.ApprovedDate != null;
        }

        public void SaveReferenceDocument(Guid documentId, int associateId, string filePath, int documentTypeId, int fileSize)
        {
            using (var scope = new TransactionScope(
                TransactionScopeOption.Required, 
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                SqlCommand command = FileStreamHelper.GetCommandForDocumentUpsert(filePath, true, documentId);
                new FileStreamHelper().SaveDocumentWithoutFileStream(
                    command, ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString, filePath);

                var associateDocumentLinkEntry = new AssociateDocument
                {
                    AssociateId = associateId,
                    DocumentId = documentId,
                    DocumentTypeId = documentTypeId,
                    FileSizes=fileSize
                };

                this.MomentaDb.AssociateDocuments.AddObject(associateDocumentLinkEntry);
                this.MomentaDb.SaveChanges();
                scope.Complete();
            }
        }

        public void AddAssociateToAssociateRole(int associateId)
        {
            var associate = this.MomentaDb.Associates
                .Include("aspnet_Users")
                .Single(x => x.ID == associateId);

            var role = this.MomentaDb.aspnet_Roles.Single(x => x.RoleName == "Associate");

            associate.aspnet_Users.aspnet_Roles.Add(role);

            this.MomentaDb.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.itrisDb.Dispose();
                }
            }

            base.Dispose(disposing);

            this.disposed = true;
        }

        private static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            double ratioX = (double)maxWidth / image.Width;
            double ratioY = (double)maxHeight / image.Height;
            double ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }

        private static void CheckNewBillingDocumentRequirement(Associate associate, Associate originalAssociate)
        {
            if (associate.OptOutSelfBilling != true || associate.RequireNewSelfBillingDoc != true)
            {
                if (originalAssociate.VATRegistration != associate.VATRegistration ||
                    originalAssociate.RegisteredCompanyName != associate.RegisteredCompanyName ||
                    originalAssociate.RegisteredCompanyAddress != associate.RegisteredCompanyAddress ||
                    originalAssociate.LimitedCompanyNumber != associate.LimitedCompanyNumber ||
                    originalAssociate.VATRegistered != associate.VATRegistered)
                {
                    associate.RequireNewSelfBillingDoc = true;
                }
            }
        }

        private static SqlCommand GetCommandForTimesheetExpenseUpload(int timesheetId, string title, DateTime date, string filePath)
        {
            DocumentUpdateDetails details = UploadHelper.GetDocumentUpdateDetails(filePath);

            var command = new SqlCommand(@"Timesheet.UpsertTimeSheetExpense_WithoutFileStreaming")
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@TimeSheetId", SqlDbType.Int);
            command.Parameters["@TimeSheetId"].Value = timesheetId;

            command.Parameters.Add("@ContentType", SqlDbType.VarChar, 256);
            command.Parameters["@ContentType"].Value = details.ContentType;

            command.Parameters.Add("@FileName", SqlDbType.VarChar, 256);
            command.Parameters["@FileName"].Value = details.FileName;

            command.Parameters.Add("@Title", SqlDbType.VarChar, 256);
            command.Parameters["@Title"].Value = title;

            command.Parameters.Add("@Date", SqlDbType.DateTime2, 7);
            command.Parameters["@Date"].Value = date;           

            command.Parameters.Add("@FileType", SqlDbType.VarChar, 256);

            command.Parameters["@FileType"].Value = details.Extension;

            command.Parameters.Add("@DocumentId", SqlDbType.UniqueIdentifier);
            command.Parameters["@DocumentId"].Direction = ParameterDirection.Output;

            command.Parameters.Add(new SqlParameter("@UserName", SqlDbType.Char, 64) { Value = details.UserName });
            command.Parameters.Add(new SqlParameter("@ApplicationName", SqlDbType.Char, 64) { Value = details.ApplicationName });

            return command;
        }

        private static SqlCommand GetCommandForInvoiceUpload(int invoiceId, string filePath)
        {
            DocumentUpdateDetails details = UploadHelper.GetDocumentUpdateDetails(filePath);

            var command = new SqlCommand(@"Timesheet.UpsertInvoice_WithoutFileStreaming")
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@InvoiceId", SqlDbType.Int);
            command.Parameters["@InvoiceId"].Value = invoiceId;

            command.Parameters.Add("@ContentType", SqlDbType.VarChar, 256);
            command.Parameters["@ContentType"].Value = details.ContentType;

            command.Parameters.Add("@FileName", SqlDbType.VarChar, 256);
            command.Parameters["@FileName"].Value = details.FileName;

            command.Parameters.Add("@FileType", SqlDbType.VarChar, 256);
            command.Parameters["@FileType"].Value = details.Extension;

            command.Parameters.Add("@StatusText", SqlDbType.VarChar, 256);
            command.Parameters["@StatusText"].Value = "Uploaded - " + details.UserName + " - " + DateTime.Now.ToString("dd/MM/yyyy");

            command.Parameters.Add("@DocumentId", SqlDbType.UniqueIdentifier);
            command.Parameters["@DocumentId"].Direction = ParameterDirection.Output;            

            command.Parameters.Add(new SqlParameter("@UserName", SqlDbType.Char, 64) { Value = details.UserName });
            command.Parameters.Add(new SqlParameter("@ApplicationName", SqlDbType.Char, 64) { Value = details.ApplicationName });                                        

            return command;
        }

        private static SqlCommand GetCommandForCVUpload(int associateId, string filePath)
        {
            DocumentUpdateDetails details = UploadHelper.GetDocumentUpdateDetails(filePath);

            var command = new SqlCommand(@"Associate.UpsertCV_WithoutFileStreaming")
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@AssociateId", SqlDbType.Int);
            command.Parameters["@AssociateId"].Value = associateId;

            command.Parameters.Add("@ContentType", SqlDbType.VarChar, 256);
            command.Parameters["@ContentType"].Value = details.ContentType;

            command.Parameters.Add("@FileName", SqlDbType.VarChar, 256);
            command.Parameters["@FileName"].Value = details.FileName;

            command.Parameters.Add("@FileType", SqlDbType.VarChar, 256);

            command.Parameters["@FileType"].Value = details.Extension;

            command.Parameters.Add(new SqlParameter("@UserName", SqlDbType.Char, 64) { Value = details.UserName });
            command.Parameters.Add(
                new SqlParameter("@ApplicationName", SqlDbType.Char, 64) { Value = details.ApplicationName });

            string parsedContents;

            try
            {
                parsedContents = FileParser.ParseFile(filePath);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);

                parsedContents =
                    string.Format(
                    "Parsing of this CV failed. Technical details (for support personnel) follow{0}{0}{1}",
                    Environment.NewLine,
                    ex);
            }

            command.Parameters.Add("@ParsedContents", SqlDbType.NVarChar, -1);
            command.Parameters["@ParsedContents"].Value = parsedContents;

            return command;
        }

        private static SqlCommand GetCommandForCVSelectWithoutFileStreaming(
            int? associateId, Guid? versionId)
        {
            Debug.Assert(associateId.HasValue != versionId.HasValue, "supply either associateId or versionId, not both");

            var cmd = new SqlCommand(@"[Associate].[SelectCV_WithoutFileStreaming]")
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(
                associateId.HasValue
                ? new SqlParameter("@AssociateId", SqlDbType.Int) { Value = associateId.Value }
                : new SqlParameter("@VersionId", SqlDbType.UniqueIdentifier) { Value = versionId.Value });

            return cmd;
        }

        private static SqlCommand GetCommandForInvoiceSelectWithoutFileStreaming(
            int? invoiceId, Guid? fileId)
        {
            Debug.Assert(invoiceId.HasValue != fileId.HasValue, "supply either invoiceId or fileId, not both");

            var cmd = new SqlCommand(@"[TimeSheet].[InvoiceFile_WithoutFileStreaming]")
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(
                invoiceId.HasValue
                ? new SqlParameter("@invoiceId", SqlDbType.Int) { Value = invoiceId.Value }
                : new SqlParameter("@fileId", SqlDbType.UniqueIdentifier) { Value = fileId.Value });

            return cmd;
        }

        private static DataTable GetAssociateItemsDataTable(
            IEnumerable<int> addresses, 
            IEnumerable<int> accountants, 
            IEnumerable<int> references, 
            IEnumerable<Guid> referenceDocuments)
        {
            var dt = new DataTable();
            dt.Columns.Add("ItemType");
            dt.Columns.Add("ItemId");
            dt.Columns.Add("ItemGuid");

            if (addresses != null)
            {
                foreach (int address in addresses)
                {
                    dt.Rows.Add("Address", address, null);
                }
            }

            if (accountants != null)
            {
                foreach (int accountant in accountants)
                {
                    // Now stored as references
                    dt.Rows.Add("Reference", accountant, null);
                }
            }

            if (references != null)
            {
                foreach (int reference in references)
                {
                    dt.Rows.Add("Reference", reference, null);
                }
            }

            if (referenceDocuments != null)
            {
                foreach (Guid document in referenceDocuments)
                {
                    dt.Rows.Add("ReferenceDocument", null, document);
                }
            }

            return dt;
        }

        private void GatherListOfPreExistingDocumentsInNewReferences(
            ICollection<ReferenceDocumentDetails> list, 
            IEnumerable<AssociateReferenceDetail> referenceCollection)
        {
            foreach (AssociateReferenceDetail reference in referenceCollection)
            {
                this.GatherListOfPreExistingDocumentsInNewReferences(list, reference.SubReferences);

                if (reference.ID != 0)
                {
                    continue;
                }

                foreach (ReferenceDocument referenceDocument in reference.ReferenceDocuments)
                {
                    Document doc = referenceDocument.Document;

                    string qualifiedEntitySetName =
                        this.MomentaDb.DefaultContainerName + "." + ObjectContext.GetEntitySetName(this.MomentaDb, doc);

                    EntityKey entityKey = doc.EntityKey
                        ?? new EntityKey(qualifiedEntitySetName, "DocumentId", doc.DocumentId);

                    object a;
                    if (this.MomentaDb.TryGetObjectByKey(entityKey, out a))
                    {
                        list.Add(new ReferenceDocumentDetails(reference, referenceDocument));
                    }
                }
            }
        }

        private void UpdateReferences(Associate associate, Site sourceSite, Associate originalAssociate)
        {
            var preExistingDocuments = new List<ReferenceDocumentDetails>();
            this.GatherListOfPreExistingDocumentsInNewReferences(preExistingDocuments, associate.AssociateReferenceDetails);

            foreach (ReferenceDocumentDetails referenceDocumentDetails in preExistingDocuments)
            {
                this.MomentaDb.Documents.ApplyCurrentValues(referenceDocumentDetails.ReferenceDocument.Document);

                referenceDocumentDetails.Reference.ReferenceDocuments.Remove(referenceDocumentDetails.ReferenceDocument);
            }

            // Add any new references
            // We cannot use a foreach loop because when you call originalAssociate.AssociateReferenceDetails.Add(reference), 
            // the entity gets removed from associate.AssociateReferenceDetails, thus rendering the existing enumerator 
            // invalid, causing an exception
            // http://stackoverflow.com/questions/5538974/the-relationship-could-not-be-changed-because-one-or-more-of-the-foreign-key-pro
            while (associate.AssociateReferenceDetails.Any(r => r.ID <= 0))
            {
                AssociateReferenceDetail associateReferenceDetail =
                    associate.AssociateReferenceDetails.First(r => r.ID <= 0);

                foreach (AssociateReferenceDetail subReference in associateReferenceDetail.SubReferences)
                {
                    subReference.ContactPermission = associateReferenceDetail.ContactPermission;

                    foreach (ReferenceDocument referenceDocument in subReference.ReferenceDocuments)
                    {
                        this.SetReferenceDocumentEntityState(referenceDocument);
                    }
                }

                originalAssociate.AssociateReferenceDetails.Add(associateReferenceDetail);

                foreach (ReferenceDocument referenceDocument in associateReferenceDetail.ReferenceDocuments)
                {
                    this.SetReferenceDocumentEntityState(referenceDocument);
                }
            }

            // Update any changes in existing references
            // After all the new references have been added to originalAssociate.AssociateReferenceDetails
            // and thus removed from associate.AssociateReferenceDetails (see above), the remainder references
            // must be the updates.
            foreach (AssociateReferenceDetail reference in associate.AssociateReferenceDetails)
            {
                AssociateReferenceDetail originalReference =
                    originalAssociate.AssociateReferenceDetails.FirstOrDefault(r => r.ID == reference.ID);

                if (originalReference == null)
                {
                    /* this can happen if the page was loaded whilst the reference exists but it 
                       was since deleted in another session. */
                    continue;
                }

                if (sourceSite == Site.AssociatePortal && originalReference.LockedOnPortal)
                {
                    /* 
                     * this can happen if they hack the reference ids submitted to the server or if they resubmit an old request.
                     * if the reference should be locked for editing on the Associate Portal and the source is AP then skip it.
                     */
                    continue;
                }

                this.MomentaDb.AssociateReferenceDetails.ApplyCurrentValues(reference);

                foreach (ReferenceDocument refDoc in reference.ReferenceDocuments)
                {
                    this.ApplyReferenceDocumentUpdates(sourceSite, refDoc);
                }

                // Add any new accountants (see above notes about adding new references)
                while (reference.SubReferences.Any(a => a.ID <= 0))
                {
                    AssociateReferenceDetail subReference = reference.SubReferences.First(a => a.ID <= 0);
                    subReference.ContactPermission = reference.ContactPermission;
                    originalReference.SubReferences.Add(subReference);

                    foreach (ReferenceDocument referenceDocument in subReference.ReferenceDocuments)
                    {
                        this.SetReferenceDocumentEntityState(referenceDocument);
                    }
                }

                // Update any changes in existing accountants
                foreach (AssociateReferenceDetail accountant in reference.SubReferences)
                {
                    accountant.ContactPermission = reference.ContactPermission;
                    AssociateReferenceDetail originalSubReference = this.MomentaDb.AssociateReferenceDetails.FirstOrDefault(r => r.ID == accountant.ID);

                    this.MomentaDb.AssociateReferenceDetails.ApplyCurrentValues(accountant);

                    foreach (ReferenceDocument refDoc in accountant.ReferenceDocuments)
                    {
                        this.ApplyReferenceDocumentUpdates(sourceSite, refDoc);
                    }
                }
            }

            foreach (ReferenceDocumentDetails referenceDocumentDetails in preExistingDocuments)
            {
                AssociateReferenceDetail reference = referenceDocumentDetails.Reference;

                var rd = new ReferenceDocument
                {
                    ReferenceId = reference.ID, 
                    DocumentId = referenceDocumentDetails.ReferenceDocument.DocumentId
                };

                reference.ReferenceDocuments.Add(rd);
            }
        }

        public void CheckAssociateDefaultDocument(int associateId)
        {
            var associate = this.MomentaDb.Associates.Where(d => d.ID == associateId).SingleOrDefault();
            var docs = this.MomentaDb.AssociateDocuments.Where(d => d.AssociateId == associateId);

            if (associate.BusinessTypeID == (int)MR_DAL.Enumerations.BusinessType.ContractLtdCompany)
            {
                if (!docs.Where(adr => adr.DocumentTypeId == (int)AssociateDocumentType.CertificateOfIncorporation).Any())
                {
                    CreateAssociateDocumentRequired(associateId, (int)AssociateDocumentType.CertificateOfIncorporation);
                }
                if (!docs.Where(adr => adr.DocumentTypeId == (int)AssociateDocumentType.ProfessionalIndemnity).Any())
                {
                    CreateAssociateDocumentRequired(associateId, (int)AssociateDocumentType.ProfessionalIndemnity);
                }
                if (!docs.Where(adr => adr.DocumentTypeId == (int)AssociateDocumentType.VATCertificate).Any())
                {
                    CreateAssociateDocumentRequired(associateId, (int)AssociateDocumentType.VATCertificate);
                }
                if (!docs.Where(adr => adr.DocumentTypeId == (int)AssociateDocumentType.PIPLInsurance).Any())
                {
                    CreateAssociateDocumentRequired(associateId, (int)AssociateDocumentType.PIPLInsurance);
                }
            }
            else if (associate.BusinessTypeID == (int)MR_DAL.Enumerations.BusinessType.Umbrella)
            {
                if (!docs.Where(adr => adr.DocumentTypeId == (int)AssociateDocumentType.UmbrellaConfirmation).Any())
                {
                    CreateAssociateDocumentRequired(associateId, (int)AssociateDocumentType.UmbrellaConfirmation);
                }
            }
        }

        public void CreateAssociateDocumentRequired(int associateId, int docType)
        {
            if (!this.MomentaDb.AssociateDocumentRequired.Where(adr => adr.AssociateId == associateId && adr.DocumentTypeId == docType).Any())
            {
                var associateDocumentRequired = new AssociateDocumentRequired();
                associateDocumentRequired.AssociateId = associateId;
                associateDocumentRequired.DocumentTypeId = docType;

                this.MomentaDb.AssociateDocumentRequired.AddObject(associateDocumentRequired);
                this.MomentaDb.SaveChanges();
            }
        }

        private void ApplyReferenceDocumentUpdates(Site sourceSite, ReferenceDocument refDoc)
        {
            string qualifiedEntitySetName =
                this.MomentaDb.DefaultContainerName + "." + ObjectContext.GetEntitySetName(this.MomentaDb, refDoc.Document);

            EntityKey entityKey = refDoc.Document.EntityKey
                ?? new EntityKey(qualifiedEntitySetName, "DocumentId", refDoc.Document.DocumentId);

            object o;

            if (!this.MomentaDb.TryGetObjectByKey(entityKey, out o))
            {
                return;
            }

            var originalReferenceDocument = (Document)o;

            bool approvedStatusToUse = (sourceSite == Site.Admin)
                ? refDoc.Document.Approved
                : originalReferenceDocument.Approved;

            this.MomentaDb.Documents.ApplyCurrentValues(refDoc.Document);

            originalReferenceDocument.Approved = approvedStatusToUse;
        }

        private SqlCommand GetCommandForDocumentSelectForFileStreaming(Guid documentId, int? associateId)
        {
            var cmd = new SqlCommand(@"[dbo].[SelectDocument_WithoutFileStreaming]")
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@DocumentId", SqlDbType.UniqueIdentifier) { Value = documentId });

            if (associateId.HasValue)
            {
                cmd.Parameters.Add(new SqlParameter("@AssociateId", SqlDbType.Int) { Value = associateId.Value });
            }

            return cmd;
        }

        private SqlCommand GetCommandForTimesheetDocumentSelectForFileStreaming(Guid documentId, int associateId)
        {
            var cmd = new SqlCommand(@"[dbo].[SelectTimesheetDocument_WithoutFileStreaming]")
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@DocumentId", SqlDbType.UniqueIdentifier) { Value = documentId });
            cmd.Parameters.Add(new SqlParameter("@AssociateId", SqlDbType.Int) { Value = associateId });

            return cmd;
        }

        private void SetReferenceDocumentEntityState(ReferenceDocument referenceDocument)
        {
            // We only want EF to attempt to update the fields in Document not DocumentFull.
            this.MomentaDb.ObjectStateManager.ChangeObjectState(
                referenceDocument.Document, 
                EntityState.Modified);

            this.MomentaDb.ObjectStateManager.ChangeObjectState(
                referenceDocument.Document.DocumentFull, 
                EntityState.Unchanged);
        }

        private class ReferenceDocumentDetails
        {
            public ReferenceDocumentDetails(AssociateReferenceDetail reference, ReferenceDocument referenceDocument)
            {
                this.Reference = reference;
                this.ReferenceDocument = referenceDocument;
            }

            public AssociateReferenceDetail Reference { get; set; }

            public ReferenceDocument ReferenceDocument { get; set; }
        }

        public bool AssociateIsApprover(int associateId)
        {
            var result = this.MomentaDb.AssociateIsApprover(associateId).FirstOrDefault();
            return (result == true ? true : false);
        }

        public IEnumerable<Associate> GetAssociatesByMobileNumber(string number)
        {
            var associates = this.MomentaDb.Associates.Where(a => a.MobilePhone != null).ToList();

            return associates.Where(a => this.SanatizeNumber(a.MobilePhone) == number);
        }

        /// <summary>
        /// Remove any non-numeric characters, replace leading zero with 44
        /// </summary>
        /// <param name="number">Phone number</param>        
        public string SanatizeNumber(string number)
        {
            string result = Regex.Replace(number, "[^0-9]", "");
            result = Regex.Replace(result, "/^0?/", "44");

            if (result.Length > 0 && result.Substring(0, 1) == "0")
            {
                result = "44" + result.Substring(1, result.Length - 1);
            }

            return result;
        }

        public IEnumerable<int> GetAssociateBusinessAreas(int associateId)
        {
            var results = (from r in this.MomentaDb.GetAssociateBusinessAreas(associateId)
                          select r.Value).ToList<int>();
            return results;
        }

        #region Assessments

        public IEnumerable<GetAssessments_Result> GetAssessments(int associateId)
        {
            return this.MomentaDb.GetAssessments(associateId);
        }

        
       
        public IEnumerable<AssessmentModel> GetAssessmentHistory(int associateId, int assessmentTypeId)
        {
            var results = from a in this.MomentaDb.Assessment
                          join t in this.MomentaDb.AssessmentType
                          on a.AssessmentTypeId equals t.AssessmentTypeId
                          where a.AssociateId == associateId && a.AssessmentTypeId == assessmentTypeId
                          select (new AssessmentModel
                          {
                                AssessmentDate = (DateTime)a.AssessmentDate,
                                AssessmentId = (int)a.AssessmentId,
                                AssessmentTypeName = t.Name,
                                AssessmentTypeId = (int)a.AssessmentTypeId,
                                Pass = a.Pass,
                                Scorable = t.Scorable,
                                Score = a.Score,
                                AssociateId = (int)a.AssociateId

                          });

            return results;
        }

        public void UpdateAssessment(Assessment assessment, string documentIds)
        {
            //this.MomentaDb.Assessment.ApplyCurrentValues(assessment);
            //this.MomentaDb.SaveChanges();      
            var documentIdList = new List<string>();
            if (documentIds != "" && documentIds != null)
            {
                documentIdList = documentIds.Split(',').ToList();
            }
            var scsb =
              new SqlConnectionStringBuilder(
                  ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString)
              {
                  Pooling = true,
                  AsynchronousProcessing = true
              };
            var command = new SqlCommand(@"Associate.UpdateAssessment")
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@AssessmentId", SqlDbType.Int);
            command.Parameters["@AssessmentId"].Value = assessment.AssessmentId;

            command.Parameters.Add("@AssociateId", SqlDbType.Int);
            command.Parameters["@AssociateId"].Value = assessment.AssociateId;

            command.Parameters.Add("@AssessmentTypeId", SqlDbType.Int);
            command.Parameters["@AssessmentTypeId"].Value = assessment.AssessmentTypeId;

            command.Parameters.Add("@AssessmentDate", SqlDbType.DateTime);
            command.Parameters["@AssessmentDate"].Value = assessment.AssessmentDate;

            command.Parameters.Add("@Score", SqlDbType.TinyInt);
            command.Parameters["@Score"].Value = assessment.Score;

            command.Parameters.Add("@Pass", SqlDbType.VarChar, 50);
            command.Parameters["@Pass"].Value = assessment.Pass;

            command.Parameters.Add("@Comment", SqlDbType.VarChar, 5000);
            command.Parameters["@Comment"].Value = Convert.ToString(assessment.Comment);

            string userName;
            string applicationName;
            Audit.GetUserAndApplicationName(out applicationName, out userName);


            command.Parameters.Add("@UserName", SqlDbType.Char, 64);
            command.Parameters["@UserName"].Value = userName;
            command.Parameters.Add("@ApplicationName", SqlDbType.Char, 64);
            command.Parameters["@ApplicationName"].Value = applicationName;
            command.Parameters.Add("@DocumentChange", SqlDbType.VarChar, 50);
            if (documentIdList.Count() > 0)
            {
                command.Parameters["@DocumentChange"].Value = "Uploaded "+documentIdList.Count()+" Documents";
            }
            else { command.Parameters["@DocumentChange"].Value = ""; }


            using (var conn = new SqlConnection(scsb.ConnectionString))
            {
                conn.Open();
                using (SqlTransaction trn = conn.BeginTransaction())
                {

                    command.Transaction = trn;
                    command.Connection = conn;
                    command.ExecuteNonQuery();

                    trn.Commit();
                }
            }

            if (documentIdList.Count() > 0)
            {
               
                for (int documentIndex = 0; documentIndex < documentIdList.Count(); documentIndex++)
                {
                    var assessmentDocument = new AssessmentDocument();
                    assessmentDocument.AssessmentId = assessment.AssessmentId;
                    assessmentDocument.AssessmentTypeId = assessment.AssessmentTypeId;
                    assessmentDocument.DocumentId = new Guid(documentIdList[documentIndex]);

                    this.MomentaDb.AssessmentDocument.AddObject(assessmentDocument);
                    this.MomentaDb.SaveChanges();
                }

            }
        }
        public Assessment CreateAssessment(int associateId, int assessmentTypeId, DateTime assessmentDate, string pass, byte score, string Comment, string documentIds)
        {
            var assessment = new Assessment();
            assessment.AssociateId = associateId;
            assessment.AssessmentDate = assessmentDate;
            assessment.AssessmentTypeId = assessmentTypeId;
            assessment.Pass = pass;
            assessment.Score = score;
            assessment.Comment = Comment;
            this.MomentaDb.Assessment.AddObject(assessment);
            this.MomentaDb.SaveChanges();
            int assessmentId = assessment.AssessmentId;
            if (documentIds!="" && documentIds!=null)
            {
                var documentIdList = documentIds.Split(',');
                for (int documentIndex = 0; documentIndex < documentIdList.Count(); documentIndex++)
                {
                    var assessmentDocument = new AssessmentDocument();
                    assessmentDocument.AssessmentId = assessmentId;
                    assessmentDocument.AssessmentTypeId = assessmentTypeId;
                    assessmentDocument.DocumentId =new Guid(documentIdList[documentIndex]);

                    this.MomentaDb.AssessmentDocument.AddObject(assessmentDocument);
                    this.MomentaDb.SaveChanges();
                }

            }
            return assessment;
        }

        public IEnumerable<AssessmentType> GetAssessmentTypes()
        {
            return this.MomentaDb.AssessmentType;
        }

        public AssociateSageId GetSageIdByAssociateId(int id, int projectId)
        {
            //var project = this.MomentaDb.Projects.FirstOrDefault(p => p.Name == projectName);
            return
                this.MomentaDb.AssociateSageId.FirstOrDefault(
                    s => s.AssociateId == id && s.ProjectId == projectId);
        }
        public AssociateSageId CreateSageIdForAssociate(int id, int projectId)
        {
            //var project = this.MomentaDb.Projects.FirstOrDefault(p => p.Name == projectName);
          int businessunitid=   this.MomentaDb.Projects.FirstOrDefault(
                    s => s.ProjectId == projectId).BusinessUnitId;

          string sageid=string.Empty; 
            switch(businessunitid)
            {
                case 3:
                    sageid="ANNE ERS";
                    break;
                case 5:
                    sageid="LTD00001";
                    break;
                case 6:
                        sageid="LTD00003";
                    break;
                case 7:
                    sageid="LTD00867";
                    break;
                default:
                    sageid="LTD01003";
                    break;
                

            }       
              
          var assessmentSage = new AssociateSageId();
          assessmentSage.AssociateId = id;
          assessmentSage.ProjectId = projectId;
          assessmentSage.SageId = sageid;
          this.MomentaDb.AssociateSageId.AddObject(assessmentSage);
          this.MomentaDb.SaveChanges();

            return
                this.MomentaDb.AssociateSageId.FirstOrDefault(
                    s => s.AssociateId == id && s.ProjectId == projectId);
        }
        public List<UmbrellaUser> GetUmbrellaEmailList(byte companyId)
        {
            return this.MomentaDb.UmbrellaUsers.Where(u => u.UmbrellaCompanyId == companyId).ToList();
        }

        public string GetBusinessUnit(int businessUnitId)
        {
            return this.MomentaDb.BusinessUnits.Where(u => u.BusinessUnitId == businessUnitId).Select(o=>o.Description).ToString();
        }
        public string GetSageCompanyIdForBusinessUnit(int businessUnitId)
        {
            var sageCompanyId = this.MomentaDb.BusinessUnits.Where(u => u.BusinessUnitId == businessUnitId).ToList().FirstOrDefault();
            return sageCompanyId != null ? sageCompanyId.SageCompanyID.ToString() : null;
        }

        public List<int> GetSageCompanyIds()
        {
            List<int> scids = new List<int>(); 
            var sagecompanyids = this.MomentaDb.BusinessUnits.Select(s => s.SageCompanyID).ToList();
            foreach (var s in sagecompanyids)
            {
                scids.Add(Convert.ToInt32(s));
            }           
           
            return scids;
        }
        public List<string> GetPurchaseSageIds(int companyid)
        {
            var purchasesageids = this.MomentaDb.GetPurchaseSageIds.Where(x => x.SageCompanyID == companyid).Select(s => s.SageId).Distinct().ToList();
            List<string> psids = new List<string>();
            foreach (var s in purchasesageids)
            {
                psids.Add(Convert.ToString(s));
            }

            return psids;


        }
     
        #endregion

        public void UpdateScheduleField(int AssId, string PropertyName, string value)
        {
            var scsb =
              new SqlConnectionStringBuilder(
                  ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString)
              {
                  Pooling = true,
                  AsynchronousProcessing = true
              };
            var command =
               new SqlCommand("scheduler.UpdateAssociateFieldSingle")
               {
                   CommandType = CommandType.StoredProcedure
               };

            command.Parameters.Add(new SqlParameter("@ColName", SqlDbType.VarChar, 500) { Value = PropertyName });
            command.Parameters.Add(new SqlParameter("@ColValue", SqlDbType.VarChar, 500) { Value = value.Trim() });
            command.Parameters.Add(new SqlParameter("@AssId", SqlDbType.Int) { Value = AssId });


            using (var conn = new SqlConnection(scsb.ConnectionString))
            {
                conn.Open();
                using (SqlTransaction trn = conn.BeginTransaction())
                {

                    command.Transaction = trn;
                    command.Connection = conn;
                    command.ExecuteNonQuery();

                    trn.Commit();
                }
            }

        }

        public void BackDateAssociateChanges(int associateId, string field, string value, int taskId)
        {
            var scsb =
              new SqlConnectionStringBuilder(
                  ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString)
              {
                  Pooling = true,
                  AsynchronousProcessing = true
              };
            var command =
               new SqlCommand("dbo.BackDateAssociateChanges")
               {
                   CommandType = CommandType.StoredProcedure
               };
            command.Parameters.Add(new SqlParameter("@associateId", SqlDbType.Int) { Value = associateId });
            command.Parameters.Add(new SqlParameter("@col", SqlDbType.VarChar, 500) { Value = field });
            command.Parameters.Add(new SqlParameter("@ColValue", SqlDbType.VarChar, 500) { Value = value.Trim() });
            command.Parameters.Add(new SqlParameter("@taskId", SqlDbType.Int) { Value = taskId });


            using (var conn = new SqlConnection(scsb.ConnectionString))
            {
                conn.Open();
                using (SqlTransaction trn = conn.BeginTransaction())
                {

                    command.Transaction = trn;
                    command.Connection = conn;
                    command.ExecuteNonQuery();

                    trn.Commit();
                }
            }

            //this.MomentaDb.BackDateAssociateChanges(associateId, field, value, changeDate);
        }

        public ScheduledTaskViewModel GetTask(int taskId)
        {
            var result = this.MomentaDb.Task.FirstOrDefault(t => t.TaskId == taskId);

            return new ScheduledTaskViewModel
            {
                Active = result.Active,
                Arguments = result.Arguments,
                AuthenticationScheme = result.AuthenticationScheme,
                Created = result.Created,
                EndDate = (DateTime)result.EndDate,
                PeriodLength = result.PeriodLength,
                PeriodType = (ScheduledTaskPeriodType)result.PeriodTypeId,
                StartDate = (DateTime)result.StartDate,
                TaskId = result.TaskId,
                TaskName = result.TaskName,
                Url = result.Url
            };
        }

    }
}