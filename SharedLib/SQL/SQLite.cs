using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SQLite;
using Dapper;
using ModelMID;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLib
{
        
    public class SQLite:SQL
    {
        SQLiteConnection connection = null;
        SQLiteTransaction transaction = null;

        public SQLite(String varConectionString):base(varConectionString)
        {
            connection = new SQLiteConnection( "Data Source="+varConectionString+ ";Version=3;");
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

        public override  Task<IEnumerable<T1>> ExecuteAsync<T, T1>(string query, T parameters)
        {
            return  connection.QueryAsync<T1>(query, parameters);
        }

        public override Task<IEnumerable<T1>> ExecuteAsync<T1>(string query)
        {
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

        public override Task<int> ExecuteNonQueryAsync<T>(string parQuery, T Parameters)
        {
            if (TypeCommit == eTypeCommit.Auto)
                return connection.ExecuteAsync(parQuery, Parameters);
            else
                return connection.ExecuteAsync(parQuery, Parameters, transaction);
        }
        public override Task<int> ExecuteNonQueryAsync(string parQuery)
        {
            if (TypeCommit == eTypeCommit.Auto)
                return connection.ExecuteAsync(parQuery);
            else
                return connection.ExecuteAsync(parQuery, null, transaction);
        }

        public override T1 ExecuteScalar<T1>(string query)
        {
            return connection.ExecuteScalar<T1>(query);
        }

        public override T1 ExecuteScalar<T,T1>(string query,T parameters)
        {
            return connection.ExecuteScalar<T1>(query, parameters);
        }

        public override Task<T1> ExecuteScalarAsync<T1>(string query)
        {
            return connection.ExecuteScalarAsync<T1>(query);
        }

        public override Task<T1> ExecuteScalarAsync<T, T1>(string query, T parameters)
        {
            return connection.ExecuteScalarAsync<T1>(query, parameters);
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
