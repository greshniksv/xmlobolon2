using System;
using System.Collections.Generic;
using System.Text;
using FirebirdSql.Data.FirebirdClient;

namespace XML_Obolon_Linux
{
	public delegate void InitialProgressEventHandler(int persent);

	public class MpRajah
	{
		public event InitialProgressEventHandler InitialProgress;
		private readonly List<RTable> _tableList = new List<RTable>();
		public bool Initialized { get; set; }

		~MpRajah()
		{
			_tableList.Clear();
		}

		public void Dispose()
		{
			_tableList.Clear();
		}

		public bool Initialization(FbCommand fbCmdn)
		{
			try
			{
				fbCmdn.CommandText = "select rdb$relation_name " +
					" from rdb$relations " +
					" where rdb$view_blr is null " +
					" and (rdb$system_flag is null or rdb$system_flag = 0) order by rdb$relation_name ";

				var re = fbCmdn.ExecuteReader();
				while (re.Read())
				{
					var tb = new RTable { Name = re[0].ToString().Trim() };
					if (tb.Name.IndexOf('.') != -1) { throw new Exception("Error table name contained [ . ] sumbol"); }
					if (tb.Name.IndexOf(',') != -1) { throw new Exception("Error table name contained [ , ] sumbol"); }
					_tableList.Add(tb);
				}
				re.Close();

				string tableList = "(";
				foreach (var tb in _tableList)
				{
					if (tableList.Length > 2)
						tableList += ",";

					tableList += "'" + tb.Name + "'";
				}
				tableList += ")";

				fbCmdn.CommandText = "select f.rdb$relation_name, f.rdb$field_name \n" +
					" from rdb$relation_fields f , rdb$relations r \n" +
					" where f.rdb$relation_name = r.rdb$relation_name \n" +
					" and r.rdb$view_blr is null \n" +
					" and (r.rdb$system_flag is null or r.rdb$system_flag = 0) \n" +
					" and f.rdb$relation_name in " + tableList + "  \n" +
					" order by 1";
				var reader = fbCmdn.ExecuteReader();
				while (reader.Read())
				{
					if (reader[0].ToString().Trim().IndexOf('.') != -1) { throw new Exception("Error table name contain [ . ] sumbol"); }
					if (reader[0].ToString().Trim().IndexOf(',') != -1) { throw new Exception("Error table name contain [ , ] sumbol"); }

					int filledTableField = 0;

					foreach (var tb in _tableList)
					{
						if (tb.Field.Count > 0)
							filledTableField++;

						if (tb.Name.Trim() == reader[0].ToString().Trim())
						{
							tb.Field.Add(reader[1].ToString().Trim());
						}
					}

					InitialProgress(filledTableField * 100 / _tableList.Count);

				}
				reader.Close();



				//int pp = 0;
				//foreach (var tb in _tableList)
				//{
				//    pp++;
				//    InitialProgress(pp * 100 / _tableList.Count);
				//    fbCmdn.CommandText = "select f.rdb$field_name " +
				//    " from rdb$relation_fields f" +
				//    " join rdb$relations r on f.rdb$relation_name = r.rdb$relation_name" +
				//    " and r.rdb$view_blr is null" +
				//    " and (r.rdb$system_flag is null or r.rdb$system_flag = 0)" +
				//    " and f.rdb$relation_name = '" + tb.Name + "'" +
				//    " order by 1 desc";
				//    //, f.rdb$field_position 
				//    var reader = fbCmdn.ExecuteReader();
				//    while (reader.Read())
				//    {
				//        if (reader[0].ToString().Trim().IndexOf('.') != -1) { throw new Exception("Error table name contain [ . ] sumbol"); }
				//        if (reader[0].ToString().Trim().IndexOf(',') != -1) { throw new Exception("Error table name contain [ , ] sumbol"); }
				//        tb.Field.Add(reader[0].ToString().Trim());
				//    }
				//    reader.Close();
				//}
			}
			catch { Initialized = false; return false; }

			Initialized = true;
			return true;
		}

