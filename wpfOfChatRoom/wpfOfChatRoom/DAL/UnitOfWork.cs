using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wpfOfChatRoom.Model;
using wpfOfChatRoom.Repositoties;

namespace wpfOfChatRoom.DAL
{
    public class UnitOfWork : IDisposable
    {
        private AccountContext context = new AccountContext();
        private GenericRepository<Data> dataRepository;
        public GenericRepository<Data> DataRepository
        {
            get
            {
                if (this.dataRepository == null)
                {
                    this.dataRepository = new GenericRepository<Data>(context);
                }
                return dataRepository;
            }
        }
        #region Save & Dispose
        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
