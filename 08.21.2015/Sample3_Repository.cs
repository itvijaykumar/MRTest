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

    using Elmah;

    using ITRIS_DAL;
    using ITRIS_DAL.Enumerations;

    using MomentaRecruitment.Common.Enumerations;
    using MomentaRecruitment.Common.Helpers;
    using MomentaRecruitment.Common.Helpers.ExtensionMethods;
    using MomentaRecruitment.Common.Models;
    using MomentaRecruitment.Common.ViewModel;

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

    public interface IIndividualRepository : IDisposable
    {
        IndividualModel GetIndividual(int id);
        Individual GetIndividual(int associateId, int roleId);
        List<IndividualModel> GetIndividualsByAssociateId(int associateId, int pageSize, int skip, List<SortDescription> sort, bool getPastRoles, out int? resultsCount);
        void UpdateIndividual(IndividualModel model);
        IEnumerable<PerformanceClause> GetPerformanceClauses(int roleId);
        PerformanceClause GetPerformanceClause(int id);
        void UpdateScheduleField(int TaskId, string PropertyName, string value);
        IEnumerable<int> GetIndividualIds(int taskId);
        IEnumerable<string> GetScheduledChangeFields(int individualId);
        void BackDateIndividualChanges(int individualId, string field, string value, DateTime changeDate);
        ScheduledTaskViewModel GetTask(int taskId);
    }
    
    public class IndividualRepository : BaseRepository, IIndividualRepository
    {
        private const string GetByIdStoredProc = "GetIndividual";
        private const string GetByAssociateStoredProc = "";
        private const string CreateStoredProc = "";
        private const string UpdateStoredProc = "";

        public IndividualModel GetIndividual(int id)
        {
            var data = this.MomentaDb.GetIndividual(id).First();
            return IndividualModel.PopulateIndividualModel(data);
        }

        public Individual GetIndividual(int associateId, int roleId)
        {
            return this.MomentaDb.Individual.FirstOrDefault(i => i.AssociateId == associateId && i.RoleId == roleId);            
        }

        public List<IndividualModel> GetIndividualsByAssociateId(int associateId, int pageSize, int skip, List<SortDescription> sort, bool getPastRoles, out int? resultsCount)
        {
            var dir = (sort != null && sort.Count > 0 ? sort[0].dir : null);
            var field = (sort != null && sort.Count > 0 ? sort[0].field : null);
            int page = (int)(skip / pageSize) + 1;
            
            var results = this.MomentaDb.GetPaginatedIndividualsByAssociate(pageSize, page, associateId, dir, field, getPastRoles).ToList();
            var returnValue = CastPaginatedIndividualsToList(results);
            
            resultsCount = (returnValue.Count > 0 ? results.FirstOrDefault().Cnt : 0);

            return returnValue;
        }

        public void UpdateIndividual(IndividualModel model)
        {
            DateTime? retentionStartDate;

            if (model.RetentionStartDate == null)
            {
                retentionStartDate = null;
            }
            else
            {
                retentionStartDate = Convert.ToDateTime(model.RetentionStartDate);
            }

            this.MomentaDb.UpdateIndividual(
                model.IndividualId,
                model.StartDate,
                model.EndDate,
                model.FullMomentaRate,
                model.AssociateRate,
                model.Retention,
                model.RetentionPeriodId,
                model.RetentionCharge,
                model.RetentionPayAway,
                model.TimeSheetApproverId,
                model.Monday,
                model.MondayOvertime,
                model.Tuesday,
                model.TuesdayOvertime,
                model.Wednesday,
                model.WednesdayOvertime,
                model.Thursday,
                model.ThursdayOvertime,
                model.Friday,
                model.FridayOvertime,
                model.Saturday,
                model.SaturdayOvertime,
                model.Sunday,
                model.SundayOvertime,
                model.WorkingHours,
                model.OverTimePayAway,
                model.OverTimePayRatio,
                model.IncentiveDaysMaxWorked,
                model.IncentiveDaysCountedAs,
                //model.AdditionalCaseRate,                
                model.OneOffPmtAmount,
                model.OneofPaymentDate,
                model.ExpenseAccomodation,
                model.ExpenseSubsistence,
                model.ExpenseTravel,
                model.ExpenseParking,
                model.ExpenseMileage,
                model.ExpenseOther,
                model.Suspended,
                model.TravelFullDay,
                model.CancelFullDay,
                model.TravelHalfDay,
                model.CancelHalfDay,
                model.NoticeAmount,
                model.NoticeIntervalId,
                model.Hourly,
                model.OverproductionCharge,
                model.OverproductionPayAway,
                retentionStartDate,
                model.OverTimeCharge
                );

            this.MomentaDb.ResetAuditModifiedTime(model.AssociateId, model.RoleId, null);

            this.UpdatePerformanceClauses(model);
        }

        public void UpdatePerformanceClauses(IndividualModel individual)
        {
            // clear
            this.MomentaDb.ClearIndividualPerformanceClause(individual.IndividualId);

            // loop through and add
            foreach (var clause in individual.PerformanceClauses)
            {
                this.MomentaDb.SetIndividualPerformanceClause(individual.IndividualId, clause.PerformanceClauseId);
            }
        }

        public IEnumerable<PerformanceClause> GetPerformanceClauses(int individualId)
        {
            return this.MomentaDb.GetPerformanceClausesForIndividual(individualId);
        }

        public PerformanceClause GetPerformanceClause(int id)
        {
            return this.MomentaDb.PerformanceClauses.Where(c => c.PerformanceClauseId == id).FirstOrDefault();
        }

        private List<IndividualModel> CastPaginatedIndividualsToList(IEnumerable<GetPaginatedIndividualsByAssociate_Result> result)
        {
            return result.Select(i => new IndividualModel
            {
                IndividualId = i.IndividualId,
                AssociateId = i.AssociateId,
                ClientId = i.ClientId,
                ClientName = i.ClientName,
                ProjectId = i.ProjectId,
                RoleId = i.RoleId,
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                FullMomentaRate = i.FullMomentaRate,
                AssociateRate = i.AssociateRate,
                Retention = i.Retention,
                RetentionPeriodId = i.RetentionPeriodId,
                RetentionCharge = i.RetentionCharge,
                RetentionPayAway = i.RetentionPayAway,
                TimeSheetApproverId = (i.TimeSheetApproverId == null ? 0 : (int)i.TimeSheetApproverId),
                Monday = i.Monday,
                MondayOvertime = i.MondayOvertime,
                Tuesday = i.Tuesday,
                TuesdayOvertime = i.TuesdayOvertime,
                Wednesday = i.Wednesday,
                WednesdayOvertime = i.WednesdayOvertime,
                Thursday = i.Thursday,
                ThursdayOvertime = i.ThursdayOvertime,
                Friday = i.Friday,
                FridayOvertime = i.FridayOvertime,
                Saturday = i.Saturday,
                SaturdayOvertime = i.SaturdayOvertime,
                Sunday = i.Sunday,
                SundayOvertime = i.SundayOvertime,
                WorkingHours = i.WorkingHours,
                OverTimePayAway = i.OverTimePayAway,
                OverTimePayRatio = i.OverTimePayRatio,
                IncentiveDaysMaxWorked = i.IncentiveDaysMaxWorked,
                IncentiveDaysCountedAs = i.IncentiveDaysCountedAs,                                
                OneOffPmtAmount = i.OneOffPmtAmount,
                OneofPaymentDate = i.OneofPaymentDate,
                ExpenseAccomodation = i.ExpenseAccomodation,
                ExpenseSubsistence = i.ExpenseSubsistence,
                ExpenseTravel = i.ExpenseTravel,
                ExpenseParking = i.ExpenseParking,
                ExpenseMileage = i.ExpenseMileage,
                ExpenseOther = i.ExpenseOther,
                Suspended = i.Suspended,
                ProjectName = i.ProjectName,
                TimeSheetApproverName = i.TimeSheetApproverName,
                RoleName = i.RoleName,
                TravelFullDay = i.TravelFullDay,
                CancelFullDay = i.CancelFullDay,
                TravelHalfDay = i.TravelHalfDay,
                CancelHalfDay = i.CancelHalfDay,
                NoticeIntervalId = i.NoticeIntervalId,
                NoticeAmount = i.NoticeAmount,                
                Hourly = i.Hourly,
                OverproductionCharge = i.OverProductionCharge,
                OverproductionPayAway = i.OverProductionPayaway,
                RetentionStartDate = (i.RetentionStartDate == null ? "" : ((DateTime)i.RetentionStartDate).ToShortDateString())
            }).ToList<IndividualModel>();
        }


        public void UpdateScheduleField(int TaskId, string PropertyName, string value)
        {
            if (value.Trim() == string.Empty)
            {
                // assign to null if value is empty string
                this.MomentaDb.UpdateIndividualFieldSingle1(PropertyName, null, TaskId.ToString());
            }
            else
            {
                this.MomentaDb.UpdateIndividualFieldSingle1(PropertyName, value, TaskId.ToString());
            }            
        }

        public IEnumerable<int> GetIndividualIds(int taskId)
        {
            return this.MomentaDb.IndividualTask.Where(x => x.TaskId == taskId).Select(x=> x.IndividualId).AsEnumerable(); 
        }

        public IEnumerable<string> GetScheduledChangeFields(int individualId) 
        {
            var fields = new List<string>();

            var changes = from i in this.MomentaDb.IndividualTask
                          join t in this.MomentaDb.Task
                          on i.TaskId equals t.TaskId
                          where i.IndividualId == individualId
                          && t.StartDate > DateTime.Now
                          select t.TaskName.Trim().ToLower();
         
            return changes.Distinct();
        }

        public void BackDateIndividualChanges(int individualId, string field,string value, DateTime changeDate)
        {
            this.MomentaDb.BackDateIndividualChanges(individualId, field,value, changeDate);
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
