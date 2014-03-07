using System;
using System.Collections.Generic;
using System.Text;
using fnLog2;
using XML_Obolon_Linux;

namespace XmlObolon_2._0.Classess
{
	static class GLOBAL
	{
		public static List<DataClass.MultipackItem> MultipackList { get; set; }
		public static DataClass.CutTara CutTaraList { get; set; }

		private static Log _sysLog = new Log();
		public static Log SysLog
		{
			get { return _sysLog; }
			set { _sysLog = value; }
		}

		private static MpRajah _fixQuery = new MpRajah();
		public static MpRajah FixQuery
		{
			get { return _fixQuery; }
			set { _fixQuery = value; }
		}

		private static MainInterface _interface = new MainInterface();
		public static MainInterface Interface
		{
			get { return _interface; }
			set { _interface = value; }
		}

		private static Database _fbDatabase = new Database();
		public static Database FbDatabase
		{
			get { return _fbDatabase; }
			set { _fbDatabase = value; }
		}

		private static DataClass.CustMsId _customerMS = new DataClass.CustMsId();
		public static DataClass.CustMsId CustomerMS
		{
			get { return _customerMS; }
			set { _customerMS = value; }
		}

		private static DataClass.ShopMs _shopsMS = new DataClass.ShopMs();
		public static DataClass.ShopMs ShopsMS
		{
			get { return _shopsMS; }
			set { _shopsMS = value; }
		}

	}

	static class Configuration
	{

		public static bool DataClassTimeProfile { get; set; }

		public static string ExeuteDir { get; set; }
		public static StringBuilder ErrorMessage { get; set; }
		public static int ReturnResult { get; set; }
		public static bool PressAnyKey { get; set; }
		public static string ZoneNum { get; set; }
		public static bool SendMessage { get; set; }
		public static int FileNumber { get; set; }
		public static string ReportingDir { get; set; }

		public static string SkladMain { get; set; }
		public static string SkladSoh { get; set; }
        public static string SkladSpoilage { get; set; }
		public static string SkladReserve { get; set; }

		public static string DbUser { get; set; }
		public static string DbPassword { get; set; }
		public static string DbHost { get; set; }
		public static string DbDatabase { get; set; }

		public static string MsDbUser { get; set; }
		public static string MsDbPassword { get; set; }
		public static string MsDbHost { get; set; }
		public static string MsDbDatabase { get; set; }

		public static string SmtpHost { get; set; }
		public static int SmtpPort { get; set; }
		public static string SmtpUser { get; set; }
		public static string SmtpPassword { get; set; }
		public static bool SmtpSsl { get; set; }

		public static string ResSmtpHost { get; set; }
		public static int ResSmtpPort { get; set; }
		public static string ResSmtpUser { get; set; }
		public static string ResSmtpPassword { get; set; }
		public static bool ResSmtpSsl { get; set; }

		public static string MailTo { get; set; }
		public static string MailFrom { get; set; }
		public static string MailToError { get; set; }
		public static string MailFromError { get; set; }

	}

	static class ErrorMessage
	{
		public static void Show(string inf,MessageType type)
		{
			Console.WriteLine(inf);
			GLOBAL.SysLog.Write(inf,type);
		}

	}


}
