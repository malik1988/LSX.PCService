using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LSX.PCService.Models;
using Chloe;
namespace LSX.PCService.Data
{
    /// <summary>
    /// 原始总捡单分析
    /// </summary>
    class OrderRawDataAnalyze
    {
     
        public static void Start()
        {
            string c="HE64";
          var rawList=  Db.Context.Query<ViewRawAnalyzed>().Where(a => a.车牌 == c).ToList();
            
        }
    }
}
