using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SQLite;
using Dapper;
namespace SharedLib
{
    
    public class SQLite
    {
        SQLiteConnection connection = null;
        public SQLite(String varConectionString)
        {
            connection = new SQLiteConnection(varConectionString);
            connection.Open();
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
        }

        public void CommitTransaction()
        {
        }

        public int ExecuteNonQuery<T>(string parQuery, T Parameters )
        {
            return connection.Execute(parQuery, Parameters);
        }
        public int ExecuteNonQuery(string parQuery)
        {
            return connection.Execute(parQuery);
        }

        public T1 ExecuteScalar<T1>(string query)
        {
            return connection.ExecuteScalar<T1>(query);
        }

        public T1 ExecuteScalar<T,T1>(string query,T parameters)
        {
            return connection.ExecuteScalar<T1>(query, parameters);
        }

    }
}
