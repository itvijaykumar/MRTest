using MomentaRecruitment.Common.Models;
using MomentaRecruitment.Common.Repositories;
using MomentaRecruitment.Common.Services.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Transactions;

namespace MomentaRecruitment.Common.Services.Scheduler
{
    public class ScheduleActionHandler
    {        
        private string _jsonVal;
        private IScheduleParamsHandler _scheduleParamsHandler;
        ScheduleEntryType _entryType;
        private readonly IIndividualRepository _individualRepository;
        private readonly IAssociateRepository _associateRepository;        
        public bool Result { get; private set; }

        public ScheduleActionHandler(string jsonVal, ScheduleEntryType entryType, IIndividualRepository individualRepository=null, IAssociateRepository associateRepository=null)
        {
            
            _jsonVal = jsonVal;
            _individualRepository = individualRepository;
            _associateRepository = associateRepository;
            _entryType = entryType;
            _scheduleParamsHandler = new ScheduleParamsHandler(_jsonVal,entryType, individualRepository, associateRepository);
        }

        public ScheduleActionHandler(string jsonVal,ScheduleEntryType entryType, IIndividualRepository individualRepository, IAssociateRepository associateReposiory, IScheduleParamsHandler scheduleParamsHandler)
        {           
            _jsonVal = jsonVal;
            _entryType = entryType;
            _scheduleParamsHandler = scheduleParamsHandler;
            _individualRepository = individualRepository;
            _associateRepository = associateReposiory;
        }

        public void Handle()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    var fields = _scheduleParamsHandler.ConvertToObject();                 

                    foreach (var item in fields)
                    {
                        if (ValidateScheduleField(item.Field))
                        {
                            _individualRepository.UpdateScheduleField(item.IndId, item.Field, item.Value);                            
                        }
                        else
                        {
                            throw new SchedulerException(item.Field  + "is not scheduling property, please make sure field has SchedulerEnabled attribute.");
                        }

                        ViewModel.ScheduledTaskViewModel task = this._individualRepository.GetTask(item.TaskId);
                        if (task.StartDate.AddMinutes(5) < DateTime.Now)
                        {
                            this._individualRepository.BackDateIndividualChanges(item.IndId, item.Field,item.Value, task.StartDate);
                        }
                    }                                      

                    scope.Complete();                    
                    Result = true;
                }
                catch (Exception)
                {
                    
                    throw;
                }             
            }
        }

        public void AssociateChangeHandle()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    var fields = _scheduleParamsHandler.ConvertToObject();
                    ViewModel.ScheduledTaskViewModel previousTask=null;
                    foreach (var item in fields)
                    {
                        //if (ValidateAssociateScheduleField(item.Field))
                        //{
                        //    _associateRepository.UpdateScheduleField(item.AssId, item.Field, item.Value);
                        //}
                        //else
                        //{
                        //    throw new SchedulerException(item.Field + "is not scheduling property, please make sure field has SchedulerEnabled attribute.");
                        //}
                        _associateRepository.UpdateScheduleField(item.AssId, item.Field, item.Value);
                        ViewModel.ScheduledTaskViewModel task = null;

                        if (previousTask==null)
                        {
                            task = this._associateRepository.GetTask(item.TaskId);
                            previousTask = task;
                        }
                        else if (previousTask.TaskId!=item.TaskId)
                        {
                            task = this._associateRepository.GetTask(item.TaskId);
                            previousTask = task;
                        }
                        if (previousTask.StartDate.AddMinutes(5) < DateTime.Now)
                        {
                            this._associateRepository.BackDateAssociateChanges(item.AssId, item.Field, item.Value, item.TaskId);
                        }
                    }

                    scope.Complete();
                    Result = true;
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }
        private bool ValidateScheduleField(string fieldName)
        {
            PropertyInfo[] props = typeof(IndividualModel).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    SchedulerEnabled authAttr = attr as SchedulerEnabled;
                    if (authAttr != null)
                    {
                        if (prop.Name == fieldName)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool ValidateAssociateScheduleField(string fieldName)
        {
            PropertyInfo[] props = typeof(AssociateModel).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    SchedulerEnabled authAttr = attr as SchedulerEnabled;
                    if (authAttr != null)
                    {
                        if (prop.Name == fieldName)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
