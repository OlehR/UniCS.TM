using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using Dapper;
using ModelMID;
using System.Threading;
using Utils;
using System.Linq;

namespace SharedLib
{
    public class SQLite : SQL, IDisposable
    {
        public SQLiteConnection Connection = null;
        public SQLiteTransaction Transaction = null;
        private bool disposedValue;
        public SQLite(String varConectionString) : base(varConectionString)
        {
            string connectionString = null;
            try
            {
                connectionString = new SQLiteConnectionStringBuilder("Data Source=" + varConectionString + ";Version=3;")
                {
                    DefaultIsolationLevel = IsolationLevel.Serializable
                }.ToString();

                Connection = new SQLiteConnection(connectionString);
                Connection.Open();
                TypeCommit = eTypeCommit.Auto;
                ExecuteNonQuery("PRAGMA synchronous = EXTRA;");
                ExecuteNonQuery("PRAGMA journal_mode = DELETE;");
                ExecuteNonQuery("PRAGMA wal_autocheckpoint = 5;");
            }
            catch (Exception ex) 
            {
                FileLogger.WriteLogMessage(this, connectionString, ex);
                throw;
            }
            FileLogger.WriteLogMessage(this, "SQLite", connectionString);
        }

        ~SQLite()
        {
            //Close();
        }

        public SQLiteTransaction GetTransaction() => Connection.BeginTransaction(IsolationLevel.Serializable);
        public override void BeginTransaction() => Transaction = Connection.BeginTransaction(IsolationLevel.Serializable);
        public override void CommitTransaction() { Transaction?.Commit(); Transaction = null; }
        public  void RollbackTransaction() { Transaction.Rollback(); Transaction = null; }

        public override void Close(bool isWait = false)
        {
            //          $"[{GetType()} -{ GetHashCode()}] close connection".WriteLogMessage();
            //Зупиняємо всі запити до БД і чекаємо 1/4 секунди. щоб встигли завершитись запити.

            if (isWait)
            {
                SetLock(true);
                Thread.Sleep(250);
            }
            if (Connection != null)
            {
                Connection.Close();
                Connection = null;
            }
            if (isWait)
            {
                WaitCollect(150);
            }
            SetLock(false);
        }

        void WaitCollect(int pMs = 150)
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(pMs);
            } catch (Exception e)
            {
                var s = e.Message;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Connection != null)
                    {
                        Connection.Close();
                        Connection.Dispose();
                        Connection = null;
                    }
                }
                disposedValue = true;
            }
            //           $"[{GetType()} -{ GetHashCode()}] Dispose connection".WriteLogMessage();
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        public override IEnumerable<T1> Execute<T, T1>(string pQuery, T parameters)
        {
            IsDapper(parameters, pQuery);
            try {
                if (IsLock) ExceptionIsLock();
                return Connection.Query<T1>(pQuery, parameters);
            } catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + Environment.NewLine + pQuery, e);
                throw;
            }
        }
    

        public override IEnumerable<T1> Execute<T1>(string query)
        {
            try { 
            if (IsLock) ExceptionIsLock();
            return Connection.Query<T1>(query);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + Environment.NewLine + query, e);
                throw;
            }
        }

        public override int ExecuteNonQuery<T>(string pQuery, T Parameters, int CountTry = 3)
        {
            IsDapper(Parameters, pQuery);
            if (IsLock) ExceptionIsLock();
            try
            {
                if (TypeCommit == eTypeCommit.Auto)
                {
                    int i= Connection.Execute(pQuery, Parameters);
                    //FileLogger.WriteLogMessage($"ExecuteNonQuery<T> CountTry=>{CountTry} SQL=>{pQuery} res=>{i}",eTypeLog.Full);
                    return i;
                }
                else
                    return Connection.Execute(pQuery, Parameters, Transaction);
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
                return Connection.Execute(pQuery);
            else
                return Connection.Execute(pQuery,null,Transaction);
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

        public override T1 ExecuteScalar<T1>(string query)
        {
            try
            {
                if (IsLock) ExceptionIsLock();
                return Connection.ExecuteScalar<T1>(query);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + Environment.NewLine + query, e);
                throw;
            }
        }

        public override T1 ExecuteScalar<T,T1>(string pQuery,T parameters)
        {
            IsDapper(parameters, pQuery);
            try
            {
                if (IsLock) ExceptionIsLock();
                return Connection.ExecuteScalar<T1>(pQuery, parameters);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + Environment.NewLine + pQuery, e);
                throw;
            }
    }

        public  int ExecuteNonQuery<T>(string pQuery, T Parameters, SQLiteTransaction pTransaction)
        {
            IsDapper(Parameters, pQuery);
            try
            {
                if (IsLock) ExceptionIsLock();
                return Connection.Execute(pQuery, Parameters, pTransaction);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + Environment.NewLine + pQuery, e);
                throw;
            }
    }

        public override int BulkExecuteNonQuery<T>(string pQuery, IEnumerable<T> pData, bool IsRepeatNotBulk = false)
        {
            if (pData == null) return 0;
            if (IsLock) ExceptionIsLock();

            SQLiteTransaction transaction = null;
            if(Transaction==null) transaction=GetTransaction();
            
            // FileLogger.ExtLogForClass(transaction.GetType(), transaction.GetHashCode(), "Begin transaction");
            try
            {
                foreach (var el in pData)
                    ExecuteNonQuery(pQuery, el, Transaction??transaction);
                transaction?.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                if (IsRepeatNotBulk)
                    try
                    {
                        FileLogger.WriteLogMessage(this, $"BulkExecuteNonQuery=>{pQuery}", ex);
                        foreach (var el in pData)
                            ExecuteNonQuery(pQuery, el);
                    }
                    catch (Exception e)
                    { throw new Exception("BulkExecuteNonQuery =>" + ex.Message, ex); }
                else
                    throw new Exception("BulkExecuteNonQuery =>" + ex.Message, ex);
            }
            //FileLogger.ExtLogForClass(transaction.GetType(), transaction.GetHashCode(), "End transaction");
            return pData.Count();
        }

        public int BulkExecuteNonQuery<T>(string pQuery, IEnumerable<T> pData, SQLiteTransaction pTr)
        {
            if (IsLock) ExceptionIsLock();         
            try
            {
                foreach (var el in pData)
                    ExecuteNonQuery(pQuery, el, pTr);                
            }
            catch (Exception ex) {throw new Exception("BulkExecuteNonQuery =>" + ex.Message, ex); }            
            return pData.Count();
        }

        void IsDapper(object pO,string pSql)
        {
            return;
            char[] delimiterChars = { ' ', ',', '.', ':','(',')', '\t', '\n','\r','=',';' };
            var Par = pSql.Split(delimiterChars).Where(el => el.StartsWith("@")).Select(el => el.Substring(1)).Distinct();

            var pr = pO.GetType().GetProperties().Select(el => el.Name);
            
            foreach (var el in Par)
            {
                if(!pr.Any(e=>e.Equals(el)))
                {
                    var res = pr.Where(e => e.ToUpper().Equals(el.ToUpper()));
                    if (res.Count() > 0)
                        FileLogger.WriteLogMessage(this, "IsDapper", $"{el}=>{res.FirstOrDefault()} SQL=>{pSql}");
                }
            }
        }

        public int GetVersion => ExecuteScalar<int>("PRAGMA user_version");

        public bool SetVersion(int pVer) => ExecuteNonQuery($"PRAGMA user_version={pVer}") > 0;
    }
}
