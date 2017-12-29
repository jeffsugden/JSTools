using JS.Tools.UnitOfWork;
using System;

namespace JS.Tools.Sql
{
    // Simple Wrapper over TransactionManager
    public sealed class SimpleUnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly TransactionManager _transactionManager;

        public SimpleUnitOfWork(SqlManager sqlManager)
        {
            _transactionManager = sqlManager.CreateTransactionManager(); // Start Transx Manager Since a unit of work begins on creation
        }

        public void SaveChanges()
        {
            _transactionManager.Complete();
        }

        public void Dispose()
        {
            _transactionManager.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    
}
