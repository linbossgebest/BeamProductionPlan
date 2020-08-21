using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamProductionPlan
{
    /// <summary>
    /// 订单详情类
    /// </summary>
    public class OrderDetail
    {
        public int Id { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 桥梁名称
        /// </summary>
        public string BeamName { get; set; }

        /// <summary>
        /// 梁片编号
        /// </summary>
        public string BeamNo { get; set; }

        /// <summary>
        /// 提货时间
        /// </summary>
        public DateTime DeliveryTime { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
    }
}
