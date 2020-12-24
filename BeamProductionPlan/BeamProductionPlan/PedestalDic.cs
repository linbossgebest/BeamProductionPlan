using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamProductionPlan
{
    /// <summary>
    /// 台座字典信息
    /// </summary>
    public class PedestalDic
    {
        public string Id { get; set; }

        /// <summary>
        /// 台座类型
        /// </summary>
        /// <returns></returns>
        public string PedestalType { get; set; }

        /// <summary>
        /// 工序时间
        /// </summary>
        public double ProcessDays { get; set; }

        /// <summary>
        /// 倒推时间
        /// </summary>
        public double BackwardDays { get; set; }
    }
}
