using System;
using System.Data;

namespace JS.Tools.Sql
{
	public sealed class ManagedConnection : IDisposable
	{
		public IDbConnection Connection { get; private set; }
		public IDbTransaction Transaction { get; private set; }
		private readonly bool _releaseOnDispose;

		internal ManagedConnection(SqlManager sqlManager, bool includeInTransaction = true)
		{
            if (includeInTransaction && sqlManager.ManagerTransactionInProgress)
			{
                _releaseOnDispose = false; // Leave clean-up for TransactionManger that is coordinating the trans via SqlManager
                Connection = sqlManager.GetTransactionConnection();
                Transaction = sqlManager.GetTransaction();
			}
			else
			{
                _releaseOnDispose = true; // Local-Connection only, we need to clean up on Dispose.
                Connection = sqlManager.GetNonTransactionConnection();
				Transaction = null; // Return null since this connection is not participating in the trans unitOfWork
			}			
		}			

		public void Dispose()
		{
			if (_releaseOnDispose)
			{
				if (Transaction != null)
				{
					Transaction.Dispose();
				}
				if (Connection != null)
				{
					Connection.Dispose();
				}
			}          
		}
	}
}