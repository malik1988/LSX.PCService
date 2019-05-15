using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSX.PCService.Data
{
    class InputDataTypeCamera : InputMessage
    {
        public string 箱号 { get { return this.caseNum; } set { this.caseNum = value; } }
        public int 数量 { get { return this.count; } set { this.count = value; } }


      

    }
}
