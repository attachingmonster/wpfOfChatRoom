using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpfOfChatRoom.ViewModels
{
    public class ViewModelChangePsw
    {
        /// <summary>
        /// 修改密码界面的信息
        /// </summary>
        public String Account { get; set; }

        public String OldPassword { get; set; }
        public String NewPassword { get; set; }
        public String SurePassword { get; set; }
    }
}
