using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamProductionPlan
{
    /// <summary>
    /// 台座管理类
    /// </summary>
    public class PedestalManage
    {
        /// <summary>
        /// 私有台座管理类构造函数
        /// </summary>
        public PedestalManage()
        {

        }

        /// <summary>
        /// 制定生产计划
        /// </summary>
        /// <param name="orderDetailList">订单详情列表</param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool MakePlan(List<OrderDetail> orderDetailList, ref string message)
        {
            List<PedestalWorkPlan> pedestalWorkPlanList = new List<PedestalWorkPlan>();//新生成的台座工作计划List
            List<PedestalWorkPlan> pedestalNotProducedWorkPlanList = new List<PedestalWorkPlan>();//未生产的台座工作计划List
            List<PedestalWorkPlan> pedestalRemoveWorkPlanList = new List<PedestalWorkPlan>();//需要移除的台座工作计划List
            List<Pedestal> pedestalExistList = new List<Pedestal>();  //当前已经使用的台座列表，用来过滤数据
            List<PedestalDic> pedestalDicList = new List<PedestalDic>();//台座字典信息List
            List<OrderDetail> orderDetailNoProducedList = new List<OrderDetail>();//未生产的订单List

            if (!GetPedestalDicList(ref pedestalDicList))
            {
                message = "下单失败，台座类型、工序等字典信息不能为空";
                //todo log
                return false; 
            }

            //状态为未生产的工作计划List
            GetPedestalWorkPlanListByStatus(BeamProductionPlanStatus.NotProduced, ref pedestalNotProducedWorkPlanList);
            //状态为未生产的订单详情List
            GetOrderDetailList("未生产", ref orderDetailNoProducedList);
            if (!orderDetailNoProducedList.IsEmpty())
            {
                List<string> orderDetailNos = orderDetailNoProducedList.Select(g => g.OrderDetailNo).ToList();//找到所有未生产的订单详情编号list
                if (!orderDetailNos.IsEmpty())
                {
                    var removeList = pedestalNotProducedWorkPlanList.Where(f => orderDetailNos.Contains(f.OrderDetailNo));
                    pedestalRemoveWorkPlanList.AddRange(removeList);//需要移除的台座工作计划
                }
                orderDetailList.AddRange(orderDetailNoProducedList);//加入所有未生产的订单DetailList，生成新的orderDetailList
            }

            foreach (var orderItem in orderDetailList.OrderBy(f => f.DeliveryTime))
            {
                if (!pedestalDicList.IsEmpty())
                {
                    foreach (var pedestalDicItem in pedestalDicList)
                    {
                        PedestalScheduleWork(orderItem, pedestalDicItem, ref pedestalExistList, ref pedestalWorkPlanList, ref message);
                    }
                }
            }

            if(!UpdatePlanData(pedestalRemoveWorkPlanList, pedestalWorkPlanList))
            {
                message = "下单失败！";
                return false;
            }
            message = "下单成功！";
            return true;
          
        }

        /// <summary>
        /// 台座工作计划
        /// </summary>
        /// <param name="orderDetail">订单详情</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="pedestalDic">台座字典信息</param>
        /// <param name="pedestalExistList">当前已经使用的台座列表，用来过滤数据</param>
        /// <param name="pedestalWorkPlanList">台座工作计划List</param>
        /// <param name="message">信息</param>
        /// <returns></returns>
        public bool PedestalScheduleWork(OrderDetail orderDetail, PedestalDic pedestalDic, ref List<Pedestal> pedestalExistList, ref List<PedestalWorkPlan> pedestalWorkPlanList, ref string message)
        {
            Pedestal pedestalItem = new Pedestal();//台座Item
            DateTime startTime = DateTime.MinValue;//台座开始时间
            DateTime endTime = DateTime.MinValue;//台座结束时间
            DateTime lastProductTime = DateTime.MinValue;//最晚生产时间
            PedestalWorkPlan pedestalWorkPlanItem = null;//台座工作计划Item
            PedestalWorkPlan firstPedestalWorkPlanItem = null;//台座工作计划中结束时间最早的台座
            string pedestalId;//台座Id
            string pedestalNumber;//台座编号
            string pedestalType = pedestalDic.PedestalType;//台座类型
            List<NoWorkDay> noWorkDayList = new List<NoWorkDay>();//非工作日期列表

            GetNoWorkDayList(ref noWorkDayList);//获取非工作日列表

            startTime = CheckCreateTime(orderDetail.CreateTime);
            if (pedestalDic.PedestalType == "预制")
            {
                startTime = startTime.AddDays(0.5);
            }

            lastProductTime = GetBackTime(orderDetail.DeliveryTime, pedestalDic.BackwardDays, noWorkDayList);

            if (!CanProduct(startTime, lastProductTime, ref message)) //没有时间安排生产
            {
                return false;
            }

            if (GetFreePedestalByPedestalType(pedestalDic.PedestalType, pedestalExistList, ref pedestalItem))//有对应台座类型的空闲台座，并去除掉这个台座
            {
                pedestalId = pedestalItem.PedestalId;
                pedestalNumber = pedestalItem.PedestalNumber;

                pedestalExistList.Add(pedestalItem);//添加已使用台座
            }
            else//没有对应台座类型的空闲台座,寻找结束时间最早的台座
            {
                firstPedestalWorkPlanItem = GetFirstWorkPlanPedestal(pedestalWorkPlanList, pedestalDic.PedestalType);

                startTime = CheckCreateTime(firstPedestalWorkPlanItem.EndTime);
                if (!CanProduct(startTime, lastProductTime, ref message)) //没有时间安排生产
                {
                    return false;
                }
                pedestalId = firstPedestalWorkPlanItem.PedestalId;
                pedestalNumber = firstPedestalWorkPlanItem.PedestalNumber;
            }
            endTime = GetEndTime(startTime, pedestalDic.ProcessDays, noWorkDayList);//增加对应工序所需天数

            pedestalWorkPlanItem = CreatePedestalWorkPlanItem(orderDetail.OrderNo, orderDetail.OrderDetailNo, orderDetail.BeamNo, orderDetail.BeamName, pedestalId, pedestalNumber, pedestalType, startTime, endTime);

            pedestalWorkPlanList.Add(pedestalWorkPlanItem);//添加台座计划

            return true;
        }

        /// <summary>
        /// 根据开始时间，累加工序天数，获取结束时间(根据非工作日期列表增加天数，计算结束时间)
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="processDays">工序天数</param>
        /// <param name="noWorkDayList">非工作日列表</param>
        /// <returns></returns>
        public DateTime GetEndTime(DateTime startTime, double processDays, List<NoWorkDay> noWorkDayList)
        {
            var endTime = startTime.AddDays(processDays);//增加对应工序所需天数
            if (!noWorkDayList.IsEmpty())
            {
                for (DateTime dt = startTime.Date; dt <= endTime.Date; dt = dt.AddDays(1))
                {
                    if (noWorkDayList.Any(f => f.NoWorkDate.Date == dt))
                    {
                        endTime = endTime.AddDays(1);
                    }
                }
            }
            return endTime;
        }

        /// <summary>
        /// 根据台座类型寻找当前空闲台座
        /// </summary>
        /// <param name="pedestalType">台座类型</param>
        /// <param name="pedestalExistList">当前已经使用的台座列表，用来过滤数据</param>
        /// <param name="pedestalItem">找到的台座</param>
        /// <returns>是否有空闲台座 true false</returns>
        public bool GetFreePedestalByPedestalType(string pedestalType, List<Pedestal> pedestalExistList, ref Pedestal pedestalItem)
        {
            List<PedestalWorkPlan> pedestalInProductWorkPlanList = new List<PedestalWorkPlan>();//状态为生产中的生产计划
            GetPedestalWorkPlanListByStatus(BeamProductionPlanStatus.InProduction, ref pedestalInProductWorkPlanList);

            var pedestalWorkPlanList = pedestalInProductWorkPlanList.Where(f => f.PedestalType == pedestalType).ToList();//根据台座类型筛选数据

            List<Pedestal> pedestalList = GetPedestalList(pedestalType);//获取对应类型台座列表
            if (!pedestalList.IsEmpty())
            {
                if (!pedestalExistList.IsEmpty())
                {
                    var pedestalIds = pedestalExistList.Select(f => f.PedestalId);
                    pedestalList.RemoveAll(f => pedestalIds.Contains(f.PedestalId));//移除已使用的台座
                }
                if (!pedestalList.IsEmpty())
                {
                    if (!pedestalWorkPlanList.IsEmpty())
                    {
                        var pedestalIds = pedestalWorkPlanList.Select(f => f.PedestalId);
                        pedestalList.RemoveAll(f => pedestalIds.Contains(f.PedestalId));//对应状态台座列表移除计划中生产中的台座
                    }
                    pedestalItem = pedestalList.FirstOrDefault();//找到第一个空闲台座
                }
            }

            return !pedestalList.IsEmpty();
        }

        /// <summary>
        /// 寻找工作计划中结束时间最早的台座
        /// </summary>
        /// <param name="pedestalWorkPlanList">新生成的台座工作计划List</param>
        /// <param name="pedestalType">台座类型</param>
        /// <returns>台座工作计划Item</returns>
        public PedestalWorkPlan GetFirstWorkPlanPedestal(List<PedestalWorkPlan> pedestalWorkPlanList, string pedestalType)
        {
            List<PedestalWorkPlan> pedestalInProductWorkPlanList = new List<PedestalWorkPlan>();//状态为生产中的生产计划List
            GetPedestalWorkPlanListByStatus(BeamProductionPlanStatus.InProduction, ref pedestalInProductWorkPlanList);

            if (!pedestalInProductWorkPlanList.IsEmpty())
            {
                pedestalInProductWorkPlanList.AddRange(pedestalWorkPlanList);
            }

            var list = from a in pedestalInProductWorkPlanList
                       group a by a.PedestalId into b
                       select new
                       {
                           PedestalId = b.Key,
                           EndTime = b.Max(a => a.EndTime)
                       };

            var item = list.OrderBy(f => f.EndTime).FirstOrDefault();

            PedestalWorkPlan pedestalWorkPlanItem = pedestalInProductWorkPlanList.FirstOrDefault(f => f.PedestalId == item.PedestalId && f.EndTime == item.EndTime);

            return pedestalWorkPlanItem;
        }

        /// <summary>
        /// 根据提货时间倒推最晚生产时间(根据非工作日期列表计算)
        /// </summary>
        /// <param name="deliveryTime">提货时间</param>
        /// <param name="days">工序所需天数</param>
        /// <param name="noWorkDayList">非工作日列表</param>
        /// <returns>最晚生产时间</returns>
        public DateTime GetBackTime(DateTime deliveryTime, double days, List<NoWorkDay> noWorkDayList)
        {
            DateTime lastedProductTime = new DateTime(deliveryTime.Year, deliveryTime.Month, deliveryTime.Day);//提货日期当天0时（即前一天12:00:00 PM）

            lastedProductTime = lastedProductTime.AddDays(-days);

            if (!noWorkDayList.IsEmpty())
            {
                for (DateTime dt = lastedProductTime.Date; dt.Date <= deliveryTime.Date; dt = dt.AddDays(1))
                {
                    if (noWorkDayList.Any(f => f.NoWorkDate.Date == dt.Date))
                    {
                        lastedProductTime = lastedProductTime.AddDays(-1);
                    }
                }
            }
            return lastedProductTime;
        }

        /// <summary>
        /// 比较是否有时间安排生产
        /// </summary>
        /// <param name="productTime">计划安排生产时间</param>
        /// <param name="lastProductTime">最晚生产时间</param>
        /// <returns></returns>
        public bool CanProduct(DateTime productTime, DateTime lastProductTime, ref string message)
        {
            var compare = productTime.CompareTo(lastProductTime);
            if (compare <= 0)
            {
                return true;
            }
            else
            {
                message = "计划安排生产时间大于所需要最晚生产时间，无法进行生产";
                return false;
            }
        }

        /// <summary>
        /// 创建台座工作计划Item
        /// </summary>
        /// <param name="orderNo">订单编号</param>
        /// <param name="beamNo">梁片编号</param>
        /// <param name="beamName">梁片名称</param>
        /// <param name="pedestalId">台座Id</param>
        /// <param name="pedestalNumber">台座编号</param>
        /// <param name="pedestalType">台座类型</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public PedestalWorkPlan CreatePedestalWorkPlanItem(string orderNo, string orderDetailNo, string beamNo, string beamName, string pedestalId, string pedestalNumber, string pedestalType, DateTime startTime, DateTime endTime)
        {
            PedestalWorkPlan pedestalWorkPlanItem = null;//台座工作计划Item
            pedestalWorkPlanItem = new PedestalWorkPlan()
            {
                Id = Guid.NewGuid().ToString(),
                OrderNo = orderNo,
                OrderDetailNo = orderDetailNo,
                PedestalId = pedestalId,
                PedestalNumber = pedestalNumber,
                PedestalType = pedestalType,
                BeamNo = beamNo,
                BeamName = beamName,
                //Status = "未生产",
                StartTime = startTime,
                EndTime = endTime
            };
            return pedestalWorkPlanItem;
        }

        /// <summary>
        /// 校验创建时间,生成开始时间
        /// </summary>
        /// <param name="CreateTime">创建时间</param>
        /// <returns>startTime台座计划开始时间</returns>
        public DateTime CheckCreateTime(DateTime CreateTime)
        {
            DateTime startTime = DateTime.MinValue;
            if (CreateTime.Hour < 8)
            {
                startTime = new DateTime(CreateTime.Year, CreateTime.Month, CreateTime.Day);
            }
            else if (CreateTime.Hour > 14)
            {
                startTime = new DateTime(CreateTime.AddDays(1).Year, CreateTime.AddDays(1).Month, CreateTime.AddDays(1).Day);
            }
            else
            {
                startTime = new DateTime(CreateTime.Year, CreateTime.Month, CreateTime.Day, 12, 0, 0);
            }
            return startTime;
        }

        #region 数据库操作

        /// <summary>
        /// 获取非工作日期列表（数据库操作）
        /// </summary>
        /// <param name="noWorkDayList"></param>
        /// <returns></returns>
        public bool GetNoWorkDayList(ref List<NoWorkDay> noWorkDayList)
        {
            return true;
        }

        /// <summary>
        /// 获取台座字典信息List(数据库操作)
        /// </summary>
        /// <param name="pedestalDicList"></param>
        /// <returns></returns>
        public bool GetPedestalDicList(ref List<PedestalDic> pedestalDicList)
        {
            return true;
        }

        /// <summary>
        /// 获取所有订单详情表中状态是未生产的数据（数据库操作）
        /// </summary>
        /// <param name="orderDetailList"></param>
        /// <returns></returns>
        public bool GetOrderDetailList(string status,ref List<OrderDetail> orderDetailList)
        {
            return true;
        }

        /// <summary>
        /// 根据台座类型获取台座列表(数据库操作)
        /// </summary>
        /// <param name="pedestalType">台座类型</param>
        /// <returns></returns>
        public List<Pedestal> GetPedestalList(string pedestalType)
        {
            List<Pedestal> pedestalList = new List<Pedestal>();
            //todo
            return pedestalList;
        }

        /// <summary>
        /// 根据计划状态获取台座工作计划List(数据库操作)
        /// </summary>
        /// <param name="status">计划状态</param>
        /// <param name="pedestalWorkPlanList"></param>
        /// <returns></returns>
        public bool GetPedestalWorkPlanListByStatus(BeamProductionPlanStatus status, ref List<PedestalWorkPlan> pedestalWorkPlanList)
        {
            return true;
        }

        /// <summary>
        /// 更新计划数据（事务数据存储）
        /// </summary>
        /// <param name="pedestalRemoveWorkPlanList">需要移除的台座工作计划</param>
        /// <param name="pedestalWorkPlanList">新生成的台座工作计划List</param>
        /// <returns></returns>
        public bool UpdatePlanData(List<PedestalWorkPlan> pedestalRemoveWorkPlanList, List<PedestalWorkPlan> pedestalWorkPlanList)
        {
            return true;
        }

        #endregion
    }
}
