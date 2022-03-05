﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SQLite;
using Dapper;
using ModelMID;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using System.Linq;

namespace SharedLib
{        
    public class SQLite:SQL,IDisposable
    {
        SQLiteConnection connection = null;
        SQLiteTransaction transaction = null;
        private bool disposedValue;

        public SQLite(String varConectionString):base(varConectionString)
        {

            var connectionString = new SQLiteConnectionStringBuilder("Data Source=" + varConectionString + ";Version=3;")
            {
                DefaultIsolationLevel = IsolationLevel.Serializable                
            }.ToString();

            connection = new SQLiteConnection(connectionString);
            connection.Open();
            TypeCommit = eTypeCommit.Auto;
        }

        ~SQLite()
        {
            //Close();
        }
        public override void Close(bool isWait = false)
        {
  //          $"[{GetType()} -{ GetHashCode()}] close connection".WriteLogMessage();
            //Зупиняємо всі запити до БД і чекаємо 1/4 секунди. щоб встигли завершитись запити.

            if (isWait)
            {
                SetLock(true);
                Thread.Sleep(250);
            }
            if (connection != null)
            {
                connection.Close();
                connection = null;
            }
            if (isWait)
            {
                WaitCollect(150);
            }
            SetLock(false);
        }

        void WaitCollect(int pMs=150)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(pMs);
        }

        public override IEnumerable<T1> Execute<T,T1>(string query, T parameters )
        {
            if (IsLock) ExceptionIsLock();
            return connection.Query<T1>(query, parameters);
        }

        public override IEnumerable<T1> Execute<T1>(string query)
        {
            if (IsLock) ExceptionIsLock();
            return connection.Query<T1>(query);
        }

        public override  Task<IEnumerable<T1>> ExecuteAsync<T, T1>(string query, T parameters)
        {
            if (IsLock) ExceptionIsLock();
            return  connection.QueryAsync<T1>(query, parameters);
        }

        public override Task<IEnumerable<T1>> ExecuteAsync<T1>(string query)
        {
            if (IsLock) ExceptionIsLock();
            return connection.QueryAsync<T1>(query);
        }
        public override void BeginTransaction()
        {
             transaction= connection.BeginTransaction();
        }

        public override void CommitTransaction()
        {
            transaction.Commit();
        }

        public override int ExecuteNonQuery<T>(string pQuery, T Parameters, int CountTry = 3)
        {
            if (IsLock) ExceptionIsLock();
            try
            {
                if (TypeCommit == eTypeCommit.Auto)
                {
                    int i= connection.Execute(pQuery, Parameters);
                    //FileLogger.WriteLogMessage($"ExecuteNonQuery<T> CountTry=>{CountTry} SQL=>{pQuery} res=>{i}",eTypeLog.Full);
                    return i;
                }
                else
                    return connection.Execute(pQuery, Parameters, transaction);
            }
            catch(Exception e) 
            {
                CountTry--;
                if(CountTry>0 && e.Message.Contains("database is locked"))
                {
                    FileLogger.WriteLogMessage($"ExecuteNonQuery<T> CountTry=>{CountTry} SQL=>{pQuery}", eTypeLog.Error);
                    WaitCollect(100);
                    return ExecuteNonQuery(pQuery, Parameters, CountTry);
                }
                throw new Exception(e.Message,e);
            }
        }

        public override int ExecuteNonQuery(string pQuery, int CountTry = 3)
        {
            if (IsLock) ExceptionIsLock();
            try
            {
                if (TypeCommit == eTypeCommit.Auto)
                return connection.Execute(pQuery);
            else
                return connection.Execute(pQuery,null,transaction);
            }
            catch (Exception e)
            {
                CountTry--;
                if (CountTry > 0 && e.Message.Contains("database is locked"))
                {
                    FileLogger.WriteLogMessage($"ExecuteNonQuery CountTry=>{CountTry} SQL=>{pQuery}");
                    WaitCollect(100);
                    return ExecuteNonQuery(pQuery,  CountTry);
                }
                throw new Exception(e.Message, e);
            }
        }

        public override Task<int> ExecuteNonQueryAsync<T>(string parQuery, T Parameters)
        {
            if (IsLock) ExceptionIsLock();
            if (TypeCommit == eTypeCommit.Auto)
                return connection.ExecuteAsync(parQuery, Parameters);
            else
                return connection.ExecuteAsync(parQuery, Parameters, transaction);
        }
        public override Task<int> ExecuteNonQueryAsync(string parQuery)
        {
            if (IsLock) ExceptionIsLock();
            if (TypeCommit == eTypeCommit.Auto)
                return connection.ExecuteAsync(parQuery);
            else
                return connection.ExecuteAsync(parQuery, null, transaction);
        }

        public override T1 ExecuteScalar<T1>(string query)
        {
            if (IsLock) ExceptionIsLock();
            return connection.ExecuteScalar<T1>(query);
        }

        public override T1 ExecuteScalar<T,T1>(string query,T parameters)
        {
            if (IsLock) ExceptionIsLock();
            return connection.ExecuteScalar<T1>(query, parameters);
        }

        public override Task<T1> ExecuteScalarAsync<T1>(string query)
        {
            if (IsLock) ExceptionIsLock();
            return connection.ExecuteScalarAsync<T1>(query);
        }

        public override Task<T1> ExecuteScalarAsync<T, T1>(string query, T parameters)
        {
            if (IsLock) ExceptionIsLock();
            return connection.ExecuteScalarAsync<T1>(query, parameters);
        }


        public  int ExecuteNonQuery<T>(string parQuery, T Parameters, SQLiteTransaction transaction)
        {
            if (IsLock) ExceptionIsLock();
            return connection.Execute(parQuery, Parameters, transaction);
        }

        public override int BulkExecuteNonQuery<T>(string parQuery, IEnumerable<T> Parameters)
        {
            if (IsLock) ExceptionIsLock();
            transaction = connection.BeginTransaction(IsolationLevel.Serializable);
          // FileLogger.ExtLogForClass(transaction.GetType(), transaction.GetHashCode(), "Begin transaction");
            try
            {
                foreach (var el in Parameters)
                    ExecuteNonQuery(parQuery, el, transaction);
                transaction.Commit();
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                new Exception("BulkExecuteNonQuery =>"+ex.Message, ex);
                return -1;
            }
           
            //FileLogger.ExtLogForClass(transaction.GetType(), transaction.GetHashCode(), "End transaction");
            return Parameters.Count();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection.Dispose();  
                        connection = null;
                    }
                } 
                disposedValue = true;
            }
 //           $"[{GetType()} -{ GetHashCode()}] Dispose connection".WriteLogMessage();
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SQLite()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
