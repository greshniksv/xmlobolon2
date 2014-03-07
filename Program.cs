using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using fnLog2;
using XmlObolon_2._0.Classess;
using fnConfig3;

namespace XmlObolon_2._0
{
	class Program
	{
		private static int _rajahInitialiger;

	    static void Help()
	    {
	        string info=string.Empty;
	        info += ": XmlObolon.exe :\n\n";
	        info += " -s = (int) Задается дата старта \" 0 - сегодня, 1 - завтра, -1 - вчера \" \n" +
	                " -d = (int) Задается глубина\n" +
                    " -n = если есть этот параметр то письмо с файлом не отсылается.\n" +
                    " -chk = запуск функции проверки движения товара.\n" +
                    " -excs \"fileName\" = создание списка клиентов и магазинов с номерами.\n" +
                    " -xmlt = терминал для работы с файлами.\n" +
                    " -addlog = расширренное логирование \n" +
                    " -cfg \"fileName\" = использовать конфиг \n" +
                    " --gen-error = генерирование расхождений \n" +
                    " -f = форсированная отправка файлов.\n";


            Console.Write(info);
	    }



	    /// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <returns>
		/// 0 - all is ok
		/// 1 - firebird database error
		/// 2 - mssql database error
		/// 3 - incorrect sql query
		/// 4 - incorrect data rest
		/// 
		/// </returns>
		static int Main(string[] args)
		{
			// Variables
			Configuration.ErrorMessage = new StringBuilder();
			Configuration.SendMessage = true;
			Configuration.DataClassTimeProfile = false;
			GLOBAL.FbDatabase.Profiler = false;
			var timeElepsedFilter = new List<long>();
			bool interfaceOff = false;
			bool webInterface = false;
			bool xmlTerminal = false;
			bool extendlog = false;
			bool findErrorOnSkg = false;
            bool SendAnyWay = false;
			//TODO: fix this ))
			bool exportClientShopFackingSheet = false;
			string exportClientShopFackingSheetFile = @"./1.csv";
			string configFile = string.Empty;
			string[] errorProductCodeList = null;// new string[0];// { "10044" };

			//TODO: fix this too ))


			

			// Processing command line options
			int cmdDate = -1;
			int cmdDeep = -1;
			bool runCheckFunction = false;
			for (int i = 0; i < args.Length; i++)
			{
			    if (args[i].ToLower() == "-f")
			    {
			        SendAnyWay = true;
                    GLOBAL.SysLog.Write("Forced send file!",MessageType.Warning);
                    Console.WriteLine("Forced send file system turning on!");
			    }

			    if (args[i].ToLower() == "-cfg")
				{
					string cfg = args[i + 1];
					if (!File.Exists(cfg))
					{
						Console.WriteLine("Config file [" + cfg + "] not found. ");
						return 1;
					}
					else
					{
						Console.WriteLine("Using config file [" + cfg + "]");
					}
					configFile = cfg;
				}
                				
				if (args[i].ToLower() == "--help" || args[i].ToLower() == "-h")
				{
                    Help();
					//GLOBAL.Interface.HelpForm();
					return 0;
				}

				if (args[i].ToLower() == "-excs")
				{
					exportClientShopFackingSheet = true;
					if (args[i + 1].ToLower() != "")
						exportClientShopFackingSheetFile = args[i + 1];
				}

				if (args[i].ToLower() == "-chk")
				{
					runCheckFunction = true;
				}

				if (args[i].ToLower() == "-xmlt")
				{
					xmlTerminal = true;
				}

				if (args[i].ToLower() == "-addlog")
				{
					extendlog = true;
				}
			
				
				if (args[i].ToLower() == "-s")
				{
					try
					{
						cmdDate = Int32.Parse(args[i + 1]);
						GLOBAL.SysLog.Write("SET: DateStart " + cmdDeep, MessageType.Information);
					}
					catch (Exception)
					{
						GLOBAL.SysLog.Write("Error parsing command line.", MessageType.Error);
						Console.WriteLine("Error parsing command line.");
						return 1;
					}
				}

				if (args[i].ToLower() == "-d")
				{
					try
					{
						cmdDeep = Int32.Parse(args[i + 1]);
						GLOBAL.SysLog.Write("SET: DateDeep " + cmdDeep, MessageType.Information);
					}
					catch (Exception)
					{
						GLOBAL.SysLog.Write("Error parsing command line.", MessageType.Error);
						Console.WriteLine("Error parsing command line.");
						return 1;
					}

					// If 15-th send report abput 15 day
					if (DateTime.Now.Day == 15 && DateTime.Now.Hour > 3 && DateTime.Now.Hour < 8)
					{
						cmdDeep = 15;
						GLOBAL.SysLog.Write("Detected 15-th !!! SET: DateDeep " + cmdDeep, MessageType.Information);
					}

					// If 30-th send report abput 30 day
					if (DateTime.Now.Day == 30 && DateTime.Now.Hour > 3 && DateTime.Now.Hour < 8)
					{
						cmdDeep = 30;
						GLOBAL.SysLog.Write("Detected 30-th !!! SET: DateDeep " + cmdDeep, MessageType.Information);
					}
				}

				//if (args[i].ToLower() == "-no")
				//{
				//    interfaceOff = true;
				//}

				if (args[i].ToLower() == "-web")
				{
					webInterface = true;
					Console.Write("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
					Console.WriteLine("<table align='left'><tr><td align='left'>");
				}

				if (args[i].ToLower() == "-n")
				{
					Configuration.SendMessage = false;
					GLOBAL.SysLog.Write("SET: SendMailMessage turn off !!!", MessageType.Warning);
				}

				if (args[i].ToLower() == "--query-profiler")
				{
					GLOBAL.FbDatabase.Profiler = true;
				}

				if (args[i].ToLower() == "--data-profiler")
				{
					Configuration.DataClassTimeProfile = true;
				}

				if (args[i].ToLower() == "--gen-error")
				{
					errorProductCodeList = args[i+1].ToLower().Split(',');
				}

			}


			//GLOBAL.FbDatabase.Profiler = "./query.log";

			// Initialization IO system
			Configuration.ExeuteDir = Directory.GetCurrentDirectory();
			configFile = configFile == string.Empty ? Configuration.ExeuteDir + @"/xml_obolon.cfg" : configFile;

            // Test config file
		    try
		    {
                TextReader tr = new StreamReader(configFile);
                tr.ReadToEnd();
                tr.Close();
		    }
		    catch (Exception ex)
		    {
                Console.WriteLine("Error read config file. Ex:"+ex);
		        return 9999;
		    }

			Config.Initialization(configFile);
			GLOBAL.SysLog.Initialization(Configuration.ExeuteDir + @"/xml_obolon.log", 1024 * 1024 * 1024, false);
			GLOBAL.SysLog.Write("\n\n\nStaring programm", MessageType.Information);
			GLOBAL.SysLog.Write("WorkDir: "+Configuration.ExeuteDir,MessageType.Information);

			if (extendlog) GLOBAL.SysLog.Write("Read [Database] section",MessageType.Information);
			Configuration.DbUser = Config.GetValue("User", "SYSDBA", "Database");
			Configuration.DbPassword = Config.GetValue("Password", "masterkey", "Database");
			Configuration.DbDatabase = Config.GetValue("DB", "209_010111.FDB", "Database");
			Configuration.DbHost = Config.GetValue("Host", "db2.poisk.lg.ua", "Database");

			if (extendlog) GLOBAL.SysLog.Write("Read [MsSqlDatabase] section", MessageType.Information);
			Configuration.MsDbUser = Config.GetValue("User", "user", "MsSqlDatabase");
			Configuration.MsDbPassword = Config.GetValue("Password", "1", "MsSqlDatabase");
			Configuration.MsDbDatabase = Config.GetValue("DB", "itdb.server", "MsSqlDatabase");
			Configuration.MsDbHost = Config.GetValue("Host", "192.168.2.7", "MsSqlDatabase");


			if (extendlog) GLOBAL.SysLog.Write("Read [Main] section", MessageType.Information);
			Configuration.PressAnyKey = Config.GetValue("AskPressAnyKey", "yes", "Main").ToLower()=="yes"?true:false;
			Configuration.ZoneNum = Config.GetValue("ZoneNum", "64", "Main");

			try{ Configuration.FileNumber = Int32.Parse(Config.GetValue("FileNumber", "1500", "Main")); }
			catch(Exception ex)
			{
				GLOBAL.SysLog.Write("Error cast FileNumber to int32. Detail:" + ex, MessageType.Error);
				return 1;
			}
			Configuration.ReportingDir = Config.GetValue("ReportingDir", Configuration.ExeuteDir + "/reporting", "Main");

			if (extendlog) GLOBAL.SysLog.Write("Read [SMTP] section", MessageType.Information);
			Configuration.SmtpHost = Config.GetValue("SMTP_Host", "mail.iteam.net.ua", "SMTP");
			try { Configuration.SmtpPort = Int32.Parse(Config.GetValue("SMTP_Port", "25", "SMTP")); }
			catch (Exception ex)
			{
				GLOBAL.SysLog.Write("Error cast SMTP_Port to int32. Detail:"+ex,MessageType.Error);
				return 1;
			}
			Configuration.SmtpUser = Config.GetValue("SMTP_User", "kadry@poisk.lg.ua", "SMTP");
			Configuration.SmtpPassword = Config.GetValue("SMTP_Password", "9yQMATkN25", "SMTP");
			Configuration.SmtpSsl = Config.GetValue("SMTP_SSL", "no", "SMTP").ToLower() == "yes" ? true : false;

			if (extendlog) GLOBAL.SysLog.Write("Read [RESERVED_SMTP] section", MessageType.Information);
			Configuration.ResSmtpHost = Config.GetValue("SMTP_Host", "smtp.gmail.com", "RESERVED_SMTP");
			try
			{
				Configuration.ResSmtpPort = int.Parse(Config.GetValue("SMTP_Port", "587", "RESERVED_SMTP"));
			}
			catch (Exception ex)
			{
				GLOBAL.SysLog.Write("Error cast SMTP_Port to int32. Detail:" + ex,MessageType.Error);
				return 1;
			}
			Configuration.ResSmtpUser = Config.GetValue("SMTP_User", "greshnik.mail@gmail.com", "RESERVED_SMTP");
			Configuration.ResSmtpPassword = Config.GetValue("SMTP_Password", "diskoteka_80", "RESERVED_SMTP");
			Configuration.ResSmtpSsl = Config.GetValue("SMTP_SSL", "yes", "RESERVED_SMTP").ToLower() == "yes";

			if (extendlog) GLOBAL.SysLog.Write("Read [Mail] section", MessageType.Information);
			Configuration.MailTo = Config.GetValue("MailTo", "robot@dn.obolon.ua", "Mail");
			Configuration.MailFrom = Config.GetValue("MailFrom", "torg@poisk.lg.ua", "Mail");
			Configuration.MailToError = Config.GetValue("MailToError", "greshnik-sv@yandex.ru", "Mail");
			Configuration.MailFromError = Config.GetValue("MailFromError", "xml.error@yandex.ua", "Mail");

			Configuration.SkladMain = Config.GetValue("Main", "0", "Store");
			Configuration.SkladSoh = Config.GetValue("Soh", "0", "Store");
            Configuration.SkladSpoilage = Config.GetValue("Spoilage", "0", "Store");
			Configuration.SkladReserve = Config.GetValue("Reserve", "0", "Store");


			if (extendlog) GLOBAL.SysLog.Write("Connect to firebird", MessageType.Information);
			// Connect to database
			if (!GLOBAL.FbDatabase.Connect())
				return Configuration.ReturnResult;


			// -----------------------------------------
			// Start XML Terminal
			if (xmlTerminal)
			{
				if (extendlog) GLOBAL.SysLog.Write("Open terminal", MessageType.Information);
				var xmlAnalize = new XmlAnalize();
				xmlAnalize.StartTerminal();
				return 0;
			}


			// -----------------------------------------
			// Start XML Terminal
			if (findErrorOnSkg)
			{
				Console.Write("Инициализация базы раджи");
				GLOBAL.FixQuery.InitialProgress += (FixQueryInitialProgress);
				GLOBAL.FbDatabase.ReaderExecOne("");

				var findErrorOnSkladreg = new FindErrorOnSkladreg();
				findErrorOnSkladreg.Find();
				return 0;
			}


			// -----------------------------------------
			// exportClientShopFackingSheet

			if (exportClientShopFackingSheet)
			{
				if (extendlog) GLOBAL.SysLog.Write("Run exportClientShopFackingSheet", MessageType.Information);
				Console.WriteLine("<br>Запуск програмы<br>");

				// Connect to database
				var mssql = GLOBAL.FbDatabase.ConnectMssql();
				
				if(mssql==null)	
					return Configuration.ReturnResult;

				var sqlCmd = mssql.CreateCommand();
				sqlCmd.CommandTimeout = 9999999;

				Console.Write("Инициализация базы раджи");
				GLOBAL.FixQuery.InitialProgress += (FixQueryInitialProgress);
				GLOBAL.FbDatabase.ReaderExecOne("");


				string query = " select l.id, l.name0%i from legal0 l where l.deleted=0 ";
				var customerList = new List<string>();
				var customerIdList = new StringBuilder();
				var output = new StringBuilder();
				//customerIdList.Append("(");
				//GLOBAL.FbDatabase.ReaderExec(query);
				//List<string> buf;
				//while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
				//{
				//    customerList.Add(buf[0] + "|" + buf[1]);

				//    if (customerIdList.Length > 3)
				//        customerIdList.Append(",");

				//    customerIdList.Append("'" + buf[0] + "'");
				//}
				//GLOBAL.FbDatabase.ReaderClose();
				//customerIdList.Append(")");


				//c.cName
				query = "select c.cName, cJuridicalName, c.id, sName, dbo.GetShopFullAddress(s.id), s.id \n" +
					" from tblCustomers c \n"+
					" left join tblShops s on s.sCustomerId = c.id and s.deleted = 0 \n" +
					" where  \n" +
					" c.deleted=0 \n"+
					" order by c.cName, s.id ";

				sqlCmd.CommandText = query;
				SqlDataReader reader = sqlCmd.ExecuteReader();
				while (reader.Read())
				{
					output.Append(reader[0] + ";" + reader[1] + ";" + reader[2] + ";(" + reader[3] + ") " + reader[4] + ";" + reader[5] + ";\n");

					//foreach (var customer in customerList)
					//{
					//    if(customer.Split('|')[0]==reader[0].ToString())
					//    {
					//        output.Append(customer.Split('|')[1]+";" + reader[1] + ";" + reader[2] + ";" + reader[3] + ";\n");
					//    }
					//}
				}
				reader.Close();
				mssql.Close();

				if(File.Exists(exportClientShopFackingSheetFile))
				{
					File.Delete(exportClientShopFackingSheetFile);
				}

				TextWriter textWriter = new StreamWriter(exportClientShopFackingSheetFile, false, Encoding.GetEncoding(1251));
				textWriter.Write(output.ToString());
				textWriter.Close();
				GLOBAL.FbDatabase.Disconnect();

				Console.WriteLine("<br>Завершено<br>");

				return 0;
			}

			


			// -----------------------------------------
			// Check data FUNCTION
			if(runCheckFunction)
			{
				if (extendlog) GLOBAL.SysLog.Write("Run multi check product move", MessageType.Information);
				DateTime startDate;
				DateTime endDate;

				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.Write(" * Система проверки движения * \n\n");
				Console.ResetColor();
				Console.Write("Введите дату начала [dd.mm.yyyy]:");
				string buf = Console.ReadLine();

				try
				{

					startDate = DateTime.Parse(buf, System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU"));
				}
				catch (Exception)
				{
					Console.WriteLine("Ошибка разбота даты начала");
					return 1;
				}

				Console.Write("Введите дату окончания [dd.mm.yyyy]:");
				buf = Console.ReadLine();

				try
				{
					endDate = DateTime.Parse(buf, System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU"));
				}
				catch (Exception)
				{
					Console.WriteLine("Ошибка разбота даты окончания");
					return 1;
				}

				Console.WriteLine("Инициализация базы раджи");
				GLOBAL.FixQuery.InitialProgress += (FixQueryInitialProgress);
				GLOBAL.FbDatabase.ReaderExecOne("");

				Console.WriteLine("Запуск провески с " + startDate.ToString("dd.MM.yyyy") + " по " + endDate.ToString("dd.MM.yyyy") + ".");

				var checkRajahDataError = new CheckRajahDataError();
				var result = checkRajahDataError.CheckData(startDate, endDate);
				//var result = checkRajahDataError.CheckDataDetail("04.11.2011");

				Console.Write(result);

				Console.Write("Конец.");
				Console.ReadKey();
				return 0;

			}

			if (extendlog) GLOBAL.SysLog.Write("Caclulate count days to work", MessageType.Information);
			List<DateTime> dateTimeList;

			if(args.Length>3)
			{
				dateTimeList = new List<DateTime>();
				for (int i = 0; i < cmdDeep; i++)
				{
					dateTimeList.Add(DateTime.Now.AddDays(cmdDate-i));
				}
			}
			else
			{
				// Select work date list
				dateTimeList = GLOBAL.Interface.SelectDateFunc();
				if (dateTimeList == null)
				{
					GLOBAL.SysLog.Write(@"No selectted date as start. Programm will be closed.", MessageType.Error);
					return Configuration.ReturnResult;
				}
				Console.Clear();
			}

			//Console.WriteLine("Col:"+dateTimeList.Count);
			//Console.Clear();

			//if (!interfaceOff)
			//	GLOBAL.Interface.Start();

			if (extendlog) GLOBAL.SysLog.Write("build path for xml", MessageType.Information);
			GLOBAL.Interface.WebInterface = webInterface;

			StringBuilder xmlFileBody=null;
			int xmlDeep = 4;

			try
			{
				Configuration.FileNumber++;
				Config.SetValue("FileNumber", Configuration.FileNumber.ToString(), "Main");
				 xmlFileBody = new StringBuilder();
			}
			catch (Exception ex)
			{
				GLOBAL.SysLog.Write("Error start build path for xml. ex:"+ex, MessageType.Information);
			}

			if (xmlFileBody == null)
			{
				GLOBAL.SysLog.Write("Not create xml class.", MessageType.Information);
				return 1;
			}


			if (extendlog) GLOBAL.SysLog.Write("Create dir for reports if not exist", MessageType.Information);
			try
			{
				//check reporting dir and create if not exist
				if (!Directory.Exists(Configuration.ReportingDir))
					Directory.CreateDirectory(Configuration.ReportingDir);
			}
			catch (Exception)
			{
				GLOBAL.SysLog.Write("Error create dir: " + Configuration.ReportingDir,MessageType.Error);
				return 1;
			}

			if (extendlog) GLOBAL.SysLog.Write("Create dir for current report", MessageType.Information);
			try
			{
				//check and create folder for report
				if (!Directory.Exists(Configuration.ReportingDir + "/" + Configuration.FileNumber + "_" + dateTimeList[0].ToString("ddMM") + "-" + dateTimeList[dateTimeList.Count-1].ToString("ddMM") + "_" + Configuration.ZoneNum))
					Directory.CreateDirectory(Configuration.ReportingDir + "/" + Configuration.FileNumber + "_" + dateTimeList[0].ToString("ddMM") + "-" + dateTimeList[dateTimeList.Count - 1].ToString("ddMM") +
											  "_" + Configuration.ZoneNum);
			}
			catch (Exception)
			{
				GLOBAL.SysLog.Write("Error create dir: " + Configuration.ReportingDir + "/" + Configuration.FileNumber + "_" + dateTimeList[0].ToString("ddMM") +
											  "_" + Configuration.ZoneNum, MessageType.Error);
				return 1;
			}

			if (extendlog) GLOBAL.SysLog.Write("build file name for report", MessageType.Information);
			//build file name for report
			string xmlFile = Configuration.ReportingDir + "/" + Configuration.FileNumber + "_" + dateTimeList[0].ToString("ddMM") + "-" + dateTimeList[dateTimeList.Count-1].ToString("ddMM") + "_" + Configuration.ZoneNum + "/" +
			                 "exp_" + Configuration.ZoneNum + "_" + dateTimeList[0].ToString("ddMM") + "_n" + Configuration.FileNumber + ".xml";

			if (extendlog) GLOBAL.SysLog.Write("Write xml header", MessageType.Information);
			//add head report file
			xmlFileBody.Append("<?xml version=\"1.0\" encoding = \"windows-1251\"?>\n");
			xmlFileBody.Append("<!DOCTYPE ФайлЭкспортаДанных SYSTEM \"export.dtd\">\n");
			xmlFileBody.Append("<ФайлЭкспортаДанных КодПлощадки=\"" + Configuration.ZoneNum + "\" Номер=\"" +
				Configuration.FileNumber + "\" ПоследняяДата=\"" + dateTimeList[0].ToString("dd.MM.yyyy") + "\" Глубина=\"" + dateTimeList.Count + "\"  >\n");

			bool errorExist = false;
			
			
			var stopwatch = new Stopwatch();
			int countCycle = 0;
			foreach (var dateTime in dateTimeList)
			{
				stopwatch.Start();
				countCycle++;
				GLOBAL.Interface.SetProgressBar((countCycle * 100) / dateTimeList.Count, countCycle + " день из " + dateTimeList.Count);
				bool getDataFix = false;
				bool rajahInitialized = GLOBAL.FixQuery.Initialized;
				

				GLOBAL.Interface.SetState(ActionState.Init);
				GLOBAL.Interface.AddText(" ");
				GLOBAL.Interface.AddText("Добавление даты: " + dateTime.ToString("dd.MM.yyyy"));

				if (!rajahInitialized)
					Console.Write(" * Инициализация раджи");
				GLOBAL.FixQuery.InitialProgress += (FixQueryInitialProgress);
				GLOBAL.FbDatabase.ReaderExecOne("");
				if (!rajahInitialized)
				{
					Console.Write("\n");
					GLOBAL.MultipackList = DataClass.GetMultipack();
					//GLOBAL.CutTaraList = DataClass.GetCutTara();
				}

				//GLOBAL.Interface.SetState(ActionState.CheckForError);

				List<DataClass.RestItem> restListMain = null;
				List<DataClass.RestItem> restListSoh = null;
                List<DataClass.RestItem> restListSpoilage = null;
                List<DataClass.RestItem> restListSpoilageOld = null;
				List<DataClass.RestItem> restListRes = null;
				List<DataClass.RestItem> restListMainOld = null;
				List<DataClass.RestItem> restListSohOld = null;
				List<DataClass.RestItem> restListResOld = null;

				List<DataClass.ComingOrderItem> comingOrder = null;
				List<DataClass.ReturnCustomerItem> returnCustomer = null;
				List<DataClass.InvoiceItem> invoiceItems = null;
				List<DataClass.DiscardedItem> discardedItems = null;
				List<DataClass.OrderItem> orderItems = null;
				List<DataClass.MoveItem> moveItems = null;
				List<DataClass.ReturnSupplierItem> returnSupplierItems = null;
				List<DataClass.RestVTRTItem> restVTRTItems = null;


				// ********************************  REST  **************************************

				if (Configuration.SkladMain.Length>3)
				{
					GLOBAL.Interface.AddText("Остатки главный склад: " + dateTime.ToString("dd.MM.yyyy"));
					GLOBAL.Interface.SetState(ActionState.GettingRest);
					restListMain = DataClass.GetRest(Configuration.SkladMain, dateTime.ToString("dd.MM.yyyy"), false);
					if (restListMain == null)
					{
					    Configuration.ReturnResult = 4;
					    errorExist = true;
					}
				}

				// Generate error 
				if (errorProductCodeList!=null)
				{
					var prodList = errorProductCodeList.Aggregate("", (current, errorProductCodeItem) => current + ((current.Length > 2 ? "," : " ") + errorProductCodeItem));
					Configuration.ErrorMessage.Append("<table width='100%'><tr bgcolor='#FF0000' style='color:#FFF'>" +
						"<td align='center'> <b> !!!! ВНИМАНИЕ !!!! </b></td></tr><tr bgcolor='#FF0000' style='color:#FFF'><td align='center'><b> Включен генератор ошибок !!!! </b>" +
						" </td></tr><tr bgcolor='#FF0000' style='color:#FFF'><td align='center'><b> Товары: " + prodList + " будут увеличены на `1` в остатках на конец дня. </b>" +
						"</td></tr></table> </ br> </ br> </ br> ");
					foreach (var restItem in restListMain)
					{
						foreach (var errorProductCode in errorProductCodeList)
						{
							if (restItem.Code.ToString() == errorProductCode) restItem.Count += 1;
						}
					}
				}

				if (!errorExist && Configuration.SkladSoh.Length > 3)
				{
					GLOBAL.Interface.AddText("Остатки СОХ склада: " + dateTime.ToString("dd.MM.yyyy"));
					restListSoh = DataClass.GetRest(Configuration.SkladSoh, dateTime.ToString("dd.MM.yyyy"), false);
					if (restListSoh == null)
					{
						Configuration.ReturnResult = 4;
						errorExist = true;
					}
				}


                if (!errorExist && Configuration.SkladSpoilage.Length > 3)
                {
                    GLOBAL.Interface.AddText("Остатки БРАК склада: " + dateTime.ToString("dd.MM.yyyy"));
                    restListSpoilage = DataClass.GetRest(Configuration.SkladSpoilage, dateTime.ToString("dd.MM.yyyy"), false);
                    if (restListSpoilage == null)
                    {
                        Configuration.ReturnResult = 4;
                        errorExist = true;
                    }
                }


				if (!errorExist && Configuration.SkladReserve.Length > 3)
				{
					GLOBAL.Interface.AddText("Остатки резервного склада: " + dateTime.ToString("dd.MM.yyyy"));
					restListRes = DataClass.GetRest(Configuration.SkladReserve, dateTime.ToString("dd.MM.yyyy"), false);
					if (restListRes == null)
					{
						Configuration.ReturnResult = 4;
						errorExist = true;
					}
				}

				// ********************************************************************************

				if (!errorExist && Configuration.SkladMain.Length > 3)
				{
					GLOBAL.Interface.AddText("Остатки главный склад: " + dateTime.AddDays(-1).ToString("dd.MM.yyyy"));
					restListMainOld = DataClass.GetRest(Configuration.SkladMain, dateTime.AddDays(-1).ToString("dd.MM.yyyy"), false);
					if (restListMainOld == null)
					{
						Configuration.ReturnResult = 4;
						errorExist = true;
					}
				}

				if (!errorExist && Configuration.SkladSoh.Length > 3)
				{
					GLOBAL.Interface.AddText("Остатки СОХ склада: " + dateTime.AddDays(-1).ToString("dd.MM.yyyy"));
					restListSohOld = DataClass.GetRest(Configuration.SkladSoh, dateTime.AddDays(-1).ToString("dd.MM.yyyy"), false);
					if (restListSohOld == null)
					{
						Configuration.ReturnResult = 4;
						errorExist = true;
					}
				}

                if (!errorExist && Configuration.SkladSpoilage.Length > 3)
                {
                    GLOBAL.Interface.AddText("Остатки БРАК склада: " + dateTime.AddDays(-1).ToString("dd.MM.yyyy"));
                    restListSpoilageOld = DataClass.GetRest(Configuration.SkladSpoilage, dateTime.AddDays(-1).ToString("dd.MM.yyyy"), false);
                    if (restListSpoilageOld == null)
                    {
                        Configuration.ReturnResult = 4;
                        errorExist = true;
                    }
                }


				if (!errorExist && Configuration.SkladReserve.Length > 3)
				{
					GLOBAL.Interface.AddText("Остатки резервного склада: " + dateTime.AddDays(-1).ToString("dd.MM.yyyy"));
					restListResOld = DataClass.GetRest(Configuration.SkladReserve, dateTime.AddDays(-1).ToString("dd.MM.yyyy"), false);
					if (restListResOld == null)
					{
						Configuration.ReturnResult = 4;
						errorExist = true;
					}
				}

				// ********************************  ComingOrder  **************************************

				if (!errorExist)
				{
					GLOBAL.Interface.SetState(ActionState.Prihod);
					GLOBAL.Interface.AddText("Извлекаем приходы");
					comingOrder = DataClass.GetComingOrder(dateTime.ToString("dd.MM.yyyy"), false);
					if (comingOrder == null)
					{
						Configuration.ReturnResult = 5;
						errorExist = true;
					}
				}

				// ********************************  Return product from customers  **************************************

				if (!errorExist)
				{
					GLOBAL.Interface.SetState(ActionState.VozvratPok);
					GLOBAL.Interface.AddText("Извлекаем возвраты клиентов");
					returnCustomer = DataClass.GetReturnCustomer(dateTime.ToString("dd.MM.yyyy"), false);
					if (returnCustomer == null)
					{
						Configuration.ReturnResult = 6;
						errorExist = true;
					}

				}

				// ********************************  Move orders  **************************************

				if (!errorExist)
				{
					GLOBAL.Interface.SetState(ActionState.Peremeshenie);
					GLOBAL.Interface.AddText("Извлекаем накладные перемещения");
					moveItems = DataClass.GetMove(dateTime.ToString("dd.MM.yyyy"), false);
					if (moveItems == null)
					{
						Configuration.ReturnResult = 7;
						errorExist = true;
					}
				}

				// ********************************  Invoice  **************************************

				

				if (!errorExist)
				{
					GLOBAL.Interface.SetState(ActionState.Prodaga);
					GLOBAL.Interface.AddText("Извлекаем расходные накладные");
					invoiceItems = DataClass.GetInvoice(dateTime.ToString("dd.MM.yyyy"), false);
					if (invoiceItems == null)
					{
						Configuration.ReturnResult = 8;
						errorExist = true;
					}
				}

				// ********************************  Spisanie  **************************************

				if (!errorExist)
				{
					GLOBAL.Interface.SetState(ActionState.Spisanie);
					GLOBAL.Interface.AddText("Извлекаем накладные списания");
					discardedItems = DataClass.GetDiscarded(dateTime.ToString("dd.MM.yyyy"), false);
					if (discardedItems == null)
					{
						Configuration.ReturnResult = 9;
						errorExist = true;
					}
				}

				// ********************************  ReturnSupplier  **************************************

				if (!errorExist)
				{
					GLOBAL.Interface.SetState(ActionState.VozvratPost);
					GLOBAL.Interface.AddText("Извлекаем накладные возврата поставщику");
					returnSupplierItems = DataClass.GetReturnSupplier(dateTime.ToString("dd.MM.yyyy"), false);
					if (returnSupplierItems == null)
					{
						Configuration.ReturnResult = 10;
						errorExist = true;
					}
				}

				// ********************************  Orders  **************************************

				if (!errorExist)
				{
					GLOBAL.Interface.SetState(ActionState.Order);
					GLOBAL.Interface.AddText("Извлекаем заявки");
					orderItems = DataClass.GetOrders(dateTime.ToString("dd.MM.yyyy"));
					if (orderItems == null)
					{
						Configuration.ReturnResult = 11;
						errorExist = true;
					}
				}

				// ********************************  RestVTRT  **************************************

				if (!errorExist)
				{
					GLOBAL.Interface.SetState(ActionState.Order);
					GLOBAL.Interface.AddText("Извлекаем остатки ТРТ");
					restVTRTItems = DataClass.GetRestVTRT(dateTime);
					if (restVTRTItems == null)
					{
						Configuration.ReturnResult = 12;
						errorExist = true;
					}
				}


				// Check product moving, for incorrect data. 

				
				GLOBAL.Interface.AddText("Проверка данных");

				bool validateInfo=true;
				if (!errorExist)
				{
					validateInfo = ValidateDataClass.Check( // return false if error exist
						restListMain, restListSoh,restListSpoilage, restListRes,
						restListMainOld, restListSohOld, restListSpoilageOld, restListResOld,
						comingOrder, returnCustomer, invoiceItems, discardedItems,
						orderItems, moveItems, returnSupplierItems, dateTime,false);
				}

				if (!validateInfo)
				{
					GLOBAL.Interface.SetState(ActionState.SendingMail);
					GLOBAL.Interface.AddText("Отправка письма с ошибкой " + Configuration.MailToError);
					var objArray = new object[]
					               	{
					               		" !!! ОШИБКА XML ( ", Path.GetFileName(xmlFile), " ) в OBOLON ",
					               		DateTime.Now.ToString("dd.MM.yyyy"), " ( ДатаСтарта: ", dateTimeList[0].ToString("dd.MM.yyyy"),
					               		" Глубь:", dateTimeList.Count, " ) !!!"
					               	};
					Services.SendMail(Configuration.MailToError, Configuration.MailToError, Configuration.MailFromError, null,
					                  string.Concat(objArray), Configuration.ErrorMessage.ToString());
					//Thread.Sleep(500);
				}


				// Run again with fix
				if (!validateInfo)
				{
					GLOBAL.Interface.AddText("Попытка обойти все ошибки");
					getDataFix = true;



					// ********************************  REST  **************************************

					if (Configuration.SkladMain.Length > 3)
					{
						GLOBAL.Interface.AddText("Остатки главный склад: " + dateTime.ToString("dd.MM.yyyy"));
						GLOBAL.Interface.SetState(ActionState.GettingRest);
						restListMain = DataClass.GetRest(Configuration.SkladMain, dateTime.ToString("dd.MM.yyyy"), getDataFix);
						if (restListMain == null)
						{
							Configuration.ReturnResult = 4;
							errorExist = true;
						}
					}

					// Generate error 
					if (errorProductCodeList != null)
					{
						var prodList = errorProductCodeList.Aggregate("", (current, errorProductCodeItem) => current + ((current.Length > 2 ? "," : " ") + errorProductCodeItem));
						Configuration.ErrorMessage.Append("<table width='100%'><tr bgcolor='#FF0000' style='color:#FFF'>" +
							"<td align='center'> <b> !!!! ВНИМАНИЕ !!!! </b></td></tr><tr bgcolor='#FF0000' style='color:#FFF'><td align='center'><b> Включен генератор ошибок !!!! </b>" +
							" </td></tr><tr bgcolor='#FF0000' style='color:#FFF'><td align='center'><b> Товары: " + prodList + " будут увеличены на `1` в остатках на конец дня. </b>" +
							"</td></tr></table> </ br> </ br> </ br> ");
						foreach (var restItem in restListMain)
						{
							foreach (var errorProductCode in errorProductCodeList)
							{
								if (restItem.Code.ToString() == errorProductCode) restItem.Count += 1;
							}
						}
					}

					if (!errorExist && Configuration.SkladSoh.Length > 3)
					{
						GLOBAL.Interface.AddText("Остатки СОХ склада: " + dateTime.ToString("dd.MM.yyyy"));
						restListSoh = DataClass.GetRest(Configuration.SkladSoh, dateTime.ToString("dd.MM.yyyy"), getDataFix);
						if (restListSoh == null)
						{
							Configuration.ReturnResult = 4;
							errorExist = true;
						}
					}

                    if (!errorExist && Configuration.SkladSpoilage.Length > 3)
                    {
                        GLOBAL.Interface.AddText("Остатки БРАК склада: " + dateTime.ToString("dd.MM.yyyy"));
                        restListSpoilage = DataClass.GetRest(Configuration.SkladSpoilage, dateTime.ToString("dd.MM.yyyy"), getDataFix);
                        if (restListSpoilage == null)
                        {
                            Configuration.ReturnResult = 4;
                            errorExist = true;
                        }
                    }


					if (!errorExist && Configuration.SkladReserve.Length > 3)
					{
						GLOBAL.Interface.AddText("Остатки резервного склада: " + dateTime.ToString("dd.MM.yyyy"));
						restListRes = DataClass.GetRest(Configuration.SkladReserve, dateTime.ToString("dd.MM.yyyy"), getDataFix);
						if (restListRes == null)
						{
							Configuration.ReturnResult = 4;
							errorExist = true;
						}
					}

					// ********************************************************************************

					if (!errorExist && Configuration.SkladMain.Length > 3)
					{
						GLOBAL.Interface.AddText("Остатки главный склад: " + dateTime.AddDays(-1).ToString("dd.MM.yyyy"));
						restListMainOld = DataClass.GetRest(Configuration.SkladMain, dateTime.AddDays(-1).ToString("dd.MM.yyyy"), getDataFix);
						if (restListMainOld == null)
						{
							Configuration.ReturnResult = 4;
							errorExist = true;
						}
					}

					if (!errorExist && Configuration.SkladSoh.Length > 3)
					{
						GLOBAL.Interface.AddText("Остатки СОХ склада: " + dateTime.AddDays(-1).ToString("dd.MM.yyyy"));
						restListSohOld = DataClass.GetRest(Configuration.SkladSoh, dateTime.AddDays(-1).ToString("dd.MM.yyyy"), getDataFix);
						if (restListSohOld == null)
						{
							Configuration.ReturnResult = 4;
							errorExist = true;
						}
					}

                    if (!errorExist && Configuration.SkladSpoilage.Length > 3)
                    {
                        GLOBAL.Interface.AddText("Остатки БРАК склада: " + dateTime.AddDays(-1).ToString("dd.MM.yyyy"));
                        restListSpoilageOld = DataClass.GetRest(Configuration.SkladSpoilage, dateTime.AddDays(-1).ToString("dd.MM.yyyy"), getDataFix);
                        if (restListSpoilageOld == null)
                        {
                            Configuration.ReturnResult = 4;
                            errorExist = true;
                        }
                    }

					if (!errorExist && Configuration.SkladReserve.Length > 3)
					{
						GLOBAL.Interface.AddText("Остатки резервного склада: " + dateTime.AddDays(-1).ToString("dd.MM.yyyy"));
						restListResOld = DataClass.GetRest(Configuration.SkladReserve, dateTime.AddDays(-1).ToString("dd.MM.yyyy"), getDataFix);
						if (restListResOld == null)
						{
							Configuration.ReturnResult = 4;
							errorExist = true;
						}
					}

					// ********************************  ComingOrder  **************************************

					if (!errorExist)
					{
						GLOBAL.Interface.SetState(ActionState.Prihod);
						GLOBAL.Interface.AddText("Извлекаем приходы с обходом");
						comingOrder = DataClass.GetComingOrder(dateTime.ToString("dd.MM.yyyy"), getDataFix);
						if (comingOrder == null)
						{
							Configuration.ReturnResult = 5;
							errorExist = true;
						}
					}

					// ********************************  Return product from customers  **************************************

					if (!errorExist)
					{
						GLOBAL.Interface.SetState(ActionState.VozvratPok);
						GLOBAL.Interface.AddText("Извлекаем возвраты клиентов с обходом");
						returnCustomer = DataClass.GetReturnCustomer(dateTime.ToString("dd.MM.yyyy"), getDataFix);
						if (returnCustomer == null)
						{
							Configuration.ReturnResult = 6;
							errorExist = true;
						}
					}

					// ********************************  Move orders  **************************************

					if (!errorExist)
					{
						GLOBAL.Interface.SetState(ActionState.Peremeshenie);
						GLOBAL.Interface.AddText("Извлекаем накладные перемещения с обходом");
						moveItems = DataClass.GetMove(dateTime.ToString("dd.MM.yyyy"), getDataFix);
						if (moveItems == null)
						{
							Configuration.ReturnResult = 7;
							errorExist = true;
						}
					}

					// ********************************  Invoice  **************************************

					if (!errorExist)
					{
						GLOBAL.Interface.SetState(ActionState.Prodaga);
						GLOBAL.Interface.AddText("Извлекаем расходные накладные с обходом");
						invoiceItems = DataClass.GetInvoice(dateTime.ToString("dd.MM.yyyy"), getDataFix);
						if (invoiceItems == null)
						{
							Configuration.ReturnResult = 8;
							errorExist = true;
						}
					}

					// ********************************  Spisanie  **************************************

					if (!errorExist)
					{
						GLOBAL.Interface.SetState(ActionState.Spisanie);
						GLOBAL.Interface.AddText("Извлекаем накладные списания с обходом");
						discardedItems = DataClass.GetDiscarded(dateTime.ToString("dd.MM.yyyy"), getDataFix);
						if (discardedItems == null)
						{
							Configuration.ReturnResult = 9;
							errorExist = true;
						}
					}

					// ********************************  ReturnSupplier  **************************************

					if (!errorExist)
					{
						GLOBAL.Interface.SetState(ActionState.VozvratPost);
						GLOBAL.Interface.AddText("Извлекаем накладные возврата поставщику с обходом");
						returnSupplierItems = DataClass.GetReturnSupplier(dateTime.ToString("dd.MM.yyyy"), getDataFix);
						if (returnSupplierItems == null)
						{
							Configuration.ReturnResult = 10;
							errorExist = true;
						}
					}

					// ********************************  Orders  **************************************

					if (!errorExist)
					{
						GLOBAL.Interface.SetState(ActionState.Order);
						GLOBAL.Interface.AddText("Извлекаем заявки с обходом");
						orderItems = DataClass.GetOrders(dateTime.ToString("dd.MM.yyyy"));
						if (orderItems == null)
						{
							Configuration.ReturnResult = 11;
							errorExist = true;
						}
					}

					if (!errorExist)
					{
						validateInfo = ValidateDataClass.Check( // return false if error exist
                            restListMain, restListSoh, restListSpoilage, restListRes,
							restListMainOld, restListSohOld, restListSpoilageOld, restListResOld,
							comingOrder, returnCustomer, invoiceItems, discardedItems,
							orderItems, moveItems, returnSupplierItems, dateTime,true);

						Configuration.ErrorMessage.Append(validateInfo
                          	? "<br /><b>Обход ошибок произведен успешно. Спите спокойно :)</b>"
                          	: "<br /><b>К сожалению обойти ошибку не удалось ! ! ! </b>");

						GLOBAL.Interface.AddText(validateInfo ? "Обход произведен успешно" : "Обойти ошибку не удалось! ");

						errorExist = !validateInfo;
                        if (SendAnyWay) errorExist = false;
					}
					
				}


				//errorExist = !validateInfo ? true : errorExist;

				//List<DataClass.RestItem> restListMain = null;
				//List<DataClass.RestItem> restListSoh = null;
				//List<DataClass.RestItem> restListRes = null;
				//List<DataClass.RestItem> restListMainOld = null;
				//List<DataClass.RestItem> restListSohOld = null;
				//List<DataClass.RestItem> restListResOld = null;

				//List<DataClass.ComingOrderItem> comingOrder = null;
				//List<DataClass.ReturnCustomerItem> returnCustomer = null;
				//List<DataClass.InvoiceItem> invoiceItems = null;
				//List<DataClass.DiscardedItem> discardedItems = null;
				//List<DataClass.OrderItem> orderItems = null;
				//List<DataClass.MoveItem> moveItems = null;
				//List<DataClass.ReturnSupplierItem> returnSupplierItems = null;

				// Convert data to xml Format 

				GLOBAL.Interface.AddText("Преобразование");
				if (!errorExist)
				{
					xmlFileBody.Append("<ЭкспортДанных ДатаДанных=\"" + dateTime.ToString("dd.MM.yyyy") + "\">\n");

					xmlFileBody.Append(DataToXML.Convert(restListMain, restListSoh, restListRes,
					                                     restListMainOld, restListSohOld, restListResOld,
					                                     comingOrder, returnCustomer, invoiceItems, discardedItems,
														 orderItems, moveItems, returnSupplierItems, restVTRTItems, 
														 dateTime.ToString("dd.MM.yyyy")));

					xmlFileBody.Append("</ЭкспортДанных>\n");
				}

    
				if ( !SendAnyWay && errorExist)
				{
					Configuration.ReturnResult = Configuration.ReturnResult > 0 ? Configuration.ReturnResult : 1;
					break;
				}

				stopwatch.Stop();
				long num9 = stopwatch.ElapsedMilliseconds / 1000;
				long realTime = num9;
				stopwatch.Reset();
				timeElepsedFilter.Add(num9);
				num9 = (timeElepsedFilter).Sum() / (timeElepsedFilter.Count);
				long num10 = num9 * (dateTimeList.Count - countCycle);
				long num11 = num10 / 60L;
				long num12 = num10 - (num11 * 60L);
				GLOBAL.SysLog.Write("Date: " + dateTime.ToString("dd-MM-yyyy") + " ProcessingTime: " + realTime + "s.", MessageType.Information);
				GLOBAL.Interface.AddText("Время добавления: " + realTime + "сек. Осталось: " + string.Concat(new object[] { num11, "min ", num12, "sec" }));
				GLOBAL.Interface.TimeToFinish(string.Concat(new object[] { num11, "min ", num12, "sec" }));
			}
			xmlFileBody.Append("</ФайлЭкспортаДанных>\n");

			GLOBAL.Interface.AddText("Запись файла");
			TextWriter writer = new StreamWriter(xmlFile, false, Encoding.GetEncoding("windows-1251"));
			writer.Write(xmlFileBody);
			writer.Close();
			Services.CreateZip(xmlFile, xmlFile + ".zip");
			xmlFile = xmlFile + ".zip";

			if (Configuration.ErrorMessage.Length>=5)
			{
				GLOBAL.SysLog.Write("Exist error message: "+Configuration.ErrorMessage,MessageType.Error);
			}

		    if (SendAnyWay){ errorExist = false; }

			if (!errorExist)
			{
				if (Configuration.SendMessage)
				{
					GLOBAL.Interface.SetState(ActionState.SendingMail);
					GLOBAL.Interface.AddText("Отправка письма " + Configuration.MailTo);
					Services.SendMail(Configuration.MailTo, Configuration.MailToError, Configuration.MailFrom, xmlFile, Path.GetFileName(xmlFile), "XML Report");
					Thread.Sleep(500);
					GLOBAL.Interface.AddText("Отправка письма " + (SendAnyWay?"С ОШИБКОЙ ":"") + Configuration.MailToError);
					var objArray = new object[9];
                    objArray[0] = "Отправка XML " + (SendAnyWay ? "С ОШИБКОЙ " : "") + " ( ";
					objArray[1] = Path.GetFileName(xmlFile);
					objArray[2] = " ) в OBOLON ";
					objArray[3] = DateTime.Now.ToString("dd.MM.yyyy");
					objArray[4] = " ( ДатаСтарта: ";
					objArray[5] = dateTimeList[0].ToString("dd.MM.yyyy");
					objArray[6] = " Глубь:";
					objArray[7] = dateTimeList.Count;
					objArray[8] = " )";
					Services.SendMail(Configuration.MailToError, Configuration.MailToError, Configuration.MailFrom, null, string.Concat(objArray), "XML Report");
					Thread.Sleep(500);
				}
				else
				{
					GLOBAL.SysLog.Write("SendMessage turn off !!!!",MessageType.Warning);
				}

				if(Configuration.ErrorMessage.Length>=5)
				{
					GLOBAL.SysLog.Write("Send error message to MAIL ", MessageType.Error);
					GLOBAL.Interface.SetState(ActionState.SendingMail);
					GLOBAL.Interface.AddText("Отправка письма с ошибкой " + Configuration.MailToError);
					var objArray = new object[] { " !!! ОШИБКА XML ( ", Path.GetFileName(xmlFile), " ) в OBOLON ", DateTime.Now.ToString("dd.MM.yyyy"), " ( ДатаСтарта: ", dateTimeList[0].ToString("dd.MM.yyyy"), " Глубь:", dateTimeList.Count, " ) !!!" };
					Services.SendMail(Configuration.MailToError, Configuration.MailToError, Configuration.MailFromError, xmlFile, string.Concat(objArray), Configuration.ErrorMessage.ToString());
					Thread.Sleep(500);
				}

			}
			else
			{
				try
				{
					File.Move(xmlFile, xmlFile + ".fail.zip");
					xmlFile += ".fail.zip";
				}
				catch (Exception ex)
				{
					GLOBAL.SysLog.Write("Error move file "+xmlFile+". ex:"+ex,MessageType.Error);
					throw;
				}

				Configuration.ReturnResult = 13;
				GLOBAL.Interface.SetState(ActionState.SendingMail);
				GLOBAL.Interface.AddText("Отправка письма с ошибкой " + Configuration.MailToError);
				var objArray = new object[] { " !!! ОШИБКА XML ( ", Path.GetFileName(xmlFile), " ) в OBOLON ", DateTime.Now.ToString("dd.MM.yyyy"), " ( ДатаСтарта: ", dateTimeList[0].ToString("dd.MM.yyyy"), " Глубь:", dateTimeList.Count, " ) !!!" };
				Services.SendMail(Configuration.MailToError, Configuration.MailToError, Configuration.MailFromError, xmlFile, string.Concat(objArray), Configuration.ErrorMessage.ToString());
				Thread.Sleep(500);
			}
			GLOBAL.Interface.Stop();

			//Console.Clear();)
			if (Configuration.ReturnResult > 0) GLOBAL.SysLog.Write("Programm return exitCode: " + Configuration.ReturnResult, MessageType.Error);
			if (!Configuration.PressAnyKey) return Configuration.ReturnResult;
			Console.Write("Press any key"); Console.ReadKey();
			return Configuration.ReturnResult;
		}

		

		static void FixQueryInitialProgress(int persent)
		{
			if ( Int32.Parse((persent / 10).ToString()) != _rajahInitialiger)
			{
				_rajahInitialiger = Int32.Parse((persent/10).ToString());
				Console.Write(".");
			}

			

			//GLOBAL.Interface.SetProgressBar(persent);
		}
	}
}
