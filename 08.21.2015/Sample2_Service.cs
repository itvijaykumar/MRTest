/*
 * Value JSON needs to include TaskId field place holder which the communications service will replace at execution time with the correct task Id
 */
#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.WebPages;

using Admin.Enumerations;
using MR_DAL.Enumerations;
using MomentaRecruitment.Common.ViewModel;
using MomentaRecruitment.Common.Models;
using MR_DAL.TimeSheet.Repository;
using IDataMapper = Admin.Helpers.IDataMapper;
using System.Web.Script.Serialization;
using Admin.Repositories;

#endregion

namespace Admin.Services
{

    public interface ISchedulerService : MomentaRecruitment.Common.Services.ISchedulerService
    {
        int CreateScheduledChange(DateTime startDate, string field, string value, IEnumerable<int> individualIds);

        int CreateScheduleExpensesChange(DateTime startDate, string value, IEnumerable<int> individualIds);

        int CreateScheduleIncentivesChange(DateTime startDate, string value, IEnumerable<int> individualIds);

        
        int CreateScheduleRatesChange(DateTime startDate, string value, IEnumerable<int> individualIds);

        int CreateScheduleBusinessTypeChange(DateTime startDate, string value, IEnumerable<int> associateIds);
        IEnumerable<ScheduledTaskViewModel>  GetUpcomingTasksForIndividual(int individualId);

        IEnumerable<IndividualModel>  GetIndividualsForTask(int taskId);

        IEnumerable<string> GetAssociateNamesForTask(int taskId);

        TaskModel GetUpcomingChangeForField(int individualId, string field);

        string ValidateEndDateforEndDate(int[] indIds, DateTime endDate);
        void CreateScheduleSageExport(DateTime scheduledDate, int[] ids);
    }

    public class SchedulerService : MomentaRecruitment.Common.Services.SchedulerService, ISchedulerService
    {
        #region Declarations/Constructors

        private IDataMapper _mapper;
        private ISchedulerRepository _schedulerRepository;
        private readonly ITimeSheetRepository timeSheetRepo;

        public SchedulerService(ISchedulerRepository schedulerRepository, IDataMapper mapper, ITimeSheetRepository timeSheetRepo)
            : base(schedulerRepository, mapper)
        {
            this._mapper = mapper;
            this._schedulerRepository = schedulerRepository;
            this.timeSheetRepo = timeSheetRepo;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Create a scheduled update to a single field
        /// </summary>
        /// <param name="startDate">Date to make update</param>
        /// <param name="field">Field being updated</param>
        /// <param name="value">JSON used to perform the change (field & value)</param>
        /// <returns>TaskID</returns>
        public int CreateScheduledChange(DateTime startDate, string field, string value, IEnumerable<int> individualIds)
        {
            return CreateScheduledTask(startDate, field, value, "Associate", "UpdateIndividualFieldSingle", individualIds, "I");
        }

        /// <summary>
        /// Create a scheduled update to expenses
        /// </summary>
        /// <param name="startDate">Date to make update</param>        
        /// <param name="value">JSON used to perform the change (fields & values)</param>
        /// <returns>TaskID</returns>
        public int CreateScheduleExpensesChange(DateTime startDate, string value, IEnumerable<int> individualIds)
        {
            return CreateScheduledTask(startDate, "Expenses", value, "Associate", "UpdateIndividualExpense", individualIds, "I");
        }

        /// <summary>
        /// Create a scheduled update to incentives
        /// </summary>
        /// <param name="startDate">Date to make update</param>        
        /// <param name="value">JSON used to perform the change (fields & values)</param>
        /// <returns>TaskID</returns>
        public int CreateScheduleIncentivesChange(DateTime startDate, string value, IEnumerable<int> individualIds)
        {
            return CreateScheduledTask(startDate, "Incentives", value, "Associate", "UpdateIndividualIncentive", individualIds, "I");
        }

        public int CreateScheduleRatesChange(DateTime startDate, string value, IEnumerable<int> individualIds)
        {
            return CreateScheduledTask(startDate, "Rates", value, "Associate", "UpdateIndividualRates", individualIds, "I");
        }

        public int CreateScheduleBusinessTypeChange(DateTime startDate, string value, IEnumerable<int> associateIds)
        {
            return CreateScheduledTask(startDate, "BusinessTypes", value, "Associate", "UpdateBusinessTypeData", associateIds, "A");
        }
        public void CreateScheduleSageExport(DateTime scheduledDate, int[] ids)
        {
            CreateScheduledTask(scheduledDate, "SageExport", "Sage", "Export", ids);
        }      

        public IEnumerable<ScheduledTaskViewModel> GetUpcomingTasksForIndividual(int individualId)
        {
            return this._schedulerRepository.GetUpcomingTasksForIndividual(individualId);
        }

        public IEnumerable<IndividualModel> GetIndividualsForTask(int taskId)
        {
            var individuals = this._schedulerRepository.GetIndividualsForTask(taskId);

            return this._mapper.MapIndividualsE2M(individuals);
        }

        public IEnumerable<string> GetAssociateNamesForTask(int taskId)
        {
            var names = this._schedulerRepository.GetAssociateNamesForTask(taskId);
            return names;
        }

        public TaskModel GetUpcomingChangeForField(int individualId, string field)
        {
            return this._schedulerRepository.GetUpcomingChangeForField(individualId, field);
        }

        public string ValidateEndDateforEndDate(int[] indIds, DateTime endDate)
        {
            return this._schedulerRepository.ValidateEndDateforEndDate(indIds, endDate);
        }
      
        #endregion

        #region Private Methods


        private void CreateScheduledTask(DateTime scheduledDate, string task, string controller, string method, int[] ids)
        {
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            string url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority;
            url += urlHelper.Action(method, controller);

            string authScheme = "TimeSheets";

            foreach (var id in ids)
            {
                this._schedulerRepository.CreateTask(ScheduledTaskPeriodType.OneTime, 0, scheduledDate, "SageExtract", string.Format("{0}/{1}",url,id), null, authScheme);
                this.timeSheetRepo.UpdateInvoiceStatus(id, InvoiceStatus.Exported);
            }
        }

        private int CreateScheduledTask(DateTime startDate, string field, string value, string controller, string method, IEnumerable<int> ids, string idType="")
        {
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            string url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority;
            url += urlHelper.Action(method, controller);

            string authScheme = "TimeSheets";

            int taskId = this._schedulerRepository.CreateTask(ScheduledTaskPeriodType.OneTime, 0, startDate, field, url, value, authScheme);
            if (idType == "A")
            {
                //nothing to do
            }
            else
            {
                foreach (int id in ids)
                {
                    this._schedulerRepository.CreateSchedulerIndividual(id, taskId);
                }
            }
            return taskId;
        }

        #endregion
       
    }
}