using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using Dapper;
using ModelMID;

namespace SharedLib
{
/* */

    public class MSSQL:SQL
    {
        SqlConnection connection = null;
        IDbTransaction transaction = null;

        //public TypeCommit TypeCommit { get; set; }
        public MSSQL(String varConectionString= @"Server=10.1.0.22;Database=DW;Uid=dwreader;Pwd=DW_Reader;"/* "Server=SQLSRV1;Database=DW;Trusted_Connection=True;"*/) :base(varConectionString)
        {            
            connection = new SqlConnection(varConectionString);
            connection.Open();
            TypeCommit = TypeCommit.Auto;
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

        public override int ExecuteNonQuery<T>(string parQuery, T Parameters)
        {
            if (TypeCommit == TypeCommit.Auto)
                return connection.Execute(parQuery, Parameters);
            else
                return connection.Execute(parQuery, Parameters, transaction);
        }
        public override int ExecuteNonQuery(string parQuery)
        {
            if (TypeCommit == TypeCommit.Auto)
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
