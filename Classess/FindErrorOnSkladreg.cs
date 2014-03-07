using System;
using System.Collections.Generic;
using System.Text;

namespace XmlObolon_2._0.Classess
{
	public class FindErrorOnSkladreg
	{
		public string Find()
		{
			Console.WriteLine("\n");
			GLOBAL.FbDatabase.ReaderExec(
			  " select  cast(date_to_str(sm.DATE_) as date), sm.sklad0, sm.goods0, sm.party0, sm.store0, \n" +
			  " sm.posref0, sm.docref0, sm.quan0%p, sm.quan0%m, sm.quanreal0%p, \n"+
			  " sm.quanreal0%m \n"+
			  " from skladreg0%m sm order by 1 desc ");

			List<string> buf;
			long readed = 0;
			long progressShow=0;
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				readed++;
				progressShow++;
				bool show = false;

				if (progressShow>=1000)
				{
					progressShow = 0;
					Console.SetCursorPosition(0,Console.CursorTop);
					Console.Write("                                             ");
					Console.SetCursorPosition(0, Console.CursorTop);
					Console.Write("Readed: " + readed + " LastDate: " + buf[0]);
				}

				if (buf[7].IndexOf('.') != -1 || buf[7].IndexOf(',') != -1)
				{
					show = true;
				}

				if (buf[8].IndexOf('.') != -1 || buf[8].IndexOf(',') != -1)
				{
					show = true;
				}

				if (buf[9].IndexOf('.') != -1 || buf[9].IndexOf(',') != -1)
				{
					show = true;
				}

				if (buf[10].IndexOf('.') != -1 || buf[10].IndexOf(',') != -1)
				{
					show = true;
				}

				Console.WriteLine("["+buf[7]+"]"+"["+buf[8]+"]"+"["+buf[9]+"]"+"["+buf[10]+"]");
				Console.ReadKey();
				if(show)
				{
					Console.WriteLine("Date: " + buf[0] + "\n Sklad:" + buf[1] + "\n Tovar:" + buf[2] +
						"\n Party:" + buf[3] + "\n Sklad:" + buf[4] + "\n PosRef:" + buf[5] +
						"\n DocRef:" + buf[6] + "\n Qp:" + buf[7] + "\n Qm:" + buf[8]
						+ "\n Qrp:" + buf[9] + "\n Qrm:" + buf[10]+"\n\n");
				}
			}


			Console.WriteLine("\n\nFinish!");
			Console.ReadKey();

			return "";
		}


	}
}
