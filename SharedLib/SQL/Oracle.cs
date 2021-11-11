using System;
using System.Collections.Generic;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using Dapper;
using ModelMID;

namespace SharedLib
{
    public class Oracle:SQL
    {
        OracleConnection connection = null;
        OracleTransaction transaction = null;
        public Oracle(String varConectionString = "Data Source = VOPAK_NEW; User Id = c; Password=c;") : base(varConectionString)
        {
            connection=new OracleConnection(varConectionString);
            connection.Open();
            TypeCommit = eTypeCommit.Auto;
        }

        public override IEnumerable<T1> Execute<T, T1>(string query, T parameters)
        {
            return connection.Query<T1>(query, parameters);
        }

        public override IEnumerable<T1> Execute<T1>(string query)
        {
            return connection.Query<T1>(query);
        }

        public override void BeginTransaction()
        {
            //transaction = connection.BeginTransaction();
        }

        public override void CommitTransaction()
        {
            // transaction.Commit();
        }

        public override int ExecuteNonQuery<T>(string parQuery, T Parameters, int CountTry = 3)
        {
            if (TypeCommit == eTypeCommit.Auto)
                return connection.Execute(parQuery, Parameters);
            else
                return connection.Execute(parQuery, Parameters, transaction);
        }
        public override int ExecuteNonQuery(string parQuery, int CountTry = 3)
        {
            if (TypeCommit == eTypeCommit.Auto)
                return connection.Execute(parQuery);
            else
                return connection.Execute(parQuery, null, transaction);
        }

        public override T1 ExecuteScalar<T1>(string query)
        {
            return connection.ExecuteScalar<T1>(query);
        }

        public override T1 ExecuteScalar<T, T1>(string query, T parameters)
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
