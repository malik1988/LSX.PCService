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
        public static void CallAgv(string start, string end) {
            //将AGV调度生成的实际运送任务订单写入数据库表
        }
    }
}
