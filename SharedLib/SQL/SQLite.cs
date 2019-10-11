using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SQLite;
using Dapper;
using ModelMID;
using System.Threading;

namespace SharedLib
{
        
    public class SQLite:SQL
    {
        SQLiteConnection connection = null;
        SQLiteTransaction transaction = null;

        public SQLite(String varConectionString):base(varConectionString)
        {
            connection = new SQLiteConnection("Data Source="+varConectionString+ ";Version=3;");
            connection.Open();
            TypeCommit = eTypeCommit.Auto;
        }
        public override void Close()
        {
            connection.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(150); 
        }


        public override IEnumerable<T1> Execute<T,T1>(string query, T parameters )
        {
            return connection.Query<T1>(query, parameters);
        }

        public override IEnumerable<T1> Execute<T1>(string query)
        {
            return connection.Query<T1>(query);
        }

        public override void BeginTransaction()
        {
             transaction= connection.BeginTransaction();
        }

        public override void CommitTransaction()
        {
            transaction.Commit();
        }

        public override int ExecuteNonQuery<T>(string parQuery, T Parameters )
        {
            if(TypeCommit==eTypeCommit.Auto)
             return connection.Execute(parQuery, Parameters);
            else
             return connection.Execute(parQuery, Parameters,transaction);
        }
        public override int ExecuteNonQuery(string parQuery)
        {
            if (TypeCommit == eTypeCommit.Auto)
                return connection.Execute(parQuery);
            else
                return connection.Execute(parQuery,null,transaction);
        }

        public override T1 ExecuteScalar<T1>(string query)
        {
            return connection.ExecuteScalar<T1>(query);
        }

        public override T1 ExecuteScalar<T,T1>(string query,T parameters)
        {
            return connection.ExecuteScalar<T1>(query, parameters);
        }

        public override int BulkExecuteNonQuery<T>(string parQuery, IEnumerable<T> Parameters)
        {
            BeginTransaction();
            try
            {
                foreach (var el in Parameters)
                    ExecuteNonQuery(parQuery, el);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            CommitTransaction();
            return 0;

        }



    }
}
