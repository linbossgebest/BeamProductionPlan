using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamProductionPlan
{
    /// <summary>
    /// 台座类
    /// </summary>
    public class Pedestral
    {
        /// <summary>
        /// 台座的ID
        /// </summary>
        /// <returns></returns>
        public string PedestalId { get; set; }
        /// <summary>
        /// 台座状态
        /// </summary>
        /// <returns></returns>
        public string PedestalStep { get; set; }
        /// <summary>
        /// 台座类型
        /// </summary>
        /// <returns></returns>
        public string PedestalType { get; set; }
        /// <summary>
        /// 台座编号
        /// </summary>
        /// <returns></returns>
        public string PedestalNumber { get; set; }
        /// 设备编号
        /// </summary>
        public string EquipmentNumber { get; set; }
        /// <summary>
        /// 负责人
        /// </summary>
        public string Princial { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 预计结束时间
        /// </summary>
        public DateTime? PlanEndTime { get; set; }
        /// <summary>
        /// 台座区域
        /// </summary>
        public string AreaseType { get; set; }
        /// <summary>
        /// 是否合格=1?"是":"否"
        /// </summary>
        public int IsConformity { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }
        /// <summary>
        /// 创建者id
        /// </summary>
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建的用户
        /// </summary>
        public string CreateUserName { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? ModifyDate { get; set; }
        /// <summary>
        /// 修改者id
        /// </summary>
        public string ModifyUserId { get; set; }
        /// <summary>
        /// 修改用户
        /// </summary>
        public string ModifyUserName { get; set; }
        /// <summary>
        /// 梁编号
        /// </summary>
        public string BridgeNumber { get; set; }
        /// <summary>
        /// 台座子类型
        /// </summary>
        public string ChildType { get; set; }
        //是否完成俯板操作
        public int? BottomColligation { get; set; }
        //是否完成顶板操作
        public int? TopColligation { get; set; }
        /// <summary>
        /// 梁id
        /// </summary>
        public string BridgeNumberTwo { get; set; }
        /// <summary>
        /// 梁场编号
        /// </summary>
        public int? GriderNumber { get; set; }
        /// <summary>
        /// 台座区域
        /// </summary>
        public string PostionNumber { get; set; }
        /// <summary>
        /// 排序码
        /// </summary>
        public int? SortCode { get; set; }
    }
}