		public string FixQuery(string query)
		{
			//query = query.ToLower();
			if (!Initialized) throw new Exception("Class MPRajah must be initialized.");

			var notFixedTableList = new List<TableAlias>();
			var notFixedFiledList = new List<FieldAlias>();

			// parse query 
			var wordMas = query.Split(' ', ',', '=', '(', ')', '<', '>', '-', '+', '/', '*');

			for (var i = 0; i < wordMas.Length; i++)
			{
				if (wordMas[i].IndexOf('0') == -1 || wordMas[i].Length <= 2 || wordMas[i].IndexOf('\'') != -1 ||
					wordMas[i].IndexOf('`') != -1)
				{
				}
				else
				{
					if (wordMas[i].IndexOf(',') != -1)
					{
						throw new Exception("Error parse query. Before and after [ , ] must be spacer.");
					}

					// in container sumbol '.'
					if (wordMas[i].IndexOf('.') != -1)
					{
						wordMas[i] = wordMas[i].ToLower();
						var wordsp = wordMas[i].Split('.');
						var fa = new FieldAlias { FullName = wordMas[i], Alias = wordsp[0] };
						if (wordsp[1].IndexOf('%') != -1)
						{
							var wordPostfix = wordsp[1].Split('%');
							fa.Name = wordPostfix[0];
							fa.NamePostfix = wordPostfix[1];
						}
						else
						{
							fa.Name = wordsp[1];
							fa.NamePostfix = string.Empty;
						}

						var addToList = true;
						foreach (var fat in notFixedFiledList)
						{
							if (fat.Name == fa.Name)
								if (fat.Alias == fa.Alias)
									if (fat.NamePostfix == fa.NamePostfix)
										addToList = false;
						}

						if (addToList)
							notFixedFiledList.Add(fa);
					}
					else
					{
						wordMas[i] = wordMas[i].ToLower();
						var ta = new TableAlias { FullName = wordMas[i] };

						if (wordMas[i].IndexOf('%') != -1)
						{
							var wordPostfix = wordMas[i].Split('%');
							ta.Name = wordPostfix[0];
							ta.NamePostfix = wordPostfix[1];
						}
						else
						{
							ta.Name = wordMas[i];
							ta.NamePostfix = string.Empty;
						}

						if (wordMas[i + 1] != ",")
						{
							ta.Alias = wordMas[i + 1];

							var addToList = true;
							foreach (var fat in notFixedTableList)
							{
								if (fat.Name == ta.Name)
									if (fat.Alias == ta.Alias)
										if (fat.NamePostfix == ta.NamePostfix)
											addToList = false;
							}

							if (addToList)
								notFixedTableList.Add(ta);


							//NotFixedTableList.Add(ta);
						}
						else
						{
							throw new Exception("Table:" + ta.Name + " dose not have alias name.");
						}
					}
				}
			}


			// Find real table name (Fix table name)
			foreach (var ta in notFixedTableList)
			{
				var existNeededTable = false;
				var findTable = ta.Name.Trim().ToLower();
				var postfix = ta.NamePostfix.Trim().ToLower();

				foreach (var table in _tableList)
				{
					if (table.Name.Length <= findTable.Length) continue;
					if (table.Name.Substring(0, findTable.Length).ToLower() != findTable.ToLower()) continue;
					if (postfix != string.Empty)
					{
						if (table.Name.ToLower().Substring(findTable.Length, table.Name.Length - findTable.Length).IndexOf(postfix) != -1)
						{
							ta.FixName = table.Name;
							ta.FieldList = table.Field;
							existNeededTable = true;
						}
					}
					else
					{
						ta.FixName = table.Name;
						ta.FieldList = table.Field;
						existNeededTable = true;
					}
				}

				if (!existNeededTable) throw new Exception("MPRajah. Not found table [" + ta.FullName + "] !");

			}


			// Find field name ( fix field name )   
			foreach (var fa in notFixedFiledList)
			{
				var existNeededField = false;
				var findField = fa.Name;
				var postfix = fa.NamePostfix;

				foreach (var ta in notFixedTableList)
				{
					if (fa.Alias != ta.Alias) continue;
					foreach (var field in ta.FieldList)
					{
						if (field.Length <= findField.Length) continue;
						if (field.Substring(0, findField.Length).ToLower() != findField.ToLower()) continue;
						if (postfix != string.Empty)
						{
							if (field.ToLower().Substring(findField.Length, field.Length - findField.Length).IndexOf(postfix) != -1)
							{
								fa.FixName = field;
								existNeededField = true;
								//break;
							}
						}
						else
						{
							fa.FixName = field;
							existNeededField = true;
							//break;
						}
					}
				}

				if (!existNeededField) throw new Exception("MPRajah. Not found field [" + fa.FullName + "] !");

			}


			// Find and remove duplicate table
			var tableDublicat = new List<string>();
			foreach (var ta in notFixedTableList)
			{
				var exist = false;
				foreach (var dtable in tableDublicat)
				{
					if (ta.FullName == dtable) { exist = true; }
				}
				if (exist) { ta.FullName = string.Empty; ta.Name = string.Empty; } else { tableDublicat.Add(ta.FullName); }
			}
			tableDublicat.Clear();


			// Find and remove duplicate table
			var filedDublicat = new List<string>();
			foreach (var ta in notFixedFiledList)
			{
				var exist = false;
				foreach (var dfiled in filedDublicat)
				{
					if (ta.FullName == dfiled) { exist = true; }
				}
				if (exist) { ta.FullName = string.Empty; ta.Name = string.Empty; } else { filedDublicat.Add(ta.FullName); }
			}
			filedDublicat.Clear();


			//foreach (TableAlias ta in NotFixedTableList)
			//{
			//    Console.WriteLine("RealName:" + ta.FixName + " \t Table: " + ta.Name + " alias:" + ta.Alias + " postfix:" + ta.NamePostfix);
			//}

			//foreach (FieldAlias fa in NotFixedFiledList)
			//{
			//    Console.WriteLine("RealName:" + fa.FixName + " \t Filed: " + fa.Name + " alias:" + fa.Alias + " postfix:" + fa.NamePostfix);
			//}


			// Raplace table name in query.
			foreach (var ta in notFixedTableList)
			{
				if (ta.FullName != string.Empty)
				{
					//query = query.Replace(ta.FullName, ta.FixName);
					query = ReplaceString(query, ta.FullName, ta.FixName, StringComparison.CurrentCultureIgnoreCase);
				}
			}

			// Raplace field name in query.
			foreach (var fa in notFixedFiledList)
			{
				if (fa.FullName != string.Empty)
				{
					//query = query.Replace(fa.FullName, fa.Alias + "." + fa.FixName);
					query = ReplaceString(query, fa.FullName, fa.Alias + "." + fa.FixName, StringComparison.CurrentCultureIgnoreCase);
				}
			}


			//Console.WriteLine("---\n\n");
			//Console.WriteLine(query);

			//Console.ReadKey();

			return query;
		}


