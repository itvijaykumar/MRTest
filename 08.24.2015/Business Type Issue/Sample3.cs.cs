namespace Admin.Repositories
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Metadata.Edm;
    using System.Data.Objects;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Transactions;
    using System.Text.RegularExpressions;

    using Admin.Helpers;
    using Admin.Models;

    using ITRIS_DAL;

    using MomentaRecruitment.Common.Exceptions;
    using MomentaRecruitment.Common.Helpers;
    using MomentaRecruitment.Common.Models;
    using MomentaRecruitment.Common.ViewModel;

    using MR_DAL;
    using MR_DAL.Enumerations;

    using AssociateApprovalStatus = MR_DAL.Enumerations.AssociateApprovalStatus;
    using BusinessType = MR_DAL.Enumerations.BusinessType;
    using IsolationLevel = System.Transactions.IsolationLevel;
    using ReferenceType = MR_DAL.ReferenceType;    

    public class AssociateRepository : MomentaRecruitment.Common.Repositories.AssociateRepository, IAssociateRepository
    {
        private static readonly Func<MomentaRecruitmentEntities, int, Associate> AssociateByIdQuery =
            CompiledQuery.Compile(
                (MomentaRecruitmentEntities db, int id) => db.Associates
                    .Include("Regions")
                    .Include("InsuranceApplication")
                    .Include("MarketSectorRoles")
                    .Include("Qualifications")
                    .Include("AssociateQualificationFreeTexts")
                    .Include("Specialism")
                    .Include("AssociateAddressHistories")
                    .Include("AssociateAddressHistories.County1")
                    .Include("CV")
                    .Include("AssociateReferenceDetails")
                    .Include("Comments")
                    .Include("County")
                    .Include("AssociateDocuments.Document")
                    .Include("AssociateDocuments.AssociateDocumentComments")
                    .Include("aspnet_Users")
                    .Include("aspnet_Users.aspnet_Membership")
                    .Include("VettingChecks")
                    .Include("VettingChecks.VettingCheckStatu")
                    .Include("VettingChecks.VettingCheckComments")
                    .Include("AssociateReferenceDetails.RefereeReferenceDetail")
                    .Include("AssociateReferenceDetails.ReferenceDocuments")
                    .Include("AssociateReferenceDetails.ReferenceDocuments.Document")
                    .Include("AssociateReferenceDetails.SubReferences").Single(a => a.ID == id));

        protected override Func<MomentaRecruitmentEntities, int, Associate> CompiledQueryForGetAssociateById
        {
            get
            {
                return AssociateByIdQuery;
            }
        }

        public IEnumerable<int> GetIndividualIdsForProject(IEnumerable<int> associateIds, int projectId)
        {
            var individuals = this.MomentaDb.Individual;

            var assids = associateIds.ToList();

            var ids = from i in individuals
                      where assids.Contains(i.AssociateId)
                      && i.ProjectId == projectId
                      select i.IndividualId;

            return ids;
        }

        public List<MR_DAL.BusinessType> GetBusinessTypes()
        {
            return this.MomentaDb.BusinessTypes.ToList();
        }

        public IEnumerable<AssociateAddressHistory> GetAssociateAddressVersions(int id, DateTime from, DateTime to)
        {
            return this.MomentaDb.spGet_dbo_AssociateAddressHistory_HistoricState(from, to, id);
        }

        public IEnumerable<AssociateReferenceDetail> GetAssociateReferenceVersions(int id, DateTime from, DateTime to)
        {
            var referenceVersions =
                this.MomentaDb.spGet_dbo_AssociateReferenceDetails_HistoricState(from, to, id).ToList();

            foreach (var reference in referenceVersions)
            {
                var accountants =
                    this.MomentaDb.spGet_dbo_AssociateReferenceAccountant_HistoricState(from, to, reference.ID);
                foreach (var accountant in accountants)
                {
                    reference.SubReferences.Add(accountant);
                }
            }

            return referenceVersions;
        }

        public Associate GetAssociateVersion(int id, DateTime when)
        {
            Associate versionedAssociate = this.MomentaDb.spGet_dbo_Associate_HistoricState(when, id).First();

            byte?[] selectedRegions = this.MomentaDb.Get_dbo_Associate_SelectedRegionHistoricState(when, id).ToArray();

            foreach (var selectedRegion in selectedRegions)
            {
                versionedAssociate.Regions.Add(new Region { Id = selectedRegion.Value });
            }

            int?[] marketSectorRoles = this.MomentaDb.Get_dbo_Associate_MarketSectorRoleHistoricState(when, id).ToArray();

            foreach (var marketSectorRole in marketSectorRoles)
            {
                versionedAssociate.MarketSectorRoles.Add(new MarketSectorRole { MarketSectorRoleId = marketSectorRole.Value });
            }

            byte?[] specialisms = this.MomentaDb.Get_dbo_Associate_SpecialismHistoricState(when, id).ToArray();

            foreach (var specialism in specialisms)
            {
                versionedAssociate.Specialism.Add(new Specialism { SpecialismId = specialism.Value });
            }

            int?[] qualifications = this.MomentaDb.Get_dbo_Associate_QualificationHistoricState(when, id).ToArray();

            foreach (var qualification in qualifications)
            {
                versionedAssociate.Qualifications.Add(new MR_DAL.Qualification { QualificationId = qualification.Value });
            }

            var qualificationFreeTexts = this.MomentaDb.Get_dbo_Associate_QualificationFreeTextHistoricState(when, id).ToList();

            foreach (var qualificationFreeText in qualificationFreeTexts)
            {
                versionedAssociate.AssociateQualificationFreeTexts.Add(qualificationFreeText);
            }

            return versionedAssociate;
        }

        public IEnumerable<ListItem<byte>> GetBusinessUnitOptions()
        {
            var businessUnits = new List<ListItem<byte>>();

            foreach (var businessUnit in this.MomentaDb.BusinessUnits)
            {
                if (businessUnit.BusinessUnitId != 1 && businessUnit.BusinessUnitId != 2)
                {
                    businessUnits.Add(ListItem.Create(businessUnit.BusinessUnitId, businessUnit.Description));
                }              
            }

            return businessUnits;
        }

        public Guid AssignAssociateOwner(string lastName)
        {
            string startingChar = lastName.Substring(0, 1).ToUpper();

            return this.MomentaDb.AssociateAdminOwnerMappings.Where(n => n.Id == startingChar).Select(n => n.AdminUserId).Single();
        }

        public int CreateAssociate(Associate associate, Guid membershipUserId)
        {
            var stubAspNetUser = new aspnet_Users { UserId = membershipUserId };
            this.MomentaDb.aspnet_Users.Attach(stubAspNetUser);

            associate.aspnet_Users = stubAspNetUser;
            this.MomentaDb.Associates.AddObject(associate);

            this.MomentaDb.SaveChanges();

            return associate.ID;
        }

        public IQueryable<Associate> GetAssociatesWithApprovalStatus(List<AssociateApprovalStatus> statuses)
        {
            var statusAsInts = statuses.ConvertAll(x => (int)x);

            return
                this.MomentaDb.Associates.Include("LockedAssociate").Where(
                    a => statusAsInts.Contains(a.AssociateApprovalStatusId));
        }

        public IQueryable<PostCode> GetPostCodes()
        {
            return this.MomentaDb.PostCode;
        }

        public IQueryable<Associate> GetAssociates()
        {
            return this.MomentaDb.Associates.Include("AssociateApprovalStatus").Include("LockedAssociate").Include("UmbrellaCompany");
        }

        public IQueryable<Associate> GetAssociatesSearch()
        {
            return this.MomentaDb.Associates
                .Include("AssociateAddressHistories")
                .Include("AssociateAddressHistories.County1")
                .Include("AssociateAddressHistories.Country")
                .Include("RegistrationType")
                .Include("Agency")
                .Include("ReferralSource")
                .Include("BusinessType")
                .Include("VisaType")
                //.Include("Availability")
                .Include("NoticeInterval")
                .Include("Country")
                .Include("County")
                .Include("Individual")
                .Include("Individual.Client")
                .Include("Individual.Role")
                .Include("Individual.Role.RoleType")
                .Include("AssociateDocuments")
                .Include("PersonTitle")
                //.Include("AssociateQualification")
                .Include("AssociateApprovalStatus");
        }

        public IQueryable<Associate> GetGraduateSearch()
        {
            return this.MomentaDb.Associates
                .Include("AssociateAddressHistories")
                .Include("Nationality")
                .Include("Individual")
                .Include("PersonTitle")
                .Include("AssociateApprovalStatus");
                //.Include("AssociateAddressHistories.County1")
                //.Include("AssociateAddressHistories.Country")
                //.Include("RegistrationType")
                //.Include("Agency")
                //.Include("ReferralSource")
                //.Include("BusinessType")
                //.Include("VisaType")
                //.Include("Availability")
                //.Include("NoticeInterval")
                //.Include("Country")
                //.Include("County")
                //.Include("Individual")
                //.Include("PersonTitle")
                //.Include("AssociateQualification")
                //.Include("AssociateApprovalStatus");
                ;
        }

        public List<Associate> GetAllAssociates()
        {
            return this.MomentaDb.Associates.ToList();
        }

        public List<Associate> GetAssociatesByClientId(int clientId)
        {
            var q1 = from a in this.MomentaDb.Associates
                     join i in this.MomentaDb.Individual on a.ID equals i.AssociateId
                     where i.ClientId == clientId
                     select a;

            return q1.Distinct().ToList();
        }

        public List<Associate> GetAssociatesByProjectId(int projectId)
        {
            var q1 = from a in this.MomentaDb.Associates
                     join i in this.MomentaDb.Individual on a.ID equals i.AssociateId
                     where i.ProjectId == projectId
                     select a;

            return q1.Distinct().ToList();
        }


        public Associate GetAssociatesById(int associateId)
        {
            var q1 = from a in this.MomentaDb.Associates
                     where a.ID == associateId
                     select a;

            return q1.FirstOrDefault();
        }

        public IQueryable<BulkTimesheetChangeSearch> GetBulkTimesheetChangeSearch()
        {
            return this.MomentaDb.BulkTimesheetChangeSearch;
        }
        
        public IOrderedQueryable<ConsolidatedEditHistory> GetAssociateEditHistory(int associateId)
        {
            return
                this.MomentaDb.ConsolidatedEditHistories.Where(h => h.AssociateID == associateId).OrderBy(
                    h => h.ModifiedTime);
        }

        public IEnumerable<ItemHistory> GetItemHistory(
            int? itemId,
            ItemHistoryType itemType,
            DateTime from,
            DateTime to,
            out DateTime? createdDate,
            out DateTime? deletedDate,
            out bool hasRestoredDelete)
        {
            if (!Enum.IsDefined(typeof(ItemHistoryType), itemType))
            {
                throw new InvalidEnumArgumentException("itemType", (int)itemType, typeof(ItemHistoryType));
            }

            var createdDateParam = new ObjectParameter("CreatedDate", typeof(DateTime));
            var deletedDateParam = new ObjectParameter("DeletedDate", typeof(DateTime));
            var hasRestoredDeleteParam = new ObjectParameter("HasRestoredDelete", typeof(bool));

            var itemHistory =
                this.MomentaDb.GetItemHistory(
                    itemId,
                    itemType.ToString(),
                    from,
                    to,
                    createdDateParam,
                    deletedDateParam,
                    hasRestoredDeleteParam).ToList();

            createdDate = createdDateParam.Value == DBNull.Value
                              ? (DateTime?)null
                              : DateTime.SpecifyKind((DateTime)createdDateParam.Value, DateTimeKind.Utc);
            deletedDate = deletedDateParam.Value == DBNull.Value
                              ? (DateTime?)null
                              : DateTime.SpecifyKind((DateTime)deletedDateParam.Value, DateTimeKind.Utc);

            hasRestoredDelete = hasRestoredDeleteParam.Value != DBNull.Value && (bool)hasRestoredDeleteParam.Value;

            return itemHistory;
        }

        public IEnumerable<ItemRestoredDeleteHistory> GetItemRestoredDeleteHistory(
            int? itemId, ItemHistoryType itemType)
        {
            return this.MomentaDb.GetItemRestoredDeleteHistory(itemId, itemType.ToString());
        }

        /// <summary>
        ///     This is required by the stored procedures that insert to Itris
        /// </summary>
        /// <param name="userId">
        /// </param>
        /// <returns>
        /// </returns>
        public string GetItrisEmployeeIdFromMembershipId(Guid userId)
        {
            var employee =
                this.MomentaDb.aspnet_Users_Itris_Employee.FirstOrDefault(e => e.UserId == userId);

            return employee != null ? employee.ITRIS_EMP_ID : null;
        }

        /// <summary>
        /// </summary>
        /// <param name="id">The Associate.Id to check the lock table for</param>
        /// <returns>The Employee's UserId in the Membership DB if locked or null if no lock exists</returns>
        public Guid? GetLockingEmployeeUserId(int id)
        {
            var lockRecord = this.MomentaDb.LockedAssociate.SingleOrDefault(a => a.AssociateId == id);

            return (lockRecord == null) ? (Guid?)null : lockRecord.EmployeeUserId;
        }

        public void InsertAssociateToItris(Associate associate, string empId)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(empId));

            string title;
            string businessType;

            if (associate.PersonTitleId == 5)
            {
                title = associate.TitleOther;
            }
            else
            {
                title = this.MomentaDb.PersonTitles
                            .Where(t => t.PersonTitleId == associate.PersonTitleId)
                            .Select(t => t.Description)
                            .FirstOrDefault();
            }

            switch ((BusinessType?)associate.BusinessTypeID)
            {
                case BusinessType.ContractLtdCompany:
                    businessType = "L";
                    break;
                case BusinessType.Umbrella:
                    businessType = "U";
                    break;
                default:
                    businessType = null;
                    break;
            }

            string countyName = associate.OtherCounty;

            if (string.IsNullOrEmpty(countyName))
            {
                County county = this.MomentaDb.Counties.SingleOrDefault(c => c.ID == associate.CountyID);
                if (county != null)
                {
                    countyName = county.Name;
                }
            }

            // Personal Details
            this.ItrisDb.ip_AddNewApplicant(
                title: title,
                firstName: associate.FirstName,
                middleName: associate.MiddleName,
                lastName: associate.LastName,
                birthDate: associate.DateOfBirth,
                mobileTel: associate.MobilePhone,
                homeTel: associate.HomePhone,
                workTel: associate.WorkPhone,
                otherTel: associate.OtherPhone,
                address: string.Format("{0} {1} {2} {3} {4} {5}", associate.UnitNumber, associate.HouseName, associate.HouseNumber, associate.Street, associate.Town, countyName),
                postCode: associate.Postcode,
                eMail: associate.Email,
                oFlags: 8,
                workType: associate.AvailabilityTypeId,
                //// Field is mandatory but not used in Portal site.
                available: associate.AvailabilityTypeId != 1 ? associate.AvailableDate : null,
                //// set again by ip_SetAppAvailability anyway
                notice: associate.AvailabilityTypeId == 2 ? associate.NoticePeriod : 0,
                //// set again by ip_SetAppAvailability anyway
                skillsList: null,
                //// set again by ip_SetAppAvailability anyway
                empID: empId,
                tFlags: 32639,
                rate: null,
                salary: null,
                rateInt: null,
                retJobID: null,
                groupID: null,
                addToScan: null,
                typeID: null,
                nationID: null,
                fAOOf: null,
                wEBAddress: null,
                lTDCoName: null,
                mediaID: null,
                cVText: null,
                originalCVPath: null,
                statusID: null,
                addToEAAReg: null,
                profile: null,
                companyID: null,
                contactID: null,
                jobTitle: null,
                ownerID: null,
                holdDays: null,
                profileHTML: null);

            Applicant newApplicant =
                this.ItrisDb.Applicants.OrderByDescending(a => a.CREATED_ON).FirstOrDefault(
                    a => a.EMAIL == associate.Email);

            if (newApplicant == null)
            {
                throw new RecordNotFoundException(
                    string.Format("newApplicant with email {0}could not be retrieved", associate.Email));
            }

            string applicantId = newApplicant.APP_ID;

            // Set Visa stuff.
            Debug.Assert(associate.HasVisa != null, "associate.HasVisa != null");

            this.ItrisDb.ip_UpdatePersonalDetails(
                appID: applicantId,
                vISA_EXPIRY: associate.HasVisa.Value ? associate.VisaExpiry : null,
                visaType: associate.HasVisa.Value ? this.MomentaDb.VisaTypes.Where(v => v.ID == associate.VisaTypeId.Value).Select(v => v.Name).Single() : null,
                vISA_NO: associate.VisaNumber,
                nI_NUMBER: associate.NI,
                tAX_CODE: null,
                nI_CODE_LETTER: null,
                pASSPORT_NO: associate.PassportNumber,
                dAYS_PER_WEEK: null,
                iD_VERIFIED: null,
                vERIFIED_BY: null,
                vAT_REGISTERED: null,
                vAT_REG_NO: null,
                bUSINESS_TYPE: businessType,
                tRADE_NAME: null,
                tRADE_DESCRIPTION: null,
                tRADE_ADDRESS: null,
                tRADE_POST_CODE: null,
                cOMP_REG_NO: null,
                sTUDENT: null,
                bANK_NAME: null,
                bANK_ADDRESS: null,
                aCCOUNT_NO: null,
                sORT_CODE: null,
                aCCOUNT_NAME: null,
                bANKBUILD_REF: null,
                sWIFT_CODE: null,
                iBAN_NUMBER: null,
                eMP_ID: null,
                gENDER: null,
                mARITAL_STATUS: null,
                umbrellaCompanyID: null);

            this.ItrisDb.ip_SetAppAvailability(
                appID: applicantId,
                date: associate.AvailabilityTypeId != 1 ? associate.AvailableDate : null,
                nextCallDate: null,
                notice: associate.AvailabilityTypeId == 2 ? associate.NoticePeriod : 0,
                workType: associate.AvailabilityTypeId,
                noticeType: associate.AvailabilityTypeId == 2 ? associate.NoticeIntervalId : null);

            // Set Comments
            this.ItrisDb.ip_UpdateApplicantDetails(
                appID: applicantId,
                empID: empId,
                qComments: string.Empty,
                forceUpdate: false,
                tFlags: null,
                oFlags: null,
                deleted: null,
                lastContacted: null,
                eMail: null,
                homeTel: null,
                workTel: null,
                mobileTel: null,
                otherTel: null,
                address: null,
                postCode: null,
                wEBAddress: null,
                dOB: null,
                lastCV: null,
                lastTyped: null,
                typeID: null,
                ownerID: null,
                fAOOf: null,
                nationID: null,
                currentPosition: null);
            this.ItrisDb.SaveChanges();

            // Update associate.ITRISAppId
            Associate momentaDBAssociate = this.MomentaDb.Associates.Single(a => a.ID == associate.ID);
            momentaDBAssociate.ITRISAppId = applicantId;
            this.MomentaDb.SaveChanges();
        }

        public void SaveUpdatedAssociate(Associate associate)
        {

            Associate momentaDBAssociate = associate;
            this.MomentaDb.SaveChanges();
        }

        public void UpdateAssociateToItris(Associate associate, string empId)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(empId));

            string title;
            string businessType;

            if (associate.PersonTitleId == 5)
            {
                title = associate.TitleOther;
            }
            else
            {
                title = this.MomentaDb.PersonTitles
                            .Where(t => t.PersonTitleId == associate.PersonTitleId)
                            .Select(t => t.Description)
                            .FirstOrDefault();
            }

            switch ((BusinessType?)associate.BusinessTypeID)
            {
                case BusinessType.ContractLtdCompany:
                    businessType = "L";
                    break;
                case BusinessType.Umbrella:
                    businessType = "U";
                    break;
                default:
                    businessType = null;
                    break;
            }

            this.ItrisDb.ip_SetApplicantsName(
                appID: associate.ITRISAppId,
                title: title,
                first: associate.FirstName,
                middle: associate.MiddleName,
                last: associate.LastName,
                userID: empId);

            this.ItrisDb.ip_UpdateApplicantDetails(
                appID: associate.ITRISAppId,
                empID: empId,
                qComments: string.Empty,
                forceUpdate: false,
                tFlags: null,
                oFlags: null,
                deleted: null,
                lastContacted: null,
                eMail: null,
                homeTel: null,
                workTel: null,
                mobileTel: null,
                otherTel: associate.OtherPhone,
                address: null,
                postCode: null,
                wEBAddress: null,
                dOB: null,
                lastCV: null,
                lastTyped: null,
                typeID: null,
                ownerID: null,
                fAOOf: null,
                nationID: null,
                currentPosition: null);

            ////this.ItrisDb.SaveChanges();
            this.ItrisDb.ip_UpdatePersonalDetails(
                appID: associate.ITRISAppId,
                vISA_EXPIRY: associate.HasVisa.Value ? associate.VisaExpiry : null,
                visaType: associate.HasVisa.Value ? this.MomentaDb.VisaTypes.Where(v => v.ID == associate.VisaTypeId.Value).Select(v => v.Name).Single() : null,
                vISA_NO: associate.HasVisa.Value ? associate.VisaNumber : null,
                nI_NUMBER: associate.NI,
                tAX_CODE: null,
                nI_CODE_LETTER: null,
                pASSPORT_NO: associate.PassportNumber,
                dAYS_PER_WEEK: null,
                iD_VERIFIED: null,
                vERIFIED_BY: null,
                vAT_REGISTERED: null,
                vAT_REG_NO: null,
                bUSINESS_TYPE: businessType,
                tRADE_NAME: null,
                tRADE_DESCRIPTION: null,
                tRADE_ADDRESS: null,
                tRADE_POST_CODE: null,
                cOMP_REG_NO: null,
                sTUDENT: null,
                bANK_NAME: null,
                bANK_ADDRESS: null,
                aCCOUNT_NO: null,
                sORT_CODE: null,
                aCCOUNT_NAME: null,
                bANKBUILD_REF: null,
                sWIFT_CODE: null,
                iBAN_NUMBER: null,
                eMP_ID: null,
                gENDER: null,
                mARITAL_STATUS: null,
                umbrellaCompanyID: null);

            this.ItrisDb.ip_SetAppAvailability(
                appID: associate.ITRISAppId,
                date: associate.AvailabilityTypeId != 1 ? associate.AvailableDate : null,
                nextCallDate: null,
                notice: associate.AvailabilityTypeId == 2 ? associate.NoticePeriod : 0,
                workType: associate.AvailabilityTypeId,
                noticeType: associate.AvailabilityTypeId == 2 ? associate.NoticeIntervalId : null);

            this.ItrisDb.ip_UpdateMainApplicantDetails(
                appID: associate.ITRISAppId,
                firstName: associate.FirstName,
                middleName: associate.MiddleName,
                lastName: associate.LastName,
                address: associate.UnitNumber + " " + associate.HouseName + " " + associate.HouseNumber + " " + associate.Street + " " + associate.Town,
                postCode: associate.Postcode,
                mobileTel: associate.MobilePhone,
                homeTel: associate.HomePhone,
                workTel: associate.WorkPhone,
                dOB: associate.DateOfBirth,
                nationID: null,
                email: associate.Email,
                rate: null,
                salary: null,
                currency: null,
                rateIntvl: null,
                about: null);

            // ip_UpdateApplicantStatus
            // @AppID CHAR(10) ,
            // @StatusID INT

            // ip_UpdateComment
            // @CommentID INT ,
            // @FormID TINYINT ,
            // @RecordID CHAR(10) ,
            // @AltAppID CHAR(10) ,
            // @AltCompanyID CHAR(10) ,
            // @AltContactID CHAR(10) ,
            // @AltJobID CHAR(10) ,
            // @AltPlaceID CHAR(10) ,
            // @AltUserEmpID CHAR(10) ,
            // @AltPreRegID INT ,
            // @EmpID CHAR(10) ,
            // @Type INT ,
            // @Comment VARCHAR(1000) ,
            // @SpokenTo TINYINT ,
            // @AltAgencyID INT = NULL ,
            // @AltAgencyAppID INT = NULL ,
            // @AltAgencyContactID INT = NULL
            this.ItrisDb.SaveChanges();
        }

        public void ArchiveAssociate(int associateId, AssociateApprovalStatus status)
        {
            this.UpdateAssociateStatus(associateId, status);
        }

        public void ChangeToFullAssociate(int associateId)
        {
            this.UpdateAssociateStatus(associateId, AssociateApprovalStatus.Registered);
        }

        public void UnArchiveAssociate(int associateId)
        {
            this.MomentaDb.UnArchiveAssociate(associateId);
        }

        public FileDownloadModel GetCvVersion(Guid id)
        {
            return this.GetCV(null, id);
        }

        public string GetParsedCvForVersion(int id, DateTime to)
        {
            Associate_CV associateCV =
                this.MomentaDb.Associate_CV.Where(
                    cv =>
                    cv.AssociateId == id && cv.ModifiedTime <= to && cv.ParsedContents != null
                    && cv.ParsedContents != string.Empty).OrderByDescending(cv => cv.ModifiedTime).FirstOrDefault();

            return associateCV == null ? null : associateCV.ParsedContents;
        }

        public void CheckAssociateDefaultDocument(int associateId)
        {
            var associate = this.MomentaDb.Associates.Where(d => d.ID == associateId).SingleOrDefault();
            var docs = this.MomentaDb.AssociateDocuments.Where(d => d.AssociateId == associateId);

            if(associate.BusinessTypeID == (int)BusinessType.ContractLtdCompany)
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
            else if (associate.BusinessTypeID == (int)BusinessType.Umbrella)
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

        public void SaveVisualInspection(Guid documentId, bool approved, string comments, string userName)
        {
            AssociateDocument associateDocument = this.MomentaDb.AssociateDocuments.Single(d => d.DocumentId == documentId);

            if (associateDocument.ApprovedDate.HasValue != approved)
            {
                associateDocument.ApprovedDate = approved ? DateTime.Now : (DateTime?)null;
                associateDocument.ApprovedBy = approved ? userName : null;
            }

            if (!string.IsNullOrEmpty(comments))
            {
                var comment = new AssociateDocumentComment
                                  {
                                      DocumentId = documentId,
                                      CreatedTime = DateTime.Now,
                                      Comment = comments,
                                      User = userName
                                  };

                this.MomentaDb.AssociateDocumentComments.AddObject(comment);
            }

            this.MomentaDb.SaveChanges();
        }

        public override IEnumerable<ListItem<byte>> GetReferenceTypeOptions()
        {
            var referenceTypes = new List<ListItem<byte>>();

            foreach (
                ReferenceType referenceType in
                    this.MomentaDb.ReferenceTypes.Where(r => r.Enabled == true && r.DisplayInAdmin == true))
            {
                referenceTypes.Add(ListItem.Create(referenceType.ID, referenceType.Description));
            }

            return referenceTypes;
        }

        public void SaveCommentForReference(Comment commentEntity, int referenceId)
        {
            this.MomentaDb.Comments.AddObject(commentEntity);

            if (referenceId != 0)
            {
                var referenceStub = new AssociateReferenceDetail { ID = referenceId };
                this.MomentaDb.AssociateReferenceDetails.Attach(referenceStub);
                commentEntity.AssociateReferenceDetail = referenceStub;
            }

            this.MomentaDb.SaveChanges();
        }

        private string GetCommunicationTypeName(CommunicationType objCommType)
        {
            string strCommTypeName = Convert.ToString(objCommType);
            switch ((CommunicationType)objCommType)
            {
                case CommunicationType.Absence:
                    strCommTypeName="Absence"; 
                    break;
                case CommunicationType.Call:
                    strCommTypeName="Call"; break;
                case CommunicationType.Email:
                    strCommTypeName="Email"; break;
                case CommunicationType.Expense:
                    strCommTypeName="Expense"; break;
                case CommunicationType.Note:
                    strCommTypeName="Note"; break;
                case CommunicationType.SMS:
                    strCommTypeName="SMS"; break;
                case CommunicationType.Status:
                    strCommTypeName="Status"; break;
                case CommunicationType.Timesheet:
                    strCommTypeName="Timesheet"; break;

                default:
                    if (Convert.ToInt32(objCommType) == 0)
                        strCommTypeName = "Emails";
                    else
                        strCommTypeName = Convert.ToString(objCommType);
                    break;
            }
            return strCommTypeName;
        }

        public List<CommunicationHistoryModel> GetPaginatedCommunicationsHistory(int associateId, int pageSize, int page, string sortColumn, string sortDirection, string searchText, out int resultsCount, int isAutomatic =0)
        {
            var result = this.MomentaDb.GetPaginatedCommunicationsHistory(pageSize, page, associateId, sortDirection, sortColumn, searchText, isAutomatic).ToList();

            resultsCount = result.Any() ? result.First().Cnt.Value : 0;

            return result.Select(c => new CommunicationHistoryModel
                                          {
                                              CommunicationType = (CommunicationType)c.CommunicationType,
                                              CommunicationTypeDesc = GetCommunicationTypeName((CommunicationType)c.CommunicationType),
                                              Created = c.Created,
                                              Description = c.Description,
                                              HasDetails = c.HasDetails.Value,
                                              IsAutomatic = c.IsAutomatic.Value,
                                              LoggedInUser = c.LoggedInUser,
                                              Id = c.Id,
                                              CommunicationSource = (CommunicationSource)c.Source
                                          }).ToList();
        }

        public string GetDetailsForAssociateCommunication(int associateCommunicationId)
        {
            return this.MomentaDb.AssociateCommunication.Single(c => c.AssociateCommunicationId == associateCommunicationId).Details;
        }

        public AssociateSentEmail GetAssociateSentEmail(int id)
        {
            return this.MomentaDb.AssociateSentEmails.First(e => e.Id == id);
        }

        public IEnumerable<AssociateEmailDocumentModel> GetAssociateEmailDocuments(int id)
        {

            var results = from a in this.MomentaDb.AssociateEmailDocument
                          join t in this.MomentaDb.AssociateDocuments
                          on a.DocumentId equals t.DocumentId
                          join t1 in this.MomentaDb.DocumentFulls
                          on t.DocumentId equals t1.DocumentId
                          join t3 in this.MomentaDb.Documents
                          on t1.DocumentId equals t3.DocumentId
                          where a.AssosciateSentEmailId == id
                          orderby t.CreatedDate descending
                          select (new AssociateEmailDocumentModel
                          {
                              DocumentId = t1.DocumentId,
                              Title = t3.Title,
                              CreatedDate = t.CreatedDate,
                              DocumentType = (AssociateDocumentType)t.DocumentTypeId,
                              Size = Math.Round((((t.FileSizes ?? 0) / 1024.00) / 1024.00), 2)

                          });

            return results.OrderByDescending(x => x.CreatedDate);
        }

        public IEnumerable<AssociateEmailDocumentModel> GetAssociateEmailUploadedDocuments(int associateId, string newlyUploadedDocs)
        {
            var results1 = new List<AssociateEmailDocumentModel>();

            if (newlyUploadedDocs != "")
            {
                Guid[] newList = newlyUploadedDocs.Split(',').Select(s => Guid.Parse(s)).ToArray();

                var varFirstResult = from t in this.MomentaDb.AssociateDocuments
                                     join t3 in this.MomentaDb.Documents
                                     on t.DocumentId equals t3.DocumentId
                                     where t.AssociateId == associateId
                                     orderby t.CreatedDate descending
                                     select (new AssociateEmailDocumentModel
                                     {
                                         DocumentId = t.DocumentId,
                                         Title = t3.Title,
                                         CreatedDate = t.CreatedDate,
                                         DocumentType = (AssociateDocumentType)t.DocumentTypeId,
                                         Size =Math.Round((((t.FileSizes ?? 0) / 1024.00) / 1024.00),2)

                                     });

                results1 = varFirstResult.Where(x => newList.Contains(x.DocumentId)).ToList();

            }

            return results1.AsEnumerable();


        }

        public void SaveCommunication(AssociateCommunication communicationEntity, string attachments)
        {
            this.MomentaDb.AssociateCommunication.AddObject(communicationEntity);
            this.MomentaDb.SaveChanges();
            if (communicationEntity.CommunicationTypeId == (int)CommunicationType.Email)
            {
                AssociateSentEmail sentEmail = new AssociateSentEmail();

                sentEmail.ToAddress = "";
                sentEmail.AssociateId = communicationEntity.AssociateId;
                sentEmail.IsAutomatic = false;
                sentEmail.LoggedInUser = communicationEntity.LoggedInUser;
                sentEmail.Subject = communicationEntity.Description;
                sentEmail.Body = communicationEntity.Details;
                sentEmail.EmailSent = DateTime.Now;

                //string strAttachments = string.Join(",", attachments.Select(cust => cust.DocumentId.ToString()));
                MomentaRecruitment.Common.Repositories.EmailRepository emailRepo = new MomentaRecruitment.Common.Repositories.EmailRepository();
                emailRepo.LogSentEmail(sentEmail, attachments);
            }
        }

        public void SaveComment(Comment commentEntity, int associateId)
        {
            this.MomentaDb.Comments.AddObject(commentEntity);

            if (associateId != 0)
            {
                var associateStub = new Associate { ID = associateId };
                this.MomentaDb.Associates.Attach(associateStub);
                commentEntity.Associate = associateStub;
            }

            this.MomentaDb.SaveChanges();
        }

        public void ApproveDocument(Guid documentId, string userName, DateTime approvedDate)
        {
            AssociateDocument document = this.MomentaDb.AssociateDocuments.Single(d => d.DocumentId == documentId);

            document.ApprovedBy = userName;
            document.ApprovedDate = approvedDate;

            this.MomentaDb.SaveChanges();
        }

        public void AddInvoiceHistory(int invoiceId, string user, string comments)
        {
            this.MomentaDb.AddInvoiceHistory(invoiceId, user, comments);
        }

        public List<GetAssociateProspects_Result> GetAssociateProspects(int associateId, int pageSize, int skip, List<SortDescription> sort, out int? resultsCount)
        {
            string dir = (sort != null && sort.Count > 0 ? sort[0].dir : null);
            string field = (sort != null && sort.Count > 0 ? sort[0].field : null);
            int page = (int)(skip / pageSize) + 1;
            List<GetAssociateProspects_Result> results = this.MomentaDb.GetAssociateProspects(associateId, page, pageSize, dir, field).ToList();
            resultsCount = (results.Count > 0 ? results.FirstOrDefault().Cnt : 0);

            return results;            
        }

        public IEnumerable<GetHistoricalRates_Result> GetHistoricalRates(int individualId)
        {
            return this.MomentaDb.GetHistoricalRates(individualId);           
        }

        public void CreateRateComment(int individualId, string rateName, string comment)
        {
            this.MomentaDb.CreateAssociateRateComment(individualId, rateName, comment);
        }

        public UmbrellaCompany GetUmbrellaCompany(byte? umbrellaCompanyId)
        {
            return this.MomentaDb.UmbrellaCompanies.Single(u => u.UmbrellaCompanyId == umbrellaCompanyId);
        }

        public List<CheckAssociateContractStatus_Result> CheckAssociateContractStatus()
        {
            return this.MomentaDb.CheckAssociateContractStatus().ToList();
        }

        public List<GetAssociateContractStatusHistory_Result> GetAssociateContractStatusHistory(int associateId)
        {
            return this.MomentaDb.GetAssociateContractStatusHistory(associateId).ToList();
        }

        public void UpdateAssociateContractStatus(int id, int associateApprovalStatusId)
        {
            this.MomentaDb.UpdateAssociateContractStatus(id, associateApprovalStatusId);
        }

        public List<GetExpiredSelfBillingAgreements_Result> GetExpiredSelfBillingAgreements()
        {
            return this.MomentaDb.GetExpiredSelfBillingAgreements().ToList();
        }

        public dynamic GetOnContractAssociatesWithElapsedRoles()
        {
            return this.MomentaDb.GetOnContractAssociatesWithElapsedRoles();
        }

        public dynamic GetOnContractAssociatesWithCurrentRoles()
        {
            return this.MomentaDb.GetOnContractAssociatesWithCurrentRoles();
        }
        public void SaveAssessmentDocument(Guid documentId, int assessmentTypeId)
        {

            var objDoc = new AssessmentDocument();

            objDoc.AssessmentTypeId = assessmentTypeId;
            objDoc.DocumentId = documentId;
            this.MomentaDb.AssessmentDocument.AddObject(objDoc);
            this.MomentaDb.SaveChanges();
        }
    }
}