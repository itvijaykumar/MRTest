
using MomentaRecruitment.Common.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace MomentaRecruitment.Common.Services.Scheduler
{
    public class ScheduleParamsHandler : IScheduleParamsHandler
    {
        private string _jsonVal;
        private IIndividualRepository _individualRepository;
        private IAssociateRepository _associateRepository;
        private ScheduleEntryType _entryType;

        public ScheduleParamsHandler(string json, ScheduleEntryType entryType, IIndividualRepository individualRepository=null, IAssociateRepository associateRepository=null)
        {
            this._jsonVal = json;
            this._entryType = entryType;
            this._individualRepository = individualRepository;
            this._associateRepository = associateRepository;
        }

        public IEnumerable<GenericField> ConvertToObject()        
        {
            List<GenericField> jsonResult = SerialiseJson();
            if (_jsonVal.IndexOf("AssId")>-1)
            {
                return jsonResult;
            }
            else
            {
                // It should be only one taskId per actions.
                IEnumerable<int> list = _individualRepository.GetIndividualIds(jsonResult.First().TaskId);

                if (jsonResult != null && jsonResult.Count > 0 && (list == null || list.Count() == 0))
                    throw new SchedulerException("Task doesn't have associated individual id.");

                List<GenericField> output = new List<GenericField>();
                jsonResult.ForEach(x =>
                {
                    list.ToList().ForEach(y => output.Add(new GenericField() { IndId = y, Field = x.Field, Value = x.Value, TaskId = x.TaskId }));
                });
                return output;
            }
        }

        private List<GenericField> SerialiseJson()
        {
            var jss = new JavaScriptSerializer();
            if (_entryType == ScheduleEntryType.Mutiple)
            {
                return jss.Deserialize<List<GenericField>>(_jsonVal);
            }
            if (_entryType == ScheduleEntryType.Single)
            {
               
                Dictionary<string, object> result = jss.Deserialize<dynamic>(_jsonVal);
                List<GenericField> resultList = new List<GenericField>();
                if (_jsonVal.IndexOf("AssId") > -1)
                { 
                
                    resultList.Add(new GenericField()
                    {
                        AssId = Convert.ToInt32(result["AssId"]),
                        TaskId = Convert.ToInt32(result["taskId"]),
                        Field = result["field"].ToString(),
                        Value = result["value"].ToString()
                    });
                }
                else
                {
                  
                    resultList.Add(new GenericField()
                    {
                        AssId =  0 ,
                        TaskId = Convert.ToInt32(result["taskId"]),
                        Field = result["field"].ToString(),
                        Value = result["value"].ToString()
                    });
                }
                return resultList;
            }
            return null;
        }

    }
}
