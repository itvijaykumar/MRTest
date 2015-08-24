using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MomentaRecruitment.Common.Services.Scheduler
{
    public class GenericField
    {
        public int AssId { get; set; }
        public int IndId { get; set; }
        public int TaskId { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
    }
}
