using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSX.PCService.Data
{
    class AgvController:SingletonBase<AgvController>
    {
        private AgvController()
        {

        }

        internal void SendOrder(string source, string dest)
        {
            throw new NotImplementedException();
        }
    }
}
