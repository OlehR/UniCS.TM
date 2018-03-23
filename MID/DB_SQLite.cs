/*
 * Побудовано на основі Библиотека удобного доступа к базе данных SQLite   Версия 1.1.2 Официальный сайт: http://krez0n.org.ua
 */
#define DEBUG
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Collections;
using System.Text;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Binary;
using MID;

/// <summary>
/// Библиотека для быстрой и удобной работы с базой данных SQLite
/// </summary>
//namespace DB_SQLite
//{
/// <summary>
/// Библиотека удобного доступа к базе данных. Класс содержит заготовки для получения, удаления, обновления и вставки данных.
/// </summary>
public partial class DB_SQLite
{
	private string lastError = string.Empty; //сообщение последней ошибки
	private string lastQuery = string.Empty; //последний выполненный запрос
	protected WDB.CallWriteLogSQL varCallWriteLogSQL;
	/// <summary>
	/// Последний выполненный запрос.
	/// </summary>
	public string LastQuery
	{
		get { return lastQuery; }
		set { lastQuery = value; }
	}

	/// <summary>
	/// Последняя ошибка.
	/// </summary>
	public string LastError
	{
		get { return lastError; }
	}

	#region Настройки
	/// <summary>
	/// Путь к файлу базы данных.
	/// </summary>
	string ConnectionString = string.Empty;
	private SQLiteConnection connect;
	private SQLiteTransaction transaction;
	private SQLiteConnectionStringBuilder csb = new SQLiteConnectionStringBuilder();
	private SQLiteCommand command = new SQLiteCommand();
	
	#endregion

	#region Инициализация
	/// <summary>
	/// Инициализация подключения.
	/// </summary>
	/// <param name="fileName">Путь к файлу базы данных</param>
	public DB_SQLite(string fileName, string password = null, WDB.CallWriteLogSQL parCallWriteLogSQL = null)
	{
		varCallWriteLogSQL=parCallWriteLogSQL;
		this.csb.DataSource = fileName;
		if(password!=null) this.csb.Password = password;
		connect = new SQLiteConnection(this.csb.ConnectionString);
	}

	
	~DB_SQLite()
	{
		Close();
	}
	#endregion

	#region Дополнительные инструменты
	/// <summary>
	/// Имя файла базы данных.
	/// </summary>
	public string Filename
	{
		get { return this.csb.DataSource; }
	}

	/// <summary>
	/// Версия SQLite.
	/// </summary>
	public string Version
	{
		get { return connect.ServerVersion; }
	}

	/// <summary>
	/// Информация о таблице.
	/// </summary>
	/// <param name="tableName">Имя таблицы</param>
	/// <returns>Перечисленные колонки в виде List<string></returns>
	public List<string> TableInfo(string tableName)
	{
		List<string> columns = new List<string>();
		DataTable dt = Execute(string.Format("PRAGMA table_info([{0}])", tableName));
		foreach (DataRow row in dt.Rows)
		{
			columns.Add(row[0].ToString());
		}
		return columns;
	}
	
	/// <summary>
	/// Список таблиц (временные таблицы не входят в список).
	/// </summary>
	/// <returns>Список таблиц в виде List<string></returns>
	public List<string> GetTables()
	{
		List<string> tables = new List<string>();
		DataTable dt = Execute("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name");
		foreach (DataRow row in dt.Rows)
		{
			tables.Add(row[0].ToString());
		}
		return tables;
	}
	
	/// <summary>
	/// Сжать базу данных.
	/// </summary>
	public void Vacuum()
	{
		ConnectionState previousConnectionState = ConnectionState.Closed;
		try
		{
			previousConnectionState = connect.State;
			if (connect.State == ConnectionState.Closed)
			{
				connect.Open();
			}
			command = new SQLiteCommand("VACUUM;", connect);
			command.ExecuteNonQuery();
		}
		catch (Exception error)
		{
			lastError = string.Format("Ошибка при выволнении команды VACUUM\n{0}", error.Message);
		}
		finally
		{
			if (previousConnectionState == ConnectionState.Closed)
			{
				connect.Close();
			}
		}
	}


	
	/// <summary>
	/// Получить текущее состояние подключения.
	/// </summary>
	/// <returns>Текущее состояние подключения</returns>
	public ConnectionState GetConnectionState()
	{
		return connect.State;
	}
	#endregion

	#region Открытие и закрытие подключения
	/// <summary>
	/// Открытие подключения.
	/// </summary>
	/// <returns>True если все прошло успешно</returns>
	public bool Open()
	{
		try
		{
			connect.Open();
			return true;
		}
		catch (Exception error)
		{
			lastError = string.Format("Ошибка при открытии подключения к базе данных {0}\n{1}", this.csb.DataSource, error.Message);
			return false;
		}
	}

	/// <summary>
	/// Закрытие подключения.
	/// </summary>
	/// <returns>True если все прошло успешно</returns>
	public bool Close()
	{
		try
		{
			if (connect.State == ConnectionState.Open)
				connect.Close();
			return true;
		}
		catch (Exception error)
		{
			lastError = string.Format("Ошибка при закрытии подключения к базе данных {0}\n{1}", this.csb.DataSource, error.Message);
			return false;
		}
	}
	#endregion

