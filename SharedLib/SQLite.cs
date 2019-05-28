using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.Sqlite;

namespace SharedLib
{
    public class Parameter
    {
        public string ColumnName { get; set; }
        public object Value { get; set; }
        public SqliteType Type { get; set; }
        public Parameter() { }
        public Parameter(string parColumnName, object parValue)
        {
            ColumnName = parColumnName;
            Value = parValue;
        }
        
    }

    public class SQLite
    {
        SqliteConnection connection = null;
        public SQLite(String varConectionString)
        {
            connection = new SqliteConnection(varConectionString);
            connection.Open();
        }

        public void Close()
        {

        }
        /// <summary>
        /// Выполняет переданный запрос в виде строки.
        /// </summary>
        /// <param name="query">Строка запроса</param>
        /// <param name="parameters">Коллекция параметров</param>
        /// <returns>Таблица с данными</returns>
        public DataTable Execute(string query, Parameter[] parameters = null)
        {
            DataTable dt = new DataTable();

            var command = connection.CreateCommand();
            command.CommandText = query;
            if (parameters != null)
                foreach (var iparam in parameters)
                    command.Parameters.AddWithValue(iparam.ColumnName, iparam.Value);

            using (var reader = command.ExecuteReader())
                return dt;
        }

        public void BeginTransaction()
        {
        }

        public void CommitTransaction()
        {
        }

        public int ExecuteNonQuery(string query, Parameter[] parameters = null)
        {
            using (var transaction = connection.BeginTransaction())
            {

                DataTable dt = new DataTable();
                var command = connection.CreateCommand();
                command.CommandText = query;
                command.Transaction = transaction;
                if (parameters != null)
                    foreach (var iparam in parameters)
                        command.Parameters.AddWithValue(iparam.ColumnName, iparam.Value);
                command.ExecuteNonQuery();
                transaction.Commit();
            }
            return 1;
        }
        public object ExecuteScalar(string query, Parameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            var command = connection.CreateCommand();
            command.CommandText = query;
            if (parameters != null)
                foreach (var iparam in parameters)
                    command.Parameters.AddWithValue(iparam.ColumnName, iparam.Value);
            return command.ExecuteScalar();

        }

    }
}
