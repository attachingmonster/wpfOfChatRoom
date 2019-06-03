using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfOfChatRoom.ViewModels
{
    /// <summary>
    /// 聊天记录，来自webapi；；这里的命名最好能够联系上这个意思
    /// </summary>
    public class ViewModelDataReturn
    {
        /// <summary>
        /// 用户账号
        /// </summary>
        public string UserAccount { get; set; }
        /// <summary>
        /// 用户发送的消息
        /// </summary>
        public string Message { get; set; }
    }
}
