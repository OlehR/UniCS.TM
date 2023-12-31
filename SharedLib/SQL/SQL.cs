using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using ModelMID;
using Utils;

namespace SharedLib
{
    public class SQL
    {
        protected string ConectionString;
        public eTypeCommit TypeCommit { get; set; }
        /// <summary>
        /// Чи можна користуватись базою
        /// </summary>
        protected  bool IsLock = false;
        public SQL(String varConectionString)
        {
            ConectionString = varConectionString;
     //       FileLogger.ExtLogForClassConstruct(GetType(), GetHashCode(), ConectionString);
        }

        ~SQL()
        {
           // FileLogger.ExtLogForClassDestruct( GetHashCode(), ConectionString);
        }
        public void SetLock(bool parIsLock)
        {
            IsLock = parIsLock;
        }

        public virtual void Close(bool isWait=false) 
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<T1> Execute<T, T1>(string query, T parameters)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<T1> Execute<T1>(string query)
        {
            throw new NotImplementedException();
        }
        public virtual Task<IEnumerable<T1>> ExecuteAsync<T, T1>(string query, T parameters)
        {
            throw new NotImplementedException();
        }

        public virtual Task<IEnumerable<T1>> ExecuteAsync<T1>(string query)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Для асинхронного коду не годиться
        /// </summary>
        public virtual void BeginTransaction()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Для асинхронного коду не годиться
        /// </summary
        public virtual void CommitTransaction() 
        {
            throw new NotImplementedException();
        }

        public virtual int ExecuteNonQuery<T>(string parQuery, T Parameters, int CountTry = 3)
        {
            throw new NotImplementedException();
        }
        public virtual int ExecuteNonQuery(string parQuery, int CountTry = 3)
        {
            throw new NotImplementedException();
        }

        public virtual Task<int> ExecuteNonQueryAsync<T>(string parQuery, T Parameters)
        {
            throw new NotImplementedException();
        }
        public virtual Task<int> ExecuteNonQueryAsync(string parQuery)
        {
            throw new NotImplementedException();
        }

        public virtual T1 ExecuteScalar<T1>(string query)
        {
            throw new NotImplementedException();
        }

        public virtual T1 ExecuteScalar<T, T1>(string query, T parameters)
        {
            throw new NotImplementedException();
        }

        public virtual Task<T1> ExecuteScalarAsync<T1>(string query)
        {
            throw new NotImplementedException();
        }

        public virtual Task<T1> ExecuteScalarAsync<T, T1>(string query, T parameters)
        {
            throw new NotImplementedException();
        }
        public virtual int BulkExecuteNonQuery<T>(string parQuery, IEnumerable<T> Parameters, bool IsRepeatNotBulk = false)
        {
            throw new NotImplementedException();
        }

        public virtual void ExceptionIsLock()
        {
            throw new Exception("SqlLite is Lock for FullUpdate");
        }
    }
}
