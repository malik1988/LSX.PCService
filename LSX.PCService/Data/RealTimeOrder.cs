using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSX.PCService.Data
{
    /// <summary>
    /// 实时订单
    /// </summary>
    
    [Serializable]
    class RealTimeOrder
    {
        public int Id { get; set; }
        public string Order { get; set; }
        public string Zncode { get; set; }
        public string Pallet { get; set; }
        public string Carton { get; set; }
        public int Num { get; set; }
        public int Channel { get; set; }
        public int Total { get; set; }
        public OrderState Status { get; set; }
    }
}
