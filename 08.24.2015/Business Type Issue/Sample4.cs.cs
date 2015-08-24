namespace Admin.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Admin.Enumerations;
    using MR_DAL;
    using MomentaRecruitment.Common.Models;

    public interface ISchedulerRepository : MomentaRecruitment.Common.Repositories.ISchedulerRepository
    {
        IQueryable<TaskSearch> GetTaskSearch();
        TaskModel GetUpcomingChangeForField(int individualId, string field);
    }

    public class SchedulerRepository : MomentaRecruitment.Common.Repositories.SchedulerRepository, ISchedulerRepository
    {
        public IQueryable<TaskSearch> GetTaskSearch()
        {
            return this.MomentaDb.TaskSearch;
        }

        public TaskModel GetUpcomingChangeForField(int individualId, string field)
        {
            var tasks = (from t in this.MomentaDb.Task
                         join i in this.MomentaDb.IndividualTask
                         on t.TaskId equals i.TaskId
                         where i.IndividualId == individualId
                         && t.TaskName.ToLower() == field.ToLower()
                         select t);

            if (tasks.Count() > 0)
            {
                var task = tasks.OrderByDescending(t => t.StartDate).ThenByDescending(x => x.TaskId).First();

                return new TaskModel
                {
                    Arguments = task.Arguments,
                    AuthenticationScheme = task.AuthenticationScheme,
                    Created = task.Created,
                    EndDate = (DateTime)task.EndDate,
                    ModifiedBy = task.ModifiedBy,
                    PeriodLength = task.PeriodLength,
                    PeriodTypeId = task.PeriodTypeId,
                    StartDate = (DateTime)task.StartDate,
                    TaskId = task.TaskId,
                    TaskName = task.TaskName,
                    Url = task.Url
                };
            }
            else
            {
                return null;
            }

        }
    }
}