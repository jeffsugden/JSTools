using System;
using System.Data;

namespace JS.Tools.Sql
{
	// This class implements IDispoable to clean-up the SQL Connections if there is an error somewhere
	// We create an instance of it in the business object
	// he will control/talk to the the SqlManger Class
	public sealed class TransactionManager : IDisposable
	{
		private readonly SqlManager _sqlManager;
        private bool _isScopeComplete = false;

        internal IsolationLevel? RequiredIsolationLevel { get; private set; }           
        		
		internal TransactionManager(SqlManager sqlManager, bool repeatableReadLevelRequired = false)
		{
            if (sqlManager == null) throw new ArgumentNullException("sqlManager was null");
            _sqlManager = sqlManager;

            if (repeatableReadLevelRequired)
                RequiredIsolationLevel = IsolationLevel.RepeatableRead;
            else
                RequiredIsolationLevel = null;

            _sqlManager.RegisterTransactionManagerScope(this);
		}
        		
		public void Complete()
		{
            if (_isScopeComplete) throw new InvalidOperationException("Transaction scope is already complete");
            _sqlManager.CompleteTransactionManagerScope(this);
            _isScopeComplete = true;
		}

		public void RollBack()
		{
            if (_isScopeComplete) throw new InvalidOperationException("Transaction scope is already complete");
            _sqlManager.CompleteTransactionManagerScope(this, rollback: true);
            _isScopeComplete = true;
		}

		public void Dispose()
		{			
            if (!_isScopeComplete)
            {
                RollBack();
            }
		}
	}
}