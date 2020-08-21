using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamProductionPlan
{
    /// <summary>
    /// 台座工作生产计划类
    /// </summary>
    public class PedestralWorkPlan
    {
        public int Id { get; set; }
        
        /// <summary>
        /// 台座编号
        /// </summary>
        public string PedestralNo { get; set; }

        /// <summary>
        /// 台座类型
        /// </summary>
        public string PedestralType { get; set; }
        
        /// <summary>
        /// 梁片编号
        /// </summary>
        public string BeamNo { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

    }
}
