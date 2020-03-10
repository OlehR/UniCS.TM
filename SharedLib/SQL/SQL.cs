using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModelMID;

namespace SharedLib
{
    public class SQL
    {
        private string ConectionString;
        public eTypeCommit TypeCommit { get; set; }
        /// <summary>
        /// Чи можна користуватись базою
        /// </summary>
        protected bool IsLock = false;
        public SQL(String varConectionString)
        {
            ConectionString = varConectionString;            
        }

        public void SetLock(bool parIsLock)
        {
            IsLock = parIsLock;
        }

        public virtual void Close() {}

        public virtual IEnumerable<T1> Execute<T, T1>(string query, T parameters)
        {
            return null;
        }

        public virtual IEnumerable<T1> Execute<T1>(string query)
        {
            return null;
        }
        public virtual Task<IEnumerable<T1>> ExecuteAsync<T, T1>(string query, T parameters)
        {
            return null;
        }

        public virtual Task<IEnumerable<T1>> ExecuteAsync<T1>(string query)
        {
            return null;
        }

        public virtual void BeginTransaction()
        {            
        }

        public virtual void CommitTransaction() {}

        public virtual int ExecuteNonQuery<T>(string parQuery, T Parameters)
        {
            return 0;
        }
        public virtual int ExecuteNonQuery(string parQuery)
        {
            return 0;
        }

        public virtual Task<int> ExecuteNonQueryAsync<T>(string parQuery, T Parameters)
        {
            return null;
        }
        public virtual Task<int> ExecuteNonQueryAsync(string parQuery)
        {
            return null;
        }

        public virtual T1 ExecuteScalar<T1>(string query)
        {
            return default(T1);
        }

        public virtual T1 ExecuteScalar<T, T1>(string query, T parameters)
        {
            return default(T1);
        }

        public virtual Task<T1> ExecuteScalarAsync<T1>(string query)
        {
            return null;
           // default(T1);
        }

        public virtual Task<T1> ExecuteScalarAsync<T, T1>(string query, T parameters)
        {
            return null;
            //default(T1);
        }
        public virtual int BulkExecuteNonQuery<T>(string parQuery, IEnumerable<T> Parameters)
        {
            return 0;
        }

        //Async



    }
}
