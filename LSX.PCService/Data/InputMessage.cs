using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSX.PCService.Data
{

    public enum MessageCmd
    {
        Add,
        Exit
    }
    public enum InputType{
        Camera, //箱号识别
        PDA,   //手持端扫描绑定LPN
        ChannelController //流水线控制
    }
    [Serializable]
    public class InputMessage
    {
        public MessageCmd cmd { get; set; }
        public InputType type { get; set; }

        /// <summary>
        /// 箱号
        /// </summary>
        public string caseNum { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int count { get; set; }

        public int channelId { get; set; }
    }
}