	#region Управление транзакциями
	/// <summary>
	/// Начать транзакцию.
	/// Соединение открывается, если оно не было открыто ранее.
	/// </summary>
	public void BeginTransaction()
	{
		try
		{
			if (connect.State == ConnectionState.Closed)
				connect.Open();
			transaction = connect.BeginTransaction();
		}
		catch (Exception error)
		{
			lastError = string.Format("Ошибка при попытке начать транзакцию!\n{0}", error.Message);
		}
	}

	/// <summary>
	/// Принять транзакцию. Соединение не закрывается.
	/// </summary>
	public void CommitTransaction()
	{
		try
		{
			if (transaction != null && connect.State != ConnectionState.Closed)
				transaction.Commit();
		}
		catch (Exception error)
		{
			lastError = string.Format("Ошибка при попытке применить транзакцию!\n{0}", error.Message);
		}
	}

	/// <summary>
	/// Отменить транзакцию. Соединение не закрывается.
	/// </summary>
	public void RollBackTransaction()
	{
		try
		{
			if (transaction != null && connect.State != ConnectionState.Closed)
				transaction.Rollback();
		}
		catch (Exception error)
		{
			lastError = string.Format("Ошибка отката транзакции!\n{0}", error.Message);
		}
	}
	#endregion

	#region Выполнение любого запроса без получения данных
	

	/// <summary>
	/// Выполняет запрос к базе данных.
	/// </summary>
	/// <param name="queries">Массив запросов</param>
	/// <returns>Код ошибки. Если 0 - ошибки нет</returns>
	/// 
	public int ExecuteNonQuery(string query, ParametersCollection parameters = null)
	{
		try
		{
			command = new SQLiteCommand(query, connect);
			if(parameters!=null)
				foreach (Parameter iparam in parameters)
			{
				//добавляем новый параметр
				command.Parameters.Add(iparam.ColumnName.StartsWith("@") ? iparam.ColumnName : "@" + iparam.ColumnName,
				                       iparam.DbType).Value = Convert.IsDBNull(iparam.Value) ? Convert.DBNull : iparam.Value;
			}
			lastQuery = command.CommandText;
			command.ExecuteNonQuery();
			if(this.varCallWriteLogSQL!=null)
				this.varCallWriteLogSQL(query,parameters);
		}
		catch (Exception error)
		{
			lastError = string.Format("{1}\nОшибка при выполнении запроса в методе ExecuteNonQuery!\nТекст запроса: {0}\n{1}", lastQuery, error.Message);
			return 1;
		}
		
		return 0;
	}
	
	#endregion

	#region Получение данных

	

	/// <summary>
	/// Выполняет переданный запрос в виде строки.
	/// </summary>
	/// <param name="query">Строка запроса</param>
	/// <param name="parameters">Коллекция параметров</param>
	/// <returns>Таблица с данными</returns>
	public DataTable Execute(string query, ParametersCollection parameters = null)
	{
		DataTable dt = new DataTable();
		
		try
		{
			command = new SQLiteCommand(query, connect);
			if(parameters!=null)
				foreach (Parameter iparam in parameters)
			{
				//добавляем новый параметр
				command.Parameters.Add(iparam.ColumnName.StartsWith("@") ? iparam.ColumnName : "@" + iparam.ColumnName,
				                       iparam.DbType).Value = Convert.IsDBNull(iparam.Value) ? Convert.DBNull : iparam.Value;
			}
			SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
			lastQuery = command.CommandText;
			adapter.Fill(dt);
		}
		catch (Exception error)
		{
			lastError = string.Format("Ошибка при получении из базы данных {0}!\nМетод: Execute(string query, ParametersCollection parameters)\nТекст запроса: {1}\n{2}", this.csb.DataSource, lastQuery, error.Message);
			return null;
		}
		return dt;
	}

	#endregion

	#region Перегрузка и добавление встроенных функций библиотеки SQLite
	/// <summary>
	/// Перевод в нижний регистр
	/// </summary>
	[SQLiteFunction(Name = "lower", Arguments = 1, FuncType = FunctionType.Scalar)]
	public class ToLowerCase : SQLiteFunction
	{
		public override object Invoke(object[] args)
		{
			if (args == null || args.Length == 0) return null;
			else
				return args[0].ToString().ToLower();
		}
	}

	/// <summary>
	/// Перевод в верхний регистр
	/// </summary>
	[SQLiteFunction(Name = "upper", Arguments = 1, FuncType = FunctionType.Scalar)]
	public class ToUpperCase : SQLiteFunction
	{
		public override object Invoke(object[] args)
		{
			if (args == null || args.Length == 0) return null;
			else
				return args[0].ToString().ToUpper();
		}
	}

