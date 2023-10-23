using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using Dapper;
using ModelMID;
using Utils;

namespace SharedLib
{

    public class MSSQL:SQL
    {
        SqlConnection connection = null;
        IDbTransaction transaction = null;

        //public TypeCommit TypeCommit { get; set; }
        public MSSQL(String varConectionString= @"Server=10.1.0.22;Database=DW;Uid=dwreader;Pwd=DW_Reader;Connect Timeout=30;"/* "Server=SQLSRV1;Database=DW;Trusted_Connection=True;"*/) :base(varConectionString)
        {
            try
            {
                connection = new SqlConnection(varConectionString);
                connection.Open();
                TypeCommit = eTypeCommit.Auto;
            } catch(Exception e) 
            { 
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name,e);
                throw;
            }
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

        public override int BulkExecuteNonQuery<T>(string parQuery, IEnumerable<T> Parameters, bool IsRepeat = false)
        {
            BeginTransaction();
            try
            {
                foreach (var el in Parameters)
                    ExecuteNonQuery(parQuery, el);
            }
            catch(Exception Ex)
            {
                transaction.Rollback();
                throw;
            }
            CommitTransaction();
            return 0;

        }

        public override void Close(bool isWait = false)
        {
            connection.Close();
        }


    }
}
