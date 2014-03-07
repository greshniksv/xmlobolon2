using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using FirebirdSql.Data.FirebirdClient;
using fnLog2;

namespace XmlObolon_2._0.Classess
{
	class Database
	{
		private FbConnection _fbConn;
		private FbCommand _fbCmd;
		private FbDataReader _fbReader;
		private string _lastError;

		public bool Profiler { get; set; }

		public string LastError
		{
			get
			{
				string r = _lastError;
				_lastError = string.Empty; 
				return r;
			}
		}
		

		public Database()
		{
			_lastError = string.Empty;
		}


		public SqlConnection ConnectMssql()
		{
			SqlConnection sqlConn = null;
			SqlCommand sqlCmd = null;
			try
			{
				var SqlCsb = new SqlConnectionStringBuilder
				             	{
				             		UserID = Configuration.MsDbUser,
				             		Password = Configuration.MsDbPassword,
				             		InitialCatalog = Configuration.MsDbDatabase,
				             		DataSource = Configuration.MsDbHost
				};

				sqlConn = new SqlConnection(SqlCsb.ConnectionString);
				sqlConn.Open();
				//sqlCmd = sqlConn.CreateCommand();
				//sqlCmd.CommandTimeout = 9999999;
			}
			catch (Exception ex)
			{
				ErrorMessage.Show("Error connection to MS_SQL. Host: " + Configuration.MsDbHost + " InitialCatalog:" + Configuration.MsDbDatabase+" \n\nEX:"+ex, MessageType.Error);
				Configuration.ReturnResult = 2;
				return null;
			}

			return sqlConn;
		}



		public bool Connect()
		{
			SqlConnection sqlConn = null;
			SqlCommand sqlCmd = null;
			try
			{
				var SqlCsb = new SqlConnectionStringBuilder
				{
					UserID = Configuration.MsDbUser,
					Password = Configuration.MsDbPassword,
					InitialCatalog = Configuration.MsDbDatabase,
					DataSource = Configuration.MsDbHost
				};

				sqlConn = new SqlConnection(SqlCsb.ConnectionString);

				sqlConn.Open();
				sqlCmd = sqlConn.CreateCommand();
				sqlCmd.CommandTimeout = 9999999;
			}
			catch (Exception ex)
			{
				ErrorMessage.Show("Error connection to MS_SQL. Host: " + Configuration.MsDbHost + " InitialCatalog:" + Configuration.MsDbDatabase+" \n\nex:"+ex, MessageType.Error);
				//ErrorMessage.Show("Error connection to MS_SQL db. Check config.",MessageType.Error);
				Configuration.ReturnResult = 2;
				return false;
			}

			// Initialization CustMsList
			sqlCmd.CommandText = "select crId, id from tblCustomers";
			SqlDataReader reader = sqlCmd.ExecuteReader();
			while (reader.Read())
			{
				GLOBAL.CustomerMS.CustMsList.Add(new DataClass.CustMsItem() { Id = reader[0].ToString(), Number = reader[1].ToString() });
			}
			reader.Close();

			// Initialization ShopsMsList
			sqlCmd.CommandText = "select id, sName+'('+dbo.GetShopFullAddress(id)+')' from tblShops";
			reader = sqlCmd.ExecuteReader();
			while (reader.Read())
			{
				GLOBAL.ShopsMS.ShopMsList.Add(new DataClass.ShopMsItem() { Id = reader[0].ToString(), Name = reader[1].ToString() });
			}
			reader.Close();
			sqlConn.Close();




			try
			{
				var fbCsb = new FbConnectionStringBuilder
				{
					Dialect = 3,
					Charset = "win1251",
					UserID = Configuration.DbUser,
					Password = Configuration.DbPassword,
					Database = Configuration.DbDatabase,
					DataSource = Configuration.DbHost
				};

				_fbConn = new FbConnection(fbCsb.ConnectionString);

				_fbConn.Open();
				_fbCmd = _fbConn.CreateCommand();
				_fbCmd.CommandTimeout = 500000;


			}
			catch (Exception)
			{
				ErrorMessage.Show("Error connect to database.", MessageType.Error);
				Configuration.ReturnResult = 2;
				return false;
			}
			return true;
		}

		public List<string> ReaderExecOne(string sql)
		{
			if (!GLOBAL.FixQuery.Initialized)
				GLOBAL.FixQuery.Initialization(_fbCmd);

			if (sql.Length < 3) return null;

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			try
			{
				sql = GLOBAL.FixQuery.FixQuery(sql);
				_fbCmd.CommandText = sql;
				_fbReader = _fbCmd.ExecuteReader();
			}
			catch (Exception ex)
			{
				_lastError = "SQL:" + sql + " \n\n Detail:" + ex;
				return null;
			}

			stopwatch.Stop();

			if (Profiler)
			{
				// **************************************
				// Query profiler

				GLOBAL.SysLog.Write("TIME: " + stopwatch.ElapsedMilliseconds + "\n\nQuery: " + sql + "\n", MessageType.Information);


				//TextWriter writer = new StreamWriter(Profiler, true, Encoding.UTF8);
				//writer.WriteLine("\n\n - " + DateTime.Now.ToString("dd-MM-yyyy hh-mm-ss") + " TIME: " +
				//                 stopwatch.ElapsedMilliseconds);
				//writer.WriteLine("Query: " + sql);
				//writer.Close();
			}


			var result = new List<string>();
			if (_fbReader.Read())
			{
				for (int i = 0; i < _fbReader.FieldCount; i++)
				{
					result.Add(_fbReader[i].ToString());
				}
			}
			else
			{
				return null;
			}
			_fbReader.Close();

			return result;
		}

		public bool ReaderExec(string sql)
		{
			if (!GLOBAL.FixQuery.Initialized)
				GLOBAL.FixQuery.Initialization(_fbCmd);

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			sql = GLOBAL.FixQuery.FixQuery(sql);
			_fbCmd.CommandText = sql;
			_fbReader = _fbCmd.ExecuteReader();

			stopwatch.Stop();

			if (Profiler)
			{
				// **************************************
				// Query profiler

				GLOBAL.SysLog.Write("TIME: " + stopwatch.ElapsedMilliseconds + "\n\nQuery: " + sql+"\n", MessageType.Information);

				//TextWriter writer = new StreamWriter(Profiler, true, Encoding.UTF8);
				//writer.WriteLine("\n\n - " + DateTime.Now.ToString("dd-MM-yyyy hh-mm-ss") + " TIME: " +
				//                 stopwatch.ElapsedMilliseconds);
				//writer.WriteLine("Query: " + sql);
				//writer.Close();
			}


			return true;
		}

		public List<string> ReaderNext()
		{
			if(_fbReader.IsClosed)
			{
				_lastError = "Try read from closed reader. SQL:" + _fbCmd.CommandText;
				throw new Exception("Try read from closed reader. SQL:" + _fbCmd.CommandText);
			}

			var result = new List<string>();
			if(_fbReader.Read())
			{
				for (int i = 0; i < _fbReader.FieldCount; i++)
				{
					result.Add(_fbReader[i].ToString());
				}
			}
			else
			{
				return null;
			}
			return result;
		}

		public void ReaderClose()
		{
			_fbReader.Close();
		}

		public void Disconnect()
		{
			_fbConn.Close();
		}

	}
}
