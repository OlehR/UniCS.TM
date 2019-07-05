﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SQLite;
using Dapper;
using ModelMID;

namespace SharedLib
{
        
    public class SQLite
    {
        SQLiteConnection connection = null;
        SQLiteTransaction transaction = null;

        public TypeCommit TypeCommit { get; set; } 
        public SQLite(String varConectionString)
        {
            connection = new SQLiteConnection("Data Source="+varConectionString+ ";Version=3;");
            connection.Open();
            TypeCommit = TypeCommit.Auto;
        }


        public void Close()
        {
            
        }

        public IEnumerable<T1> Execute<T,T1>(string query, T parameters )
        {
            return connection.Query<T1>(query, parameters);
        }

        public IEnumerable<T1> Execute<T1>(string query)
        {
            return connection.Query<T1>(query);
        }

        public void BeginTransaction()
        {
             transaction= connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            transaction.Commit();
        }

        public int ExecuteNonQuery<T>(string parQuery, T Parameters )
        {
            if(TypeCommit==TypeCommit.Auto)
             return connection.Execute(parQuery, Parameters);
            else
             return connection.Execute(parQuery, Parameters,transaction);
        }
        public int ExecuteNonQuery(string parQuery)
        {
            if (TypeCommit == TypeCommit.Auto)
                return connection.Execute(parQuery);
            else
                return connection.Execute(parQuery,null,transaction);
        }

        public T1 ExecuteScalar<T1>(string query)
        {
            return connection.ExecuteScalar<T1>(query);
        }

        public T1 ExecuteScalar<T,T1>(string query,T parameters)
        {
            return connection.ExecuteScalar<T1>(query, parameters);
        }

        public int BulkExecuteNonQuery<T>(string parQuery, IEnumerable<T> Parameters)
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