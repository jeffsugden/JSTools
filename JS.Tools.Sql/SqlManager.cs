using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace JS.Tools.Sql
{
    public class SqlManager
    {
        // Connection Generation/Info
        private readonly DbProviderFactory _factory;
        private readonly string _connString;

        // Management for TransactionScopes (Logical UnitOfWork)
        internal bool ManagerTransactionInProgress { get; private set; }
        private readonly Stack<TransactionManager> _currentTransactionManagers = new Stack<TransactionManager>();
        private IsolationLevel? _currentTransactionIsolationLevel = null;
        private bool _transactionRollbackFlag = false;

        // Management for DbTransaction
        private IDbConnection _currentDbTransactionConnection = null;
        private IDbTransaction _currentDbTransaction = null;
        private bool _dbTransactionInProgress = false;

        public SqlManager(string connStringName)
        {
            var connStringSettings = ConfigurationManager.ConnectionStrings[connStringName];
            _factory = DbProviderFactories.GetFactory(connStringSettings.ProviderName);
            _connString = connStringSettings.ConnectionString;

            ManagerTransactionInProgress = false;
        }
        public SqlManager(string connectionString, string providerName)
        {            
            _factory = DbProviderFactories.GetFactory(providerName);
            _connString = connectionString;

            ManagerTransactionInProgress = false;
        }

        public ManagedConnection CreateManagedConnection(bool includeInTransaction = true)
        {
            // DbTransaction initalization is deferred until a ManagedConnection is created to avoid hitting the Db if there is no work being done though this Manager
            if (includeInTransaction && ManagerTransactionInProgress && !_dbTransactionInProgress)
            {
                BeginDbTransaction();
            }
            return new ManagedConnection(this, includeInTransaction);
        }

        public ManagedConnection CreateManagedConnectionAsync(bool includeInTransaction = true)
        {
            // DbTransaction initalization is deferred until a ManagedConnection is created to avoid hitting the Db if there is no work being done though this Manager
            if (includeInTransaction && ManagerTransactionInProgress && !_dbTransactionInProgress)
            {
                BeginDbTransaction();
            }
            return new ManagedConnection(this, includeInTransaction);
        }

        public TransactionManager CreateTransactionManager(bool repeatableReadLevelRequired = false)
        {
            return new TransactionManager(this, repeatableReadLevelRequired);
        }

        private DbConnection CreateDbConnection()
        {
            var connection = _factory.CreateConnection();
            connection.ConnectionString = _connString;
            return connection;
        }

        internal IDbConnection GetTransactionConnection()
        {
            return _currentDbTransactionConnection;
        }

        internal IDbConnection GetNonTransactionConnection()
        {
            return CreateDbConnection();
        }

        /* Gets the current active transaction object
       * null if no tranaction is in progress */
        internal IDbTransaction GetTransaction()
        {
            return _currentDbTransaction;
        }

        internal void RegisterTransactionManagerScope(TransactionManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager");            
            if (!ManagerTransactionInProgress)
            {
                BeginManagerTransaction(manager.RequiredIsolationLevel);
            }
            else
            {
                if (manager.RequiredIsolationLevel.HasValue && manager.RequiredIsolationLevel.Value != _currentTransactionIsolationLevel)
                {
                    throw new InvalidOperationException("Manager Transaction already running, unable to set a new IsolationLevel in TransactionManager");
                }
            }
            _currentTransactionManagers.Push(manager);
        }

        internal void CompleteTransactionManagerScope(TransactionManager manager, bool rollback = false)
        {
            if (manager == null) throw new ArgumentNullException("manager");
            if (_currentTransactionManagers.Peek() != manager) throw new InvalidOperationException("TransManger did not register, or has completed out of order"); // require nested scopes to complete order last started to first started
            _currentTransactionManagers.Pop(); // Remove from Manager Scope  
            if (rollback) // Set Current Transaction Rollback flag if needed
            {
                _transactionRollbackFlag = true;
            }
            if (ManagerTransactionInProgress && _currentTransactionManagers.Count == 0) // End ManagerTransaction if last Scope has left the list
            {
                if (_dbTransactionInProgress) // Only need to tell db if dbTransaction was initiated
                {
                    if (_transactionRollbackFlag)
                    {
                        _currentDbTransaction.Rollback();
                    }
                    else
                    {
                        _currentDbTransaction.Commit();
                    }
                    EndDbTransaction();
                }
                EndManagerTransaction();
            }
        }

        private void BeginManagerTransaction(IsolationLevel? isolationLevel)
        {
            if (ManagerTransactionInProgress)
            {
                throw new Exception("Manager Transaction is already in Progress");
            }
            ManagerTransactionInProgress = true;
            _currentTransactionIsolationLevel = isolationLevel;
            _transactionRollbackFlag = false;
        }

        private void EndManagerTransaction()
        {
            if (!ManagerTransactionInProgress) throw new InvalidOperationException("Manager does not have a Transaction in progress");            
            if (_dbTransactionInProgress) throw new InvalidOperationException("DbTransaction is still in progress, manager transaction cannot be ended");
            ManagerTransactionInProgress = false;
        }

        private void BeginDbTransaction()
        {
            if (_dbTransactionInProgress)
            {
                throw new Exception("Database Transaction is already in Progress");
            }

            // Create new DbConnection
            _currentDbTransactionConnection = CreateDbConnection();

            // Start DbTransaction
            if (_currentTransactionIsolationLevel.HasValue)
                _currentDbTransaction = _currentDbTransactionConnection.BeginTransaction(_currentTransactionIsolationLevel.Value);
            else
                _currentDbTransaction = _currentDbTransactionConnection.BeginTransaction();

            // Save State
            _dbTransactionInProgress = true;
        }

        private void EndDbTransaction()
        {
            if (_dbTransactionInProgress)
            {
                // Close the connection, clear tranansaction variable holders.
                // If tranaction was still pendening, it is autormatticlly rolled back when
                // we can dispose on the connection.
                _currentDbTransaction.Dispose();
                _currentDbTransactionConnection.Dispose();
                _currentDbTransaction = null;
                _currentDbTransactionConnection = null;
                _dbTransactionInProgress = false;
            }
            else
            {
                throw new Exception("There is not a Database Transaction in progress");
            }
        }
    }
}