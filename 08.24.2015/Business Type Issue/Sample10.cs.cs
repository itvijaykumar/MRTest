namespace Admin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Objects.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Data.Objects;

    using Admin.Helpers;
    using Admin.Models.Search;
    using MR_DAL;

    using AssociateApprovalStatusEnum = MR_DAL.Enumerations.AssociateApprovalStatus;
    using AssociateRegistrationType = MR_DAL.Enumerations.AssociateRegistrationType;
    using AvailabilityTypeEnum = MR_DAL.Enumerations.AvailabilityType;
    using NoticeIntervalEnum = MR_DAL.Enumerations.NoticeInterval;
    using MR_DAL.Enumerations;

    // associate search
    public partial class SearchService
    {
        public void PostCodeRadius(string postcode, out decimal latitude, out decimal longitude)
        {
            // get the postcodes
            var postcodes = this.associateRepo.GetPostCodes();

            // get the coordinates of the postcode
            var query = from p in postcodes
                        where p.postcode == postcode
                        select new { p.latitude, p.longitude };

            var coords = query.FirstOrDefault();

            if (coords != null)
            {
                latitude = coords.latitude;
                longitude = coords.longitude;
            }
            else
            {
                latitude = 0;
                longitude = 0;
            }
        }

        public IEnumerable<AssociateModel> SearchAssociate(AssociateSearchModel search, int pageSize, int page, string sortColumn, string sortDirection, out int total)
        {
            // get the associates
            var associates = this.associateRepo.GetAssociatesSearch();
            var test = associates.ToList();
              // free text
            associates = associates.Where(a => (a.AssociateApprovalStatusId != 8) && (a.AssociateApprovalStatusId != 9) && (a.AssociateApprovalStatusId != 10));
            associates = AssociateFreeText(search, associates);

            // personal information
            associates = AssociatePersonalInformation(search, associates);

            // associate type
            associates = AssociateType(search, associates);

            // availability
            associates = AssociateAvailability(search, associates);

            // rate
            associates = AssociateRate(search, associates);

            // location
            associates = AssociateLocation(search, associates);

            // qualifications
            associates = AssociateQualifications(search, associates);

            // assessment
            associates = AssociateAssessment(search, associates);
            
            // visa
            associates = AssociateVisa(search, associates);

            // project
            associates = AssociateProject(search, associates);
            var test1 = associates.ToList();
            // get the result
            var result = AssociateSearchResult(search, associates);

            // set up the order by
            IOrderedQueryable<AssociateModel> ordered = (IOrderedQueryable<AssociateModel>)result;
            if (result.Count() > 0)
            {
                switch (sortColumn)
                {
                    case "FirstName":
                        if (sortDirection == "asc")
                        {
                            ordered = result.Where(x => x.FirstName != null).OrderBy(x => x.FirstName);
                        }
                        else
                        {
                            ordered = result.Where(x => x.FirstName != null).OrderByDescending(x => x.FirstName);
                        }

                        break;
                    case "Email":
                        if (sortDirection == "asc")
                        {
                            ordered = result.OrderBy(x => x.Email);
                        }
                        else
                        {
                            ordered = result.OrderByDescending(x => x.Email);
                        }

                        break;
                    case "LastName":
                        if (sortDirection == "asc")
                        {
                            ordered = result.OrderBy(x => x.LastName);
                        }
                        else
                        {
                            ordered = result.OrderByDescending(x => x.LastName);
                        }

                        break;
                    case "CVCreatedDate":
                        if (sortDirection == "asc")
                        {
                            ordered = result.Where(x => x.CVCreatedDate != null).OrderBy(x => x.CVCreatedDate);
                        }
                        else
                        {
                            ordered = result.OrderByDescending(row => row.CVCreatedDate ?? row.CVCreatedDate);          
                           //  ordered = result.Where(x => x.CVCreatedDate != null).OrderByDescending(x => x.CVCreatedDate);
                        }

                        break;
                    case "StatusName":
                        if (sortDirection == "asc")
                        {
                            ordered = result.OrderBy(x => x.StatusName);
                        }
                        else
                        {
                            ordered = result.OrderByDescending(x => x.StatusName);
                        }

                        break;
                    //case "LastName":
                    default:
                        if (String.IsNullOrEmpty(sortDirection) || sortDirection == "asc")
                        {
                            ordered = result.OrderBy(x => x.LastName);
                        }
                        else
                        {
                            ordered = result.OrderByDescending(x => x.LastName);
                        }

                        break;
                }
            }
            // return the search result
            return SearchResult(pageSize, page, out total, ordered);
        }

        public IEnumerable<GraduateAssociateModel> SearchGraduate(AssociateSearchModel search, int pageSize, int page, string sortColumn, string sortDirection, out int total)
        {
            // get the associates
            var associates = this.associateRepo.GetGraduateSearch();

            // free text
            //associates = AssociateFreeText(search, associates);

            // personal information
            associates = AssociatePersonalInformation(search, associates);

            // associate type
            associates = AssociateType(search, associates);

            // availability
            // associates = AssociateAvailability(search, associates);

            // rate
            // associates = AssociateRate(search, associates);

            // location
            // associates = AssociateLocation(search, associates);

            // qualifications
            // associates = AssociateQualifications(search, associates);

            // assessment
            // associates = AssociateAssessment(search, associates);

            // visa
            // associates = AssociateVisa(search, associates);

            // project
            associates = AssociateProject(search, associates);

            // role dates
            associates = AssociateRoleDates(search, associates);

            // get the result
            var result = GraduateSearchResult(search, associates);

            // set up the order by
            var ordered = (IOrderedQueryable<GraduateAssociateModel>)result;

            switch (sortColumn)
            {
                case "FirstName":
                    if (sortDirection == "asc")
                    {
                        ordered = result.OrderBy(x => x.FirstName);
                    }
                    else
                    {
                        ordered = result.OrderByDescending(x => x.FirstName);
                    }

                    break;
             
                case "LastName":
                    if (sortDirection == "asc")
                    {
                        ordered = result.OrderBy(x => x.LastName);
                    }
                    else
                    {
                        ordered = result.OrderByDescending(x => x.LastName);
                    }

                    break;
              
            }

            // return the search result
            return SearchResult(pageSize, page, out total, ordered);
        }
        
        private static IQueryable<AssociateModel> AssociateSearchResult(AssociateSearchModel search, IQueryable<Associate> associates)
        {
            IQueryable<AssociateModel> result;
            string matchOn = "";

            // matchOn: client
            if (search.ClientId.HasValue)
            {
                matchOn = "client";
            }

            // matchOn: project
            if (search.ProjectId.HasValue)
            {
                matchOn = "project";
            }

            // matchOn: role
            if (search.RoleId.HasValue)
            {
                matchOn = "role";
            }

            // client
            if (matchOn == "client")
            {
                // prepare the result
                result = from a in associates
                         select new AssociateModel
                         {
                             AssociateId = a.ID,
                             FirstName = a.FirstName,
                             LastName = a.LastName,
                             Client = a.Individual.FirstOrDefault(x => x.Role.Requirement.Project.ClientId == search.ClientId).Client.Name,
                             Email = a.Individual.FirstOrDefault(x => x.Role.Requirement.Project.ClientId == search.ClientId).Associate.Email,
                             HomePhone = a.Individual.FirstOrDefault(x => x.Role.Requirement.Project.ClientId == search.ClientId).Associate.HomePhone,
                             WorkPhone = a.Individual.FirstOrDefault(x => x.Role.Requirement.Project.ClientId == search.ClientId).Associate.WorkPhone,
                             MobilePhone = a.Individual.FirstOrDefault(x => x.Role.Requirement.Project.ClientId == search.ClientId).Associate.MobilePhone,
                             Role = a.Individual.FirstOrDefault(x => x.Role.Requirement.Project.ClientId == search.ClientId).Role.RoleType.Name,
                             CVCreatedDate = a.Individual.FirstOrDefault(x => x.Role.Requirement.Project.ClientId == search.ClientId).Associate.AssociateDocuments.FirstOrDefault().CreatedDate,
                             Status = a.Prospect.FirstOrDefault(x => x.Role.Requirement.Project.ClientId == search.ClientId).ProspectStatusId,
                             StatusName = a.Prospect.FirstOrDefault(x => x.Role.Requirement.Project.ClientId == search.ClientId).ProspectStatus.Description
                         };
            }
            // project
            else if (matchOn == "project")
            {
                // prepare the result
                result = from a in associates
                         select new AssociateModel
                         {
                             AssociateId = a.ID,
                             FirstName = a.FirstName,
                             LastName = a.LastName,
                             Client = a.Individual.FirstOrDefault(x => x.Role.Requirement.ProjectId == search.ProjectId).Client.Name,
                             Email = a.Individual.FirstOrDefault(x => x.Role.Requirement.ProjectId == search.ProjectId).Associate.Email,
                             HomePhone = a.Individual.FirstOrDefault(x => x.Role.Requirement.ProjectId == search.ProjectId).Associate.HomePhone,
                             WorkPhone = a.Individual.FirstOrDefault(x => x.Role.Requirement.ProjectId == search.ProjectId).Associate.WorkPhone,
                             MobilePhone = a.Individual.FirstOrDefault(x => x.Role.Requirement.ProjectId == search.ProjectId).Associate.MobilePhone,
                             Role = a.Individual.FirstOrDefault(x => x.Role.Requirement.ProjectId == search.ProjectId).Role.RoleType.Name,
                             CVCreatedDate = a.Individual.FirstOrDefault(x => x.Role.Requirement.ProjectId == search.ProjectId).Associate.AssociateDocuments.FirstOrDefault().CreatedDate,
                             Status = a.Prospect.FirstOrDefault(x => x.Role.Requirement.ProjectId == search.ProjectId).ProspectStatusId,
                             StatusName = a.Prospect.FirstOrDefault(x => x.Role.Requirement.ProjectId == search.ProjectId).ProspectStatus.Description
                         };
            }
            // role
            else if (matchOn == "role")
            {
                // prepare the result
                result = from a in associates
                         select new AssociateModel
                         {
                             AssociateId = a.ID,
                             FirstName=a.FirstName,
                             LastName = a.LastName,
                             Client = a.Individual.FirstOrDefault(x => x.Role.RoleId == search.RoleId).Client.Name,
                             Email = a.Individual.FirstOrDefault(x => x.Role.RoleId == search.RoleId).Associate.Email,
                             HomePhone = a.Individual.FirstOrDefault(x => x.Role.RoleId == search.RoleId).Associate.HomePhone,
                             WorkPhone = a.Individual.FirstOrDefault(x => x.Role.RoleId == search.RoleId).Associate.WorkPhone,
                             MobilePhone = a.Individual.FirstOrDefault(x => x.Role.RoleId == search.RoleId).Associate.MobilePhone,
                             Role = a.Individual.FirstOrDefault(x => x.Role.RoleId == search.RoleId).Role.RoleType.Name,
                             CVCreatedDate = a.Individual.FirstOrDefault(x => x.Role.RoleId == search.RoleId).Associate.AssociateDocuments.FirstOrDefault().CreatedDate,
                             Status = a.Prospect.FirstOrDefault(x => x.Role.RoleId == search.RoleId).ProspectStatusId,
                             StatusName = a.Prospect.FirstOrDefault(x => x.Role.RoleId == search.RoleId).ProspectStatus.Description
                         };
            }
            else
            {
                List<AssociateModel> aList = new List<AssociateModel>(); 
                foreach (var a in associates)
                {
                    DateTime? cv = null;
                    string client = null;
                    string role = null;

                    if(a.AssociateDocuments.Where(x => x.DocumentTypeId == (int)AssociateDocumentType.CV).FirstOrDefault()!= null)
                    {
                        cv = a.AssociateDocuments.Where(x => x.DocumentTypeId == (int)AssociateDocumentType.CV).FirstOrDefault().CreatedDate;
                    }
                    if (a.Individual.OrderByDescending(x => x.StartDate).Any())
                    {
                        client = a.Individual.OrderByDescending(x => x.StartDate).FirstOrDefault().Client.Name; 
                        role = a.Individual.OrderByDescending(x => x.StartDate).FirstOrDefault().Role.RoleType.Name;
                    }
                    aList.Add( new AssociateModel
                             {
                                 AssociateId = a.ID,
                                 FirstName = a.FirstName,
                                 LastName = a.LastName,
                                 Email = a.Email,
                                 HomePhone = a.HomePhone,
                                 WorkPhone = a.WorkPhone,
                                 MobilePhone = a.MobilePhone,
                                 Status = a.AssociateApprovalStatusId,
                                 StatusName = a.AssociateApprovalStatus.Description,
                                 Client = client,
                                 Role = role,
                                 CVCreatedDate = cv
                             });

                }
                result = aList.AsQueryable();
                // prepare the result
                /*result = from a in associates
                         select new AssociateModel
                         {
                             AssociateId = a.ID,
                             FirstName = a.FirstName,
                             LastName = a.LastName,
                             Email = a.Email,
                             Status = a.AssociateApprovalStatusId,
                             StatusName = a.AssociateApprovalStatus.Description,
                             Client = a.Individual.OrderByDescending(x => x.StartDate).FirstOrDefault().Client.Name,
                             Role = a.Individual.OrderByDescending(x => x.StartDate).FirstOrDefault().Role.RoleType.Name,
                             CVCreatedDate = a.AssociateDocuments.Where(x => x.DocumentTypeId == (int)AssociateDocumentType.CV).FirstOrDefault().CreatedDate
                         };*/
            }
            return result;
        }


        private static IQueryable<GraduateAssociateModel> GraduateSearchResult(AssociateSearchModel search, IQueryable<Associate> associates)
        {
           
                // prepare the result
                return  from a in associates
                    select new GraduateAssociateModel
                    {
                        Id = a.ID,
                        Title = a.PersonTitle.Description,
                        LastName = a.LastName,
                        FirstName = a.FirstName,
                        MiddleName = a.MiddleName,
                        HomeAddress = a.AddressFreeText +" "+ a.HouseName + " "+ a.HouseNumber+ " " + a.Street + " " + a.Town,
                        Postcode = a.Postcode,
                        Email = a.Email,
                        DOB = a.DateOfBirth,
                        NINumber = a.NI,
                        Nationality = a.Nationality.Name,
                        PassportNumber = a.PassportNumber,
                        HomeTelNo = a.HomePhone,
                        Mobile = a.MobilePhone,
                        EmergencyContactName = a.EmergencyContactName,
                        EmergencyContactRelationship = a.EmergencyContactRelationship,
                        EmergencyContactPhone = a.EmergencyContactMobilePhone + " " +a.EmergencyContactDaytimePhone,
                       
                        BankName = a.BankAcctBankName,
                        BankAccountName = a.BankAcctName,
                        BankSortCode = a.BankAcctSort,
                        BankAccountNo = a.BankAcctNumber,
                        BuildingSocietyReference = a.BuildingSocietyReference,

                        Status = a.AssociateApprovalStatusId,
                        StatusName = a.AssociateApprovalStatus.Description

                    };
          
        }

        private static IQueryable<Associate> AssociateAvailability(AssociateSearchModel search, IQueryable<Associate> associates)
        {

            if (search.AvailableFrom.HasValue && search.AvailableTo.HasValue)
            {
                associates = from a in associates
                             where !a.Individual.Any(i => i.StartDate < search.AvailableTo.Value && (
                                    i.EndDate.Value > search.AvailableFrom.Value
                                    || (EntityFunctions.AddDays(i.EndDate.Value, (i.NoticeAmount * -1)) > search.AvailableFrom.Value && i.NoticeIntervalId == (int)NoticeIntervalEnum.Days)
                                    || (EntityFunctions.AddDays(i.EndDate.Value, (i.NoticeAmount * -7)) > search.AvailableFrom.Value && i.NoticeIntervalId == (int)NoticeIntervalEnum.Weeks)
                                    || (EntityFunctions.AddMonths(i.EndDate.Value, (i.NoticeAmount * -1)) > search.AvailableFrom.Value && i.NoticeIntervalId == (int)NoticeIntervalEnum.Months)
                                )
                            )
                             select a;
            }

            if (search.NoticePeriod.HasValue && search.NoticeIntervalId.HasValue)
            {
                associates = from a in associates
                             where a.Individual.Any(i => (i.Role.NoticeIntervalId == search.NoticeIntervalId.Value || i.NoticeIntervalId == search.NoticeIntervalId.Value)
                                 && ((i.Role.NoticeAmount == null && i.NoticeAmount == null) || i.Role.NoticeAmount <= search.NoticePeriod.Value || i.NoticeAmount <= search.NoticePeriod.Value))
                             select a;
            }

            return associates;
        }

        private static bool DetermineIfAssociateAvailableTo(Associate x)
        {
            // immediately available
            var currentlyAvailable = (byte?)AvailabilityTypeEnum.CurrentlyAvailable;
            if (x.AvailabilityTypeId == currentlyAvailable)
            {
                return true;
            }

            // todo: permanently employee
            // todo: temporary/contract with notice
            return false;
        }

        private static bool DetermineIfAssociateAvailableTo(int noticePeriod, int interval, Associate x)
        {
            // immediately available
            var currentlyAvailable = (byte?)AvailabilityTypeEnum.CurrentlyAvailable;
            if (x.AvailabilityTypeId == currentlyAvailable)
            {
                return true;
            }

            // todo: permanently employee
            // todo: temporary/contract with notice
            return false;
        }

        private static bool DetermineIfAssociateAvailableFrom(Associate x)
        {
            // immediately available
            var currentlyAvailable = (byte?)AvailabilityTypeEnum.CurrentlyAvailable;
            if (x.AvailabilityTypeId == currentlyAvailable)
            {
                return true;
            }

            // todo: permanently employee
            // todo: temporary/contract with notice
            return false;
        }

        private static bool DetermineIfAssociateAvailableFrom(int noticePeriod, int interval, Associate x)
        {
            // immediately available
            var currentlyAvailable = (byte?)AvailabilityTypeEnum.CurrentlyAvailable;
            if (x.AvailabilityTypeId == currentlyAvailable)
            {
                return true;
            }

            // todo: permanently employee
            // todo: temporary/contract with notice
            return false;
        }

        private static bool DetermineNoticeAvailability(int noticePeriod, int interval, Associate x, DateTime today)
        {
            // immediately available
            var currentlyAvailable = (byte?)AvailabilityTypeEnum.CurrentlyAvailable;
            if (x.AvailabilityTypeId == currentlyAvailable)
            {
                return true;
            }

            // on contract
            var onContract = (byte?)AssociateApprovalStatusEnum.OnContract;
            if (x.AssociateApprovalStatusId == onContract)
            {
                // associates on more than one contract are not available
                if (x.Individual.Count > 1)
                {
                    return false;
                }

                // todo: lookup the notice period from the requirement
                var associateNoticePeriod = x.NoticePeriod.Value * interval;

                var availableDate = x.AvailableDate.Value;

                var available = today.AddDays(associateNoticePeriod);

                return availableDate <= available;
            }

            // permanently employee
            var permanentlyEmployed = (byte?)AvailabilityTypeEnum.PermanentlyEmployed;
            if (x.AvailabilityTypeId == permanentlyEmployed)
            {
                if (x.NoticePeriod == null)
                {
                    return false;
                }

                var associateNoticePeriod = x.NoticePeriod.Value * interval;
                return associateNoticePeriod <= noticePeriod;
            }

            // temporary/contract with notice
            var withNotice = (byte?)AvailabilityTypeEnum.TemporaryContractWithNotice;
            if (x.AvailabilityTypeId == withNotice)
            {
                if (x.NoticePeriod == null || x.AvailableDate == null)
                {
                    return false;
                }

                var associateNoticePeriod = x.NoticePeriod.Value * interval;

                var availableDate = x.AvailableDate.Value;

                var available = today.AddDays(associateNoticePeriod);

                return availableDate <= available;
            }

            return false;
        }

        private static IQueryable<Associate> AssociatePersonalInformation(AssociateSearchModel search, IQueryable<Associate> associates)
        {
            // frist name
            if (!string.IsNullOrEmpty(search.FirstName))
            {
                associates = associates.Where(x => x.FirstName.Contains(search.FirstName));
            }

            // last name
            if (!string.IsNullOrEmpty(search.LastName))
            {
                associates = associates.Where(x => x.LastName.Contains(search.LastName));
            }

            // email address
            if (!string.IsNullOrEmpty(search.EmailAddress))
            {
                associates = associates.Where(x => x.Email.Contains(search.EmailAddress));
            }

            // phone number
            if (!string.IsNullOrEmpty(search.PhoneNumber))
            {
                associates = associates.Where(x =>
                    x.HomePhone.Contains(search.PhoneNumber) ||
                    x.OtherPhone.Contains(search.PhoneNumber) ||
                    x.MobilePhone.Contains(search.PhoneNumber) ||
                    x.WorkPhone.Contains(search.PhoneNumber));
            }

            // associate id
            if (search.AssociateId.HasValue)
            {
                associates = associates.Where(x => x.ID == search.AssociateId);
            }

            // status
            if (search.ApprovalStatusId.HasValue)
            {
                associates = associates.Where(x => x.AssociateApprovalStatusId == search.ApprovalStatusId);
            }

            // business unit
            if (search.BusinessUnitId != null && search.BusinessUnitId.Any())
            {
                var tempAssociates = new List<Associate>();

                for (var count = 0; count < search.BusinessUnitId.Count(); count++)
                {
                    var unit = search.BusinessUnitId.ElementAt(0);
                    tempAssociates.AddRange(
                        associates.Where(y => y.Individual.Any(i => i.Project.BusinessUnitId == unit)));
                }

                associates = tempAssociates.AsQueryable();
            }

            // business area
            if (search.BusinessAreaId != null && search.BusinessAreaId.Count() > 0)
            {
                if (search.BusinessAreaId.Count() > 0)
                {
                    var unit = search.BusinessAreaId.First();
                    associates = associates.Where(a => a.BusinessArea.Any(b => search.BusinessAreaId.Contains((byte)b.BusinessAreaId)));
                }
                
            }

            if (!string.IsNullOrEmpty(search.CV))
            {
                string userExpression = search.CV.ToLower();
                var associateLists = new List<IQueryable<Associate>>();
                var mylist = new List<Associate>();

                foreach (var exp in OrExpression(userExpression))
                {
                    associateLists.Add(AndExpression(exp, associates));
                }

                associates = associateLists.SelectMany(x => x).Distinct().AsQueryable();

                /*// CV
                if (!string.IsNullOrEmpty(search.CV))
                {
                    associates = associates.Where(x => x.CV.CV_Full.ParsedContents.Contains(search.CV));
                }*/
            }
            return associates;
        }

        private static string[] OrExpression(string userExpression)
        {
            var orList = userExpression.Split(new string[] { "or" }, StringSplitOptions.None).Select(s => s.Trim()).ToArray();

            return orList;
        }

        private static IQueryable<Associate> AndExpression(string userExpression, IQueryable<Associate> associates)
        {
            var andList = userExpression.Split(new string[] { "and" }, StringSplitOptions.None).Select(s => s.Trim()).ToArray();
            foreach (var exp in andList)
            {
                associates = associates.Where(x => x.CV.CV_Full.ParsedContents.Contains(exp));
            }

            return associates;
        }

        private static IQueryable<Associate> AssociateType(AssociateSearchModel search, IQueryable<Associate> associates)
        {
            Expression<Func<Associate, bool>> predicate = null;

            // full asscociates
            if (search.FullAssociate.HasValue)
            {
                predicate = ApplyAssociateSearchPredicate(predicate, x => x.AssociateRegistrationTypeId == (byte?)AssociateRegistrationType.Contract);
            }

            // full agency associates
            if (search.AgencyAssociate.HasValue)
            {
                predicate = ApplyAssociateSearchPredicate(predicate, x => x.AssociateRegistrationTypeId == (byte?)AssociateRegistrationType.Agency);
            }

            // employed asscociates
            if (search.EmployedAssociate.HasValue)
            {
                predicate = ApplyAssociateSearchPredicate(predicate, x => x.AssociateRegistrationTypeId == (byte?)AssociateRegistrationType.Employed);
            }

            // interim management assocaite
            if (search.InterimManagementAssociate.HasValue)
            {
                predicate = ApplyAssociateSearchPredicate(predicate, x => x.AssociateRegistrationTypeId == (byte?)AssociateRegistrationType.InterimManagement);
            }

            if (predicate != null)
            {
                associates = associates.Where(predicate);
            }

            return associates;
        }

        private static Expression<Func<Associate, bool>> ApplyAssociateSearchPredicate(Expression<Func<Associate, bool>> predicate, Expression<Func<Associate, bool>> expression)
        {
            if (predicate == null)
            {
                predicate = expression;
            }
            else
            {
                predicate = predicate.Or(expression);
            }

            return predicate;
        }

        private IQueryable<Associate> AssociateVisa(AssociateSearchModel search, IQueryable<Associate> associates)
        {
            // visa expiry start
            if (search.VisaExpiryStart.HasValue)
            {
                associates = associates.Where(x => x.VisaExpiry > search.VisaExpiryStart);
            }

            // visa expiry end
            if (search.VisaExpiryEnd.HasValue)
            {
                associates = associates.Where(x => x.VisaExpiry < search.VisaExpiryEnd);
            }

            // visa type
            if (search.VisaTypeId.HasValue)
            {
                associates = associates.Where(x => x.VisaTypeId == search.VisaTypeId);
            }

            return associates;
        }


        private IQueryable<Associate> AssociateRoleDates(AssociateSearchModel search, IQueryable<Associate> associates)
        {
            if (search.From.HasValue)
            {
                associates = associates.Where(x => x.Individual.Any(y => y.EndDate >= search.From));
            }

            if (search.To.HasValue)
            {
                associates = associates.Where(x => x.Individual.Any(y => y.StartDate <= search.To));
            }

            return associates;
        }

        private IQueryable<Associate> AssociateProject(AssociateSearchModel search, IQueryable<Associate> associates)
        {
            // client
            if (search.ClientId.HasValue)
            {
                associates = associates.Where(x => x.Individual.FirstOrDefault(a=>a.ClientId==search.ClientId).ClientId==search.ClientId);
            }

            // project
            if (search.ProjectId.HasValue)
            {
                associates = associates.Where(x => x.Individual.FirstOrDefault(a => a.ProjectId == search.ProjectId).ProjectId == search.ProjectId);

            }

            // role
            if (search.RoleId.HasValue)
            {
                associates = associates.Where(x => x.Individual.FirstOrDefault(a => a.RoleId == search.RoleId).RoleId == search.RoleId);
            }

            if (search.BulkTimesheetChanges.HasValue)
            {
                associates = associates.Where(x => x.Individual.Any(y => y.IndividualTask.Count() > 0));
            }

            return associates;
        }

        private IQueryable<Associate> AssociateAssessment(AssociateSearchModel search, IQueryable<Associate> associates)
        {
            // asessments
            if (search.Assessments != null && search.Assessments.Count() > 0)
            {
                foreach (var a in search.Assessments)
                {
                    associates = associates.Where(x => x.Assessment.Any(y => y.AssessmentTypeId == a.AssessmentType
                                                                             && y.Score >= a.Above
                                                                             && y.Score <= a.Below
                                                                             && (
                                                                                 ((bool) a.Passed) && y.Pass == "Pass")
                                                                             || ((bool) a.Failed && y.Pass == "Fail")
                                                                             || (!(bool) a.Passed && !(bool) a.Failed)
                        )
                        );
                }
            }

            return associates;
        }

        private IQueryable<Associate> AssociateQualifications(AssociateSearchModel search, IQueryable<Associate> associates)
        {
            // qualifications
            if (search.Qualifications != null && search.Qualifications.Any())
            {
                foreach (var qualificationId in search.Qualifications)
                {
                    associates = associates.Where(x => x.Qualifications.Any(y => y.QualificationId == qualificationId));
                }
            }

            // qualifications other
            if (search.QualificationsOther != null && search.QualificationsOther.Any())
            {
                // associates = associates.Where(x => x.Qualifications.Where(y => search.Qualifications.Contains(y.QualificationId)).Count() > 0);
            }

            return associates;
        }

        private IQueryable<Associate> AssociateLocation(AssociateSearchModel search, IQueryable<Associate> associates)
        {
            // postcode
            if (search.Postcode != null && !search.Miles.HasValue)
            {
                associates = associates.Where(x => x.Postcode.Contains(search.Postcode) || (x.AddressData != null && x.AddressData.Contains(search.Postcode)));
            }

            // miles
            if (search.Postcode != null && search.Miles.HasValue)
            {
                decimal latitude;
                decimal longitude;
                PostCodeRadius(search.Postcode, out latitude, out longitude);

                if (longitude != 0 && longitude != 0)
                {
                    var distance = (double)search.Miles.Value;
                    associates = from a in associates
                                 let s = SqlFunctions.SquareRoot(Math.Pow((double)(latitude - a.Latitude) * 110.7, 2) + Math.Pow((double)(longitude - a.Longitude) * 75.6, 2))
                                 where s <= distance
                                 select a;
                }
            }

            return associates;
        }

        private IQueryable<Associate> AssociateRate(AssociateSearchModel search, IQueryable<Associate> associates)
        {
            // rate from
            if (search.RateFrom.HasValue)
            {
                associates = associates.Where(x => x.Individual.Any(y => y.AssociateRate >= search.RateFrom));
            }

            // rate to
            if (search.RateTo.HasValue)
            {
                associates = associates.Where(x => x.Individual.Any(y => y.AssociateRate <= search.RateTo));
            }

            return associates;
        }
    }
}