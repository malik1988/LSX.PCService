using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSX.PCService.Interfaces
{
    /// <summary>
    /// AGV呼叫接口
    /// - 发送起始点给AGV调度系统
    /// - AGV调度系统返回订单发送成功/失败和实际运送任务订单号
    /// </summary>
    class CallAgvApi
    {
        /// <summary>
        /// 创建AGV调度任务
        /// </summary>
        /// <param name="palletOrder">托盘编号</param>
        /// <param name="start">起点库位</param>
        /// <param name="end">终点库位</param>
        public static void AgvCreateTask(string palletOrder,string  start, string end) {
            //将AGV调度生成的实际运送任务订单写入数据库表
        }
        /// <summary>
        /// 通知AGV调度更新交通灯状态
        /// </summary>
        /// <param name="lightColor">交通灯状态</param>
        public static void AgvUpdateTrafficLight(string lightColor)
        {
            //更新
        }
    }
}
