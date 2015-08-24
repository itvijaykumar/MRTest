using System.Web.Script.Serialization;
using AssociatePortal.Services;
using MR_DAL.Enumerations;

namespace Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using Admin.Services;
    using MomentaRecruitment.Common.ViewModel;

    public class SchedulerController : Controller
    {
        private ISchedulerService _schedulerService;
        private ITimesheetService _timesheetService;

        public SchedulerController(ISchedulerService schedulerService, ITimesheetService timesheetService)
        {
            _schedulerService = schedulerService;
            _timesheetService = timesheetService;
        }

        [HttpPost]
        public JsonResult CreateTask(int[] ids, DateTime scheduledDate, string field, string value)
        {
            try
            {

                if (field == "EndDate")
                {
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    Dictionary<string, object> dObj = jss.Deserialize<dynamic>(value);
                    var fieldValue = dObj["value"].ToString();
                    var endDate = DateTime.ParseExact(fieldValue, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    var result = this._schedulerService.ValidateEndDateforEndDate(ids, endDate);

                    if (!string.IsNullOrEmpty(result))
                    {
                        return this.Json(result);
                    }
                }

                int taskId = 0;

                switch(field.ToLower()) {
                    case "expenses":
                        taskId = this._schedulerService.CreateScheduleExpensesChange(scheduledDate, value, ids);
                        break;
                    case "incentives":
                        taskId = this._schedulerService.CreateScheduleIncentivesChange(scheduledDate, value, ids);
                        break;
                    case "rates":
                        taskId = this._schedulerService.CreateScheduleRatesChange(scheduledDate, value, ids);
                        break;
                    case "sage-export":
                        this._schedulerService.CreateScheduleSageExport(scheduledDate, ids);
                        _timesheetService.UpdateInvoiceListStatus(ids, InvoiceStatus.Scheduled);
                        break;
                    case "businesstypes":
                        taskId = this._schedulerService.CreateScheduleBusinessTypeChange(scheduledDate, value, ids);
                        break;
                    default:
                        taskId = this._schedulerService.CreateScheduledChange(scheduledDate, field, value, ids);
                        break;
                }

                return this.Json(true);
            }
            catch (Exception ex)
            {
                return this.Json(ex.Message);
            }
        }

        

        [HttpPost]
        public JsonResult GetTask(int taskId)
        {
            var result = this._schedulerService.GetTask(taskId);
            return this.Json(result);
        }

        [HttpPost]
        public JsonResult DeleteTask(int taskId)
        {
            var result = this._schedulerService.DeleteTask(taskId);
            return this.Json(result);
        }

        [HttpPost]
        public JsonResult GetUpcomingTasksForIndividual(int individualId)
        {
            IEnumerable<ScheduledTaskViewModel> results = this._schedulerService.GetUpcomingTasksForIndividual(individualId);                        
            return this.Json(results);
        }

        [HttpPost]
        public JsonResult GetUpcomingChangeForField(int individualId, string field)
        {
            var change = this._schedulerService.GetUpcomingChangeForField(individualId, field);
            return this.Json(change);
        }

        [HttpPost]
        public JsonResult GetTaskIndividuals(int taskId)
        {
            var results = this._schedulerService.GetAssociateNamesForTask(taskId);                          
            return this.Json(results);
        }
    }
}
