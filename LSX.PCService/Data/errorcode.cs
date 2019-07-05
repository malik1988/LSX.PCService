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
        文件导入_无效的Excel文件=0x10,
        文件导入_文件内容已经导入过,

        栈板分析_同一个栈板号存在不同的质检标志=0x20,

        LPN绑定_失败,
        LPN绑定_09码未集齐,

        

        按灯控制_无效灯ID=0x30,
        按灯控制_无可用灯,
        按灯控制_数据库中指定IP的灯不存在,
        按灯控制_发送灯后超时未收到应答,

        通道控制_设备离线=0x40,
        通道控制_重试3次后发送失败或超时未收到应答,



    }
}