		public string ReplaceString(string str, string oldValue, string newValue, StringComparison comparison)
		{
			var sb = new StringBuilder();

			var previousIndex = 0;
			var index = str.IndexOf(oldValue, comparison);
			while (index != -1)
			{
				sb.Append(str.Substring(previousIndex, index - previousIndex));
				sb.Append(newValue);
				index += oldValue.Length;

				previousIndex = index;
				index = str.IndexOf(oldValue, index, comparison);
			}
			sb.Append(str.Substring(previousIndex));

			return sb.ToString();
		}

	}

	class TableAlias
	{
		public TableAlias() { Name = string.Empty; FixName = string.Empty; Alias = string.Empty; NamePostfix = string.Empty; }

		public string FullName { get; set; }
		public List<string> FieldList { get; set; }
		public string NamePostfix { get; set; }
		public string FixName { get; set; }
		public string Name { get; set; }
		public string Alias { get; set; }
	}

	class FieldAlias
	{
		public FieldAlias() { Name = string.Empty; FixName = string.Empty; Alias = string.Empty; NamePostfix = string.Empty; }

		public string FullName { get; set; }
		public string NamePostfix { get; set; }
		public string Name { get; set; }
		public string FixName { get; set; }
		public string Alias { get; set; }
	}

	class RTable
	{
		private List<string> _field = new List<string>();

		public List<string> Field { get { return _field; } set { _field = value; } }
		public string Name { get; set; }
	}

}
