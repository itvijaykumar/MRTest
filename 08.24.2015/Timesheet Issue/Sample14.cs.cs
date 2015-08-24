using System.Globalization;
using java.awt.@event;
using MR_DAL;

namespace MomentaRecruitment.Common.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Principal;
    using System.Transactions;

    using AssociatePortal.Services;
    using AssociatePortal.ViewModel;

    using Elmah;

    using MomentaRecruitment.Common.Enumerations;
    using MomentaRecruitment.Common.Models;
    using MomentaRecruitment.Common.Repositories;
    using MomentaRecruitment.Common.ViewModel;

    using MR_DAL.Enumerations;
    using MR_DAL.TimeSheet;
    using MR_DAL.TimeSheet.Repository;

    using AbsenceDurationType = AssociatePortal.ViewModel.AbsenceDurationType;
    using MomentaRecruitment.Common.Services.Timesheet;
    using System.Threading.Tasks;
    using HtmlAgilityPack;

    public class TimesheetService : ServiceBase, ITimesheetService
    {
        private ITimeSheetRepository timeSheetRepository;

        private IAssociateRepository associateRepository;

        private ITimesheetAssociateService timesheetAssociateService;

        private IEmailService emailService;

        private IUnitOfWork unitOfWork;

        public TimesheetService(
            IEmailService emailService,
            ITimeSheetRepository timeSheetRepository,
            IAssociateRepository associateRepository,
            IUnitOfWork unitOfWork,
            IPrincipal currentUser,
            ITimesheetAssociateService timesheetAssociateService)
            : base(currentUser, timesheetAssociateService)
        {
            this.emailService = emailService;
            this.timeSheetRepository = timeSheetRepository;
            this.associateRepository = associateRepository;
            this.unitOfWork = unitOfWork;
            this.timesheetAssociateService = timesheetAssociateService;
        }

        private Attendences[] GetAttendences(TimesheetValidationDetail validationDetails)
        {
            var attendences = this.timeSheetRepository.GetAttendenceOptions(validationDetails.AssociateId.Value).Select(x => new Attendences() { AttendenceOptionId = x.AttendanceOptionId, AttendenceOption = x.Description }).ToList();

            if (!validationDetails.TravelFullDay.Value)
                attendences.Remove(attendences.Where(x =>
                              x.AttendenceOption.Replace(" ", string.Empty).ToLower() ==
                                      "TravelFullDay".ToLower()).First());

            if (!validationDetails.TravelHalfDay.Value)
            {
                attendences.Remove(attendences.Where(x =>
                              x.AttendenceOption.Replace(" ", string.Empty).ToLower() ==
                                      "TravelHalfDay".ToLower()).First());
            }

            if (!validationDetails.CancelHalfDay.Value)
            {
                attendences.Remove(attendences.Where(x =>
                              x.AttendenceOption.Replace(" ", string.Empty).ToLower() ==
                                      "CancelHalfDay".ToLower()).First());
            }

            if (!validationDetails.CancelFullDay.Value)
            {
                attendences.Remove(attendences.Where(x =>
                              x.AttendenceOption.Replace(" ", string.Empty).ToLower() ==
                                      "CancelFullDay".ToLower()).First());
            }

            if (!validationDetails.Hourly.Value)
            {
                attendences.Remove(attendences.Where(x =>
                              x.AttendenceOption.Replace(" ", string.Empty).ToLower() ==
                                      "Hourly".ToLower()).First());
            }

            return attendences.ToArray();
        }

        public TimesheetViewModel GetTimeSheetEntryOld_ToBeDeletedOnceWeHaveStableBuild_Backup(int timesheetId)
        {

            if (timesheetId == default(int))
            {
                throw new ArgumentException("Invalid timesheetId");
            }

            // get the timesheet from database.
            var timesheet = this.timeSheetRepository.GetTimeSheetById(timesheetId);

            //getting validation details/ indiviudals.
            var validationDetails = this.timeSheetRepository.GetTimeSheetAssignRoleDetail(timesheetId);

            validationDetails.TimesheetTypeId = timesheet.TimeSheetTypeId.GetValueOrDefault();

            //Get all the attendence for this timesheet.
            var attendences = GetAttendences(validationDetails);
            //get all the timesheetentry belong to timesheet.
            var timesheetEntry = this.timeSheetRepository.GetTimeSheetEntryByTimeSheetId(timesheetId);

            //create a view to retunrn to view
            var timesheetVM = new TimesheetViewModel
            {
                TimesheetAssociate = this.GetAssociate(validationDetails, timesheet),
                TimesheetId = timesheet.TimeSheetId,
                Status = ((TimesheetStatus)timesheet.TimeSheetStatusId).ToString(),
                AssociateId = validationDetails.AssociateId.GetValueOrDefault(),
                TimesheetApproverAssociateId = validationDetails.TimeSheetApproverAssociateId.HasValue ? validationDetails.TimeSheetApproverAssociateId.Value : 0,
                TimesheetApproverId = validationDetails.TimeSheetApproverId.HasValue ? validationDetails.TimeSheetApproverId.Value : 0,
                CurrentUserId = AssociateId,
                TimeSheetTypeId = timesheet.TimeSheetTypeId.Value
            };

            timesheetVM.IncentiveDays = timesheet.IncentiveDay.HasValue ? timesheet.IncentiveDay.Value : 0;

            //get timesheet history.
            this.GetHistory(ref timesheetVM);

            // get mandatory reciept information. 
            this.GetRecieptInfo(ref timesheetVM, timesheet.RoleId);

            int dayId = 1;
            string[] days;


            timesheetVM.Entries = GetDays(validationDetails, out days);
            // getting document
            this.GetDocuments(ref timesheetVM);

            // build timesheet viewmodel.
            this.MapTimeSheet(ref timesheetVM, validationDetails);

            // it gets absence details for asoociate.
            var absence = this.timeSheetRepository.GetThisWeekAbsence(timesheet.AssociateId, timesheet.StartDate.Date);

            // create a timesheet entries for days.
            foreach (var day in days)
            {
                var dayentry = new TimeSheetEntryViewModel();

                switch (day)
                {
                    case "Monday":
                        dayentry.Day = day;
                        dayentry.DayId = dayId;
                        dayentry.Attendences = attendences;

                        //check if monday is allowed.
                        if (!validationDetails.Monday.GetValueOrDefault() && !validationDetails.MondayOvertime.GetValueOrDefault())
                        {
                            dayentry.IsDay = false;
                            break;
                        }
                        //check if monday overtime is allowed.
                        dayentry.IsOvertime = validationDetails.MondayOvertime.GetValueOrDefault() ? true : false;
                        dayentry.IsDay = true;
                        dayentry.IsWorkingDay = validationDetails.Monday.GetValueOrDefault();

                        // build timesheet entry
                        this.MapEntry(ref dayentry, timesheetEntry, validationDetails);

                        // not sure what this does.
                        if (validationDetails.StartDate != null && DateTime.Parse(dayentry.Date) < (DateTime)validationDetails.StartDate)
                        {
                            dayentry.IsDay = false;
                            break;
                        }

                        // apply absence
                        this.ApplyAbsence(ref dayentry, timesheetEntry, absence);
                        // map ansence attendence option.
                        this.MapAbsenceAttendence(ref dayentry, attendences);
                        break;

                    case "Tuesday":
                        dayentry.Day = day;
                        dayentry.DayId = dayId;
                        dayentry.Attendences = attendences;

                        if (!validationDetails.Tuesday.GetValueOrDefault() && !validationDetails.TuesdayOvertime.GetValueOrDefault())
                        {
                            dayentry.IsDay = false;
                            break;
                        }

                        dayentry.IsOvertime = validationDetails.TuesdayOvertime.GetValueOrDefault() ? true : false;
                        dayentry.IsDay = true;
                        dayentry.IsWorkingDay = validationDetails.Tuesday.GetValueOrDefault();
                        this.MapEntry(ref dayentry, timesheetEntry, validationDetails);

                        if (validationDetails.StartDate != null && DateTime.Parse(dayentry.Date) < (DateTime)validationDetails.StartDate)
                        {
                            dayentry.IsDay = false;
                            break;
                        }

                        this.ApplyAbsence(ref dayentry, timesheetEntry, absence);
                        this.MapAbsenceAttendence(ref dayentry, attendences);
                        break;

                    case "Wednesday":
                        dayentry.Day = day;
                        dayentry.DayId = dayId;
                        dayentry.Attendences = attendences;

                        if (!validationDetails.Wednesday.GetValueOrDefault() && !validationDetails.WednesdayOvertime.GetValueOrDefault())
                        {
                            dayentry.IsDay = false;
                            break;
                        }

                        dayentry.IsDay = true;
                        dayentry.IsOvertime = validationDetails.WednesdayOvertime.GetValueOrDefault() ? true : false;
                        dayentry.IsWorkingDay = validationDetails.Wednesday.GetValueOrDefault();
                        this.MapEntry(ref dayentry, timesheetEntry, validationDetails);

                        if (validationDetails.StartDate != null && DateTime.Parse(dayentry.Date) < (DateTime)validationDetails.StartDate)
                        {
                            dayentry.IsDay = false;
                            break;
                        }

                        this.ApplyAbsence(ref dayentry, timesheetEntry, absence);
                        this.MapAbsenceAttendence(ref dayentry, attendences);
                        break;

                    case "Thursday":
                        dayentry.Day = day;
                        dayentry.DayId = dayId;
                        dayentry.Attendences = attendences;

                        if (!validationDetails.Thursday.GetValueOrDefault() && !validationDetails.ThursdayOvertime.GetValueOrDefault())
                        {
                            dayentry.IsDay = false;
                            break;
                        }

                        dayentry.IsDay = true;
                        dayentry.IsOvertime = validationDetails.ThursdayOvertime.GetValueOrDefault() ? true : false;
                        dayentry.IsWorkingDay = validationDetails.Thursday.GetValueOrDefault();
                        this.MapEntry(ref dayentry, timesheetEntry, validationDetails);

                        if (validationDetails.StartDate != null && DateTime.Parse(dayentry.Date) < (DateTime)validationDetails.StartDate)
                        {
                            dayentry.IsDay = false;
                            break;
                        }

                        this.ApplyAbsence(ref dayentry, timesheetEntry, absence);
                        this.MapAbsenceAttendence(ref dayentry, attendences);
                        break;

                    case "Friday":
                        dayentry.Day = day;
                        dayentry.DayId = dayId;
                        dayentry.Attendences = attendences;

                        if (!validationDetails.Friday.GetValueOrDefault() && !validationDetails.FridayOvertime.GetValueOrDefault())
                        {
                            dayentry.IsDay = false;
                            break;
                        }

                        dayentry.IsDay = true;
                        dayentry.IsOvertime = validationDetails.FridayOvertime.Value ? true : false;
                        dayentry.IsWorkingDay = validationDetails.Friday.Value;
                        this.MapEntry(ref dayentry, timesheetEntry, validationDetails);

                        if (validationDetails.StartDate != null && DateTime.Parse(dayentry.Date) < (DateTime)validationDetails.StartDate)
                        {
                            dayentry.IsDay = false;
                            break;
                        }

                        this.ApplyAbsence(ref dayentry, timesheetEntry, absence);
                        this.MapAbsenceAttendence(ref dayentry, attendences);
                        break;

                    case "Saturday":
                        dayentry.Day = day;
                        dayentry.DayId = dayId;
                        dayentry.Attendences = attendences;

                        if (!validationDetails.Saturday.GetValueOrDefault() && !validationDetails.SaturdayOvertime.GetValueOrDefault())
                        {
                            dayentry.IsDay = false;
                            break;
                        }

                        dayentry.IsDay = true;
                        dayentry.IsOvertime = validationDetails.SaturdayOvertime.GetValueOrDefault() ? true : false;
                        dayentry.IsWorkingDay = validationDetails.Saturday.GetValueOrDefault();
                        this.MapEntry(ref dayentry, timesheetEntry, validationDetails);

                        if (validationDetails.StartDate != null && DateTime.Parse(dayentry.Date) < (DateTime)validationDetails.StartDate)
                        {
                            dayentry.IsDay = false;
                            break;
                        }

                        this.ApplyAbsence(ref dayentry, timesheetEntry, absence);
                        this.MapAbsenceAttendence(ref dayentry, attendences);
                        break;

                    case "Sunday":
                        dayentry.Day = day;
                        dayentry.DayId = 0;
                        dayentry.Attendences = attendences;

                        if (!validationDetails.Sunday.GetValueOrDefault() && !validationDetails.SundayOvertime.GetValueOrDefault())
                        {
                            dayentry.IsDay = false;
                            break;
                        }

                        dayentry.IsDay = true;
                        dayentry.IsOvertime = validationDetails.SundayOvertime.GetValueOrDefault() ? true : false;
                        dayentry.IsWorkingDay = validationDetails.Sunday.GetValueOrDefault();
                        this.MapEntry(ref dayentry, timesheetEntry, validationDetails);

                        if (validationDetails.StartDate != null && DateTime.Parse(dayentry.Date) < (DateTime)validationDetails.StartDate)
                        {
                            dayentry.IsDay = false;
                            break;
                        }

                        this.ApplyAbsence(ref dayentry, timesheetEntry, absence);
                        this.MapAbsenceAttendence(ref dayentry, attendences);
                        break;
                }

                timesheetVM.Entries[dayId - 1] = dayentry;
                dayId++;
            }

            return timesheetVM;
        }

        private void MapAbsenceAttendence(ref TimeSheetEntryViewModel dayentry, Attendences[] attendance)
        {
            if (dayentry.Absence == MR_DAL.Enumerations.AbsenceDurationType.AMOnly.ToString() || dayentry.Absence == MR_DAL.Enumerations.AbsenceDurationType.PMOnly.ToString())
            {
                dayentry.Attendences = attendance.Where(x => x.AttendenceOptionId == 0 || x.AttendenceOptionId == 2).ToArray();
            }
        }

        private void GetDocuments(ref TimesheetViewModel timesheet)
        {
            var documents = this.timeSheetRepository.GetTimesheetDocuments(timesheet.TimesheetId);
            timesheet.Documents = documents.Select(
                doc =>
                    new TimeSheetDocumentViewModel()
                    {
                        DocumentId = doc.DocumentId,
                        Title = doc.Title,
                        TimesheetId = doc.TimeSheetId,
                        Date = doc.Date.ToShortDateString(),
                        Pass = doc.Pass,
                        Comment = doc.Comment,
                        Username = doc.Username
                    }).ToArray();
        }

        private void GetHistory(ref TimesheetViewModel timesheet)
        {
            var aName = timesheet.TimesheetAssociate.AssociateName;

            timesheet.History = this.timeSheetRepository.GetTimesheetHistory(timesheet.TimesheetId)
                .Select(
                history =>
                    new TimesheetHistoryViewModel()
                    {
                        Associate = history.Name,
                        Comment = history.Comment,
                        Date = history.Date.ToShortDateString(),
                        Status = ((TimesheetStatus)history.TimeSheetStatusId).ToString(),
                        Time = history.Date.ToShortTimeString()
                    }).Take(10).ToArray();
        }

        private void GetRecieptInfo(ref TimesheetViewModel timesheet, int roleId)
        {
            var reInfo = timeSheetRepository.GetRecieptInfo(roleId, timesheet.AssociateId);

            timesheet.ExpenseReceiptDetails = reInfo.Where(x => x.Receipt.GetValueOrDefault() == true).Select(x => x.Expense).AsEnumerable();
        }

        public DashBoardTimeSheetModel GetDashBoardData(int currentUserId)
        {
            int associateId = 0;

            if (currentUserId > 0)
            {
                associateId = currentUserId;
            }
            else
            {
                associateId = Convert.ToInt16(this.AssociateId);
            }

            var userTimesheet = this.timeSheetRepository.GetUserTimeSheet(associateId);
            var userInvoice = this.timeSheetRepository.GetInvoicesForAssociate(associateId);
            var validationDetails = this.timeSheetRepository.GetTimeSheetEntryValidationDetails(associateId);

            var model = new DashBoardTimeSheetModel()
            {
                DashBoardTimeSheets = userTimesheet.Select(x =>
                    new DashBoardTimeSheetViewModel()
                    {
                        EndDate = x.EndDate.ToShortDateString(),
                        ProjectName = x.Project_Name,
                        StartDate = x.StartDate.ToShortDateString(),
                        Status = ((TimesheetStatus)x.TimeSheetStatusId).ToString(),
                        TimeSheetId = x.TimeSheetId,
                        Role = x.Role,
                        TimesheetTypeId = x.TimeSheetTypeId.HasValue ? x.TimeSheetTypeId.Value : 1
                    }).ToArray(),
                DashBoardCompanyInvoice = userInvoice.Select(x =>
                    new DashBoardCompanyInvoiceViewModel()
                    {
                        StartDate = x.StartDate.ToShortDateString(),
                        EndDate = x.EndDate.ToShortDateString(),
                        ProjectName = x.ProjectName,
                        ReportNumber = x.InvoiceId,
                        Role = x.RoleName,
                        Status = ((MR_DAL.Enumerations.InvoiceStatus)x.InvoiceStatusId).ToString()
                    }).ToArray(),
                IsAssociateTimesheetSuspended = validationDetails.Suspended //.HasValue && validationDetails.Suspended.Value
            };

            model.DashBoardAwaitingTimeSheets = this.timeSheetRepository.GetAwaitingApproval(associateId).Select(awaitingApproval => new DashBoardAwaitingTimeSheetViewModel()
            {
                TimeSheetId = awaitingApproval.TimeSheetId,
                Associate = string.Concat(awaitingApproval.FirstName, " ", awaitingApproval.LastName),
                ProjectName = awaitingApproval.ProjectName,
                Status = ((TimesheetStatus)awaitingApproval.StatusId).ToString(),
                StartDate = awaitingApproval.StartDate.ToShortDateString(),
                EndDate = awaitingApproval.EndDate.ToShortDateString(),
                TimesheetTypeId = awaitingApproval.TimeSheetTypeId.HasValue ? awaitingApproval.TimeSheetTypeId.Value : 1,
                RetainerGenDate = awaitingApproval.RetainerGenDate,
                AssociateId = awaitingApproval.AssociateId
            }).ToArray();

            var ids = model.DashBoardAwaitingTimeSheets.Where(x => x.TimesheetTypeId == 2 && x.RetainerGenDate == null).Select(x => new { TimesheetId = x.TimeSheetId, AssociateId = x.AssociateId, Associate = x.Associate });

            return model;
        }

        private void MarkRetainer(IEnumerable<ScheduleRetainer> ids)
        {
            foreach (var item in ids)
            {
                string Details = "Retainer timesheet has been generated for " + item.Name + " on " + DateTime.Now.ToShortDateString();
                // CreateCommunicationsHistoryItem(item.AssociateId, CommunicationType.Timesheet, "Retainer" + TimesheetStatus.Submitted.ToString(), CurrentUser.Identity.Name, Details);

                var emailModelAssociate = this.emailService.GetTimeSheetEmail(item.TimeSheetId.Value, EmailTemplate.RetainerGenerationAssociate);
                this.emailService.SendEmail(emailModelAssociate);
                var emailModelMomenta = this.emailService.GetTimeSheetEmail(item.TimeSheetId.Value, EmailTemplate.RetainerGenerationMomenta);
                emailModelMomenta.ToAddress = "timesheets@momentagroup.com";
                this.emailService.SendEmail(emailModelMomenta);
                // this.EmailService.SendMultipleEmails(emailModelMomenta, new List<string>{"timesheets@momentagroup.com"});

                //this.timeSheetRepository.MarkReatinerEntry(item.TimeSheetId.Value);
            }
        }

        public bool SaveTimeSheet(TimesheetViewModel model)
        {
            bool status;
            try
            {

                SaveTimeSheetWithoutStatus(model);

                this.timeSheetRepository.UpdateStatus(model.TimesheetId, TimesheetStatus.Updated, associateId: model.AssociateId, approverAssociateId: model.TimesheetApproverAssociateId);
                string Details = "Timesheet has been " + TimesheetStatus.Updated.ToString() + " by " + AssociateFullName + " on " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString();
                // CreateCommunicationsHistoryItem(model.AssociateId, CommunicationType.Timesheet, "Timesheet " + TimesheetStatus.Updated.ToString(), CurrentUser.Identity.Name, Details);
                status = true;

                if (model.TimeSheetTypeId == (int)TimesheetTypeId.Adjustment)
                {
                    if (model.Status == TimesheetStatus.Blank.ToString())
                    {
                        var email = this.emailService.GetTimeSheetEmail(model.TimesheetId,
                           EmailTemplate.AdjustmentTimeSheetCreated);
                        this.emailService.SendEmail(email);
                    }
                    else if (model.Status == TimesheetStatus.Rejected.ToString())
                    {
                        var emailAmended = this.emailService.GetTimeSheetEmail(model.TimesheetId,
                           EmailTemplate.AdjustmentTimesheetAmended);
                        this.emailService.SendEmail(emailAmended);
                    }
                    //if (model.Status != TimesheetStatus.Rejected.ToString()) { 

                    //}
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                status = false;
            }
            return status;
        }

        public TimeSheetEntry GetEntry(int entryId)
        {
             return this.timeSheetRepository.GetTimeSheetEntry(entryId);
        }


        public void UpdateEntry(TimeSheetEntry entry)
        {
            this.timeSheetRepository.UpdateTimeSheetEntry(entry);
        }

        public bool SubmitTimeSheet(TimesheetViewModel model)
        {
            bool status;

            // ToDo: while refactoring, implement dirty object.
            try
            {
                foreach (var entry in model.Entries)
                {
                    if (entry.IsDay || model.Status == TimesheetStatus.Blank.ToString())
                    {
                        this.timeSheetRepository.UpdateTimeSheetEntry(this.UnMap(entry));
                    }
                }

                this.timeSheetRepository.UpdateTimeSheet(new TimeSheet() { TimeSheetId = model.TimesheetId, IncentiveDay = model.IncentiveDays });

                int approverAssociateId = this.GetApproverIdForTimeSheet(model.TimesheetId);
                var isEmailSent = timeSheetRepository.ValidateEmailsubmission(model.TimesheetId);

                this.timeSheetRepository.UpdateStatus(model.TimesheetId, TimesheetStatus.Submitted, associateId: model.AssociateId, approverAssociateId: model.TimesheetApproverAssociateId);
                var Details = "Timesheet has been " + TimesheetStatus.Submitted.ToString() + " by " + AssociateFullName + " on " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString();

                // CreateCommunicationsHistoryItem(model.AssociateId, CommunicationType.Timesheet, "Timesheet " + TimesheetStatus.Submitted.ToString(), CurrentUser.Identity.Name, Details);
                if (!isEmailSent)
                {
                    var email = this.emailService.GetTimeSheetEmail(model.TimesheetId, EmailTemplate.TimeSheetApprovalOutstanding);
                    this.emailService.SendEmail(email);
                }

                status = true;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                status = false;
            }

            return status;
        }

        public bool ApproveTimesheet(TimesheetViewModel model, bool isClientContact = false)
        {
            bool status;
            try
            {
                if (isClientContact)
                {
                    this.timeSheetRepository.UpdateStatus(model.TimesheetId, TimesheetStatus.Approved, approverClientContactId: model.CurrentUserId);
                }
                else
                {
                    this.timeSheetRepository.UpdateStatus(model.TimesheetId, TimesheetStatus.Approved, approverAssociateId: model.CurrentUserId);
                }
                status = true;
                // Send email
                var emailTemplate = EmailTemplate.TimeSheetApproved;
                if (model.TimeSheetTypeId == (int)TimesheetTypeId.Adjustment)
                {
                    emailTemplate = EmailTemplate.AdjustmentTimesheetApproved;
                }
                var email = this.emailService.GetTimeSheetEmail(model.TimesheetId, emailTemplate);

                if (model.TimeSheetTypeId == (int)TimesheetTypeId.Adjustment)
                {
                    email.Subject = string.Format("Adjustment {0}", email.Subject);
                }

                this.emailService.SendEmail(email);

                // chase only if the timesheet includes expenses requiring receipts and no receipts have been uploaded
                // get individual based on associate id

                // do we have any uploaded receipts? If so then no need to check
                if (model.Documents == null)
                {
                    // check if value has been entered for an expense
                    var expenseEntered = false;
                    var expenseAccomdation = model.Entries.Any(timeSheetEntryViewModel => Convert.ToDecimal(timeSheetEntryViewModel.Accomodation) > 0);
                    var expenseMeal = model.Entries.Any(timeSheetEntryViewModel => Convert.ToDecimal(timeSheetEntryViewModel.MealAllowance) > 0);
                    var expenseNoOfMiles = model.Entries.Any(timeSheetEntryViewModel => Convert.ToDecimal(timeSheetEntryViewModel.NoOfMiles) > 0);
                    var expenseTravel = model.Entries.Any(timeSheetEntryViewModel => Convert.ToDecimal(timeSheetEntryViewModel.Travel) > 0);
                    var expenseOther = model.Entries.Any(timeSheetEntryViewModel => Convert.ToDecimal(timeSheetEntryViewModel.Other) > 0);
                    var expenseParking = model.Entries.Any(timeSheetEntryViewModel => Convert.ToDecimal(timeSheetEntryViewModel.Parking) > 0);
                    expenseEntered = expenseAccomdation || expenseMeal || expenseNoOfMiles || expenseTravel || expenseOther || expenseParking;
                    if (expenseEntered)
                    {
                        // check individual for receipt requirement
                        var individual = this.timesheetAssociateService.GetIndividual(model.AssociateId, model.RoleId);

                        //check for receipt requirement
                        var propertyList = typeof(Individual).GetProperties();
                        // check for receipt properties
                        var receiptsRequired = false;
                        var requiredAccomdation = false;
                        var requiredMeal = false;
                        var requiredNoOfMiles = false;
                        var requiredTravel = false;
                        var requiredOther = false;
                        var requiredParking = false;

                        foreach (
                            var property in
                                propertyList.Where(property => property.Name.ToLowerInvariant().Contains("receipt")))
                        {
                            if (property.GetValue(individual, null) != null)
                            {
                                receiptsRequired = (bool)property.GetValue(individual, null);

                                if (receiptsRequired)
                                {
                                    switch (property.Name.ToLowerInvariant())
                                    {
                                        case "expenseaccomodationreceipt":
                                            requiredAccomdation = receiptsRequired = individual.ExpenseAccomodationReceipt.GetValueOrDefault();
                                            break;
                                        case "ExpenseSubsistenceReceipt":
                                            requiredMeal = receiptsRequired = individual.ExpenseSubsistenceReceipt.GetValueOrDefault();
                                            break;
                                        case "ExpenseTravelReceipt":
                                            requiredTravel = receiptsRequired = individual.ExpenseTravelReceipt.GetValueOrDefault();
                                            break;
                                        case "ExpenseParkingReceipt":
                                            requiredParking = receiptsRequired = individual.ExpenseParkingReceipt.GetValueOrDefault();
                                            break;
                                        case "ExpenseMileageReceipt":
                                            requiredNoOfMiles = receiptsRequired = individual.ExpenseMileageReceipt.GetValueOrDefault();
                                            break;
                                        case "ExpenseOtherReceipt":
                                            requiredOther = receiptsRequired = individual.ExpenseOtherReceipt.GetValueOrDefault();
                                            break;
                                    }
                                    break;
                                }
                            }
                        }

                        // check roles for receipt requirement
                        if (!receiptsRequired)
                        {
                            var types = this.timesheetAssociateService.GetRoleExpenseOptionRequiredReceiptsType(model.RoleId);

                            foreach (var type in types)
                            {
                                switch (type)
                                {
                                    case (byte)ExpenseType.Accommodation:
                                        requiredAccomdation = receiptsRequired = true;
                                        break;
                                    case (byte)ExpenseType.MealAllowance:
                                        requiredMeal = receiptsRequired = true;
                                        break;
                                    case (byte)ExpenseType.Travel:
                                        requiredTravel = receiptsRequired = true;
                                        break;
                                    case (byte)ExpenseType.Parking:
                                        requiredParking = receiptsRequired = true;
                                        break;
                                    case (byte)ExpenseType.Mileage:
                                        requiredNoOfMiles = receiptsRequired = true;
                                        break;
                                    case (byte)ExpenseType.Other:
                                        requiredOther = receiptsRequired = true;
                                        break;
                                }
                            }
                        }

                        // check project for receipt requirement
                        if (!receiptsRequired)
                        {
                            var types = this.timesheetAssociateService.GetProjectExpenseOptionRequiredReceiptsType(model.ProjectId);

                            foreach (var type in types)
                            {
                                switch (type)
                                {
                                    case (byte)ExpenseType.Accommodation:
                                        requiredAccomdation = receiptsRequired = true;
                                        break;
                                    case (byte)ExpenseType.MealAllowance:
                                        requiredMeal = receiptsRequired = true;
                                        break;
                                    case (byte)ExpenseType.Travel:
                                        requiredTravel = receiptsRequired = true;
                                        break;
                                    case (byte)ExpenseType.Parking:
                                        requiredParking = receiptsRequired = true;
                                        break;
                                    case (byte)ExpenseType.Mileage:
                                        requiredNoOfMiles = receiptsRequired = true;
                                        break;
                                    case (byte)ExpenseType.Other:
                                        requiredOther = receiptsRequired = true;
                                        break;
                                }
                            }
                        }

                        if (receiptsRequired && expenseEntered)
                        {
                            if ((expenseAccomdation && requiredAccomdation) || (expenseMeal && requiredMeal) || (expenseTravel && requiredTravel)
                                || (expenseParking && requiredParking) || (expenseNoOfMiles && requiredNoOfMiles) || (expenseOther && requiredOther))
                            {
                                var associate = timeSheetRepository.GetAssociatebyId(model.AssociateId);
                                var template = EmailTemplate.TimeSheetApprovedChaseReceipt;
                                if (associate.BusinessTypeID == (int)BusinessType.Umbrella)
                                {
                                    template = EmailTemplate.UmbrellaTimeSheetReceiptChaser;
                                }

                                //Send chase email
                                var chaseEmail = this.emailService.GetTimeSheetEmail(model.TimesheetId, template);
                                this.emailService.SendEmail(chaseEmail);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                status = false;
            }

            return status;
        }

        public bool MarkAsPending(TimesheetViewModel model)
        {
            bool status;
            try
            {
                this.timeSheetRepository.UpdateStatus(model.TimesheetId, TimesheetStatus.Pending, associateId: model.AssociateId, approverAssociateId: model.CurrentUserId, comment: model.Comment, timesheetChecker: model.TimesheetChecker);
                var Details = "Timesheet has been marked as " + TimesheetStatus.Pending.ToString() + " by " + AssociateFullName + " on " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString();

                //CreateCommunicationsHistoryItem(model.AssociateId, CommunicationType.Timesheet, "Timesheet " + TimesheetStatus.Pending.ToString(), CurrentUser.Identity.Name, Details);
                status = true;
                var email = this.emailService.GetTimeSheetEmail(model.TimesheetId,
                         EmailTemplate.AssociateExpenseReciepts);
                this.emailService.SendEmail(email);



                var associate = this.timesheetAssociateService.GetAssociate<AssociateModel>(model.AssociateId);

                var umbrellaUsers = this.associateRepository.GetUmbrellaUsers(associate.UmbrellaCompanyId);

                foreach (var u in umbrellaUsers)
                {
                    var umbrellaChange = this.emailService.GetTimeSheetEmail(model.TimesheetId, EmailTemplate.UmbrellaCompanyExpenseReciepts);
                    umbrellaChange.ToAddress = u.EMail;
                    umbrellaChange.Body = umbrellaChange.Body.Replace("[UmbrellaAgencyContactFirstName]", u.Name);
                    this.emailService.SendEmail(umbrellaChange);

                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                status = false;
            }

            return status;
        }

        public bool MarkAsCompleted(TimesheetViewModel model)
        {
            bool status;

            try
            {
                int allReceipts = (model.Documents == null ? 0 : model.Documents.Count());
                int acceptedReceipts = (model.Documents == null ? 0 : model.Documents.Where(d => d.Pass == true).Count());
                int completeacceptedReceipts = (model.Documents == null ? 0 : model.Documents.Where(d => d.Pass == true || d.Pass == false).Count());

                if (allReceipts > 0 && acceptedReceipts == 0 || completeacceptedReceipts != allReceipts)
                {
                    ErrorSignal.FromCurrentContext().Raise(new Exception("Unable to complete timesheet as no receipts have been accepted"));
                    status = false;
                }
                else
                {
                    this.timeSheetRepository.UpdateStatus(model.TimesheetId, TimesheetStatus.Checked, approverAssociateId: model.CurrentUserId, comment: model.Comment, timesheetChecker: model.TimesheetChecker);
                    var Details = "Timesheet has been marked as " + TimesheetStatus.Checked.ToString() + " by " + AssociateFullName + " on " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString();

                    //CreateCommunicationsHistoryItem(model.AssociateId, CommunicationType.Timesheet, "Timesheet " + TimesheetStatus.Complete.ToString(), CurrentUser.Identity.Name, Details);
                    status = true;
                }
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                status = false;
            }

            return status;
        }

        public bool RejectTimesheet(TimesheetViewModel model, bool isClientContact = false)
        {
            bool status;
            try
            {
                if (isClientContact)
                {
                    this.timeSheetRepository.UpdateStatus(model.TimesheetId, TimesheetStatus.Rejected, approverClientContactId: model.CurrentUserId, comment: model.Comment, timesheetChecker: model.TimesheetChecker);
                }
                else
                {
                    this.timeSheetRepository.UpdateStatus(model.TimesheetId, TimesheetStatus.Rejected, approverAssociateId: model.CurrentUserId, comment: model.Comment, timesheetChecker: model.TimesheetChecker);
                }
                status = true;

                // Send email
                var emailTemplate = EmailTemplate.TimeSheetRejected;
                if (model.TimeSheetTypeId == (int)TimesheetTypeId.Adjustment)
                {
                    emailTemplate = EmailTemplate.AdjustmentTimesheetRejected;
                }
                var email = this.emailService.GetTimeSheetEmail(model.TimesheetId, emailTemplate);

                if (model.TimeSheetTypeId == (int)TimesheetTypeId.Adjustment)
                {
                    email.Subject = string.Format("Adjustment {0}", email.Subject);
                }

                this.emailService.SendEmail(email);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                status = false;
            }

            return status;
        }

        public bool ReSubmitTimesheet(TimesheetViewModel model)
        {
            bool status;

            // ToDo: while refactoring, implement dirty object.
            try
            {
                foreach (var entry in model.Entries)
                {
                    if (entry.IsDay || model.Status == TimesheetStatus.Blank.ToString())
                    {
                        this.timeSheetRepository.UpdateTimeSheetEntry(this.UnMap(entry));
                    }
                }

                this.timeSheetRepository.UpdateTimeSheet(new TimeSheet() { TimeSheetId = model.TimesheetId, IncentiveDay = model.IncentiveDays });

                int approverAssociateId = this.GetApproverIdForTimeSheet(model.TimesheetId);
                var emailstatus = timeSheetRepository.ValidateEmailsubmission(model.TimesheetId);

                SendEmail(model, emailstatus, () => this.timeSheetRepository.UpdateStatus(model.TimesheetId, TimesheetStatus.Submitted, associateId: model.AssociateId, comment: model.Comment));
                var Details = "Timesheet has been Re-" + TimesheetStatus.Submitted.ToString() + " by " + AssociateFullName + " on " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString();

                status = true;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                status = false;
            }

            return status;
        }
        public bool UpdateNotes(int TimeSheetEntryId, string ExpenseColumn, string ExpenseNotes)
        {
            bool status;

            // ToDo: while refactoring, implement dirty object.
            try
            {

                status=timeSheetRepository.UpdateNotes(TimeSheetEntryId,ExpenseColumn,ExpenseNotes);               
                
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                status = false;
            }

            return status;


        
        }

        public bool AssociateRejectTimesheet(TimesheetViewModel model)
        {
            bool status;

            // ToDo: while refactoring, implement dirty object.
            try
            {
                this.timeSheetRepository.UpdateStatus(model.TimesheetId, TimesheetStatus.Updated, approverAssociateId: model.CurrentUserId, comment: model.Comment);
                var Details = "Timesheet has been " + TimesheetStatus.Rejected.ToString() + " by " + AssociateFullName + " on " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString();

                status = true;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                status = false;
            }

            return status;
        }
        /*
        public IEmailService EmailService
        {
            get { return emailService; }
            set { emailService = value; }
        }*/

        public bool SuspendTimesheet(int id, bool suspensionStatus, string comment)
        {
            bool status;

            // ToDo: while refactoring, implement dirty object.
            try
            {
                this.timeSheetRepository.SuspendAssociateTimesheets(id, suspensionStatus);
                var Details = (suspensionStatus ? "Suspended" : "Reactivated") + ": By " + CurrentUser.Identity.Name + " on " + DateTime.Now.ToShortDateString() + " at " + DateTime.Now.ToShortTimeString() + ". Reason: " + comment;


                CreateCommunicationsHistoryItem(id, CommunicationType.Timesheet, "Timesheet(s) " + (suspensionStatus ? "Suspended" : "Reactivated"), CurrentUser.Identity.Name, Details);

                var emailtemplate = suspensionStatus ? EmailTemplate.TimeSheetSuspended : EmailTemplate.TimeSheetReactivated;
                var email = this.emailService.GetSuspendTimeSheetEmail(id, emailtemplate);
                this.emailService.SendEmail(email);

                status = true;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                status = false;
            }

            return status;
        }

        #region Private

        private void MapEntry(ref TimeSheetEntryViewModel dayentry, IEnumerable<TimeSheetEntry> timesheetEntry, TimesheetValidationDetail validationDetails)
        {
            int dayId = dayentry.DayId;

            var entry = timesheetEntry.FirstOrDefault(x => x.DayOfWeekId == dayId);


            if (entry != null)
            {
                dayentry.TimeSheetEntryId = entry.TimeSheetEntryId;
                dayentry.Date = entry.Date.ToShortDateString();
                dayentry.Hour = entry.Hour.ToString();
                dayentry.Minute = entry.Minute.ToString();
                dayentry.TimesheetId = entry.TimeSheetId;
                dayentry.OverTime = entry.Overtime.HasValue && entry.Overtime.Value > 0 ? entry.Overtime.ToString() : string.Empty;
                dayentry.OverProduction = entry.OverProduction.HasValue && entry.OverProduction.Value > 0 ? entry.OverProduction.ToString() : string.Empty;
                dayentry.OneOfPayment = entry.OneOffPayment.HasValue && entry.OneOffPayment.Value > 0 ? entry.OneOffPayment.ToString() : string.Empty;
                dayentry.Accomodation = entry.Accomodation.HasValue && entry.Accomodation.Value > 0 ? entry.Accomodation.ToString() : string.Empty;
                dayentry.AccomodationNotes = entry.AccomodationNotes;
                dayentry.MealAllowance = entry.MealAllowance.HasValue && entry.MealAllowance.Value > 0 ? entry.MealAllowance.ToString() : string.Empty;
                dayentry.MealAllowanceNotes = entry.MealAllowanceNotes;
                dayentry.Travel = entry.Travel.HasValue && entry.Travel.Value > 0 ? entry.Travel.ToString() : string.Empty;
                dayentry.TravelNotes = entry.TravelNotes;
                dayentry.NoOfMiles = entry.NumberOfMiles.HasValue && entry.NumberOfMiles.Value > 0 ? ((int)entry.NumberOfMiles).ToString() : string.Empty;
                dayentry.NoOfMilesNotes = entry.NumberOfMilesNotes;
                dayentry.Mileage = entry.Mileage.HasValue && entry.Mileage.Value > 0 ? entry.Mileage.ToString() : string.Empty;
                dayentry.Parking = entry.Parking.HasValue && entry.Parking.Value > 0 ? entry.Parking.ToString() : string.Empty;
                dayentry.ParkingNotes = entry.ParkingNotes;
                dayentry.Other = entry.Other.HasValue && entry.Other.Value > 0 ? entry.Other.ToString() : string.Empty;
                dayentry.OtherNotes = entry.OtherNotes;
                dayentry.ActualHour = entry.ActualHour > 0 ? entry.ActualHour.ToString() : string.Empty;
                dayentry.ActualMinute = entry.ActualMinute > 0 ? entry.ActualMinute.ToString() : string.Empty;
                dayentry.AttendenceId = entry.AttendanceOptionId.HasValue ? (int)entry.AttendanceOptionId.Value : default(int);
            }
        }

        private void ApplyAbsence(ref TimeSheetEntryViewModel dayentry, IEnumerable<TimeSheetEntry> timesheetEntry, IEnumerable<Absence> absence)
        {

            int dayId = dayentry.DayId;

            var entry = timesheetEntry.FirstOrDefault(x => x.DayOfWeekId == dayId);

            var todayAbsence = absence.Where(x => entry.Date.Date >= x.AbsentFrom && entry.Date.Date <= x.AnticipatedReturn);

            if (todayAbsence.Count() > 0)
            {
                if (todayAbsence.First().AnticipatedReturn.Date == entry.Date.Date && todayAbsence.First().AbsentFrom.Date != entry.Date.Date)
                {
                    todayAbsence = null;
                }
            }

            if (todayAbsence == null || todayAbsence.Count() == 0)
            {
                dayentry.Absence = AbsenceDurationType.None.ToString();
                return;
            }

            var validationDetails = todayAbsence.First();
            var anticipatedDate = validationDetails.AnticipatedReturn.Date;

            if (entry.Date.Date <= anticipatedDate.Date && !(entry.Date.Date < validationDetails.AbsentFrom.Date))
            {
                dayentry.Absence = ((MR_DAL.Enumerations.AbsenceDurationType)validationDetails.AbsenceDurationTypeId).ToString();

                if (validationDetails.AbsenceDurationTypeId == (int)AbsenceDurationType.AMOnly
                    || validationDetails.AbsenceDurationTypeId == (int)AbsenceDurationType.PMOnly)
                {
                    if (todayAbsence.Count() > 1 && todayAbsence.ElementAt(0).AbsenceDurationTypeId != 3 && todayAbsence.ElementAt(0).AbsenceDurationTypeId != 3)
                    {
                        dayentry.AttendenceId = 9;
                        dayentry.Absence = MR_DAL.Enumerations.AbsenceDurationType.FullDay.ToString();
                    }
                    else
                    {
                        // dayentry.AttendenceId = 2; // Half day 
                    }

                }

                if (validationDetails.AbsenceDurationTypeId == (int)MR_DAL.Enumerations.AbsenceDurationType.FullDay)
                {
                    switch (validationDetails.AbsenceTypeId)
                    {
                        case 1:
                            dayentry.AttendenceId = 9; // Planned 
                            break;

                        case 2:
                            dayentry.AttendenceId = 8; // Sick
                            break;

                        case 3:
                            dayentry.AttendenceId = 10; // Unplanned  
                            break;
                    }

                    dayentry.ClearData();
                }
            }
            else
            {
                dayentry.Absence = AbsenceDurationType.None.ToString();
            }
        }

        private void MapTimeSheet(ref TimesheetViewModel timesheet, TimesheetValidationDetail validationDetails)
        {
            timesheet.IncentiveDayMaxWorked = validationDetails.IncentiveDaysMaxWorked.HasValue ? (int)validationDetails.IncentiveDaysMaxWorked : default(int);
            timesheet.IncentiveDaysCountedAs = validationDetails.IncentiveDaysCountedAs.HasValue ? (decimal)validationDetails.IncentiveDaysCountedAs : default(decimal);
            timesheet.OverTimePayAway = validationDetails.OverTimePayAway.HasValue ? (decimal)validationDetails.OverTimePayAway : default(decimal);
            timesheet.OverTimePayRatio = validationDetails.OverTimePayRatio.HasValue ? (decimal)validationDetails.OverTimePayRatio : default(decimal);
            timesheet.OneOfPayment = validationDetails.OneOffPmtAmount.HasValue ? (decimal)validationDetails.OneOffPmtAmount : default(decimal);
            timesheet.OverProduction = validationDetails.OverProductionPayaway.HasValue ? (decimal)validationDetails.OverProductionPayaway : default(decimal);
            timesheet.AccomodationLimit = validationDetails.ExpenseAccomodation.HasValue ? (decimal)validationDetails.ExpenseAccomodation : default(decimal);
            timesheet.Suspended = validationDetails.Suspended; //.HasValue ? Convert.ToBoolean(validationDetails.Suspended) : default(bool);
            timesheet.AssociateRate = validationDetails.AssociateRate.HasValue ? (decimal)validationDetails.AssociateRate : default(decimal);
            timesheet.IncentiveRate = validationDetails.IncentivePayaway.HasValue ? (decimal)validationDetails.IncentivePayaway : default(decimal);

            timesheet.MealAllowanceLimit = validationDetails.ExpenseSubsistence.HasValue ? (decimal)validationDetails.ExpenseSubsistence : default(decimal);
            timesheet.TravelLimit = validationDetails.ExpenseTravel.HasValue ? (decimal)validationDetails.ExpenseTravel : default(decimal);

            timesheet.NoOfMilesLimit = validationDetails.ExpenseSubsistence.HasValue ? (decimal)validationDetails.ExpenseSubsistence : default(decimal);
            timesheet.MileageLimit = validationDetails.ExpenseMileage.HasValue ? (decimal)validationDetails.ExpenseMileage : default(decimal);
            timesheet.ParkingLimit = validationDetails.ExpenseParking.HasValue ? (decimal)validationDetails.ExpenseParking : default(decimal);
            timesheet.OtherLimit = validationDetails.ExpenseOther.HasValue ? (decimal)validationDetails.ExpenseOther : default(decimal);
            timesheet.WorkingHours = validationDetails.WorkingHours.HasValue ? (decimal)validationDetails.WorkingHours : default(decimal);
            timesheet.OneofPaymentDate = validationDetails.OneofPaymentDate.HasValue ? validationDetails.OneofPaymentDate.Value : default(DateTime);
            timesheet.IncentiveDaysIn7 = validationDetails.IncentiveDaysIn7.HasValue ? validationDetails.IncentiveDaysIn7.Value : default(int);
            timesheet.OneOfPaymentLimit = validationDetails.OneOffPmtAmount.HasValue ? (decimal)validationDetails.OneOffPmtAmount : default(decimal);

        }

        private AssociateDetails GetAssociate(TimesheetValidationDetail individual, TimeSheet timeSheet)
        {
            var associate = this.timesheetAssociateService.GetAssociate<AssociateModel>(timeSheet.AssociateId);

            return new AssociateDetails()
            {
                AssociateName = string.Concat(associate.FirstName, " " + associate.LastName),
                ClientName = individual.ClientName,
                Manager = individual.TimeSheetApprover,
                ProjectName = individual.ProjectName,
                Role = individual.RoleName,
                Rate = individual.AssociateRate.HasValue ? individual.AssociateRate.Value : default(decimal),
                WeekStartDate = timeSheet.StartDate.ToShortDateString(),
                WeekEndDate = timeSheet.EndDate.ToShortDateString(),
                IsAgencyOrUmbrella = individual.IsAgencyOrUmbrella == 1 || timeSheet.AssociateId != base.AssociateId ? true : false
            };
        }

        private TimeSheetEntry UnMap(TimeSheetEntryViewModel entryVM)
        {
            if (entryVM == null)
            {
                return null;
            }

            return new TimeSheetEntry
            {
                Date = Convert.ToDateTime(entryVM.Date),
                Hour = Convert.ToByte(entryVM.Hour),
                Minute = Convert.ToByte(entryVM.Minute),
                Overtime = Convert.ToDecimal(entryVM.OverTime),
                OverProduction = Convert.ToDecimal(entryVM.OverProduction),
                OneOffPayment = Convert.ToDecimal(entryVM.OneOfPayment),
                Accomodation = Convert.ToDecimal(entryVM.Accomodation),
                AccomodationNotes = entryVM.AccomodationNotes,
                MealAllowance = Convert.ToDecimal(entryVM.MealAllowance),
                MealAllowanceNotes = entryVM.MealAllowanceNotes,
                Travel = Convert.ToDecimal(entryVM.Travel),
                TravelNotes = entryVM.TravelNotes,
                NumberOfMiles = Convert.ToDecimal(entryVM.NoOfMiles),
                NumberOfMilesNotes = entryVM.NoOfMilesNotes,
                Mileage = Convert.ToDecimal(entryVM.Mileage),
                Parking = Convert.ToDecimal(entryVM.Parking),
                ParkingNotes = entryVM.ParkingNotes,
                Other = Convert.ToDecimal(entryVM.Other),
                OtherNotes = entryVM.OtherNotes,
                TimeSheetEntryId = entryVM.TimeSheetEntryId,
                AttendanceOptionId = Convert.ToByte(entryVM.AttendenceId),
                TimeSheetId = entryVM.TimesheetId,
                DayOfWeekId = Convert.ToByte(entryVM.DayId),
                ActualHour = Convert.ToInt32(entryVM.ActualHour),
                ActualMinute = Convert.ToInt32(entryVM.ActualMinute),
            };
        }

        private bool ValidateAbsense(ref TimeSheetEntryViewModel dayentry, TimeSheetEntry timesheetEntry, TimesheetValidationDetail validationDetails)
        {
            var anticipatedDate = validationDetails.AnticipatedReturn;

            if (anticipatedDate.HasValue && anticipatedDate.Value >= DateTime.Now.Date)
            {
                if (timesheetEntry.Date <= anticipatedDate.Value && !(timesheetEntry.Date < validationDetails.AbsentFrom))
                {
                    dayentry.Absence = validationDetails.AbsenceDurationTypeId.HasValue
                        ? ((AbsenceDurationType)validationDetails.AbsenceDurationTypeId).ToString()
                        : AbsenceDurationType.None.ToString();

                    return true;
                }
            }

            dayentry.Absence = AbsenceDurationType.None.ToString();

            return false;
        }

        private TimeSheetEntryViewModel[] GetDays(TimesheetValidationDetail vData, out string[] days)
        {

            if ((vData.SaturdayOvertime.GetValueOrDefault() && vData.SundayOvertime.GetValueOrDefault()) || (vData.Saturday.GetValueOrDefault() && vData.Sunday.GetValueOrDefault()))
            {
                days = new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            }
            else
            {
                if (vData.Sunday.GetValueOrDefault() || vData.SundayOvertime.GetValueOrDefault())
                    days = new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Sunday" };

                else if (vData.Saturday.GetValueOrDefault() || vData.SaturdayOvertime.GetValueOrDefault())
                    days = new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

                else
                    days = new string[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
            }

            return new TimeSheetEntryViewModel[days.Count()];
        }

        private void SendEmail(TimesheetViewModel model, bool status, Action action)
        {
            action();

            if (!status)
            {
                EmailModel emailModel = this.emailService.GetTimeSheetEmail(model.TimesheetId, EmailTemplate.TimeSheetApprovalOutstanding);
                this.emailService.SendEmail(emailModel);
            }
        }

        #endregion

        public bool DeleteDocument(Guid id, int? associateId = null)
        {
            if (!associateId.HasValue)
            {
                associateId = this.AssociateId;
            }

            var status = timeSheetRepository.DeleteDocument(id, associateId.Value);

            return status;
        }

        public bool AcceptDocument(Guid id, int associateId)
        {
            var comment = DateTime.Now.ToString("dd/MM/yyyy") + " : checked";

            var status = timeSheetRepository.AcceptDocument(id, associateId, comment, CurrentUser.Identity.Name);

            return status;
        }

        public bool RejectDocument(int timesheetId, Guid documentId, int associateId, string comment)
        {
            // attach the date to the comment
            comment = DateTime.Now.ToString("dd/MM/yyyy") + " : " + comment;

            var associate = timeSheetRepository.GetAssociatebyId(associateId);

            var template = EmailTemplate.TimeSheetReceiptRejected;
            if (associate.BusinessTypeID == (int)BusinessType.Umbrella)
            {
                template = EmailTemplate.UmbrellaTimeSheetReceiptRejected;
            }
            // reject the document
            var rejected = timeSheetRepository.RejectDocument(documentId, associateId, comment, CurrentUser.Identity.Name);

            // notify the associate
            if (rejected)
            {
                var email = this.emailService.GetTimeSheetReceiptEmail(timesheetId, template, comment);
                this.emailService.SendEmail(email);
            }

            return rejected;
        }

        public int SendBlankOrUpdatedReminderEmail(string action)
        {
            // Find all associates with Timesheets with status blank or updated

            var timesheets = this.timeSheetRepository.GetTimeSheetsByStatus(TimesheetStatus.Blank).ToList<TimeSheet>();
            timesheets.AddRange(this.timeSheetRepository.GetTimeSheetsByStatus(TimesheetStatus.Updated));

            timesheets = !string.IsNullOrEmpty(action) ? ExtendedTimeSheetPeriod(timesheets) : TimeSheetPeriod(timesheets);

            var emails = new List<EmailModel>();

            // build a list of emails
            foreach (var timesheet in timesheets)
            {
                var email = this.emailService.GetTimeSheetEmail(timesheet.TimeSheetId, EmailTemplate.TimeSheetAssociateSubmit);
                if (email != null && !string.IsNullOrEmpty(email.ToAddress))
                {
                    emails.Add(email);
                }
            }

            // get a list of distinct recipients
            var distinctRecipients = (from e in emails select e.ToAddress).Distinct().ToList<string>();

            // only send one email per recipient by getting first email from list that matches the recipient
            foreach (var recipient in distinctRecipients)
            {
                var email = emails.First(e => e.ToAddress == recipient);
                this.emailService.SendEmail(email);
            }

            return timesheets.Count;
        }
        public int SendBlankOrUpdatedReminderEmailExtended(string action, int weeks)
        {
            // Find all associates with Timesheets with status blank or updated

            var timesheets = this.timeSheetRepository.GetTimeSheetsByStatus(TimesheetStatus.Blank).ToList<TimeSheet>();
            timesheets.AddRange(this.timeSheetRepository.GetTimeSheetsByStatus(TimesheetStatus.Updated));

            timesheets = !string.IsNullOrEmpty(action) ? ExtendedTimeSheetPeriodWeeks(timesheets, weeks) : TimeSheetPeriod(timesheets);

            var emails = new List<EmailModel>();

            // build a list of emails
            foreach (var timesheet in timesheets)
            {
                var email = this.emailService.GetTimeSheetEmail(timesheet.TimeSheetId, EmailTemplate.TimeSheetAssociateSubmit);
                if (email != null && !string.IsNullOrEmpty(email.ToAddress))
                {
                    if (emails.Where(x => x.ToAddress == email.ToAddress).ToList().Count == 0)
                        emails.Add(email);
                }
            }

            // get a list of distinct recipients
            //  var distinctRecipients = (from e in emails select e.ToAddress).Distinct().ToList<string>();

            // only send one email per recipient by getting first email from list that matches the recipient
            foreach (var email in emails)
            {
                //var email = emails.First(e => e.ToAddress == recipient);
                this.emailService.SendEmail(email);
            }

            return timesheets.Count;
        }
        public int SendApproverReminderEmail()
        {
            // Find all timesheets awaiting approval (i.e. status == submitted)
            var timesheets = this.timeSheetRepository.GetTimeSheetsByStatus(TimesheetStatus.Submitted).ToList<TimeSheet>();

            var emails = new List<EmailModel>();

            // build a list of emails
            foreach (var timesheet in timesheets)
            {
                // wrong email
                var email = this.emailService.GetTimeSheetEmail(timesheet.TimeSheetId, EmailTemplate.TimeSheetApprovalReminder);
                if (email != null && !string.IsNullOrEmpty(email.ToAddress))
                {
                    if (timesheet.TimeSheetTypeId == (int)TimesheetTypeId.Adjustment)
                    {
                        email.Subject = string.Format("Adjustment {0}", email.Subject);
                    }

                    emails.Add(email);
                }
            }

            // get a list of distinct client contact IDs
            var clientContactIds = (from e in emails select e.ClientContactId).Distinct().ToList<int>();

            // only send one email per recipient by getting first email from list that matches the client contact ID
            foreach (var id in clientContactIds)
            {
                var email = emails.First(e => e.ClientContactId == id);
                this.emailService.SendEmail(email);
            }

            return timesheets.Count;
        }

        public int SendPaymentDateReminderEmail()
        {
            // Find all invoices where payment date is within 10days with the status of processed
            var invoices = this.timeSheetRepository.GetInvoicesPendingPayment().ToList<Invoice>();

            foreach (var invoice in invoices)
            {
                var associate = timeSheetRepository.GetAssociatebyId(invoice.AssociateId);
                byte? umbrellaCompanyId = null;
                var template = EmailTemplate.TimeSheetPaymentDateReminder;
                if (associate.BusinessTypeID == (int)BusinessType.Umbrella)
                {
                    template = EmailTemplate.UmbrellaTimeSheetPaymentDateReminder;
                    umbrellaCompanyId = associate.UmbrellaCompanyId;
                }
                var email = this.emailService.GetInvoiceEmail(invoice.InvoiceId, template, umbrellaCompanyId);

                this.emailService.SendEmail(email);
            }

            return invoices.Count;
        }

        public void CreateCommunicationsHistoryItem(int associateId, CommunicationType type, string description, string loggedInUser, string details)
        {
            this.timeSheetRepository.CreateCommunicationsHistoryItem(associateId, type, description, loggedInUser, details);
        }

        // todo: is this still used? if not remove it
        public IEnumerable<TimeSheetsGridViewModel> GetTimeSheetsForInvoicing(int associateId)
        {
            var timesheetVMs = new List<TimeSheetsGridViewModel>();
            var timesheets = this.timeSheetRepository.GetTimeSheetsForInvoicing(associateId);
            foreach (var timesheet in timesheets)
            {

                var vm = new TimeSheetsGridViewModel
                {
                    ProjectName = timesheet.ProjectName,
                    StartDate = timesheet.StartDate,
                    EndDate = timesheet.EndDate,
                    RoleName = timesheet.RoleName,
                    Status = timesheet.Status,
                    ManagerName = timesheet.ManagerName,
                    TimeSheetId = timesheet.TimeSheetId,
                    AssociateId = timesheet.AssociateId,
                    TimesheetTypeId = (int)(timesheet.TimeSheetTypeId)
                };

                timesheetVMs.Add(vm);
            }

            return (IEnumerable<TimeSheetsGridViewModel>)timesheetVMs;
        }

        public IEnumerable<TimeSheetsGridViewModel> GetInvoicedTimeSheets(int associateId)
        {
            var timesheetVMs = new List<TimeSheetsGridViewModel>();
            var timesheets = this.timeSheetRepository.GetInvoicedTimeSheets(associateId);

            foreach (var timesheet in timesheets)
            {
                var vm = new TimeSheetsGridViewModel
                {
                    ProjectName = timesheet.ProjectName,
                    StartDate = timesheet.StartDate,
                    EndDate = timesheet.EndDate,
                    RoleName = timesheet.RoleName,
                    Status = timesheet.Status,
                    ManagerName = timesheet.ManagerName,
                    TimeSheetId = timesheet.TimeSheetId,
                    AssociateId = timesheet.AssociateId
                };

                timesheetVMs.Add(vm);
            }

            return (IEnumerable<TimeSheetsGridViewModel>)timesheetVMs;
        }

        public IEnumerable<InvoiceGridViewModel> GetCurrentInvoices(int associateId)
        {
            var invoiceVMs = new List<InvoiceGridViewModel>();
            var invoices = this.timeSheetRepository.GetCurrentInvoices(associateId);

            foreach (var invoice in invoices)
            {
                var vm = new InvoiceGridViewModel
                {
                    ProjectName = invoice.ProjectName,
                    StartDate = invoice.StartDate,
                    EndDate = invoice.EndDate,
                    RoleName = invoice.RoleName,
                    Status = invoice.Status,
                    InvoiceId = invoice.InvoiceId,
                    AssociateId = invoice.AssociateId,
                    HasFile = bool.Parse(invoice.HasFile)
                };

                invoiceVMs.Add(vm);
            }

            return (IEnumerable<InvoiceGridViewModel>)invoiceVMs;
        }

        public int CreateInvoice(int associateId, int[] timesheetIds, decimal vatRate, bool selfBilling = false)
        {
            int invoiceId = 0;
            try
            {
                // Get a list of the active timesheets for the associate
                var timesheets = this.timeSheetRepository.GetTimeSheetsForInvoicing(associateId)
                    .Where(t => timesheetIds.Contains(t.TimeSheetId))
                    .ToList<GetTimeSheetsForInvoicing_Result>();

                if (timesheets.Count > 0)
                {

                    // get a distinct list of project names for the timesheets
                    var projectNames = (from t in timesheets
                                        where timesheetIds.Contains(t.TimeSheetId)
                                        select t.ProjectName).Distinct().ToArray<string>();

                    // set project name, if more than one then set to "multiple"
                    var projectName = (projectNames.Count() > 1 ? "Multiple" : projectNames.First());

                    // get a distinct list of role names for the timesheets
                    var roleNames = (from t in timesheets
                                     where timesheetIds.Contains(t.TimeSheetId)
                                     select t.RoleName).Distinct().ToArray<string>();

                    // set role name, if more than one then set to "multiple"
                    var roleName = (roleNames.Count() > 1 ? "Multiple" : roleNames.First());

                    // get minimum timesheet start date to define invoice start
                    var start = (from t in timesheets
                                 where timesheetIds.Contains(t.TimeSheetId)
                                 select t.StartDate).Min();

                    // get maximum timesheet end date to define invoice end
                    var end = (from t in timesheets
                               where timesheetIds.Contains(t.TimeSheetId)
                               select t.EndDate).Max();

                    // create the invoice
                    invoiceId = this.timeSheetRepository.CreateInvoice(associateId, start, end, InvoiceStatus.Invoiced,
                        projectName, roleName, selfBilling);

                    if (invoiceId > 0)
                    {

                        // add each timesheet to the invoice and check timesheet status, mark as complete if no expenses
                        foreach (var sheet in timesheets)
                        {
                            TimesheetViewModel timesheet = this.GetTimeSheetEntry(sheet.TimeSheetId);

                            this.timeSheetRepository.AddTimeSheetToInvoice(invoiceId, sheet.TimeSheetId);
                            string Details = "Invoice Generated";
                            this.timeSheetRepository.CreateTimesheetHistory(sheet.TimeSheetId, TimesheetStatus.Invoiced,
                                Details,
                                associateId,
                                sheet.ManagerId);

                            // this.GetRecieptInfo(ref timesheet, timesheet.RoleId);

                            if (timesheet.ExpenseReceiptDetails.Count() == 0)
                            {
                                this.timeSheetRepository.UpdateStatus(sheet.TimeSheetId, TimesheetStatus.Checked,
                                    comment: "Timesheet with no receipts required marked as complete at invoice time", associateId: sheet.AssociateId, approverAssociateId: sheet.ManagerId);
                            }

                        }

                        // add to activity
                        string details = "Invoice Report Number " + invoiceId.ToString("00000") + " generated on " +
                                         DateTime.Now.ToShortDateString();

                        // add to history
                        this.timeSheetRepository.AddInvoiceHistory(new InvoiceHistory
                        {
                            AssociateId = associateId,
                            InvoiceId = invoiceId,
                            InvoiceStatusId = (int)InvoiceStatus.Invoiced,
                            LoggedInUser = selfBilling ? "Momenta" : CurrentUser.Identity.Name,
                            Comments = details
                        });

                        decimal totalAmount = 0;
                        decimal vatAmount = 0;
                        if (timesheets.Any(t => t.TimeSheetTypeId == 2))
                        {
                            //totalAmount = timesheets.Sum(ts => timeSheetRepository.GetTimeSheetById(ts.TimeSheetId).RetainerEntry.FirstOrDefault().Total.Value);
                            foreach (var ts in timesheets)
                            {
                                decimal retainerTotal = 0;
                                if (timeSheetRepository.GetTimeSheetById(ts.TimeSheetId).RetainerEntry.Any() && timeSheetRepository.GetTimeSheetById(ts.TimeSheetId).RetainerEntry.FirstOrDefault().Total.HasValue)
                                {
                                    retainerTotal = timeSheetRepository.GetTimeSheetById(ts.TimeSheetId).RetainerEntry.FirstOrDefault().Total.Value;
                                }
                                totalAmount += retainerTotal;
                            }

                            vatAmount = totalAmount * vatRate;
                        }
                        else
                        {
                            // add total amount and vat amount
                            var invoicesReportData = GetInvoicesReportData(invoiceId);
                            totalAmount = invoicesReportData.Sum(invoiceReportViewModel => invoiceReportViewModel.Total);
                            vatAmount = totalAmount * vatRate;
                        }
                        this.timeSheetRepository.ProcessInvoice(invoiceId, (int)TimesheetStatus.Invoiced, (int)InvoiceStatus.Invoiced, totalAmount, vatAmount);

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("There was an error generating the invoice report", ex);
            }

            return invoiceId;
        }

        public void CreateInvoiceFile(int invoiceId, string filePath, InvoiceFileStatus status)
        {
            this.timeSheetRepository.CreateInvoiceFile(invoiceId, filePath, status);

            var invoice = this.GetInvoice(invoiceId);

            //add to history
            this.timeSheetRepository.AddInvoiceHistory(new InvoiceHistory
            {
                AssociateId = invoice.AssociateId,
                InvoiceId = invoiceId,
                InvoiceStatusId = (int)InvoiceStatus.Invoiced,
                LoggedInUser = CurrentUser.Identity.Name,
                Comments = "Invoice file \"" + filePath + "\" uploaded"
            });

            //send email(s)
            var invoiceNotification = this.emailService.GetInvoiceEmail(invoiceId, EmailTemplate.InvoiceCreatedNotification);
            this.emailService.SendEmail(invoiceNotification);
        }

        // todo: is this still used? if not remove it
        public IEnumerable<TimeSheetsGridViewModel> GetTimeSheetsByInvoiceId(int invoiceId)
        {
            var timesheetVMs = new List<TimeSheetsGridViewModel>();
            var timesheets = this.timeSheetRepository.GetTimeSheetsByInvoiceId(invoiceId);

            foreach (var timesheet in timesheets)
            {
                var vm = new TimeSheetsGridViewModel
                {
                    ProjectName = timesheet.ProjectName,
                    StartDate = timesheet.StartDate,
                    EndDate = timesheet.EndDate,
                    RoleName = timesheet.RoleName,
                    Status = timesheet.Status,
                    ManagerName = timesheet.ManagerName,
                    TimeSheetId = timesheet.TimeSheetId,
                    AssociateId = timesheet.AssociateId,
                    TimesheetTypeId = (int)timesheet.TimeSheetTypeId
                };

                timesheetVMs.Add(vm);
            }

            return (IEnumerable<TimeSheetsGridViewModel>)timesheetVMs;
        }

        public InvoiceGridViewModel GetInvoice(int invoiceId)
        {
            var invoice = this.timeSheetRepository.GetInvoice(invoiceId);

            var invoiceHistory = this.timeSheetRepository.GetInvoiceHistory(invoiceId).OrderBy(a=>a.Date).FirstOrDefault();

            var files = this.timeSheetRepository.GetInvoiceFiles(invoiceId);
            var vm = new InvoiceGridViewModel
            {
                
                ProjectName = invoice.ProjectName,
                StartDate = invoice.StartDate,
                EndDate = invoice.EndDate,
                RoleName = invoice.RoleName,
                Status = invoice.Status,
                InvoiceId = invoice.InvoiceId,
                AssociateId = invoice.AssociateId,
                HasFile = invoice.Hasfile ==1?true:false,
                SelfBilling = invoice.SelfBilling,
                RetainerRetentionPayAway=Convert.ToDecimal(invoice.RetentionPayAway),
                InvoiceCreatedDate = invoiceHistory.Date
            };

            return vm;
        }

        public IEnumerable<InvoiceReportViewModel> GetInvoicesReportData(int invoiceId)
        {
            try
            {
                var results = new List<InvoiceReportViewModel>();
                var data = this.timeSheetRepository.GetInvoicesReportData(invoiceId).OrderBy(x=>x.StartDate).OrderBy(x=>x.EntryDate);
                var incentiveDaysIncluded = false;
                var varPrevStartDate=DateTime.Now.AddDays(1);
                decimal incentiveDays = 0;
                int rowcount=0;
                foreach (var row in data)
                {
                    DateTime startDate = (DateTime)row.StartDate;
                    DateTime entryDate = (DateTime)row.EntryDate;

                    if (varPrevStartDate == startDate)
                    {
                        row.IncentiveDay = 0;
                    }
                    else
                    {
                        varPrevStartDate = startDate;
                    }

                    IEnumerable<IndividualChanges> individualchanges = null;
                    // get historic values  
                    if (rowcount == 0)
                    {
                       individualchanges = this.GetHistoricIndividualChanges(row.IndividualId);
                        rowcount = 1;
                    }
                    if (individualchanges != null) { 
                    row.IncentivePayaway = this.GetHistoricIndividualValue<decimal>(row.IndividualId, "IncentivePayaway", (DateTime)row.EndDate, row.IncentivePayaway,individualchanges);
                    row.OverproductionPayaway = this.GetHistoricIndividualValue<decimal>(row.IndividualId, "OverproductionPayaway", (DateTime)row.EndDate, row.OverproductionPayaway,individualchanges);
                    row.OverTimePayAway = this.GetHistoricIndividualValue<decimal>(row.IndividualId, "OverTimePayAway", (DateTime)row.EndDate, row.OverTimePayAway,individualchanges);
                    row.OneOffPayment = this.GetHistoricIndividualValue<decimal>(row.IndividualId, "OneOffPayment", (DateTime)row.EndDate, row.OneOffPayment,individualchanges);
                    row.AssociateRate = this.GetHistoricIndividualValue<decimal>(row.IndividualId, "AssociateRate", (DateTime)row.EntryDate, row.AssociateRate,individualchanges);
                    }
                    var vm = results.FirstOrDefault(r => r.TimeSheetId == row.TimeSheetId && r.Rate == row.AssociateRate);
                   
                    if (vm == null)
                    {
                        vm = new InvoiceReportViewModel
                        {
                            StartDate = startDate.ToString("dd/MM/yyyy"),
                            EntryDate = entryDate.ToString("dd/MM/yyyy"),
                            ProjectName = row.ProjectName,
                            ClientName = row.ClientName,
                            TimeSheetStatus = row.TimeSheetStatus,
                            RoleName = row.RoleName,
                            Rate = row.AssociateRate,
                            IncentiveDays = 0,
                            TimeSheetId = row.TimeSheetId,
                            TimesheetTypeId = row.TimesheetTypeId,
                            OverProduction = 0,
                            OverTime = 0,
                            OneOff = 0,
                            Expenses = 0,
                            Total = 0,
                            Hours = 0,
                            Days = 0,
                            DailyCharge = 0
                        };

                        results.Add(vm);
                        incentiveDaysIncluded = false;
                    }

                    var absent = false;
                    decimal rowCharge = 0;

                    switch (row.AttendanceOptionId)
                    {
                        case (int)MR_DAL.Enumerations.AttendanceOption.FullDay:
                        case (int)MR_DAL.Enumerations.AttendanceOption.CancelFullDay:
                        case (int)MR_DAL.Enumerations.AttendanceOption.TravelFullDay:
                        case (int)MR_DAL.Enumerations.AttendanceOption.Adjustment:
                        case (int)MR_DAL.Enumerations.AttendanceOption.NegativeFullDay:
                        case (int)MR_DAL.Enumerations.AttendanceOption.NegativeHalfDay:

                            if (row.AttendanceOptionId == (int)MR_DAL.Enumerations.AttendanceOption.NegativeFullDay
                                || row.AttendanceOptionId == (int)MR_DAL.Enumerations.AttendanceOption.NegativeHalfDay)
                            {
                                rowCharge = -1 * vm.Rate;
                            }
                            else
                            {
                                rowCharge = 1 * vm.Rate;
                            }

                            switch (row.AbsenceDurationTypeId)
                            {
                                case (int)MR_DAL.Enumerations.AbsenceDurationType.FullDay:
                                    absent = true;
                                    break;
                                case (int)MR_DAL.Enumerations.AbsenceDurationType.AMOnly:
                                case (int)MR_DAL.Enumerations.AbsenceDurationType.PMOnly:
                                    vm.Hours += ((decimal)row.Hour + (decimal)row.Minute / 60) / 2;
                                    vm.Days += (decimal)0.5;
                                    break;
                                default:
                                    vm.Hours += (decimal)row.Hour + (decimal)row.Minute / 60;
                                    if (row.Hour > 0)
                                    {
                                        vm.Days += 1;
                                    }
                                    else if (row.Hour < 0)
                                    {
                                        vm.Days = vm.Days - 1;
                                    }
                                    break;
                            }
                            break;

                        case (int)MR_DAL.Enumerations.AttendanceOption.HalfDay:
                        case (int)MR_DAL.Enumerations.AttendanceOption.CancelHalfDay:
                        case (int)MR_DAL.Enumerations.AttendanceOption.TravelHalfDay:
                            vm.Hours += (decimal)row.Hour + (decimal)row.Minute / 60;
                            rowCharge = (decimal)0.5 * vm.Rate;
                            vm.Days += (decimal)0.5;
                            break;
                        case (int)MR_DAL.Enumerations.AttendanceOption.Hourly:
                            vm.Hours += (decimal)row.Hour + (decimal)row.Minute / 60;
                            rowCharge = vm.Rate * (((decimal)row.Hour + ((decimal)row.Minute / 60)) / row.WorkingHours);
                            break;
                        case (int)MR_DAL.Enumerations.AttendanceOption.PlannedAbsence:
                        case (int)MR_DAL.Enumerations.AttendanceOption.SickDay:
                        case (int)MR_DAL.Enumerations.AttendanceOption.UnplannedAbsence:
                            absent = true;
                            break;
                    }

                    if (!absent)
                    {
                        if (!incentiveDaysIncluded)
                        {
                            incentiveDays = (decimal)(row.IncentiveDay * row.IncentivePayaway);
                            vm.IncentiveDays = incentiveDays;
                            //rowCharge += incentiveDays;
                            incentiveDaysIncluded = true;
                        }

                        decimal rowOverProduction = 0;
                        decimal rowOverTime = 0;
                        decimal rowOneOff = 0;
                        decimal rowExpenses = 0;

                        rowOverProduction = (decimal)(row.OverProduction * row.OverproductionPayaway);
                        var overTimeHours = ConvertToHourFraction(row.Overtime);
                        rowOverTime = (decimal)(overTimeHours * row.OverTimePayAway);
                        rowOneOff = (decimal)row.OneOffPayment;

                        vm.OverProduction += rowOverProduction;
                        vm.OverTime += rowOverTime;
                        vm.OneOff += rowOneOff;

                        rowExpenses += (decimal)row.Travel;
                        rowExpenses += (decimal)row.MealAllowance;
                        rowExpenses += (decimal)row.Mileage;
                        rowExpenses += (decimal)row.Other;
                        rowExpenses += (decimal)row.Accomodation;
                        rowExpenses += (decimal)row.Parking;
                        vm.Expenses += rowExpenses;

                        vm.Total += rowOverProduction;
                        vm.Total += rowOverTime;
                        vm.Total += rowOneOff;
                        vm.Total += rowExpenses;
                        //vm.Total += vm.IncentiveDays;
                        vm.Total += rowCharge;
                        vm.DailyCharge += rowCharge;
                    }
                }

                // handle incentive days and daily charge
                //results.FirstOrDefault().IncentiveDays = incentiveDays;
                //results.First().Total += incentiveDays;

                foreach (var invoiceReportViewModel in results)
                {
                    invoiceReportViewModel.ReceiptsRequiringAction =
                        GetReceiptsRequiringAction(invoiceReportViewModel.TimeSheetId);
                }

                return (IEnumerable<InvoiceReportViewModel>)results;
            }
            catch (Exception ex)
            {
                throw new Exception("The was a problem retreiving the invoice report", ex);
            }
        }

        public IEnumerable<InvoiceReportRetainerViewModel> GetInvoicesReportRetainerData(int invoiceId)
        {
            try
            {
                var results = new List<InvoiceReportRetainerViewModel>();
                var data = this.timeSheetRepository.GetInvoiceReportRetainerData(invoiceId);

                foreach (var row in data)
                {
                    var vm = results.FirstOrDefault(r => r.TimeSheetId == row.TimeSheetId);

                    if (vm == null)
                    {
                        vm = new InvoiceReportRetainerViewModel
                        {
                            StartDate = ((DateTime)row.StartDate).ToString("dd/MM/yyyy"),
                            ProjectName = row.ProjectName,
                            ClientName = row.ClientName,
                            RoleName = row.RoleName,
                            TimeSheetId = row.TimeSheetId,
                            Days = row.NoOfDays.HasValue ? row.NoOfDays.Value : 0,
                            Total = (row.Total == null ? 0 : (decimal)row.Total),
                            AssociateRate = (row.AssociateRate.HasValue ? (decimal)row.AssociateRate.Value : 0),
                            RetentionPayAway = (decimal)row.RetentionPayAway
                        };

                        results.Add(vm);
                    }
                }

                return (IEnumerable<InvoiceReportRetainerViewModel>)results;
            }
            catch (Exception ex)
            {
                throw new Exception("The was a problem retreiving the invoice report", ex);
            }
        }

        public IEnumerable<InvoiceFileViewModel> GetInvoiceFiles(int invoiceId, bool includeUserName = true)
        {
            var files = new List<InvoiceFileViewModel>();
            var invoices = this.timeSheetRepository.GetInvoiceFiles(invoiceId);

            foreach (var invoice in invoices)
            {
                var vm = new InvoiceFileViewModel
                {
                    InvoiceFileId = invoice.InvoiceFileId.ToString(),
                    InvoiceId = invoiceId,
                    InvoiceFileStatus = ((MR_DAL.Enumerations.InvoiceFileStatus)invoice.InvoiceFileStatusId).ToString(),
                    FileType = invoice.FileType,
                    ContentType = invoice.ContentType,
                    FileName = invoice.FileName,
                    InvoiceFileDate = (DateTime)invoice.FileDate,
                    StatusText = FormatStatusText(invoice.StatusText, includeUserName)
                };

                files.Add(vm);
            }

            return (List<InvoiceFileViewModel>)files;
        }

        private bool GetReceiptsRequiringAction(int timeSheetId)
        {
            return this.timeSheetRepository.GetTimesheetDocuments(timeSheetId).Any(document => document.Pass == null);
        }

        private string FormatStatusText(string status, bool includeUserName)
        {
            if (includeUserName)
            {
                return status;
            }
            else
            {
                try
                {

                    string[] parts = status.Split('-');
                    return parts[0] + " - " + parts[2];
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
        public void UpdateInvoiceFileStatus(int associateId, string invoiceFileId, MR_DAL.Enumerations.InvoiceFileStatus fileStatus, int invoiceId, string comment, bool sendEmail, string attachments)
        {
            string details = fileStatus.ToString() + " - " + CurrentUser.Identity.Name + " - " + DateTime.Now.ToShortDateString();
            string plainText = HtmlEntity.DeEntitize(comment);

            if (comment == string.Empty)
            {
                comment = CurrentUser.Identity.Name + " - " + DateTime.Now.ToShortDateString();
            }

            this.timeSheetRepository.UpdateInvoiceFileStatus(new Guid(invoiceFileId), fileStatus, details);

            // send email(s)                
            if (fileStatus == InvoiceFileStatus.Rejected)
            {
                if (sendEmail)
                {
                    var invoiceNotification = this.emailService.GetInvoiceEmail(invoiceId,
                        EmailTemplate.InvoiceRejection);
                    invoiceNotification.Body = comment;
                    List<MomentaRecruitment.Common.Services.EmailAttachment> rejectionattachments = new List<MomentaRecruitment.Common.Services.EmailAttachment>();

                    if (attachments != null && attachments != "")
                    {
                        MomentaRecruitment.Common.Services.EmailAttachment emaila;
                        string[] emailattachments = null;
                        emailattachments = attachments.Split(',');
                        foreach (string ea in emailattachments)
                        {
                            if (ea != "")
                            {
                                emaila = new MomentaRecruitment.Common.Services.EmailAttachment();
                                emaila.DocumentId = new Guid(ea);
                                rejectionattachments.Add(emaila);
                            }
                        }
                    }

                    this.emailService.SendEmail(invoiceNotification, rejectionattachments);

                }
            }

            var files = this.timeSheetRepository.GetInvoiceFiles(invoiceId);
            var file = files.FirstOrDefault(f => f.InvoiceFileId == new Guid(invoiceFileId));

            // add to history
            this.timeSheetRepository.AddInvoiceHistory(new InvoiceHistory
            {
                AssociateId = associateId,
                InvoiceId = invoiceId,
                InvoiceStatusId = (int)InvoiceStatus.Invoiced,
                LoggedInUser = CurrentUser.Identity.Name,
                Comments = "Invoice File " + fileStatus.ToString() + ": " + file.FileName
            });
        }

        public IEnumerable<InvoiceCommentViewModel> GetInvoiceComments(int invoiceId)
        {
            var results = new List<InvoiceCommentViewModel>();
            var comments = this.timeSheetRepository.GetInvoiceComments(invoiceId);

            foreach (var comment in comments)
            {
                var vm = new InvoiceCommentViewModel
                {
                    InvoiceId = invoiceId,
                    InvoiceCommentId = comment.InvoiceCommentId,
                    CommentDate = (DateTime)comment.CommentDate,
                    UserName = comment.UserName,
                    Comment = comment.Comment
                };

                results.Add(vm);
            }

            return (List<InvoiceCommentViewModel>)results;
        }

        public string GetCompanyName(int invoiceId)
        {
            return this.timeSheetRepository.GetCompanyName(invoiceId);
        }

        public void CreateInvoiceComment(int invoiceId, string userName, string comment)
        {
            this.timeSheetRepository.CreateInvoiceComment(invoiceId, userName, comment);
        }

        public void ProcessInvoice(int invoiceId, int timesheetStatus, int invoiceStatus, decimal vatRate, bool isRetainer = false)
        {
            try
            {
                var timesheets = this.timeSheetRepository.GetTimeSheetsByInvoiceId(invoiceId).ToList();

                var invoice = this.GetInvoice(invoiceId);

                var associate = this.timesheetAssociateService.GetAssociate<AssociateModel>(invoice.AssociateId);

                if (!isRetainer)
                {
                    // check all timesheets have been completed
                    if (timesheets.Any(timesheet => timesheet.Status != "Checked")) // changed from "Complete"
                    {
                        throw new Exception("Unable to process invoice as there are incomplete timesheets.");
                    }
                }

                // only check if associate is not self billing
                if (!associate.IsSelfBilling)
                {
                    // check invoice has file
                    var files = this.GetInvoiceFiles(invoiceId);
                    if (!invoice.SelfBilling.GetValueOrDefault() && (files == null || files.Count() == 0))
                    {
                        throw new Exception("Unable to process invoice as it doesn't have an invoice file.");
                    }

                    // check uploaded invoice has been checked                
                    bool fileChecked = false;
                    foreach (var file in files)
                    {
                        fileChecked |= (file.InvoiceFileStatus == InvoiceFileStatus.Checked.ToString());
                    }

                    if (!fileChecked)
                    {
                        throw new Exception("Unable to process invoice as the invoice file has not been checked.");
                    }

                }

                var invoicesReportData = GetInvoicesReportData(invoiceId);
                var totalAmount = invoicesReportData.Sum(invoiceReportViewModel => invoiceReportViewModel.Total);
                var vatAmount = totalAmount * vatRate;

                this.timeSheetRepository.ProcessInvoice(invoiceId, timesheetStatus, invoiceStatus, totalAmount, vatAmount);

                foreach (var item in timesheets)
                {
                    string Details = "Invoice Processed";
                    this.timeSheetRepository.CreateTimesheetHistory(item.TimeSheetId, TimesheetStatus.Processed, Details, item.AssociateId, item.ManagerId);
                }

                // add to history
                this.timeSheetRepository.AddInvoiceHistory(new InvoiceHistory
                {
                    AssociateId = invoice.AssociateId,
                    InvoiceId = invoiceId,
                    InvoiceStatusId = (int)InvoiceStatus.Processed,
                    LoggedInUser = CurrentUser.Identity.Name,
                    Comments = "Invoice Processed"
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<InvoiceHistory> GetInvoiceHistory(int invoiceId)
        {
            return this.timeSheetRepository.GetInvoiceHistory(invoiceId);
        }

        public bool ValidateInvoiceRetainerTimesheets(int associateid, int[] timesheetIds)
        {
            var timesheets = this.timeSheetRepository.GetTimeSheetsFromList(timesheetIds);

            var unapprovedTimesheets = (from t in timesheets
                                        where t.TimeSheetStatusId != (int)TimesheetStatus.Approved
                                        select t.TimeSheetId);

            return (unapprovedTimesheets.Count() == 0);
        }

        public bool ValidateInvoiceTimeSheets(int associateId, int[] timesheetIds)
        {
            var timesheetsWeekly = this.timeSheetRepository.GetTimeSheetsFromList(timesheetIds).Where(t => t.TimeSheetTypeId.Equals((int)TimesheetTypeId.Weekly));


            var unapprovedTimesheets = (from t in timesheetsWeekly
                                        where t.TimeSheetStatusId < (int)TimesheetStatus.Approved
                                        select t.TimeSheetId);

            if (unapprovedTimesheets.Count() > 0)
            {
                return false;
            }

            var roleIds = (from t in timesheetsWeekly
                           select t.RoleId).Distinct().ToArray<int>();
            if (roleIds.Count() > 1)
            {
                return false;
            }
            var isValid = true;

            foreach (int roleId in roleIds)
            {
                var allRoleTimeSheets = this.timeSheetRepository.GetUnInvoicedTimeSheetsForAssociateRole(associateId, roleId).Where(o => o.TimeSheetTypeId.Equals((int)TimesheetTypeId.Weekly));

                var roleTimeSheets = from t in timesheetsWeekly
                                     where t.RoleId == roleId
                                     select t;
                var unapprovedTimesheets1 = (from t in allRoleTimeSheets
                                             where t.TimeSheetStatusId < (int)TimesheetStatus.Approved
                                             select t.TimeSheetId);
                var fourExpected = (allRoleTimeSheets.Count() >= 4);
                int maxExpected = (fourExpected ? 4 : allRoleTimeSheets.Count());

                isValid &= this.ValidateRoleTimeSheets(roleTimeSheets, fourExpected, maxExpected, unapprovedTimesheets1.Count(), associateId, timesheetIds, allRoleTimeSheets);

            }

            return isValid;
        }

        private bool ValidateRoleTimeSheets(IEnumerable<TimeSheet> timesheets, bool fourExpected, int maxExpected, int unApproved, int associateId,
             int[] timesheetIds, IEnumerable<TimeSheet> allUnInvtimesheets)
        {
            var uninvoicedTimeSheets = allUnInvtimesheets;
            var isValid = true;
            var roleExpired = false;
            Role role = this.timesheetAssociateService.GetRole((timesheets.ToList())[0].RoleId);
            if (DateTime.Compare(DateTime.Now.AddDays(28), role.EndDate) < 0) //role expired=false
            {
                roleExpired = false;
            }
            else
            {
                if (uninvoicedTimeSheets.Count() > 4)
                    roleExpired = false;
                else
                    roleExpired = true;
            }

            if (timesheets.Count() > 0 && timesheets.Count() == maxExpected && fourExpected == true)
            {
                isValid = true;
            }
            else if (timesheets.Count() > 0 && timesheets.Count() < maxExpected && fourExpected == true)
            {
                if (unApproved == 0)
                    isValid = true;
                else
                    isValid = false;
            }
            else
            { isValid = timesheets.Count() > 0 && timesheets.Count() <= maxExpected; }


            if (isValid == true)
            {
                if (roleExpired == false)
                {
                    var orderedUnInvTimeSheets = uninvoicedTimeSheets.OrderBy(o => o.StartDate).ToList<TimeSheet>();
                    isValid = false;
                    for (int intRow = 0; intRow < orderedUnInvTimeSheets.Count(); intRow = intRow + 4)
                    {
                        var firstStartDate = orderedUnInvTimeSheets[intRow].StartDate;
                        var lastStartDate = orderedUnInvTimeSheets[intRow].StartDate;
                        if ((intRow + 3) > (orderedUnInvTimeSheets.Count() - 1))
                            lastStartDate = orderedUnInvTimeSheets[orderedUnInvTimeSheets.Count() - 1].StartDate;
                        else
                            lastStartDate = orderedUnInvTimeSheets[intRow + 3].StartDate;

                        var approvedTimesheetsPacket = uninvoicedTimeSheets.Where(o => o.StartDate >= firstStartDate && o.StartDate <= lastStartDate && o.TimeSheetStatusId == (int)TimesheetStatus.Approved).ToList<TimeSheet>();
                        if (approvedTimesheetsPacket.Count == 4)
                        {
                            var unSelApprTimesheets = approvedTimesheetsPacket.Select(o => o.TimeSheetId).ToArray().Except(timesheetIds);
                            if (unSelApprTimesheets.Count() > 0 && unSelApprTimesheets.Count() == maxExpected)
                            {
                                continue;
                            }
                            else if (unSelApprTimesheets.Count() > 0 && unSelApprTimesheets.Count() < maxExpected)
                            {
                                isValid = false;
                                break;
                            }
                            else
                            {
                                //if all r selected, then continue
                                isValid = true;
                                break;
                            }
                        }


                    }



                    var orderedTimesheets = timesheets.OrderBy(o => o.StartDate).Take(4);

                    var timeSheetsArr = orderedTimesheets as TimeSheet[] ?? orderedTimesheets.ToArray();
                    var firstTimesheet = timeSheetsArr.First();
                    var fourthTimesheet = timeSheetsArr.Last();


                    if (isValid == true)
                    {
                        //Checking in the same duration whether adjustment timesheets or not
                        var adjtimesheets = this.timeSheetRepository.GetTimeSheetsByStatus(TimesheetStatus.Approved)
                            .Where(
                                t => t.AssociateId.Equals(associateId)
                                    && t.RoleId.Equals((timesheets.ToList())[0].RoleId)
                                    && t.TimeSheetTypeId.Equals((int)TimesheetTypeId.Adjustment) &&
                                    t.StartDate >= firstTimesheet.StartDate && t.StartDate <= fourthTimesheet.StartDate)
                            .OrderBy(o => o.TimeSheetId);
                        //if there are no timesheets then proceed
                        if (!adjtimesheets.Any())
                        {
                            isValid = true;
                        }
                        else
                        {
                            //if adj timesheets exists in that duration, then check whether they selected or not
                            int[] adjTimesheetIds = (from t in adjtimesheets
                                                     select t.TimeSheetId).ToArray();


                            var adjTimesheetsNotInSelAdjTimesheets = adjTimesheetIds.Except(timesheetIds);
                            //if not selected then, return false, so that user can get message to select all the respective adj timesheets also
                            if (adjTimesheetsNotInSelAdjTimesheets.Count() > 0)
                            { isValid = false; }
                            else
                            {
                                //if all r selected, then continue
                                isValid = true;
                            }
                        }

                    }
                }
            }
            return isValid;
        }

        public void GenerateScheduleRetainer()
        {
            var result = this.timeSheetRepository.GenerateScheduleRetainer();
            MarkRetainer(result);
        }

        private int GetApproverIdForTimeSheet(int timeSheetId)
        {
            return this.timeSheetRepository.GetApproverIdForTimeSheet(timeSheetId);
        }

        public T GetHistoricIndividualValue<T>(int individualId, string field, DateTime date, T defaultValue, IEnumerable<IndividualChanges> indobj = null)
        {
            return this.timeSheetRepository.GetHistoricIndividualValue<T>(individualId, field, date, defaultValue, indobj);
        }
        public IEnumerable<IndividualChanges> GetHistoricIndividualChanges(int individualId)
        {
            return this.timeSheetRepository.GetHistoricIndividualChanges(individualId);
        }

        public T GetHistoricAssociateValue<T>(int associateId, string field, DateTime date, T defaultValue, IEnumerable<AssociateChanx> indobj = null)
        {
            return this.timeSheetRepository.GetHistoricAssociateValue<T>(associateId, field, date, defaultValue, indobj);
        }
        public IEnumerable<AssociateChanx> GetHistoricAssociateChanges(int associateId)
        {
            return this.timeSheetRepository.GetHistoricAssociateChanges(associateId);
        }
        public TimesheetViewModel GetTimeSheetEntry(int timesheetId)
        {

            if (timesheetId == default(int))
            {
                throw new ArgumentException("Invalid timesheetId");
            }

            int tmsId = timesheetId;
            IEnumerable<TimeSheetEntry> timesheetEntry = null;
            IEnumerable<Document> documents = null;
            Associate associate = null;
            IEnumerable<Absence> absence = null;
            TimesheetHistory[] histories = null;
            IEnumerable<ExpensesRecieptDetails> reInfo = null;
            TimeSheet timesheet = null;
            TimesheetValidationDetail validationDetails = null;
            // fill object with data from database.
            PopulateObjects(tmsId, ref timesheetEntry, ref documents, ref associate, ref absence, ref histories, ref reInfo, ref timesheet, ref validationDetails);

            //create view model.
            var timesheetVM = new TimesheetViewModel
            {
                TimesheetAssociate = this.GetAssociateNew(validationDetails, timesheet, associate),
                TimesheetId = timesheet.TimeSheetId,
                Status = ((TimesheetStatus)timesheet.TimeSheetStatusId).ToString(),
                AssociateId = validationDetails.AssociateId.GetValueOrDefault(),
                TimesheetApproverAssociateId = validationDetails.TimeSheetApproverAssociateId.HasValue ? validationDetails.TimeSheetApproverAssociateId.Value : 0,
                TimesheetApproverId = validationDetails.TimeSheetApproverId.HasValue ? validationDetails.TimeSheetApproverId.Value : 0,
                CurrentUserId = AssociateId,
                IncentiveDays = timesheet.IncentiveDay.HasValue ? timesheet.IncentiveDay.Value : 0,
                RoleId = timesheet.RoleId,
                ProjectId = timesheet.ProjectId,
                TimeSheetTypeId = timesheet.TimeSheetTypeId.Value
            };

            // get timsheet histiory
            this.GetHistoryNew(ref timesheetVM, histories);

            
            // get manadatory details for reciept uploading.
            this.GetRecieptInfoNew(ref timesheetVM, timesheet.RoleId, reInfo);

            // getting document loaded.
            this.GetDocumentsNew(ref timesheetVM, documents);

            // build timesheet object.
            this.MapTimeSheet(ref timesheetVM, validationDetails);

            validationDetails.TimesheetTypeId = timesheet.TimeSheetTypeId.GetValueOrDefault();

            // this create a timesheet entry from monday till sunday . this handle a switch statement in old code. 
            var tsEntryMaker = new TimeSheetEntryMaker(validationDetails, timesheetEntry, absence, timeSheetRepository, timesheet.TimeSheetTypeId.Value);


            timesheetVM.Entries = tsEntryMaker.MakeTimesheetEntries();
            List<string> ExpenseReceiptExcept = new List<string>();
            if (timesheetVM.AccomodationLimit == 0)
            {
                ExpenseReceiptExcept.Add("Accommodation");
                //timesheetVM.ExpenseReceiptDetails = timesheetVM.ExpenseReceiptDetails.Except(ExpenseReceiptExcept.AsEnumerable());

            }
            if (timesheetVM.MealAllowanceLimit == 0)
            {
                ExpenseReceiptExcept.Add("Meal Allowance");
            }
            if (timesheetVM.MileageLimit == 0)
            {
                ExpenseReceiptExcept.Add("Travel");
            }
            if (timesheetVM.NoOfMilesLimit == 0)
            {
                ExpenseReceiptExcept.Add("Mileage (rate per mile)");
            }
            if (timesheetVM.TravelLimit == 0)
            {
                ExpenseReceiptExcept.Add("Parking");
            }
            if (ExpenseReceiptExcept.Count > 0)
            {
                timesheetVM.ExpenseReceiptDetails = timesheetVM.ExpenseReceiptDetails.Except(ExpenseReceiptExcept.AsEnumerable());
            }
            //var attendances = timesheetVM.Entries[0].Attendences;

            //for(int intEntryRow=0; intEntryRow<timesheetVM.Entries.Count(); intEntryRow++)
            //{

            //    timesheetVM.Entries[intEntryRow].Attendences = attendances;
            //}

            //timesheetVM.Entries[0].IsOvertime = true;

            return timesheetVM;
        }


        public void CreateTimesheetEntry()
        {


            // var tsEntryMaker = new TimeSheetEntryMaker(validationDetails, timesheetEntry, absence, timeSheetRepository);

        }




        public void printTimer(Stopwatch watch, string source)
        {
#if DEBUG
            watch.Stop();
            var ts = watch.Elapsed;
            var elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Debug.WriteLine("Timesheet " + source + " : " + elapsedTime);
#endif
        }

        private void GetDocumentsNew(ref TimesheetViewModel timesheet, IEnumerable<Document> documents)
        {
            //var documents = this.timeSheetRepository.GetTimesheetDocuments(timesheet.TimesheetId);


            timesheet.Documents = documents.Select(
                doc =>
                    new TimeSheetDocumentViewModel()
                    {
                        DocumentId = doc.DocumentId,
                        Title = doc.Title,
                        TimesheetId = doc.TimeSheetId,
                        Date = doc.Date.ToShortDateString(),
                        Pass = doc.Pass,
                        Comment = doc.Comment,
                        Username = doc.Username
                    }).ToArray();
        }

        private AssociateDetails GetAssociateNew(TimesheetValidationDetail individual, TimeSheet timeSheet, Associate associate)
        {
            var invoiceData = this.timeSheetRepository.GetInvoiceByTimesheetId(timeSheet.TimeSheetId);
            bool varHasBeenVatRegistered = false;
            string companyName = "";
            DateTime varCheckDate=timeSheet.StartDate;
            if (timeSheet.TimeSheetStatusId < (int)TimesheetStatus.Submitted)
            {
                varCheckDate = (DateTime)DateTime.Now;
            }
            else
            {
                varCheckDate = (DateTime)timeSheet.StartDate;
            }
            if (invoiceData == null)
            {
                 companyName = this.timeSheetRepository.GetCompanyNameByTimesheet(timeSheet.TimeSheetId);
                
                    
            }
            else if (invoiceData.InvoiceId == 0)
            {
                companyName = this.timeSheetRepository.GetCompanyNameByTimesheet(timeSheet.TimeSheetId);

            }
            else
            {
                companyName=this.GetCompanyName(invoiceData.InvoiceId);
                var invoice = this.GetInvoice(invoiceData.InvoiceId);
                varCheckDate = (DateTime)invoice.InvoiceCreatedDate;
             
            }
            varHasBeenVatRegistered = this.GetHistoricAssociateValue<bool>(associate.ID, "HasBeenVatRegistered", varCheckDate, associate.HasBeenVatRegistered, null);
            if (companyName == null)
            {
                string umbrellaCompany = "";
                if (associate.UmbrellaCompanyId > 0)
                    umbrellaCompany = associateRepository.GetUmbrellaCompany(associate.UmbrellaCompanyId).Name;

                companyName = associate.RegisteredCompanyName;

                if (associate.BusinessTypeID.GetValueOrDefault() == (int)MR_DAL.Enumerations.BusinessType.Umbrella)
                {
                    companyName = umbrellaCompany;
                }
            }

            companyName = string.IsNullOrEmpty(companyName) ? string.Empty : string.Format("Company - {0}", companyName);

            var vatRegistration = varHasBeenVatRegistered== true
                ? "VAT Registered - Yes"
                : "VAT Registered - No";

            return new AssociateDetails()
            {
                AssociateName = string.Concat(associate.FirstName, " " + associate.LastName),
                ClientName = individual.ClientName,
                Manager = individual.TimeSheetApprover,
                ProjectName = individual.ProjectName,
                Role = individual.RoleName,
                RoleId = individual.RoleId,
                Rate = individual.AssociateRate.HasValue ? individual.AssociateRate.Value : default(decimal),
                WeekStartDate = timeSheet.StartDate.ToShortDateString(),
                WeekEndDate = timeSheet.EndDate.ToShortDateString(),
                IsAgencyOrUmbrella = individual.IsAgencyOrUmbrella == 1 || timeSheet.AssociateId != base.AssociateId ? true : false,
                VATRegistration = vatRegistration,
                CompanyName = companyName
            };
        }

        private void GetHistoryNew(ref TimesheetViewModel timesheet, TimesheetHistory[] histories)
        {
            var aName = timesheet.TimesheetAssociate.AssociateName;

            //timesheet.History = this.timeSheetRepository.GetTimesheetHistory(timesheet.TimesheetId)
            timesheet.History = histories
                .Select(
                history =>
                    new TimesheetHistoryViewModel()
                    {
                        Associate = history.TimeSheetStatusId == 8 ? "Momenta" : history.Name,
                        Comment = history.Comment,
                        Date = history.Date.ToShortDateString(),
                        Status = ((TimesheetStatus)history.TimeSheetStatusId).ToString(),
                        Time = history.Date.ToShortTimeString()
                    }).Take(10).ToArray();
        }

        private void GetRecieptInfoNew(ref TimesheetViewModel timesheet, int roleId, IEnumerable<ExpensesRecieptDetails> details)
        {
            //var reInfo = timeSheetRepository.GetRecieptInfo(roleId);
            var reInfo = details;
            //timesheet.ExpenseReceiptDetails = reInfo.SelectMany(x => new List<Dictionary<string, bool>>() { new Dictionary<string, bool>() { { x.Expense, x.Receipt == 1 ? true : false } } }).AsEnumerable();
            timesheet.ExpenseReceiptDetails = reInfo.Where(x => x.Receipt.GetValueOrDefault() == true).Select(x => x.Expense).AsEnumerable();
        }

        private void PopulateObjects(int tmsId, ref IEnumerable<TimeSheetEntry> timesheetEntry, ref IEnumerable<Document> documents, ref Associate associate, ref IEnumerable<Absence> absence, ref TimesheetHistory[] histories, ref IEnumerable<ExpensesRecieptDetails> reInfo, ref TimeSheet timesheet, ref TimesheetValidationDetail validationDetails)
        {
            Associate _associate = null;
            IEnumerable<Absence> _absence = null;
            TimesheetHistory[] _histories = null;
            IEnumerable<ExpensesRecieptDetails> _reInfo = null;
            try
            {
                Task<TimeSheet> timesheetTask = Task<TimeSheet>.Factory.StartNew((Id) =>
                {
                    var id = (int)Id;
                    var tms = timeSheetRepository.GetAllTimesheetData(id);
                    return tms;
                }, tmsId).ContinueWith<TimeSheet>((x) =>
                {
                    _absence = this.timeSheetRepository.GetThisWeekAbsence(x.Result.AssociateId, x.Result.StartDate.Date);
                    _reInfo = timeSheetRepository.GetRecieptInfo(x.Result.RoleId, x.Result.AssociateId);
                    return x.Result;
                });

                Task<TimesheetValidationDetail> validationDetailsTask = Task<TimesheetValidationDetail>.Factory.StartNew((Id) =>
                {
                    var id = (int)Id;
                    var temp = timeSheetRepository.GetTimeSheetAssignRoleDetail(id);
                    _histories = this.timeSheetRepository.GetTimesheetHistory(id).ToArray();
                    return temp;
                }, tmsId).ContinueWith<TimesheetValidationDetail>((x) =>
                {
                    _associate = timeSheetRepository.GetAssociatebyId(x.Result.AssociateId.Value);
                    return x.Result;
                });

                Task.WaitAll(new Task[] { timesheetTask, validationDetailsTask });
                timesheet = timesheetTask.Result;
                validationDetails = validationDetailsTask.Result;
                timesheetEntry = timesheetTask.Result.TimeSheetEntries;
                documents = timesheetTask.Result.Document;
                associate = _associate;

                if (timesheet.TimeSheetStatusId == (int)TimesheetStatus.Blank)
                {
                    absence = _absence;
                }
                else
                {
                    absence = new List<Absence>();
                }
                histories = _histories;
                reInfo = _reInfo;

            }
            catch (Exception)
            {

                throw;
            }

        }

        public bool BilledToClient(string[] timsheetIds)
        {
            try
            {
                foreach (var item in timsheetIds)
                {
                    timeSheetRepository.UpdateTimesheetStatusWithNoHistory(Convert.ToInt32(item), TimesheetStatus.BilledToClient);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int? GetInvoiceFileStatus(Guid id)
        {
            return this.timeSheetRepository.GetInvoiceFileStatus(id);
        }

        public void UpdateInvoiceListStatus(int[] ids, InvoiceStatus status)
        {
            foreach (var id in ids)
            {
                this.timeSheetRepository.UpdateInvoiceStatus(id, status);
            }
        }

        public bool SaveTimeSheetWithoutStatus(TimesheetViewModel model)
        {
            bool status;
            try
            {
                foreach (var entry in model.Entries)
                {
                    if (entry.IsDay || model.Status == TimesheetStatus.Blank.ToString())
                    {
                        this.timeSheetRepository.UpdateTimeSheetEntry(this.UnMap(entry));
                    }
                }
                this.timeSheetRepository.UpdateTimeSheet(new TimeSheet() { TimeSheetId = model.TimesheetId, IncentiveDay = model.IncentiveDays });
                status = true;
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                status = false;
            }
            return status;
        }

        private static List<TimeSheet> TimeSheetPeriod(IEnumerable<TimeSheet> timesheets)
        {
            var startDate = DateTime.Now.AddDays(-7);
            return timesheets.Where(t => (DateTime.Compare(t.StartDate, startDate) >= 0)).ToList();
        }
        private static List<TimeSheet> ExtendedTimeSheetPeriod(IEnumerable<TimeSheet> timesheets)
        {


            var daysOfWeek = new[]{
                    "Monday",
                    "Tuesday",
                    "Wednesday",
                    "Thursday",
                    "Friday",
                    "Saturday",
                    "Sunday"
                };

            var today = DateTime.Now.DayOfWeek.ToString();
            //// days since Monday plus 1
            var daysLapsedSinceMonday = Array.FindIndex(daysOfWeek, day => day.Contains(today)) + 1;
            //// if today is Friday daysLapsedSinceMonday = 5, therefore startdate = Sunday (five days ago)
            var startDate = DateTime.Now.AddDays((daysLapsedSinceMonday * -1));
            return timesheets.Where(t => (DateTime.Compare(t.StartDate, startDate) <= 0)).ToList();
        }
        private static List<TimeSheet> ExtendedTimeSheetPeriodWeeks(IEnumerable<TimeSheet> timesheets, int weeks)
        {


            var daysOfWeek = new[]{
                    "Monday",
                    "Tuesday",
                    "Wednesday",
                    "Thursday",
                    "Friday",
                    "Saturday",
                    "Sunday"
                };

            var today = DateTime.Now.DayOfWeek.ToString();
            //// days since Monday plus 1
            var daysLapsedSinceMonday = Array.FindIndex(daysOfWeek, day => day.Contains(today));
            //+ 1;
            //// if today is Friday daysLapsedSinceMonday = 5, therefore startdate = Sunday (five days ago)
            var startDate = DateTime.Now.AddDays((daysLapsedSinceMonday * -1));
            startDate = startDate.AddDays(-7 * weeks);
            var endDate = startDate.AddDays(6);
            return timesheets.Where(t => t.StartDate >= startDate && t.StartDate <= endDate).ToList();
        }
        private static decimal ConvertToHourFraction(decimal overtime)
        {
            var timeString = overtime.ToString(CultureInfo.InvariantCulture);
            var timeparts = timeString.Split('.');
            return Convert.ToDecimal(timeparts[0]) + Convert.ToDecimal(timeparts[1]) / 60;
        }
    }
}