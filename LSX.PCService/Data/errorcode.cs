using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSX.PCService.Data
{
    enum ErrorCode
    {
        成功=0,
        无效的Excel文件=0x10,
        

        按灯控制_无效灯ID=0x20,
        按灯控制_无可用灯,
        按灯控制_数据库中指定IP的灯不存在,
        按灯控制_发送灯后超时未收到应答,

    }
}
