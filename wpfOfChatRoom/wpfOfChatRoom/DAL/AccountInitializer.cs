using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfOfChatRoom.Methods;
using wpfOfChatRoom.Model;

namespace wpfOfChatRoom.DAL
{
    class AccountInitializer :
          DropCreateDatabaseIfModelChanges<AccountContext>
    {
        protected override void Seed(AccountContext context)
        {
           
        }
    }
}
