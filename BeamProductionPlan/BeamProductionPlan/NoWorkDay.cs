using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamProductionPlan
{
    public class NoWorkDay
    {
        public string Id { get; set; }

        /// <summary>
        /// 非工作日期
        /// </summary>
        public DateTime NoWorkDate { get; set; }
    }
}