	/// <summary>
	/// Текущая дата
	/// </summary>
	[SQLiteFunction(Name = "date", Arguments = 0, FuncType = FunctionType.Scalar)]
	public class DateNow : SQLiteFunction
	{
		public override object Invoke(object[] args)
		{
			if (args == null || args.Length == 0) return null;
			else
				return DateTime.Now.Date;
		}
	}

	/// <summary>
	/// Текущая дата и время
	/// </summary>
	[SQLiteFunction(Name = "now", Arguments = 0, FuncType = FunctionType.Scalar)]
	public class DateTimeNow : SQLiteFunction
	{
		public override object Invoke(object[] args)
		{
			if (args == null || args.Length == 0) return null;
			else
				return DateTime.Now;
		}
	}
	#endregion
}

#region Параметры
/// <summary>
/// Класс параметра, для передачи конструктору запроса.
/// </summary>

public class Parameter
{
	#region Поля
	string _columnName;
	object _value;
	DbType _dbType;
	#endregion

	/// <summary>
	/// Значение поля.
	/// </summary>
	public object Value
	{
		get { return _value; }
		set { _value = value; }
	}

	/// <summary>
	/// Название поля в базе данных.
	/// </summary>
	public string ColumnName
	{
		get { return _columnName; }
		set { _columnName = value; }
	}

	/// <summary>
	/// Тип значения.
	/// </summary>
	public DbType DbType
	{
		get { return _dbType; }
		set { _dbType = value; }
	}
	public string ToStringJSON()
	{
		return "[\""+_columnName+"\",\""+_value.ToString()+"\","+ Convert.ToInt32( _dbType)+"]";
	}
}

/// <summary>
/// Коллекция параметров.
/// </summary>
[Serializable]
public class ParametersCollection : CollectionBase
{
	/// <summary>
	/// Добавляет параметры в коллекцию.
	/// </summary>
	/// <param name="iparam">Параметр</param>
	public virtual void Add(Parameter iparam)
	{
		//добавляем в общую коллекцию
		this.List.Add(iparam);
	}

	/// <summary>
	/// Добавляет параметр в коллекцию.
	/// </summary>
	/// <param name="columnName">Имя поля/колонки</param>
	/// <param name="value">Значение</param>
	/// <param name="dbType">Тип значения</param>
	public virtual void Add(string columnName, object value, DbType dbType)
	{
		//Инициализируем объект с параметром
		Parameter iparam = new Parameter();
		//присваиваем название поля
		iparam.ColumnName = columnName;
		//присваиваем значение
		iparam.Value = value;
		//присваиваем тип значения
		iparam.DbType = dbType;
		//добавляем в общую коллекцию
		//List описан в "родителе"
		this.List.Add(iparam);
	}

	/// <summary>
	/// Возвращает элемент по индексу.
	/// </summary>
	/// <param name="Index">Индекс</param>
	/// <returns>Параметр, для передачи конструктору запроса</returns>
	public virtual Parameter this[int Index]
	{
		get
		{
			//возвращает элемент по индексу
			//используется в конструкции foreach
			return (Parameter)this.List[Index];
		}
	}
	public string ToJSON()
	{
		StringBuilder varRez=new StringBuilder("{ \"ParametersCollection\":[");
		foreach (Parameter iparam in this.List)
			varRez.Append(iparam.ToStringJSON()+",");
		return varRez.ToString().Substring(0,varRez.Length-1)+"]}";
		;
	}
	
	public virtual void AddJSON(string varJSON)
	{
		//StringSplitOptions options;		options= StringSplitOptions.RemoveEmptyEntries
		string varArrays= varJSON.Substring(varJSON.IndexOf('[')+1,varJSON.LastIndexOf(']')-varJSON.IndexOf('[')-1).Replace("\"","");
		string[] varParams = varArrays.Split(new String[] {"],["},StringSplitOptions.RemoveEmptyEntries);
		
		for(int i=0; i< varParams.Length ;i++)
		{
			string varParam=varParams[i].Trim(new Char[] { ']','[', ' ', '\"' } );
			string[] varValues = varParam.Split(',');
			DbType varDbType = (DbType) Convert.ToInt32(varValues[2]);
			object varValue;
			switch (varDbType)
			{
				case DbType.DateTime:
					varValue= (object) DateTime.ParseExact(varValues[1], "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
					break;
				default:
					varValue= (object) varValues[1];
					break;
			}
			this.Add(varValues[0],varValue,varDbType);
		}
	}
}

#endregion

#region Класс исключений
/// <summary>
/// Класс исключения для проверки строки запроса для базы данных.
/// </summary>
class ExceptionWarning : Exception
{
	private string _messageText;

	/// <summary>
	/// Текст сообщения об ошибке.
	/// </summary>
	public string MessageText
	{
		get { return _messageText; }
	}

	/// <summary>
	/// Текст сообщения об ошибке.
	/// </summary>
	/// <param name="messagetext">Сообщение об ошибке</param>
	public ExceptionWarning(string messagetext)
		: base()
	{
		_messageText = messagetext;
	}
}
#endregion


//}