using System.Collections.ObjectModel;

namespace LSX.PCService.Controllers
{
    /// <summary>
    /// 从CWMS获取订单
    /// </summary>
    class CwmsOrderHelper
    {
       /// <summary>
       /// 从CWMW中获取发货单对应的数据并保存到数据库中（发货单任务表）
       /// </summary>
       /// <param name="orders"></param>
 
        public static void DownloadOrders(Collection<string> orders)
        {
     
           // TODO: 

            //调用API获取所有发货单订单数据
            //将所有订单写入数据库中

        }


    }
}
