using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fnLog2;


namespace XmlObolon_2._0.Classess
{

	static class DataClass
	{
		/// <summary>
		/// Fiend error with same skald in Nakladna or Order
		/// </summary>
		/// <param name="date"> Format date (11.01.2011) </param>
		/// <returns> return true is no error </returns>
		public static bool FindError(string date)
		{
			GLOBAL.SysLog.Write("Start find errors ", MessageType.Information);
			GLOBAL.Interface.SetState(ActionState.CheckForError);

			GLOBAL.Interface.AddText("Поиск совмещенных складов в накладных");
			var orderWmsNakl = new List<string>();
			var orderWmsOrd = new List<string>();
			bool returnInfo = false;

			string query1 = "select " +
				"  n.id, n.number0%i, n.name0%i from nakladna0 n " +
				" where " +
				" date_to_str(n.date0) = '" + date + "' and " +
				" (select count(distinct nd.sklad0) from nakldetail0 nd " +
				" where nd.invoce0=n.id " +
				" and nd.deleted=0) >1 " +
				" order by n.number0%i";

			GLOBAL.FbDatabase.ReaderExec(query1);
			List<string> buf;
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				orderWmsNakl.Add("2;" + buf[0] + ";" + buf[1] + ";" + buf[2] + ";");
			}
			GLOBAL.FbDatabase.ReaderClose();

			if (orderWmsNakl.Count > 0)
				GLOBAL.Interface.AddText("Найдено " + orderWmsNakl.Count + " некоректных накладных");


			GLOBAL.Interface.AddText("Поиск совмещенных складов в ордерах");
			query1 = "select \n" +
			" (select count(distinct sk.name0%i) from orddetail0 od, sklad00 sk \n" +
			" where \n" +
			" od.invoce0 = o.id and \n" +
			" od.sklad0 = sk.id and \n" +
			" sk.deleted=0 and od.deleted =0) as SkladCount, \n" +
			" o.id, o.number0%i, o.name0%i \n" +
			" from order0 o where \n" +
			" date_to_str(o.date0) ='" + date + "' and \n" +
			" o.deleted =0 and \n" +
			" (select count(distinct sk.name0%i) from orddetail0 od, sklad00 sk \n" +
			" where \n" +
			" od.invoce0 = o.id and \n" +
			" od.sklad0 = sk.id and \n" +
			" sk.deleted=0 and od.deleted =0) > 1 \n" +
			" order by SkladCount desc";

			GLOBAL.FbDatabase.ReaderExec(query1);

			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				orderWmsOrd.Add(buf[0] + ";" + buf[1] + ";" + buf[2] + ";" + buf[3] + ";");
			}
			GLOBAL.FbDatabase.ReaderClose();

			if (orderWmsOrd.Count > 0)
				GLOBAL.Interface.AddText("Найдено " + orderWmsOrd.Count + " некоректных накладных");


			// convert to HTML
			string html = string.Empty;
			if (orderWmsNakl.Count > 0)
			{
				returnInfo = true;
				html += "<br /><br /><table border='0' style='font-weight:normal;font-size: 11px;'><tr><td> Таблица накладных с совмещенными складами (NAKLADNA) </td></tr><tr><td>";

				html += "<table border='1' style='font-weight:normal;font-size: 11px;'><tr><td>Кол-во складов</td><td>ID</td><td>Номер накладной</td><td>Название операции</td></tr>";
				foreach (var orderwms in orderWmsNakl)
				{
					html += "<tr><td>" + orderwms.Split(';')[0] + "</td><td>" + orderwms.Split(';')[1] + "</td><td>" + orderwms.Split(';')[2] + "</td><td>" + orderwms.Split(';')[3] + "</td></tr>";
				}

				html += "</table>";

				html += "</td></tr></table>";
			}
			else
			{
				html += "<br /> В расходных и возвратных накладных совмещенные склады отсутствуют.";
			}


			if (orderWmsOrd.Count > 0)
			{
				returnInfo = true;
				html += "<br /><br /><table border='0' style='font-weight:normal;font-size: 11px;'><tr><td> Таблица накладных с совмещенными складами (ORDERS) </td></tr><tr><td>";

				html += "<table border='1' style='font-weight:normal;font-size: 11px;'><tr><td>Кол-во складов</td><td>ID</td><td>Номер накладной</td><td>Название операции</td></tr>";
				html = orderWmsOrd.Aggregate(html, (current, orderwms) => current + ("<tr><td>" + orderwms.Split(';')[0] + "</td><td>" + orderwms.Split(';')[1] + "</td><td>" + orderwms.Split(';')[2] + "</td><td>" + orderwms.Split(';')[3] + "</td></tr>"));
				html += "</table>";

				html += "</td></tr></table>";
			}
			else
			{
				html += "<br /> В приходных и возвратных ордерах совмещенные склады отсутствуют.";
			}

			Configuration.ErrorMessage.Append(html);

			return returnInfo;
		}

		// #####################################################################################################
		// Find partiali processed order       ################################################################
		// #####################################################################################################

		public static bool FindErrorInVozvrat(string date)
		{
			var html = new StringBuilder();
			bool returnValue = false;
			string query =
				"select distinct nk.number0%i as naklNum,vz.number0%i as vozvNum, \n" +
				"(select sk.name0%i from sklad00 sk where sk.id=nkd.sklad0 ) as rashod, \n" +
				"(select sk.name0%i from sklad00 sk where sk.id=vzd.sklad0 ) as vozvrat, \n" +
				"cast(date_to_str(vz.date0) as date) as daate \n" +
				"from vozvratorder0 vz, vozvratorddetail0 vzd, \n" +
				"nakladna0 nk, nakldetail0 nkd \n" +
				"where vzd.invoce0 = vz.id \n" +
				"and vz.deleted=0 and vzd.deleted=0 \n" +
				"and nk.deleted =0 and nkd.deleted=0 \n" +
				"and date_to_str(vz.date0) = '" + date + "' \n" +
				"and nk.id = nkd.invoce0 \n" +
				"and vzd.party0 = nkd.id \n" +
				"and (vzd.sklad0 in ('" + Configuration.SkladMain + "','" + Configuration.SkladSoh + "','" + Configuration.SkladReserve + "') " +
				"or nkd.sklad0 in ('" + Configuration.SkladMain + "','" + Configuration.SkladSoh + "','" + Configuration.SkladReserve + "') ) \n" +
				"and vzd.sklad0 <> nkd.sklad0";

			GLOBAL.FbDatabase.ReaderExec(query);
			List<string> buf;
			html.Append("<table> <tr><th> Номер накладной </th><th> Номер возврата </th><th> Склад накладной </th><th> Склад возврата </th><th> Дата </th></tr> ");
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				returnValue = true;
				html.Append("<tr><td>" + buf[0] + "</td><td>" + buf[1] + "</td><td>" + buf[2] + "</td><td>" + buf[3] + "</td><td>" + buf[4] + "</td></tr>");
			}
			GLOBAL.FbDatabase.ReaderClose();
			html.Append("</table>");

			Configuration.ErrorMessage.Append(returnValue ? html.ToString() : "<br /> В возвратах ошибки отсутствуют.");

			return returnValue;
		}


		// #####################################################################################################
		// Find partiali processed order       ################################################################
		// #####################################################################################################

		public static bool FindPPOrder(string date)
		{
			GLOBAL.Interface.AddText("Поиск частично и ошибочно отгр. накл.");
			string html = string.Empty;

			string query = "select n.number0%i, cast(date_to_str(n.date0) as date ), n.skl_prov0 " +
				" from nakladna0 n \n" +
				" where n.shortname0%i like 'РН' and n.skl_prov0 not in ('Да','Нет') and n.deleted='0'  \n" +
				" and cast(date_to_str(n.date0) as date ) ='" + date + "'  \n";

			GLOBAL.FbDatabase.ReaderExec(query);
			List<string> buf;
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				html += "<br>Частично и ошибочно отгр. накл. Расходная накладная [" + buf[0] + "] с отгрузкой [" + buf[2] + "] на дату: " + buf[1] + "";
			}
			GLOBAL.FbDatabase.ReaderClose();


			query = "select o.number0%i, o.skl_prov0, cast(date_to_str(o.date0) as date ) " +
				" from order0 o \n" +
				" where o.shortname0%i like 'ПО' and o.skl_prov0 not in ('Да','Нет') and o.deleted='0'  \n" +
				" and cast(date_to_str(o.date0) as date ) ='" + date + "'  \n";

			GLOBAL.FbDatabase.ReaderExec(query);
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				html += "<br>Частично и ошибочно отгр. накл. Расходная накладная [" + buf[0] + "] с отгрузкой [" + buf[1] + "] на дату: " + buf[2] + "";
			}
			GLOBAL.FbDatabase.ReaderClose();

			if (html.Length > 1)
				Configuration.ErrorMessage.Append(html);

			return html.Length > 1 ? true : false;
		}


		// #####################################################################################################
		// Check for correct data       ########################################################################
		// #####################################################################################################

		public static class CheckData22
		{
			private static readonly List<CheckProductItem> _checkProduct = new List<CheckProductItem>();

			public static List<CheckProductItem> CheckProduct
			{
				get { return _checkProduct; }
			}

			public static void AddProduct(int productId)
			{
				bool exist = _checkProduct.Any(checkProductItem => checkProductItem.ProductId == productId);

				if (!exist)
					_checkProduct.Add(new CheckProductItem(productId));
			}

			public static CheckProductItem GetItem(int id)
			{
				return _checkProduct.FirstOrDefault(checkProductItem => checkProductItem.ProductId == id);
			}
		}

		public class CheckProductItem
		{
			public CheckProductItem(int productId)
			{
				ProductId = productId;
				RestBegin = 0;
				RestEnd = 0;
				Rashod = 0;
				Prihod = 0;
				Spisanie = 0;
				VozvPok = 0;
				VozvPost = 0;
				Peremeshenie = 0;

				RashodList = new List<string>();
				PrihodList = new List<string>();
				SpisanieList = new List<string>();
				VozvPokList = new List<string>();
				VozvPostList = new List<string>();
				PeremeshenieList = new List<string>();
			}

			public int ProductId { get; set; }
			public double RestBegin { get; set; }
			public double RestEnd { get; set; }

			public double Rashod { get; set; }
			public List<string> RashodList { get; set; }
			public double Prihod { get; set; }
			public List<string> PrihodList { get; set; }
			public double Spisanie { get; set; }
			public List<string> SpisanieList { get; set; }
			public double VozvPok { get; set; }
			public List<string> VozvPokList { get; set; }
			public double VozvPost { get; set; }
			public List<string> VozvPostList { get; set; }
			public double Peremeshenie { get; set; }
			public List<string> PeremeshenieList { get; set; }
		}

		// #####################################################################################################
		// REST              ###################################################################################
		// #####################################################################################################

		public class RestItem
		{
			public int Code { get; set; }
			public int Count { get; set; }
			public double Price { get; set; }
			public bool Checked { get; set; }
		}

		public class RestCacheItem
		{
			public string Sklad { get; set; }
			public List<RestItem> RestList { get; set; }
			public DateTime Date { get; set; }
		}

		static private readonly List<RestCacheItem> CachedRestList = new List<RestCacheItem>();

		public static List<RestItem> GetRest(string sklad, string date, bool fix)
		{
			GLOBAL.SysLog.Write("Get rest for sklad: " + sklad + " and date: " + date, MessageType.Information);
			var restList = new List<RestItem>();
			string query;
			List<string> buf;
			var testRestList = new List<RestItem>();
			var ttRestList = new List<RestItem>();
			DateTime cacheRestDate = DateTime.Parse(date, System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU")).AddDays(-41);
			List<RestItem> selectedRestList = null;


			if (!fix)
			{

				bool exist = false;
				foreach (var restCacheItem in CachedRestList)
				{
					if (restCacheItem.Sklad == sklad)
					{
						exist = true;
						selectedRestList = restCacheItem.RestList;
						cacheRestDate = restCacheItem.Date;
						break;
					}
				}

				if (!exist)
				{
					selectedRestList = new List<RestItem>();
					CachedRestList.Add(new RestCacheItem { Sklad = sklad, RestList = selectedRestList, Date = cacheRestDate });


					// Create cache
					query =
						" select w.WIDTH0, " +
						" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,sum(s.QUAN0%P - s.QUAN0%M)/50,iif(max(w.GOODCHARSVALUES10)=30,sum(s.QUAN0%P - s.QUAN0%M)/30,sum(s.QUAN0%P - s.QUAN0%M)) ), sum(s.QUAN0%P - s.QUAN0%M) ),0) , " +                        
						" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,min( pl.PRICE10 )*50,iif(max(w.GOODCHARSVALUES10)=30,min( pl.PRICE10 )*30,min( pl.PRICE10 ))), min( pl.PRICE10 ) ),0),"+
						//" iif(max(w.goodcharsvalues10)=50,sum(s.QUAN0%P - s.QUAN0%M)/50,iif(max(w.goodcharsvalues10)=30,sum(s.QUAN0%P - s.QUAN0%M)/30,sum(s.QUAN0%P - s.QUAN0%M)) )," +
						//" iif(max(w.goodcharsvalues10)=50,min( pl.PRICE10 )*50,iif(max(w.goodcharsvalues10)=30,min( pl.PRICE10 )*30,min( pl.PRICE10 ))),  \n" +
						" s.goods0 \n" +
						" from SKLADREG0%M s, \n" +
						" WAREHOUS0 w,   \n" +
						" PRICELIST0 pl   \n" +
						" where pl.GOODS0 = s.goods0 and pl.sklad0 = s.sklad0 and cast(date_to_str(s.DATE_) as date)<='" + cacheRestDate.ToString("dd.MM.yyyy") + "' and w.deleted=0 \n" +
						" and s.GOODS0 = w.id and w.WIDTH0 > 0 and w.ARTIKUL0%I like '%ОБОЛОНЬ%' and s.SKLAD0 = '" + sklad + "'  \n" + // and s.level_=1 \n" +
						" group by w.WIDTH0,s.goods0 \n" +
						" order by w.WIDTH0 ";

					GLOBAL.FbDatabase.ReaderExec(query);


					if (Configuration.DataClassTimeProfile)
						GLOBAL.SysLog.Write("Extract data from query", MessageType.Information);

					while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
					{
						try
						{
							int code = Int32.Parse(buf[0]);
							bool existProduct = false;

							int count = Int32.Parse(buf[1]);
							double price = double.Parse(buf[2]);

							foreach (var multipackItem in GLOBAL.MultipackList)
							{
								if (buf[3] == multipackItem.ProductId)
								{
									count *= multipackItem.MultipackCount;
									price /= multipackItem.MultipackCount;
								}
							}

							foreach (var selectedRestItem in selectedRestList)
							{
								if (selectedRestItem.Code == code)
								{
									existProduct = true;
									//Console.WriteLine("Sklad: " + sklad + "\tCode:" + restItem.Code + "\t" + Int32.Parse(buf[1]) + "\t" + restItem.Count);
									selectedRestItem.Count = selectedRestItem.Count + count;
								}
							}

							if (!existProduct)
							{
								//Console.WriteLine("Sklad: " + sklad + "\tCode:" + Int32.Parse(buf[0]) + "\t" + Int32.Parse(buf[1]));
								selectedRestList.Add(new RestItem
								{
									Code = Int32.Parse(buf[0]),
									Count = count,
									Price = price,
									Checked = false
								});
							}


						}
						catch (Exception ex)
						{
							GLOBAL.SysLog.Write("Error cast rest type from db." +
												" code:[" + buf[0] + "] count:[" + buf[1] + "] price:[" + buf[2] + "] detail:" + ex,
												MessageType.Error);

							Configuration.ErrorMessage.Append("<br /> <br /> Error cast rest type from db." +
															  " code:[" + buf[0] + "] count:[" + buf[1] + "] price:[" + buf[2] + "] detail:" +
															  ex + "<br /><br />");

							GLOBAL.FbDatabase.ReaderClose();
							return null;
						}

					}
					GLOBAL.FbDatabase.ReaderClose();

				}


				//    // Calculate rest of cache
				query =
					" select w.WIDTH0, " +
					" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,sum(s.QUAN0%P - s.QUAN0%M)/50,iif(max(w.GOODCHARSVALUES10)=30,sum(s.QUAN0%P - s.QUAN0%M)/30,sum(s.QUAN0%P - s.QUAN0%M)) ), sum(s.QUAN0%P - s.QUAN0%M) ),0) , " +
					" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,min( pl.PRICE10 )*50,iif(max(w.GOODCHARSVALUES10)=30,min( pl.PRICE10 )*30,min( pl.PRICE10 ))), min( pl.PRICE10 ) ),0), " +
					//" iif(max(w.goodcharsvalues10)=50,sum(s.QUAN0%P - s.QUAN0%M)/50,iif(max(w.goodcharsvalues10)=30,sum(s.QUAN0%P - s.QUAN0%M)/30,sum(s.QUAN0%P - s.QUAN0%M)) )," +
					//" iif(max(w.goodcharsvalues10)=50,min( pl.PRICE10 )*50,iif(max(w.goodcharsvalues10)=30,min( pl.PRICE10 )*30,min( pl.PRICE10 ))),  \n" +
					" s.goods0 \n" +
					" from SKLADREG0%M s, \n" +
					" WAREHOUS0 w,   \n" +
					" PRICELIST0 pl   \n" +
					" where pl.GOODS0 = s.goods0 and pl.sklad0 = s.sklad0 and cast(date_to_str(s.DATE_) as date)>'" + cacheRestDate.ToString("dd.MM.yyyy") +
					"' and s.GOODS0 = w.id and cast(date_to_str(s.DATE_) as date)<='" + date + "' and w.deleted=0 \n" +
					" and w.WIDTH0 > 0 and w.ARTIKUL0%I like '%ОБОЛОНЬ%' and s.SKLAD0 = '" + sklad + "'  \n" + // and s.level_=1 \n" +
					" group by w.WIDTH0,s.goods0 \n" +
					" order by w.WIDTH0 ";

				GLOBAL.FbDatabase.ReaderExec(query);

				var usedProduct = new List<string>();
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("Extract data from query", MessageType.Information);

				while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
				{
					try
					{
						int code = Int32.Parse(buf[0]);
						bool existProduct = false;

						int count = Int32.Parse(buf[1]);
						double price = double.Parse(buf[2]);

						foreach (var multipackItem in GLOBAL.MultipackList)
						{
							if (buf[3] == multipackItem.ProductId)
							{
								count *= multipackItem.MultipackCount;
								price /= multipackItem.MultipackCount;
							}
						}

						foreach (var testRestItem in testRestList)
						{
							if (testRestItem.Code == code)
							{
								existProduct = true;
								//Console.WriteLine("Sklad: " + sklad + "\tCode:" + restItem.Code + "\t" + Int32.Parse(buf[1]) + "\t" + restItem.Count);
								testRestItem.Count = testRestItem.Count + count;
								usedProduct.Add(testRestItem.Code.ToString());
							}
						}

						if (!existProduct)
						{
							//Console.WriteLine("Sklad: " + sklad + "\tCode:" + Int32.Parse(buf[0]) + "\t" + Int32.Parse(buf[1]));
							testRestList.Add(new RestItem
							{
								Code = Int32.Parse(buf[0]),
								Count = count,
								Price = price,
								Checked = false
							});
						}


					}
					catch (Exception ex)
					{
						GLOBAL.SysLog.Write("Error cast rest type from db." +
											" code:[" + buf[0] + "] count:[" + buf[1] + "] price:[" + buf[2] + "] detail:" + ex,
											MessageType.Error);

						Configuration.ErrorMessage.Append("<br /> <br /> Error cast rest type from db." +
														  " code:[" + buf[0] + "] count:[" + buf[1] + "] price:[" + buf[2] + "] detail:" +
														  ex + "<br /><br />");

						GLOBAL.FbDatabase.ReaderClose();
						return null;
					}

				}
				GLOBAL.FbDatabase.ReaderClose();

				
				// Add selected cahce to current rest
				foreach (var restItem in selectedRestList)
				{
					bool existProduct = false;
					foreach (var item in testRestList)
					{
						if (item.Code == restItem.Code)
						{
							existProduct = true;
							item.Count += restItem.Count;
						}
					}

					if(!existProduct)
					{
						testRestList.Add(new RestItem { Code = restItem.Code, 
							Count = restItem.Count, Price = restItem.Price, 
							Checked = restItem.Checked });
					}


					//ttRestList.AddRange(from item in testRestList
					//					where item.Code == restItem.Code
					//					select new RestItem { Code = item.Code, Count = item.Count + restItem.Count, Checked = false });
				}

			}


















			if (fix)
			{
				// query =
				//" select w.WIDTH0, " +
				//" iif(max(w.goodcharsvalues10)=50,sum(s.QUAN0%P - s.QUAN0%M)/50,iif(max(w.goodcharsvalues10)=30,sum(s.QUAN0%P - s.QUAN0%M)/30,sum(s.QUAN0%P - s.QUAN0%M)) )," +
				//" iif(max(w.goodcharsvalues10)=50,min( pl.PRICE10 )*50,iif(max(w.goodcharsvalues10)=30,min( pl.PRICE10 )*30,min( pl.PRICE10 )))  \n" +
				//" from SKLADREG0%T s \n" +
				//" left join WAREHOUS0 w on  s.GOODS0 = w.id \n" +
				//" left join PRICELIST0 pl on pl.GOODS0 = s.goods0 and pl.sklad0 = s.sklad0 \n" +
				//" where cast(date_to_str(s.DATE_) as date)<='" + date + "' and w.deleted=0 \n" +
				//" and w.WIDTH0 > 0 and w.ARTIKUL0%I like '%ОБОЛОНЬ%' and s.SKLAD0 = '" + sklad + "' and s.level_=1 \n" +
				//" group by w.WIDTH0 \n" +
				//" order by w.WIDTH0 ";


				/////////////////// Work but long time :)
				query =
				" select w.WIDTH0, " +
				" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,sum(s.QUAN0%P - s.QUAN0%M)/50,iif(max(w.GOODCHARSVALUES10)=30,sum(s.QUAN0%P - s.QUAN0%M)/30,sum(s.QUAN0%P - s.QUAN0%M)) ), sum(s.QUAN0%P - s.QUAN0%M) ),0) , " +
				" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,min( pl.PRICE10 )*50,iif(max(w.GOODCHARSVALUES10)=30,min( pl.PRICE10 )*30,min( pl.PRICE10 ))), min( pl.PRICE10 ) ),0)," +
				//" iif(max(w.goodcharsvalues10)=50,sum(s.QUAN0%P - s.QUAN0%M)/50,iif(max(w.goodcharsvalues10)=30,sum(s.QUAN0%P - s.QUAN0%M)/30,sum(s.QUAN0%P - s.QUAN0%M)) )," +
				//" iif(max(w.goodcharsvalues10)=50,min( pl.PRICE10 )*50,iif(max(w.goodcharsvalues10)=30,min( pl.PRICE10 )*30,min( pl.PRICE10 ))),  \n" +
				" s.goods0 \n" +
				" from SKLADREG0%M s \n" +
				" left join WAREHOUS0 w on  s.GOODS0 = w.id \n" +
				" left join PRICELIST0 pl on pl.GOODS0 = s.goods0 and pl.sklad0 = s.sklad0 \n" +
				" where cast(date_to_str(s.DATE_) as date)<='" + date + "' and w.deleted=0 \n" +
				" and w.WIDTH0 > 0 and w.ARTIKUL0%I like '%ОБОЛОНЬ%' and s.SKLAD0 = '" + sklad + "'  \n" + // and s.level_=1 \n" +
				" group by w.WIDTH0,s.goods0 \n" +
				" order by w.WIDTH0 ";

				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("Start execute getRest query", MessageType.Information);

				GLOBAL.FbDatabase.ReaderExec(query);

				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("Extract data from query", MessageType.Information);

				while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
				{
					try
					{
						int code = Int32.Parse(buf[0]);
						bool existProduct = false;

						int count = Int32.Parse(buf[1]);
						double price = double.Parse(buf[2]);

						foreach (var multipackItem in GLOBAL.MultipackList)
						{
							if (buf[3] == multipackItem.ProductId)
							{
								count *= multipackItem.MultipackCount;
								price /= multipackItem.MultipackCount;
							}
						}

						foreach (var restItem in restList)
						{
							if (restItem.Code == code)
							{
								existProduct = true;
								//Console.WriteLine("Sklad: " + sklad + "\tCode:" + restItem.Code + "\t" + Int32.Parse(buf[1]) + "\t" + restItem.Count);
								restItem.Count = restItem.Count + count;
							}
						}

						if (!existProduct)
						{
							//Console.WriteLine("Sklad: " + sklad + "\tCode:" + Int32.Parse(buf[0]) + "\t" + Int32.Parse(buf[1]));
							restList.Add(new RestItem
							{
								Code = Int32.Parse(buf[0]),
								Count = count,
								Price = price,
								Checked = false
							});
						}




					}
					catch (Exception ex)
					{
						GLOBAL.SysLog.Write("Error cast rest type from db." +
							" code:[" + buf[0] + "] count:[" + buf[1] + "] price:[" + buf[2] + "] detail:" + ex, MessageType.Error);

						Configuration.ErrorMessage.Append("<br /> <br /> Error cast rest type from db." +
							" code:[" + buf[0] + "] count:[" + buf[1] + "] price:[" + buf[2] + "] detail:" + ex + "<br /><br />");

						GLOBAL.FbDatabase.ReaderClose();
						return null;
					}

				}

				GLOBAL.FbDatabase.ReaderClose();
			}


			////TEST-------------------------------------------------------------------------

			//foreach (var restItem in ttRestList)
			//{
			//    foreach (var item in restList)
			//    {
			//        if (restItem.Code == item.Code)
			//        {
			//            if (restItem.Count != item.Count)
			//            {
			//                Console.WriteLine("Not match: " + restItem.Code + ". NewF: " + restItem.Count + " OldF: " + item.Count);

			//                foreach (var restItem1 in testRestList)
			//                {
			//                    if (restItem1.Code == restItem.Code)
			//                    {
			//                        Console.WriteLine("Added rest: " + restItem1.Count);
			//                        break;
			//                    }
			//                }

			//                foreach (var restItem1 in selectedRestList)
			//                {
			//                    if (restItem1.Code == restItem.Code)
			//                    {
			//                        Console.WriteLine("Cached rest: " + restItem1.Count);
			//                        break;
			//                    }
			//                }

			//                Console.ReadKey();
			//            }
			//        }
			//    }
			//}


			// END TEST-------------------------------------------------------------------------

			List<RestItem> bufRestList;
			if (fix)
			{
				bufRestList = restList.Where(restItem => restItem.Count > 0).ToList();
				restList.Clear();
			}
			else
			{
				bufRestList = testRestList.Where(restItem => restItem.Count > 0).ToList();
				testRestList.Clear();
			}

			return bufRestList;
		}

		// #####################################################################################################
		// ProductComing          ##############################################################################
		// #####################################################################################################

		public class ComingProductItem
		{
			public int Code { get; set; }
			public int Count { get; set; }
			public double Price { get; set; }
		}

		public class ComingOrderItem
		{
			public ComingOrderItem()
			{
				ComingProductList = new List<ComingProductItem>();
			}

			public List<ComingProductItem> ComingProductList { get; set; }
			public string DocNumber { get; set; }
			public string NaklPostNumber { get; set; }
			public string Date { get; set; }
			public string EDRPOU { get; set; }
			public string CustomerId { get; set; }
			public string CustomerRjId { get; set; }
			public string CustomerName { get; set; }
			public string ID { get; set; }
            public string Store { get; set; }
		}


		public static List<ComingOrderItem> GetComingOrder(string date, bool fix)
		{
			GLOBAL.SysLog.Write("Get incoming order ", MessageType.Information);
			var comingOrderList = new List<ComingOrderItem>();
			string query = string.Empty;

			if (!fix)
			{
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("Set normal query", MessageType.Information);

				query = "select o.id,date_to_str(o.DATE0),o.INCOMDOC0,o.NUMBER0, \n" +
						" min(iif (l.OKPO0>'', l.OKPO0,   \n" +
						" (select na2.NAME0%I from LEGAL0 l2, NUMACCOUNT0 na2, BANKS0 b2   \n" +
						" where na2.PARTNER0=l2.id   \n" +
						" and na2.BANK0=b2.id   \n" +
						" and b2.NAME0%I='ОКПО'  \n" +
						" and l2.id=l.id))) as OKPO,  \n" +
						" l.NAME0, l.id,  \n" +

                        " iif(od.SKLAD0 = '" + Configuration.SkladMain + "','Основной'," +
                        " iif(od.SKLAD0 = '" + Configuration.SkladSoh + "', 'СОХ'," +
                        " iif(od.SKLAD0 = '" + Configuration.SkladSpoilage + "', 'Брак'," +
                        " iif(od.SKLAD0 = '" + Configuration.SkladReserve + "', 'РезервВип','ОЩИБКА')))) " +

						" from ORDER0 o, LEGAL0 l , ORDDETAIL0 od  \n" +
						" where date_to_str(o.DATE0)='" + date + "' and o.skl_prov0 = 'Да'  \n" +
						" and o.deleted = 0 and od.deleted = 0 and o.NAME0%I = 'ПРИХОДНЫЙ ОРДЕР' and od.INVOCE0 = o.id  \n" +
						" and o.PAYER0 = l.id and od.SKLAD0 in ('" + Configuration.SkladMain + "','" + Configuration.SkladSoh +
						"','" + Configuration.SkladReserve + "') \n" +
                        " group by o.id,date_to_str(o.DATE0),o.INCOMDOC0,o.NUMBER0, l.NAME0, l.id, od.SKLAD0  order by o.NUMBER0 ";
			}
			else
			{
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("Set fix query", MessageType.Information);

				query = " select distinct ord.id,date_to_str(ord.DATE0),ord.INCOMDOC0,ord.NUMBER0, \n" +
						" iif ((select lg.OKPO0 from legal0 lg where lg.id = ord.payer0)>'', \n" +
						" (select lg.OKPO0 from legal0 lg where lg.id = ord.payer0), \n" +
						" (select na2.NAME0%I from LEGAL0 l2, NUMACCOUNT0 na2, BANKS0 b2    \n" +
						" where na2.PARTNER0=l2.id    \n" +
						" and na2.BANK0=b2.id    \n" +
						" and b2.NAME0%I='ОКПО'   \n" +
						" and l2.id=ord.payer0)) as OKPO, \n" +
						" (select lg.NAME0 from legal0 lg where lg.id = ord.payer0), \n" +
						" (select lg.id from legal0 lg where lg.id = ord.payer0), \n" +

                        " iif(sm.sklad0 = '" + Configuration.SkladMain + "','Основной'," +
                        " iif(sm.sklad0 = '" + Configuration.SkladSoh + "', 'СОХ'," +
                        " iif(sm.sklad0 = '" + Configuration.SkladSpoilage + "', 'Брак'," +
                        " iif(sm.sklad0 = '" + Configuration.SkladReserve + "', 'РезервВип','ОЩИБКА')))) " +

						" from order0 ord, skladreg0%m sm \n" +
						" where \n" +
						" ord.id = sm.docref0 \n" +
						" and ord.NAME0%I = 'ПРИХОДНЫЙ ОРДЕР' \n" +
						" and sm.sklad0 in ('" + Configuration.SkladMain + "','" + Configuration.SkladSoh + "','" +
						Configuration.SkladReserve + "') \n" +
						" and date_to_str(ord.DATE0)='" + date + "' ";
			}

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Execute order query", MessageType.Information);

			GLOBAL.FbDatabase.ReaderExec(query);
			List<string> buf;

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Extract order data", MessageType.Information);

			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				string edrpou = buf[4];
				if (edrpou.IndexOf('/') != -1)
				{
					edrpou = edrpou.Substring(0, edrpou.IndexOf('/'));
				}

				string naklPos = buf[2];
				string nakP = "";
				if (naklPos.IndexOf('-') != -1)
				{
					for (int i = naklPos.Length - 1; i > 0; i--)
					{
						if (naklPos[i] == '-')
						{
							break;
						}
						nakP = naklPos[i] + nakP;
					}
				}
				else
				{
					nakP = naklPos;
				}

				comingOrderList.Add(new ComingOrderItem
				{
					ID = buf[0],
					DocNumber = buf[3],
					NaklPostNumber = nakP,
					Date = buf[1],
					EDRPOU = edrpou,
					CustomerName = buf[5],
					CustomerId = GLOBAL.CustomerMS.GetNumberById(buf[6]),
					CustomerRjId = buf[6],
                    Store = buf[7]
				});
			}
			GLOBAL.FbDatabase.ReaderClose();
			GLOBAL.Interface.AddText("Обнаружено " + comingOrderList.Count + " накладных");

			int counter = 0;
			foreach (var comingOrderItem in comingOrderList)
			{
				GLOBAL.Interface.SetProgressBar(counter * 100 / comingOrderList.Count);

				if (!fix)
				{
					if (Configuration.DataClassTimeProfile)
						GLOBAL.SysLog.Write("set normal query", MessageType.Information);

					query =
						" select w.WIDTH0, "+
						" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,sum(od.QUAN0)/50,iif(max(w.GOODCHARSVALUES10)=30,sum(od.QUAN0)/30,sum(od.QUAN0)) ), sum(od.QUAN0) ),0) , " +
						" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,od.PRICE0*50,iif(max(w.GOODCHARSVALUES10)=30,od.PRICE0*30,od.PRICE0)), od.PRICE0 ),0)" +
						//" iif(w.goodcharsvalues10=50,sum(od.QUAN0)/50, iif(w.goodcharsvalues10=30,sum(od.QUAN0)/30,sum(od.QUAN0)))," +
						//" iif(w.goodcharsvalues10=50,od.PRICE0*50,iif(w.goodcharsvalues10=30,od.PRICE0*30,od.PRICE0)) \n" +
						" ,od.GOODS0 \n" +
						" from ORDER0 o, ORDDETAIL0 od , WAREHOUS0 w  \n" +
						" where o.deleted = 0 and od.deleted = 0 and o.id = od.INVOCE0 and od.GOODS0 = w.id  \n" +
						" and w.NAME0%I not like '%БУТЫЛКА%' and w.TARA0 = 0  \n" +
						" and w.WIDTH0 >0 and o.id = '" + comingOrderItem.ID + "' and w.ARTIKUL0%I like '%ОБОЛОНЬ%' \n" +
						" group by w.WIDTH0,od.PRICE0,w.goodcharsvalues10,od.GOODS0 ";
				}
				else
				{
					if (Configuration.DataClassTimeProfile)
						GLOBAL.SysLog.Write("set fix query", MessageType.Information);

					query = " select w.WIDTH0, \n" +

							" iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(w.GOODCHARSVALUES10=50, \n" +
							"   sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m))/50, iif(w.GOODCHARSVALUES10=30, \n" +
							"   sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m))/30,sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m)))), sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m))), \n" +

							"   iif(cast(w.WIDTH0 as varchar(8)) like '90%', " +                            
							"   iif(w.GOODCHARSVALUES10=50,(select ordd.price0 from orddetail0 ordd where ordd.id = sm.posref0)*50, \n" +
							"   iif(w.GOODCHARSVALUES10=30,(select ordd.price0 from orddetail0 ordd where ordd.id = sm.posref0)*30, \n" +
							"   (select ordd.price0 from orddetail0 ordd where ordd.id = sm.posref0))),(select ordd.price0 from orddetail0 ordd where ordd.id = sm.posref0)), \n" +
								
						
							//"   sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m))/50, iif(w.GOODCHARSVALUES10=30, \n" +
							//"   sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m))/30,sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m)))), \n" +
							
							//"   iif(w.GOODCHARSVALUES10=50,(select ordd.price0 from orddetail0 ordd where ordd.id = sm.posref0)*50, \n" +
							//"   iif(w.GOODCHARSVALUES10=30,(select ordd.price0 from orddetail0 ordd where ordd.id = sm.posref0)*30, \n" +
							//"   (select ordd.price0 from orddetail0 ordd where ordd.id = sm.posref0))), \n" +
							" sm.goods0 \n" +
							" from order0 ord, skladreg0%m sm,  WAREHOUS0 w  \n" +
							" where \n" +
							"  w.NAME0%I not like '%БУТЫЛКА%' \n" +
							"  and sm.goods0 = w.id \n" +
							"  and w.TARA0 = 0 \n" +
							"  and ord.id = sm.docref0 \n" +
							"  and w.WIDTH0 >0 \n" +
							"  and ord.id = '" + comingOrderItem.ID + "' \n" +
							"  and w.ARTIKUL0%I like '%ОБОЛОНЬ%' \n" +
							"  group by w.WIDTH0,w.GOODCHARSVALUES10, sm.quan0%p , sm.quan0%m, sm.posref0,sm.goods0 ";
				}

				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("Execute order item query", MessageType.Information);

				GLOBAL.FbDatabase.ReaderExec(query);

				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("Extract data", MessageType.Information);

				while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
				{
					try
					{

						int count = Int32.Parse(buf[1]);
						double price = double.Parse(buf[2]);

						foreach (var multipackItem in GLOBAL.MultipackList)
						{
							if (buf[3] == multipackItem.ProductId)
							{
								count *= multipackItem.MultipackCount;
								price /= multipackItem.MultipackCount;
							}
						}

						//foreach (var cutTaraCustomerItem in GLOBAL.CutTaraList.CutTaraCustomerList)
						//{
						//    if(cutTaraCustomerItem.ID==comingOrderItem.CustomerRjId)
						//    {
						//        price = GLOBAL.CutTaraList.CutTaraPriceList.Where(cutTaraPriceItem => buf[3] == cutTaraPriceItem.ProductId).Aggregate(price, (current, cutTaraPriceItem) => current - (current - cutTaraPriceItem.Price));
						//    }
						//}


						comingOrderItem.ComingProductList.Add(new ComingProductItem
						{
							Code = Int32.Parse(buf[0]),
							Count = Math.Abs(count),
							Price = price
						});
					}
					catch (Exception ex)
					{
						Configuration.ErrorMessage.Append("Error cast ComingProductList." +
							" code:" + buf[0] + " Count:" + buf[1] + " Price:" + buf[2] + ".  <br /> <br />Ex:" + ex);
						GLOBAL.SysLog.Write("Error cast ComingProductList." +
							" code:" + buf[0] + " Count:" + buf[1] + " Price:" + buf[2] + ". \n\n ex:" + ex, MessageType.Error);
						GLOBAL.FbDatabase.ReaderClose();
						return null;
					}
				}
				GLOBAL.FbDatabase.ReaderClose();
			}

			return comingOrderList;
		}

		// #####################################################################################################
		// Return product from customers           #############################################################
		// #####################################################################################################


		public class ReturnCustomerProductItem
		{
			public int Code { get; set; }
			public int Count { get; set; }
			public double Price { get; set; }
		}

		public class ReturnCustomerItem
		{
			public ReturnCustomerItem()
			{
				ReturnCustomerProductList = new List<ReturnCustomerProductItem>();
			}

			public List<ReturnCustomerProductItem> ReturnCustomerProductList { get; set; }
			public string DocNumber { get; set; }
			public string NaklPostNumber { get; set; }
			public string Date { get; set; }
			public string EDRPOU { get; set; }
			public string CustomerId { get; set; }
			public string CustomerRjId { get; set; }
			public string CustomerName { get; set; }
			public string ShopId { get; set; }
			public string ShopName { get; set; }
			public string TZ { get; set; }
			public string ID { get; set; }
            public string Store { get; set; }
		}

		public static List<ReturnCustomerItem> GetReturnCustomer(string date, bool fix)
		{
			GLOBAL.SysLog.Write("Get return customers", MessageType.Information);

			var returnCustomerItem = new List<ReturnCustomerItem>();
			string query = string.Empty;
			//=
			//    " select v.id, date_to_str(v.DATE0), v.NUMBER0,  \n" +
			//    " min(iif (l.OKPO0>'', l.OKPO0,   \n" +
			//    " (select na2.NAME0%I from LEGAL0 l2, NUMACCOUNT0 na2, BANKS0 b2   \n" +
			//    " where na2.PARTNER0=l2.id   \n" +
			//    " and na2.BANK0=b2.id   \n" +
			//    " and b2.NAME0%I='ОКПО'  \n" +
			//    " and l2.id=l.id))) as OKPO,  \n" +
			//    " l.NAME0, w.INDEX0, date_to_str(n.date0),n.osnov0, l.id  \n" +
			//    " from NAKLADNA0 n, NAKLDETAIL0 nd,  \n" +
			//    " VOZVRATORDER0 v, VOZVRATORDDETAIL0 vd, LEGAL0 l, WORKERS0 w  \n" +
			//    " where nd.INVOCE0=n.id  \n" +
			//    " and vd.INVOCE0=v.id  \n" +
			//    " and vd.PARTY0=nd.id  \n" +
			//    " and v.PAYER0=l.id  \n" +
			//    " and vd.deleted=0 \n" +
			//    " and v.deleted=0 \n" +
			//    " and n.deleted=0 \n" +
			//    " and nd.deleted=0 \n" +
			//    " and vd.nullrecord=0  \n" +
			//    " and w.id = n.MANAGER0  \n" +
			//    " and cast(date_to_str(v.DATE0) as date)='" + date + "'  \n" +
			//    " and nd.SKLAD0 in ('" + Configuration.SkladMain + "','" + Configuration.SkladSoh + "','" + Configuration.SkladReserve + "')  \n" +
			//    " group by v.id, date_to_str(v.DATE0), v.NUMBER0, l.NAME0, w.INDEX0, date_to_str(n.date0), n.osnov0,l.id \n" +
			//    " order by v.NUMBER0 ";

			if (!fix)
			{
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("set normal query", MessageType.Information);

				query = " select v.id, date_to_str(v.DATE0), v.NUMBER0,  " +
						"\n min(iif (l.OKPO0>'', l.OKPO0,   " +
						"\n (select max( na2.NAME0%I ) from LEGAL0 l2, NUMACCOUNT0 na2, BANKS0 b2   " +
						"\n where na2.PARTNER0=l2.id   " +
						"\n and na2.BANK0=b2.id   " +
						"\n and b2.NAME0%I='ОКПО'  " +
						"\n and l2.id=l.id))) as OKPO,  " +
						"\n l.NAME0, w.INDEX0, date_to_str(n.date0),n.osnov0, l.id,  " +

                        " iif(nd.SKLAD0 = '" + Configuration.SkladMain + "','Основной'," +
                        " iif(nd.SKLAD0 = '" + Configuration.SkladSoh + "', 'СОХ'," +
                        " iif(nd.SKLAD0 = '" + Configuration.SkladSpoilage + "', 'Брак'," +
                        " iif(nd.SKLAD0 = '" + Configuration.SkladReserve + "', 'РезервВип','ОЩИБКА')))) " +
                                               
						"\n from NAKLADNA0 n, NAKLDETAIL0 nd,  " +
						"\n VOZVRATORDER0 v, VOZVRATORDDETAIL0 vd, LEGAL0 l, WORKERS0 w  \n" +
						" where nd.INVOCE0=n.id  \n" +
						" and vd.INVOCE0=v.id  \n" +
						" and vd.PARTY0=nd.id  \n" +
						" and v.PAYER0=l.id  \n" +
						" and vd.deleted=0 \n" +
						" and v.deleted=0 \n" +
						" and n.deleted=0 \n" +
						" and nd.deleted=0 \n" +
						" and vd.nullrecord=0  \n" +
						" and w.id = n.MANAGER0  \n" +
						" and cast(date_to_str(v.DATE0) as date)='" + date + "'  \n" +
						" and nd.SKLAD0 in ('" + Configuration.SkladMain + "','" +
						Configuration.SkladSoh + "','" + Configuration.SkladReserve + "')  " +
						"\n group by v.id, date_to_str(v.DATE0), v.NUMBER0, l.NAME0, " +
                        "w.INDEX0, date_to_str(n.date0), n.osnov0,l.id,nd.SKLAD0 " +
						"\n order by v.NUMBER0 ";
			}
			else
			{
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("set fix query", MessageType.Information);

				query = "select distinct v.id, date_to_str(v.DATE0), v.NUMBER0, \n" +
						" iif ((select lg.OKPO0 from legal0 lg where lg.id = v.PAYER0 )>'', \n" +
						" (select lg.OKPO0 from legal0 lg where lg.id = v.PAYER0 ), \n" +
						" (select max( na2.NAME0%I ) from LEGAL0 l2, NUMACCOUNT0 na2, BANKS0 b2 \n" +
						" where na2.PARTNER0=l2.id \n" +
						" and na2.BANK0=b2.id \n" +
						" and b2.NAME0%I='ОКПО' \n" +
						" and l2.id=v.PAYER0)) as OKPO , \n" +
						" (select lg.name0%i from legal0 lg where lg.id = v.PAYER0 ), \n" +
						" (select min(wr.INDEX0) from  workers0 wr where wr.id = (select distinct min( nk.MANAGER0 ) from nakladna0 nk, nakldetail0 nkd, vozvratorddetail0 vzd \n" +
						" where vzd.PARTY0=nkd.id and nk.id = nkd.invoce0 and vzd.invoce0 = v.id ) ), \n" +
						" (select distinct max( date_to_str(nk.date0) ) from nakladna0 nk, nakldetail0 nkd, vozvratorddetail0 vzd \n" +
						" where vzd.PARTY0=nkd.id and nk.id = nkd.invoce0 and vzd.invoce0 = v.id ), \n" +
						"  (select distinct max( nk.osnov0 ) from nakladna0 nk, nakldetail0 nkd, vozvratorddetail0 vzd \n" +
						" where vzd.PARTY0=nkd.id and nk.id = nkd.invoce0 and vzd.invoce0 = v.id ), \n" +
						" (select min(lg.id) from legal0 lg where lg.id = v.PAYER0 ), \n" +

                        " iif(sm.sklad0 = '" + Configuration.SkladMain + "','Основной'," +
                        " iif(sm.sklad0 = '" + Configuration.SkladSoh + "', 'СОХ'," +
                        " iif(sm.sklad0 = '" + Configuration.SkladSpoilage + "', 'Брак'," +
                        " iif(sm.sklad0 = '" + Configuration.SkladReserve + "', 'РезервВип','ОЩИБКА')))) " +

						" from  VOZVRATORDER0 v, skladreg0%m sm \n" +
						" where sm.docref0 = v.id \n" +
						" and cast(date_to_str(v.DATE0) as date)='" + date + "' \n" +
						" and sm.sklad0 in ('" + Configuration.SkladMain + "','" +
						Configuration.SkladSoh + "','" + Configuration.SkladReserve + "') ";
			}

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Execuete returnCustomer query", MessageType.Information);

			GLOBAL.FbDatabase.ReaderExec(query);
			List<string> buf;

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Extract data", MessageType.Information);

			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				string CustId = buf[8];
				string magazine = buf[7];
				string magazineCode;
				//if ((magazine.IndexOf('[') == -1) || (magazine.IndexOf(']') == -1))
				//{
				//    Configuration.ErrorMessage.Append("<br /> К возврату с номером:" + buf[2] + " привязана расходная накладная в которой не указан магазин. Магазин надо указать! ");
				//    GLOBAL.FbDatabase.ReaderClose();
				//    return null;
				//}

				if ((magazine.IndexOf('[') == -1) || (magazine.IndexOf(']') == -1))
				{
					Configuration.ErrorMessage.Append("<br /> К возврату с номером:" + buf[2] + " привязана расходная накладная в которой не указан магазин. Магазин надо указать! ");
					magazineCode = "";
					magazine = "";
				}
				else
				{
					magazineCode = magazine.Substring(magazine.IndexOf('[') + 1, magazine.IndexOf(']') - 1);
					magazine = magazine.Substring(magazine.IndexOf(']') + 1, magazine.Length - (magazine.IndexOf(']') + 1));
				}

				string edrpou = buf[3];
				if (edrpou.IndexOf('-') != -1)
				{
					int mcol = 0;
					for (int i = 0; i < edrpou.Length; i++)
					{
						if (edrpou[i] == '-')
						{
							mcol++;
						}
						if (mcol == 2)
						{
							edrpou = edrpou.Substring(i, edrpou.Length - i);
						}
					}
				}

				if (edrpou.IndexOf('/') != -1)
				{
					edrpou = edrpou.Substring(0, edrpou.IndexOf('/'));
				}

				bool duplicateExist = false;
				foreach (var customerItem in returnCustomerItem)
				{
					if (customerItem.DocNumber == buf[2])
						duplicateExist = true;
				}

				if (!duplicateExist)
					returnCustomerItem.Add(new ReturnCustomerItem
					{
						CustomerId = GLOBAL.CustomerMS.GetNumberById(CustId),
						CustomerRjId = CustId,
						CustomerName = buf[4],
						EDRPOU = edrpou,
						ShopId = magazineCode,
						ShopName = GLOBAL.ShopsMS.GetNameById(magazineCode), //magazine,
						DocNumber = buf[2],
						NaklPostNumber = string.Empty,
						TZ = buf[5],
						ID = buf[0],
						Date = buf[1],
                        Store = buf[9]
					});
			}
			GLOBAL.FbDatabase.ReaderClose();
			GLOBAL.Interface.AddText("Извлечено " + returnCustomerItem.Count + " накладных");


			GLOBAL.Interface.AddText("Обработка позиций");
			int counter = 0;
			foreach (var customerItem in returnCustomerItem)
			{
				counter++;
				GLOBAL.Interface.SetProgressBar(counter * 100 / returnCustomerItem.Count);

				if (!fix)
				{
					if (Configuration.DataClassTimeProfile)
						GLOBAL.SysLog.Write("set normal query", MessageType.Information);


					query =
						"select w.WIDTH0, \n" +
						//" iif(w.GOODCHARSVALUES10=50,sum(vd.QUAN0)/50, \n" +
						//" iif(w.GOODCHARSVALUES10=30,sum(vd.QUAN0)/30,sum(vd.QUAN0))), \n" +
						" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,sum(vd.QUAN0)/50,iif(max(w.GOODCHARSVALUES10)=30,sum(vd.QUAN0)/30,sum(vd.QUAN0)) ), sum(vd.QUAN0) ),0) , " +

						" (select COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,nd.PRICE0*50,iif(max(w.GOODCHARSVALUES10)=30,nd.PRICE0*30,nd.PRICE0)), nd.PRICE0 ),0) \n" +
						" from nakldetail0 nd where vd.PARTY0=nd.id ), \n" +

						//" (select  iif(w.GOODCHARSVALUES10=50,nd.PRICE0*50, \n" +
						//" iif(w.GOODCHARSVALUES10=30,nd.PRICE0*30,nd.PRICE0)) \n" +
						//" from nakldetail0 nd where vd.PARTY0=nd.id ), \n" +

						" vd.GOODS0 \n" +
						" from \n" +
						" VOZVRATORDER0 v, VOZVRATORDDETAIL0 vd, LEGAL0 l,WAREHOUS0 w   \n" +
						" where \n" +
						" vd.INVOCE0=v.id \n" +
						" and v.PAYER0=l.id \n" +
						" and vd.deleted=0 \n" +
						" and v.deleted=0   \n" +
						" and vd.nullrecord=0 \n" +
						" and vd.GOODS0 = w.id \n" +
						" and v.id = '" + customerItem.ID + "' \n" +
						" and w.WIDTH0 > 0   \n" +
						" and w.NAME0%I not like '%БУТЫЛКА%' and w.ARTIKUL0%I like '%ОБОЛОНЬ%'  \n" +
						" group by w.WIDTH0, w.GOODCHARSVALUES10, l.id, vd.GOODS0,vd.PARTY0 \n" +
						"order by w.WIDTH0 ";

					//" select w.WIDTH0, iif(w.goodcharsvalues10=50,sum(vd.QUAN0)/50, iif(w.goodcharsvalues10=30,sum(vd.QUAN0)/30,sum(vd.QUAN0))), iif(w.goodcharsvalues10=50,nd.PRICE0*50,iif(w.goodcharsvalues10=30,nd.PRICE0*30,nd.PRICE0)), \n" +
					//" vd.GOODS0  \n" +
					//" from NAKLADNA0 n, NAKLDETAIL0 nd,  \n" +
					//" VOZVRATORDER0 v, VOZVRATORDDETAIL0 vd, LEGAL0 l,WAREHOUS0 w  \n" +
					//" where nd.INVOCE0=n.id  \n" +
					//" and vd.INVOCE0=v.id  \n" +
					//" and vd.PARTY0=nd.id  \n" +
					//" and v.PAYER0=l.id  \n" +
					//" and vd.deleted=0  \n" +
					//" and v.deleted=0  \n" +
					//" and n.deleted=0  \n" +
					//" and nd.deleted=0  \n" +
					//" and vd.nullrecord=0  \n" +
					//" and vd.GOODS0 = w.id  \n" +
					//" and v.id = '" + customerItem.ID + "'  \n" +
					//" and w.WIDTH0 > 0  \n" +
					//" and w.NAME0%I not like '%БУТЫЛКА%' and w.ARTIKUL0%I like '%ОБОЛОНЬ%' \n" +
					//" group by w.WIDTH0, nd.PRICE0,w.goodcharsvalues10, l.id, vd.GOODS0 \n" +
					//" order by w.WIDTH0 ";



				}
				else
				{
					if (Configuration.DataClassTimeProfile)
						GLOBAL.SysLog.Write("set fix query", MessageType.Information);


					query = " select (select wh.WIDTH0 from warehous0 wh where wh.id = sm.goods0), \n" +

						" iif((select cast(wh.WIDTH0 as varchar(8)) from WAREHOUS0 wh where wh.id = sm.GOODS0) like '90%', " +                        
						" iif((select IIF(wh.GOODCHARSVALUES10='',0,wh.GOODCHARSVALUES10) from warehous0 wh where wh.id = sm.goods0)=50, \n" +
						"    sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m))/50, \n" +
						" iif((select IIF(wh.GOODCHARSVALUES10='',0,wh.GOODCHARSVALUES10) from warehous0 wh where wh.id = sm.goods0)=30, \n" +
						"    sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m))/30,sum(IIF(sm.quan0%p > 0, \n" +
						"    sm.quan0%p, sm.quan0%m))))  , sum(IIF(sm.quan0%p > 0,sm.quan0%p, sm.quan0%m)) ), \n" +

						" iif((select cast(wh.WIDTH0 as varchar(8)) from WAREHOUS0 wh where wh.id = sm.GOODS0) like '90%', " +                                                
						" iif((select IIF(wh.GOODCHARSVALUES10='',0,wh.GOODCHARSVALUES10) from warehous0 wh where wh.id = sm.goods0)=50, \n" +
						"    (select distinct nkd.price0 from nakldetail0 nkd, vozvratorddetail0 vzd \n" +
						" where vzd.PARTY0=nkd.id and vzd.invoce0 = sm.docref0 and vzd.id = sm.posref0 )*50, \n" +
						" iif((select IIF(wh.GOODCHARSVALUES10='',0,wh.GOODCHARSVALUES10) from warehous0 wh where wh.id = sm.goods0)=30, \n" +
						"     (select distinct nkd.price0 from nakldetail0 nkd, vozvratorddetail0 vzd \n" +
						" where vzd.PARTY0=nkd.id and vzd.invoce0 = sm.docref0 and vzd.id = sm.posref0 )*30, \n" +
						" (select distinct nkd.price0 from nakldetail0 nkd, vozvratorddetail0 vzd \n" +
						" where vzd.PARTY0=nkd.id and vzd.invoce0 = sm.docref0 and vzd.id = sm.posref0 ))) , \n" +
                        " (select distinct nkd.price0 from nakldetail0 nkd, vozvratorddetail0 vzd \n" +
						" where vzd.PARTY0=nkd.id and vzd.invoce0 = sm.docref0 and vzd.id = sm.posref0 )) , \n"+                        

						" sm.goods0 \n" +
						" from  skladreg0%m sm \n" +
						" where sm.docref0 = '" + customerItem.ID + "' \n" +
						" and (select wh.WIDTH0 from warehous0 wh where wh.id = sm.goods0) > 0 \n" +
						" group by  sm.goods0, sm.quan0%p, sm.quan0%m, sm.posref0, sm.docref0,sm.goods0 ";
				}


				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("execute order item query", MessageType.Information);

				GLOBAL.FbDatabase.ReaderExec(query);

				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("extract data", MessageType.Information);

				while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
				{

					try
					{

						int count = Int32.Parse(buf[1]);
						double price = double.Parse(buf[2]);

						// find multipack
						foreach (var multipackItem in GLOBAL.MultipackList)
						{
							if (buf[3] == multipackItem.ProductId)
							{
								count *= multipackItem.MultipackCount;
								price /= multipackItem.MultipackCount;
							}
						}

						//foreach (var cutTaraCustomerItem in GLOBAL.CutTaraList.CutTaraCustomerList)
						//{
						//    if (cutTaraCustomerItem.ID == customerItem.CustomerRjId)
						//    {
						//        price = GLOBAL.CutTaraList.CutTaraPriceList.Where(cutTaraPriceItem => buf[3] == cutTaraPriceItem.ProductId).Aggregate(price, (current, cutTaraPriceItem) => current - (current - cutTaraPriceItem.Price));
						//    }
						//}


						customerItem.ReturnCustomerProductList.Add(new ReturnCustomerProductItem
						{
							Code = Int32.Parse(buf[0]),
							Count = Math.Abs(count),
							Price = price
						});
					}
					catch (Exception ex)
					{
						Configuration.ErrorMessage.Append("Error cast Return product from customers." +
							" code:" + buf[0] + " Count:" + buf[1] + " Price:" + buf[2] + ".  <br /> <br />Ex:" + ex);
						GLOBAL.SysLog.Write("Error cast Return product from customers." +
							" code:" + buf[0] + " Count:" + buf[1] + " Price:" + buf[2] + ". \n\n ex:" + ex, MessageType.Error);
						GLOBAL.FbDatabase.ReaderClose();
						return null;
					}
				}
				GLOBAL.FbDatabase.ReaderClose();

			}

			return returnCustomerItem;
		}

		// #####################################################################################################
		// MOVE product            #############################################################################
		// #####################################################################################################

		public class MoveProductItem
		{
			public int Code { get; set; }
			public int Count { get; set; }
			public double Price { get; set; }
		}

		public class MoveItem
		{
			public MoveItem()
			{
				MoveProductList = new List<MoveProductItem>();
			}

			public List<MoveProductItem> MoveProductList { get; set; }
			public string DocNumber { get; set; }
			public string CustomerId { get; set; }
			public string CustomerRjId { get; set; }
			public string CustomerName { get; set; }
			public string ID { get; set; }
		    public string Store { get; set; }
		}

		public static List<MoveItem> GetMove(string date, bool fix)
		{
			GLOBAL.SysLog.Write("Get move order ", MessageType.Information);

			var moveItemList = new List<MoveItem>();
			string query = string.Empty;

			if (!fix)
			{
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("set normal query", MessageType.Information);

				query =
					" select m.id, m.number0%i, skl2.name0%i, skl2.id, \n" +

                    " iif( m.sklad_in0 = '" + Configuration.SkladMain + "','Основной'," +
                    " iif( m.sklad_in0 = '" + Configuration.SkladSoh + "', 'СОХ'," +
                    " iif( m.sklad_in0 = '" + Configuration.SkladSpoilage + "', 'Брак'," +
                    " iif( m.sklad_in0 = '" + Configuration.SkladReserve + "', 'РезервВип','ОЩИБКА')))) " +

					" from move0 m, sklad0 skl2 where \n" +
					" m.name0%i = 'НАКЛАДНАЯ НА ВНУТРЕННЕЕ ПЕРЕМЕЩЕНИЕ' \n" +
					" and cast( date_to_str(m.date0) as date) = '" + date + "' \n" +
					" and m.sklad_in0 in ('" + Configuration.SkladMain + "','" + Configuration.SkladSoh + "','" +
					Configuration.SkladReserve + "') \n" +
					" and skl2.id = m.sklad_out0 \n" +
					" and m.deleted =0";
			}
			else
			{
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("set fix query", MessageType.Information);

				query = " select distinct m.id, m.NUMBER0%I, \n" +
						" (select skl2.NAME0%I from SKLAD0 skl2 where skl2.id = m.SKLAD_OUT0 ), \n" +
						"  m.SKLAD_OUT0, \n" +

                        " iif( m.sklad_in0 = '" + Configuration.SkladMain + "','Основной'," +
                        " iif( m.sklad_in0 = '" + Configuration.SkladSoh + "', 'СОХ'," +
                        " iif( m.sklad_in0 = '" + Configuration.SkladSpoilage + "', 'Брак'," +
                        " iif( m.sklad_in0 = '" + Configuration.SkladReserve + "', 'РезервВип','ОЩИБКА')))) " +

						"  from skladreg0%m sm, MOVE0 m \n" +
						"  where m.id = sm.docref0 \n" +
						"  and m.NAME0%I = 'НАКЛАДНАЯ НА ВНУТРЕННЕЕ ПЕРЕМЕЩЕНИЕ' \n" +
						"  and cast( date_to_str(m.DATE0) as date) = '" + date + "' \n" +
						"  and m.SKLAD_IN0 in ('" + Configuration.SkladMain + "','" + Configuration.SkladSoh + "','" +
						Configuration.SkladReserve + "') ";
			}

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Execute Move query", MessageType.Information);

			GLOBAL.FbDatabase.ReaderExec(query);
			List<string> buf;

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Extract data", MessageType.Information);

			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				moveItemList.Add(new MoveItem { ID = buf[0], CustomerId = GLOBAL.CustomerMS.GetNumberById(buf[3]), CustomerRjId = buf[3], CustomerName = buf[2], DocNumber = buf[1], Store = buf[4]});
			}
			GLOBAL.FbDatabase.ReaderClose();
			GLOBAL.Interface.AddText("Найдено " + moveItemList.Count + " накладных перемещения ");


			int counter = 0;
			foreach (var moveItem in moveItemList)
			{
				counter++;
				GLOBAL.Interface.SetProgressBar(counter * 100 / moveItemList.Count);

				if (!fix)
				{
					if (Configuration.DataClassTimeProfile)
						GLOBAL.SysLog.Write("Set normal query", MessageType.Information);

					query =
						" select w.width0, "+

						" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,sum(md.QUAN0)/50,iif(max(w.GOODCHARSVALUES10)=30,sum(md.QUAN0)/30,sum(md.QUAN0)) ), sum(md.QUAN0) ),0) , " +
						" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,min(md.postavprice0)*50,iif(max(w.GOODCHARSVALUES10)=30,min(md.postavprice0)*30,min(md.postavprice0))), min(md.postavprice0) ),0)" +
						
						//" iif(w.goodcharsvalues10=50,sum(md.QUAN0)/50,  \n" +
						//" iif(w.goodcharsvalues10=30,sum(md.QUAN0)/30,sum(md.QUAN0))),  \n" +
						//" iif(w.goodcharsvalues10=50,min( md.postavprice0 )*50, \n" +
						//" iif(w.goodcharsvalues10=30,min( md.postavprice0 )*30,min( md.postavprice0 ))), \n" +
						" ,md.goods0 \n" +
						" from movedetail0 md \n" +
						" join warehous0 w on w.id = md.goods0 \n" +
						" where md.deleted = 0 and w.width0 > 0 and w.ARTIKUL0%I like '%ОБОЛОНЬ%' and md.invoce0 = '" + moveItem.ID + "' \n" +
						" group by w.width0, w.goodcharsvalues10,md.goods0";
				}
				else
				{
					if (Configuration.DataClassTimeProfile)
						GLOBAL.SysLog.Write("set fix query", MessageType.Information);


					//query = " select distinct (select wh.WIDTH0 from WAREHOUS0 wh where wh.id = sm.goods0), \n" +
					//        "  iif((select cast(wh.WIDTH0 as varchar(8)) from WAREHOUS0 wh where wh.id = sm.GOODS0) like '90%', " +
					//        " iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.goods0)=50, \n" +
					//        " sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m))/50, \n" +
					//        " iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.goods0)=30, \n" +
					//        " sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m))/30, \n" +
					//        " sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m))))  , sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m)) ), \n" +

					//        " iif((select cast(wh.WIDTH0 as varchar(8)) from WAREHOUS0 wh where wh.id = sm.GOODS0) like '90%', " +
					//        " iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.goods0)=50, \n" +
					//        " min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.posref0) )*50, \n" +
					//        " iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.goods0)=30, \n" +
					//        " min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.posref0) )*30, \n" +
					//        " min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.posref0) ))) , min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.posref0) ) )  , \n" +
					//        " sm.goods0 \n" +
					//        " from skladreg0%m sm \n" +
					//        " where \n" +
					//        " (select wh.WIDTH0 from WAREHOUS0 wh \n" +
					//        " where wh.id = sm.goods0 and wh.ARTIKUL0%I like '%ОБОЛОНЬ%' ) > 0 \n" +
					//        " and sm.docref0 = '" + moveItem.ID + "' \n" +
					//        " group by sm.goods0, sm.quan0%p, sm.quan0%m,sm.goods0 ";


					query = 
							" select distinct wh.WIDTH0 , "+

							" iif(cast(wh.WIDTH0 as varchar(8)) like '90%', "+
							" iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.GOODS0)=50, \n"+
							" sum(IIF(sm.QUAN0%P > 0, sm.QUAN0%P, sm.QUAN0%M))/50,  \n" +
							" iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.GOODS0)=30,  \n"+
							" sum(IIF(sm.QUAN0%P > 0, sm.QUAN0%P, sm.QUAN0%M))/30,  \n" +
							" sum(IIF(sm.QUAN0%P > 0, sm.QUAN0%P, sm.QUAN0%M)))) , sum(IIF(sm.QUAN0%P > 0, sm.QUAN0%P, sm.QUAN0%M)) ), \n" +

							" iif(cast(wh.WIDTH0 as varchar(8)) like '90%', \n"+
							" iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.GOODS0)=50,  \n"+
							" min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.POSREF0) )*50,  \n"+
							" iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.GOODS0)=30,  \n"+
							" min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.POSREF0) )*30,  \n"+
							" min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.POSREF0) ))) , "+
							" min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.POSREF0) )  ), \n" +

							" sm.GOODS0  \n"+
							" from SKLADREG0%M sm, WAREHOUS0 wh \n" +
							" where  \n"+
							" wh.id = sm.GOODS0 \n"+
							" and wh.ARTIKUL0%I like '%ОБОЛОНЬ%' \n" +
							" and sm.DOCREF0 = '" + moveItem.ID + "' \n" +
							" group by sm.GOODS0, sm.QUAN0%P, sm.QUAN0%M,sm.GOODS0, wh.WIDTH0 \n";


				}

				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("execute order item query", MessageType.Information);

				GLOBAL.FbDatabase.ReaderExec(query);

				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("Extaract data", MessageType.Information);

				while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
				{
					try
					{

						int count = Int32.Parse(buf[1]);
						double price = double.Parse(buf[2]);

						foreach (var multipackItem in GLOBAL.MultipackList)
						{
							if (buf[3] == multipackItem.ProductId)
							{
								count *= multipackItem.MultipackCount;
								price /= multipackItem.MultipackCount;
							}
						}

						//foreach (var cutTaraCustomerItem in GLOBAL.CutTaraList.CutTaraCustomerList)
						//{
						//    if (cutTaraCustomerItem.ID == moveItem.CustomerRjId)
						//    {
						//        price = GLOBAL.CutTaraList.CutTaraPriceList.Where(cutTaraPriceItem => buf[3] == cutTaraPriceItem.ProductId).Aggregate(price, (current, cutTaraPriceItem) => current - (current - cutTaraPriceItem.Price));
						//    }
						//}

						moveItem.MoveProductList.Add(new MoveProductItem
						{
							Code = Int32.Parse(buf[0]),
							Count = Math.Abs(count),
							Price = price
						});
					}
					catch (Exception ex)
					{
						Configuration.ErrorMessage.Append("Error cast MOVE order item." +
							" code:" + buf[0] + " Count:" + buf[1] + " Price:" + buf[2] + ".  <br /> <br />Ex:" + ex);
						GLOBAL.SysLog.Write("Error cast MOVE order item." +
							" code:" + buf[0] + " Count:" + buf[1] + " Price:" + buf[2] + ". \n\n ex:" + ex, MessageType.Error);
						GLOBAL.FbDatabase.ReaderClose();
						return null;
					}
				}
				GLOBAL.FbDatabase.ReaderClose();
			}




			return moveItemList;
		}


		// #####################################################################################################
		// Invoice        ######################################################################################
		// #####################################################################################################

		public class InvoiceProductItem
		{
			public int Code { get; set; }
			public int Count { get; set; }
			public double Price { get; set; }
		}

		public class InvoiceItem
		{
			public InvoiceItem()
			{
				InvoiceProductList = new List<InvoiceProductItem>();
			}

			public List<InvoiceProductItem> InvoiceProductList { get; set; }
			public string DocNumber { get; set; }
			//public string NaklPostNumber { get; set; }
			//public string Date { get; set; }
			public string EDRPOU { get; set; }
			public string CustomerId { get; set; }
			public string CustomerRjId { get; set; }
			public string CustomerName { get; set; }
			public string ShopId { get; set; }
			public string ShopName { get; set; }
			public string TZ { get; set; }
			public string ID { get; set; }
			public string Note { get; set; }
            public string Store { get; set; }
		}


		public static List<InvoiceItem> GetInvoice(string date, bool fix)
		{
			GLOBAL.SysLog.Write("Get rashod order ", MessageType.Information);
			//var sw = new Stopwatch();
			//sw.Start();


			var invoiceItemList = new List<InvoiceItem>();
			string query = string.Empty;

			if (!fix)
			{
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("set normal query", MessageType.Information);

				query = " select n.id, min(iif (l.OKPO0>'', l.OKPO0, \n" +
						" (select min(na2.NAME0%I) from LEGAL0 l2, NUMACCOUNT0 na2, BANKS0 b2  \n" +
						" where na2.PARTNER0=l2.id   \n" +
						" and na2.BANK0=b2.id   \n" +
						" and b2.NAME0%I='ОКПО'  \n" +
						" and l2.id=l.id))) as OKPO,  \n" +
						" l.NAME0 ,n.NUMBER0 , \n" +
						" n.OSNOV0, w.INDEX0, l.id, n.note0,  \n" +

                        " iif( nd.SKLAD0 = '" + Configuration.SkladMain + "','Основной'," +
                        " iif( nd.SKLAD0 = '" + Configuration.SkladSoh + "', 'СОХ'," +
                        " iif( nd.SKLAD0 = '" + Configuration.SkladSpoilage + "', 'Брак'," +
                        " iif( nd.SKLAD0 = '" + Configuration.SkladReserve + "', 'РезервВип','ОЩИБКА')))) " +

						" from NAKLADNA0 n, NAKLDETAIL0 nd, LEGAL0 l, WORKERS0 w  \n" +
						" where n.deleted=0 and nd.deleted=0 and n.NAME0%I = 'РАСХОДНАЯ НАКЛАДНАЯ' \n" +
						" and date_to_str(n.DATE0) = '" + date + "' and n.PAYER0 = l.id  \n" +
						" and n.MANAGER0 = w.id and nd.invoce0=n.id \n" +
						" and nd.SKLAD0 in ('" + Configuration.SkladMain + "','" + Configuration.SkladSoh + "','" +
						Configuration.SkladReserve + "') \n" +
                        " group by n.id, l.NAME0 ,n.NUMBER0, n.OSNOV0, w.INDEX0, l.id, n.note0,nd.SKLAD0 \n" +
						" order by n.NUMBER0 ";
			}
			else
			{
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("set fix query", MessageType.Information);

				query = " select distinct n.id, \n" +
					"  iif ((select leg.okpo0 from LEGAL0 leg where leg.id = n.payer0 )>'', (select leg.okpo0 from LEGAL0 leg where leg.id = n.payer0 ), \n" +
					"  (select min(na2.NAME0%I) from LEGAL0 l2, NUMACCOUNT0 na2, BANKS0 b2   \n" +
					"  where na2.PARTNER0=l2.id    \n" +
					"  and na2.BANK0=b2.id    \n" +
					"  and b2.NAME0%I='ОКПО'   \n" +
					"  and l2.id=n.payer0)) as OKPO, \n" +
					"  (select leg.name0%i from LEGAL0 leg where leg.id = n.payer0 ), \n" +
					"   n.number0%i , n.osnov0, \n" +
					"  (select wor.index0 from WORKERS0 wor where wor.id = n.manager0 ), \n" +
					"   n.payer0, n.note0, \n" +

                    " iif( sm.sklad0 = '" + Configuration.SkladMain + "','Основной'," +
                    " iif( sm.sklad0 = '" + Configuration.SkladSoh + "', 'СОХ'," +
                    " iif( sm.sklad0 = '" + Configuration.SkladSpoilage + "', 'Брак'," +
                    " iif( sm.sklad0 = '" + Configuration.SkladReserve + "', 'РезервВип','ОЩИБКА')))) " +

					"  from skladreg0%m sm, NAKLADNA0 n \n" +
					"  where sm.docref0 = n.id \n" +
					"  and n.NAME0%I = 'РАСХОДНАЯ НАКЛАДНАЯ' \n" +
					"  and sm.sklad0 in ('" + Configuration.SkladMain + "','" + Configuration.SkladSoh + "','" +
					Configuration.SkladReserve + "') \n" +
					"  and date_to_str(sm.date_) = '" + date + "' ";
			}

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Execute invoice query", MessageType.Information);

			GLOBAL.FbDatabase.ReaderExec(query);
			List<string> buf;

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Extract data", MessageType.Information);

			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				// GLOBAL.CustomerMS.GetNumberById(buf[6])

				string custId = buf[6];
				string tz = buf[5];
				string trt = buf[4];
				string trtCode = string.Empty;
				if (trt.IndexOf('[') + 1 > 0)
				{
					trtCode = trt.Substring(trt.IndexOf('[') + 1, trt.IndexOf(']') - 1);
				}
				else
				{
					trt = "";
				}
				int p = trt.IndexOf(']') + 1;
				if (p > 0)
					trt = trt.Substring(p, trt.Length - p);

				string nom = buf[3].ToLower();

				string edrpou = (tz.Length < 4 ? "" : buf[1]);
				if (edrpou.IndexOf('/') != -1)
				{
					edrpou = edrpou.Substring(0, edrpou.IndexOf('/'));
				}

				invoiceItemList.Add(new InvoiceItem
				{
					ID = buf[0],
					CustomerId = GLOBAL.CustomerMS.GetNumberById(custId),
					CustomerRjId = custId,
					CustomerName = buf[2],
					Note = buf[7],
					DocNumber = nom,
					EDRPOU = edrpou,
					ShopId = trtCode,
					ShopName = GLOBAL.ShopsMS.GetNameById(trtCode),//trt,
					TZ = (tz.Length < 4 ? "" : tz),
                    Store = buf[8]
				});
			}
			GLOBAL.FbDatabase.ReaderClose();
			GLOBAL.Interface.AddText("Найдено " + invoiceItemList.Count + " расходных накладных");

			if (invoiceItemList.Count <= 0) return invoiceItemList;

			var stringBuilder = new StringBuilder();
			stringBuilder.Append("(");
			foreach (var invoiceItem in invoiceItemList)
			{
				if (stringBuilder.Length > 3) stringBuilder.Append(",");
				stringBuilder.Append("'" + invoiceItem.ID + "'");
			}
			stringBuilder.Append(")");

			if (!fix)
			{
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("set normal query", MessageType.Information);

				query = " select n.id, w.WIDTH0,  \n" +
						" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,sum(nd.QUAN0)/50,iif(max(w.GOODCHARSVALUES10)=30,sum(nd.QUAN0)/30,sum(nd.QUAN0)) ), sum(nd.QUAN0) ),0) , " +
						" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,nd.PRICE0*50,iif(max(w.GOODCHARSVALUES10)=30,nd.PRICE0*30,nd.PRICE0)), nd.PRICE0 ),0)" +

						//" iif(w.goodcharsvalues10=50,sum(nd.QUAN0)/50,"
						//" iif(w.goodcharsvalues10=30,sum(nd.QUAN0)/30,sum(nd.QUAN0))), \n" +
						//" iif(w.goodcharsvalues10=50,nd.PRICE0*50,iif(w.goodcharsvalues10=30,nd.PRICE0*30,nd.PRICE0)) \n" +
						" ,nd.goods0 \n" +
						" from NAKLADNA0 n, NAKLDETAIL0 nd , WAREHOUS0 w \n" +
						" where n.deleted=0 and nd.deleted=0 and n.SKL_PROV0 like 'Да' \n" +
						" and nd.SKLADOTGR0=1  and n.NAME0%I = 'РАСХОДНАЯ НАКЛАДНАЯ' \n" +
						" and nd.INVOCE0 = n.id and nd.goods0 = w.id  and date_to_str(n.DATE0) = '" + date + "' and w.id = nd.GOODS0 \n" +
						" and w.NAME0%I not like '%БУТЫЛКА%' and w.ARTIKUL0%I like '%ОБОЛОНЬ%' and w.TARA0 = 0 and w.WIDTH0 > 0 \n" +
						" group by  n.id,w.WIDTH0,nd.PRICE0,w.goodcharsvalues10,nd.goods0";
			}
			else
			{
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("set fix query", MessageType.Information);


				// form skaldRegM
				query = " select n.id,  w.WIDTH0, \n" +
						" iif(cast(w.WIDTH0 as varchar(8)) like '90%', \n" +                    
						" iif(w.GOODCHARSVALUES10=50,sum( IIF(sm.quan0%p<>0,sm.quan0%p,sm.quan0%m))/50, \n" +
						" iif(w.GOODCHARSVALUES10=30,sum( IIF(sm.quan0%p<>0,sm.quan0%p,sm.quan0%m))/30, \n" +
						" sum( IIF(sm.quan0%p<>0,sm.quan0%p,sm.quan0%m)))) , sum( IIF(sm.quan0%p<>0,sm.quan0%p,sm.quan0%m)) ) , \n" +

						" iif(cast(w.WIDTH0 as varchar(8)) like '90%', \n" + 
						" iif(w.GOODCHARSVALUES10=50,(select nd.PRICE0 from NAKLDETAIL0 nd where nd.id = sm.posref0 )*50, \n" +
						"  iif(w.GOODCHARSVALUES10=30,(select nd.PRICE0 from NAKLDETAIL0 nd where nd.id = sm.posref0 )*30, \n" +
						" (select nd.PRICE0  from NAKLDETAIL0 nd where nd.id = sm.posref0 ))) , (select nd.PRICE0  from NAKLDETAIL0 nd where nd.id = sm.posref0 ) ) \n" +
						
						" ,sm.goods0 \n" +
						" from nakladna0 n, WAREHOUS0 w, skladreg0%m sm \n" +
						" where n.id = sm.docref0 \n" +
						" and n.NAME0%I = 'РАСХОДНАЯ НАКЛАДНАЯ' \n" +
						" and sm.goods0 = w.id \n" +
						" and w.NAME0%I not like '%БУТЫЛКА%' \n" +
						" and w.ARTIKUL0%I like '%ОБОЛОНЬ%' \n" +
						" and w.TARA0 = 0 \n" +
						" and w.WIDTH0 > 0  \n" +
						" and n.id in " + stringBuilder + " \n" +
						" group by  n.id, w.WIDTH0, w.GOODCHARSVALUES10, sm.posref0,sm.goods0 ";
			}

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("execute order item query", MessageType.Information);

			GLOBAL.FbDatabase.ReaderExec(query);

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Extract data", MessageType.Information);


			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{


				foreach (var invoiceItem in invoiceItemList.Where(invoiceItem => invoiceItem.ID == buf[0]))
				{
					try
					{

						int count = Int32.Parse(buf[2]);
						double price = double.Parse(buf[3]);

						foreach (var multipackItem in GLOBAL.MultipackList)
						{
							if (buf[4] == multipackItem.ProductId)
							{
								count *= multipackItem.MultipackCount;
								price /= multipackItem.MultipackCount;
							}
						}

						//foreach (var cutTaraCustomerItem in GLOBAL.CutTaraList.CutTaraCustomerList)
						//{
						//    if (cutTaraCustomerItem.ID == invoiceItem.CustomerRjId)
						//    {
						//        price = GLOBAL.CutTaraList.CutTaraPriceList.Where(cutTaraPriceItem => buf[3] == cutTaraPriceItem.ProductId).Aggregate(price, (current, cutTaraPriceItem) => current - (current - cutTaraPriceItem.Price));
						//    }
						//}

						invoiceItem.InvoiceProductList.Add(new InvoiceProductItem
						{
							Code = Int32.Parse(buf[1]),
							Count = Math.Abs(count),
							Price = price
						});

					}
					catch (Exception ex)
					{
						Configuration.ErrorMessage.Append("Incorrect data in invoice. Product - Code: " + buf[1] + " Count: " + buf[2] + " Price: " + buf[3] + " NaklNum: " + invoiceItem.DocNumber);
						GLOBAL.SysLog.Write("Incorrect data in invoice. Product - Code: " + buf[1] + " Count: " + buf[2] + " Price: " + buf[3] + " NaklNum: " + invoiceItem.DocNumber, MessageType.Error);
						return null;
					}
				}
			}
			GLOBAL.FbDatabase.ReaderClose();


			//sw.Stop();
			//GLOBAL.SysLog.Write("Invoise time: " + sw.ElapsedMilliseconds, MessageType.Information);

			return invoiceItemList;
		}

		//// #####################################################################################################
		//// Orders        ######################################################################################
		//// #####################################################################################################

		public class OrderProductItem
		{
			public int Code { get; set; }
			public int Count { get; set; }
			public double Price { get; set; }
		}

		public class OrderItem
		{
			public OrderItem()
			{
				OrderProductList = new List<OrderProductItem>();
			}

			public List<OrderProductItem> OrderProductList { get; set; }
			public string DocNumber { get; set; }
			//public string NaklPostNumber { get; set; }
			//public string Date { get; set; }
			public string EDRPOU { get; set; }
			public string CustomerId { get; set; }
			public string CustomerRjId { get; set; }
			public string CustomerName { get; set; }
			public string ShopId { get; set; }
			public string ShopName { get; set; }
			public string TZ { get; set; }
			public string ID { get; set; }
			public string Note { get; set; }
			public string CreateDate { get; set; }
            public string Store { get; set; }
		}


		public static List<OrderItem> GetOrders(string date)
		{
			GLOBAL.SysLog.Write("Get quered rashod order ", MessageType.Information);
			//var sw = new Stopwatch();
			//sw.Start();


			var ordersItemList = new List<OrderItem>();

			//string query =
			//        " select n.id, min(iif (l.OKPO0>'', l.OKPO0, \n" +
			//        " (select min(na2.NAME0%I) from LEGAL0 l2, NUMACCOUNT0 na2, BANKS0 b2  \n" +
			//        " where na2.PARTNER0=l2.id   \n" +
			//        " and na2.BANK0=b2.id   \n" +
			//        " and b2.NAME0%I='ОКПО'  \n" +
			//        " and l2.id=l.id))) as OKPO,  \n" +
			//        " l.NAME0 ,n.NUMBER0 , \n" +
			//        " n.OSNOV0, w.INDEX0, l.id, n.note0, \n" +
			//        " date_to_str(n.DATE0)  " +
			//        " from NAKLADNA0 n, NAKLDETAIL0 nd, LEGAL0 l, WORKERS0 w, objects ob \n" +
			//        " where n.deleted=0 and nd.deleted=0 and n.NAME0%I = 'РАСХОДНАЯ НАКЛАДНАЯ' \n" +
			//        //" and date_to_str(n.DATE0) = '" + date + "' "+
			//        " and n.PAYER0 = l.id  \n" +
			//        " and ob.id=n.id and date_to_str(ob.create_time) = '" + date + "' \n" +                    
			//        " and n.MANAGER0 = w.id and nd.invoce0=n.id \n" +
			//        " and nd.SKLAD0 in ('" + Configuration.SkladMain + "','" + Configuration.SkladSoh + "','" + Configuration.SkladReserve + "') \n" +
			//        " group by n.id, l.NAME0 ,n.NUMBER0, n.OSNOV0, w.INDEX0, l.id, n.note0 \n" +
			//        " order by n.NUMBER0 ";

			string query = "select n.id, \n" +
				 " min(iif ( (select l.OKPO0 from LEGAL0 l where l.id=n.PAYER0) >'', \n" +
				 " (select l.OKPO0 from LEGAL0 l where l.id=n.PAYER0), \n" +
				 " (select min(na2.NAME0%I) from LEGAL0 l2, NUMACCOUNT0 na2, BANKS0 b2   \n" +
				 " where na2.PARTNER0=l2.id    \n" +
				 " and na2.BANK0=b2.id    \n" +
				 " and b2.NAME0%I='ОКПО'   \n" +
				 " and l2.id=n.PAYER0))) as OKPO, \n" +
				 " (select l.NAME0 from LEGAL0 l where l.id=n.PAYER0) , \n" +
				 " n.NUMBER0 , \n" +
				 " n.OSNOV0, \n" +
				 " (select w.INDEX0 from WORKERS0 w where  w.id = n.MANAGER0) , \n" +
				 "  n.PAYER0, n.NOTE0 , date_to_str(n.DATE0), \n" +

                 " (select first 1  iif( nd.SKLAD0 = '" + Configuration.SkladMain + "','Основной'," +
                 " iif( nd.SKLAD0 = '" + Configuration.SkladSoh + "', 'СОХ'," +
                 " iif( nd.SKLAD0 = '" + Configuration.SkladSpoilage + "', 'Брак'," +
                 " iif( nd.SKLAD0 = '" + Configuration.SkladReserve + "', 'РезервВип','ОЩИБКА')))) " +
                 "from NAKLDETAIL0 nd where nd.INVOCE0=n.id )"+

				 " from NAKLADNA0 n, objects ob \n" +
				 " where n.deleted=0 \n" +
				 " and n.NAME0%I = 'РАСХОДНАЯ НАКЛАДНАЯ'  \n" +
				 " and n.note0 not like '%БРАК%'  \n" +
				 " and ob.id=n.id \n" +
				 " and date_to_str(ob.create_time) = '" + date + "' \n" +
				 " and (select count(nd.SKLAD0) from NAKLDETAIL0 nd where nd.INVOCE0=n.id and nd.deleted=0 and nd.SKLAD0 in ('" + Configuration.SkladMain + "','" + Configuration.SkladSoh + "','" + Configuration.SkladReserve + "') ) > 0 \n" +
				 " group by n.id, n.NUMBER0, n.OSNOV0, n.PAYER0, n.NOTE0,n.DATE0 ,n.MANAGER0 \n" +
				 " order by n.NUMBER0 ";

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Execute GetOrders query", MessageType.Information);

			GLOBAL.FbDatabase.ReaderExec(query);
			List<string> buf;

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Extract data", MessageType.Information);

			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				// GLOBAL.CustomerMS.GetNumberById(buf[6])

				string custId = buf[6];
				string createTime = buf[8];
				try
				{
					//createTime = DateTime.Parse(createTime).ToString("dd.MM.yyyy");
					createTime = DateTime.Parse(createTime, System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU")).ToString("dd.MM.yyyy");
				}
				catch (Exception)
				{
					throw new Exception("Error conver create date to DateTime");
				}

				string tz = buf[5];
				string trt = buf[4];
				string trtCode = string.Empty;
				if (trt.IndexOf('[') + 1 > 0)
				{
					trtCode = trt.Substring(trt.IndexOf('[') + 1, trt.IndexOf(']') - 1);
				}
				else
				{
					trt = "";
				}
				int p = trt.IndexOf(']') + 1;
				if (p > 0)
					trt = trt.Substring(p, trt.Length - p);

				string nom = buf[3].ToLower();

				string edrpou = (tz.Length < 4 ? "" : buf[1]);
				if (edrpou.IndexOf('/') != -1)
				{
					edrpou = edrpou.Substring(0, edrpou.IndexOf('/'));
				}

				ordersItemList.Add(new OrderItem
				{
					ID = buf[0],
					CustomerId = GLOBAL.CustomerMS.GetNumberById(custId),
					CustomerName = buf[2],
					CustomerRjId = custId,
					Note = buf[7],
					DocNumber = nom,
					EDRPOU = edrpou,
					ShopId = trtCode,
					ShopName = GLOBAL.ShopsMS.GetNameById(trtCode), //trt,
					TZ = (tz.Length < 4 ? "" : tz),
					CreateDate = createTime,
                    Store = buf[9]
				});
			}
			GLOBAL.FbDatabase.ReaderClose();
			GLOBAL.Interface.AddText("Найдено " + ordersItemList.Count + " расходных накладных");

			if (ordersItemList.Count <= 0) return ordersItemList;

			var stringBuilder = new StringBuilder();
			stringBuilder.Append("(");
			foreach (var invoiceItem in ordersItemList)
			{
				if (stringBuilder.Length > 3) stringBuilder.Append(",");
				stringBuilder.Append("'" + invoiceItem.ID + "'");
			}
			stringBuilder.Append(")");

			query = " select n.id, w.WIDTH0, "+
	
					" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,sum(nd.QUAN0)/50,iif(max(w.GOODCHARSVALUES10)=30,sum(nd.QUAN0)/30,sum(nd.QUAN0)) ), sum(nd.QUAN0) ),0) , " +
					" COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,nd.PRICE0*50,iif(max(w.GOODCHARSVALUES10)=30,nd.PRICE0*30,nd.PRICE0)), nd.PRICE0 ),0)" +
						
					//" iif(w.goodcharsvalues10=50,sum(nd.QUAN0)/50, \n" +
					//" iif(w.goodcharsvalues10=30,sum(nd.QUAN0)/30,sum(nd.QUAN0))), \n" +
					//" iif(w.goodcharsvalues10=50,nd.PRICE0*50,iif(w.goodcharsvalues10=30,nd.PRICE0*30,nd.PRICE0)) \n" +
					" ,nd.GOODS0 \n" +
					" from NAKLADNA0 n, NAKLDETAIL0 nd , WAREHOUS0 w, objects ob \n" +
					" where n.deleted=0 and ob.id=n.id and nd.deleted=0 and n.SKL_PROV0 like 'Да' \n" +
					" and nd.SKLADOTGR0=1  and n.NAME0%I = 'РАСХОДНАЯ НАКЛАДНАЯ' \n" +
					" and nd.INVOCE0 = n.id  and date_to_str(ob.create_time) = '" + date + "' and w.id = nd.GOODS0 \n" +
					" and w.NAME0%I not like '%БУТЫЛКА%' and w.ARTIKUL0%I like '%ОБОЛОНЬ%' and w.TARA0 = 0 and w.WIDTH0 > 0 \n" +
					" group by  n.id,w.WIDTH0,nd.PRICE0,w.goodcharsvalues10,nd.GOODS0";


			//" select n.id, w.WIDTH0, iif(w.goodcharsvalues10=50,sum(nd.QUAN0)/50, \n" +
			//        " iif(w.goodcharsvalues10=30,sum(nd.QUAN0)/30,sum(nd.QUAN0))), \n" +
			//        " iif(w.goodcharsvalues10=50,nd.PRICE0*50,iif(w.goodcharsvalues10=30,nd.PRICE0*30,nd.PRICE0)) \n" +
			//        " ,nd.GOODS0 \n" +
			//        " from NAKLADNA0 n, NAKLDETAIL0 nd , WAREHOUS0 w, \n" +
			//        " where n.deleted=0 and nd.deleted=0 and n.SKL_PROV0 like 'Да' \n" +
			//        " and nd.SKLADOTGR0=1  and n.NAME0%I = 'РАСХОДНАЯ НАКЛАДНАЯ' \n" +
			//        " and nd.INVOCE0 = n.id  and n.id in " + stringBuilder + " and w.id = nd.GOODS0 \n" +
			//        " and w.NAME0%I not like '%БУТЫЛКА%' and w.ARTIKUL0%I like '%ОБОЛОНЬ%' and w.TARA0 = 0 and w.WIDTH0 > 0 \n" +
			//        " group by  n.id,w.WIDTH0,nd.PRICE0,w.goodcharsvalues10,nd.GOODS0";

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Execute order item query", MessageType.Information);

			GLOBAL.FbDatabase.ReaderExec(query);

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Extract data", MessageType.Information);

			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				foreach (var invoiceItem in ordersItemList.Where(invoiceItem => invoiceItem.ID == buf[0]))
				{
					try
					{

						int count = Int32.Parse(buf[2]);
						double price = double.Parse(buf[3]);

						foreach (var multipackItem in GLOBAL.MultipackList)
						{
							if (buf[4] == multipackItem.ProductId)
							{
								count *= multipackItem.MultipackCount;
								price /= multipackItem.MultipackCount;
							}
						}

						//foreach (var cutTaraCustomerItem in GLOBAL.CutTaraList.CutTaraCustomerList)
						//{
						//    if (cutTaraCustomerItem.ID == invoiceItem.CustomerRjId)
						//    {
						//        price = GLOBAL.CutTaraList.CutTaraPriceList.Where(cutTaraPriceItem => buf[3] == cutTaraPriceItem.ProductId).Aggregate(price, (current, cutTaraPriceItem) => current - (current - cutTaraPriceItem.Price));
						//    }
						//}

						invoiceItem.OrderProductList.Add(new OrderProductItem
						{
							Code = Int32.Parse(buf[1]),
							Count = Math.Abs(count),
							Price = price
						});
					}
					catch (Exception ex)
					{
						Configuration.ErrorMessage.Append("Incorrect data in order. Product - Code: " + buf[1] + " Count: " + buf[2] + " Price: " + buf[3] + " NaklNum: " + invoiceItem.DocNumber);
						GLOBAL.SysLog.Write("Incorrect data in order. Product - Code: " + buf[1] + " Count: " + buf[2] + " Price: " + buf[3] + " NaklNum: " + invoiceItem.DocNumber, MessageType.Error);
						return null;
					}
				}
			}
			GLOBAL.FbDatabase.ReaderClose();


			//sw.Stop();
			//GLOBAL.SysLog.Write("Invoise time: " + sw.ElapsedMilliseconds, MessageType.Information);

			return ordersItemList;
		}

		// #####################################################################################################
		// ReturnSupplier        ###############################################################################
		// #####################################################################################################

		public class ReturnSupplierItem
		{
			public ReturnSupplierItem()
			{
				this.ReturnSupplierProductList = new List<DataClass.ReturnSupplierProductItem>();
			}

			public string CustomerId { get; set; }
			public string CustomerRjId { get; set; }
			public string CustomerName { get; set; }
			public string DocNumber { get; set; }
			public string EDRPOU { get; set; }
			public string ID { get; set; }
			public List<DataClass.ReturnSupplierProductItem> ReturnSupplierProductList { get; set; }
			public string ShopId { get; set; }
			public string ShopName { get; set; }
            public string Store { get; set; }
		}

		public class ReturnSupplierProductItem
		{
			public int Code { get; set; }
			public int Count { get; set; }
			public double Price { get; set; }
		}

		public static List<ReturnSupplierItem> GetReturnSupplier(string date, bool fix)
		{
			GLOBAL.SysLog.Write("get return suplier order ", MessageType.Information);
			List<string> buf;

			var list = new List<ReturnSupplierItem>();
			string sql = string.Empty;

			if (!fix)
			{
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("set normal query", MessageType.Information);


				sql = " select n.id,   \n" +
					  " min(iif (l.OKPO0>'', l.OKPO0,  \n" +
					  " (select max( na2.NAME0%I ) from LEGAL0 l2, NUMACCOUNT0 na2, BANKS0 b2    \n" +
					  " where na2.PARTNER0=l2.id  \n" +
					  " and na2.BANK0=b2.id \n" +
					  " and b2.NAME0%I='ОКПО' \n" +
					  " and l2.id=l.id))) as OKPO, \n" +
					  " l.NAME0 ,n.NUMBER0 , \n" +
					  " n.OSNOV0 , w.INDEX0, \n" +
					  " l.id, \n" +

                      " iif( nd.SKLAD0 = '" + Configuration.SkladMain + "','Основной'," +
                      " iif( nd.SKLAD0 = '" + Configuration.SkladSoh + "', 'СОХ'," +
                      " iif( nd.SKLAD0 = '" + Configuration.SkladSpoilage + "', 'Брак'," +
                      " iif( nd.SKLAD0 = '" + Configuration.SkladReserve + "', 'РезервВип','ОЩИБКА')))) " +

					  " from NAKLADNA0 n, NAKLDETAIL0 nd, LEGAL0 l, WORKERS0 w, warehous0 wr2  \n" +
					  " where n.deleted=0 and nd.deleted=0 and n.NAME0%I = 'ВОЗВРАТНАЯ НАКЛАДНАЯ'  \n" +
					  " and date_to_str(n.DATE0) = '" + date + "' and n.PAYER0 = l.id  \n" +
					  " and wr2.id = nd.goods0 \n" +
					  " and wr2.NAME0%I not like '%БУТЫЛКА%' and wr2.TARA0 = 0 and wr2.WIDTH0 > 0 \n" +
					  " and n.MANAGER0 = w.id and nd.INVOCE0 = n.id \n" +
					  " and nd.SKLAD0 in ('" + Configuration.SkladMain + "','" + Configuration.SkladSoh + "','" +
					  Configuration.SkladReserve + "')  \n" +
                      " group by n.id, l.NAME0 ,n.NUMBER0, n.OSNOV0, w.INDEX0, l.id,nd.SKLAD0 \n" +
					  " order by n.NUMBER0";
			}
			else
			{
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("set fix query", MessageType.Information);

				sql = " select distinct n.id, \n" +
					  "  iif ((select leg.okpo0 from LEGAL0 leg where leg.id = n.payer0 )>'', (select leg.okpo0 from LEGAL0 leg where leg.id = n.payer0 ), \n" +
					  "  (select min(na2.NAME0%I) from LEGAL0 l2, NUMACCOUNT0 na2, BANKS0 b2   \n" +
					  "  where na2.PARTNER0=l2.id    \n" +
					  "  and na2.BANK0=b2.id    \n" +
					  "  and b2.NAME0%I='ОКПО'   \n" +
					  "  and l2.id=n.payer0)) as OKPO, \n" +
					  "  (select leg.name0%i from LEGAL0 leg where leg.id = n.payer0 ), \n" +
					  "   n.number0%i , n.osnov0, \n" +
					  "  (select wor.index0 from WORKERS0 wor where wor.id = n.manager0 ), \n" +
					  "   n.payer0, \n" +

                      " iif( sm.sklad0 = '" + Configuration.SkladMain + "','Основной'," +
                      " iif( sm.sklad0 = '" + Configuration.SkladSoh + "', 'СОХ'," +
                      " iif( sm.sklad0 = '" + Configuration.SkladSpoilage + "', 'Брак'," +
                      " iif( sm.sklad0 = '" + Configuration.SkladReserve + "', 'РезервВип','ОЩИБКА')))) " +

					  "  from skladreg0%m sm, NAKLADNA0 n \n" +
					  "  where sm.docref0 = n.id \n" +
					  "  and n.NAME0%I = 'ВОЗВРАТНАЯ НАКЛАДНАЯ' \n" +
					  "  and sm.sklad0 in ('" + Configuration.SkladMain + "','" + Configuration.SkladSoh + "','" +
					  Configuration.SkladReserve + "') \n" +
					  "  and date_to_str(sm.date_) = '" + date + "' ";
			}

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Execute query", MessageType.Information);

			if (!GLOBAL.FbDatabase.ReaderExec(sql))
			{
				return null;
			}

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Extract data", MessageType.Information);


			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				string str2 = buf[4];
				string str3 = string.Empty;
				if ((str2.IndexOf('[') + 1) > 0)
				{
					str3 = str2.Substring(str2.IndexOf('[') + 1, str2.IndexOf(']') - 1);
				}
				string id = buf[6];
				int startIndex = str2.IndexOf(']') + 1;
				if (startIndex > 0)
				{
					str2 = str2.Substring(startIndex, str2.Length - startIndex);
				}
				string str5 = buf[3].ToLower();
				string str6 = buf[1];
				if (str6.IndexOf('/') != -1)
				{
					str6 = str6.Substring(0, str6.IndexOf('/'));
				}
				var item = new ReturnSupplierItem
				{
					ID = buf[0],
					CustomerId = GLOBAL.CustomerMS.GetNumberById(id),
					CustomerName = buf[2],
					CustomerRjId = id,
					DocNumber = str5,
					EDRPOU = str6,
					ShopId = str3,
					ShopName = GLOBAL.ShopsMS.GetNameById(str3), //str2
                    Store = buf[7]
				};
				list.Add(item);
			}
			GLOBAL.FbDatabase.ReaderClose();
			GLOBAL.Interface.AddText("Найдено " + list.Count + " накладных возврата поставщику");
			//int count = 0;
			foreach (ReturnSupplierItem item2 in list)
			{
				//count++;
				//GLOBAL.Interface.SetProgressBar((count * 100) / list.Count);

				if (!fix)
				{
					if (Configuration.DataClassTimeProfile)
						GLOBAL.SysLog.Write("set normal query", MessageType.Information);


					sql = " select w.WIDTH0,"+

						  " COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,sum(nd.QUAN0)/50,iif(max(w.GOODCHARSVALUES10)=30,sum(nd.QUAN0)/30,sum(nd.QUAN0)) ), sum(nd.QUAN0) ),0) , " +
						  " COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,nd.PRICE0*50,iif(max(w.GOODCHARSVALUES10)=30,nd.PRICE0*30,nd.PRICE0)), nd.PRICE0 ),0)" +

						  //" iif(w.goodcharsvalues10=50,sum(nd.QUAN0)/50, \n" +
						  //" iif(w.goodcharsvalues10=30,sum(nd.QUAN0)/30,sum(nd.QUAN0))),  \n" +
						  //"iif(w.goodcharsvalues10=50,nd.PRICE0*50, \n" +
						  //"iif(w.goodcharsvalues10=30,nd.PRICE0*30,nd.PRICE0))  \n" +
						  " ,nd.GOODS0 \n" +
						  " from NAKLADNA0 n, NAKLDETAIL0 nd , WAREHOUS0 w  \n" +
						  " where n.deleted=0 and nd.deleted=0  \n" +
						  " and n.NAME0%I = 'ВОЗВРАТНАЯ НАКЛАДНАЯ'  \n" +
						  " and nd.INVOCE0 = n.id and n.id = '" + item2.ID + "'  \n" +
						  " and w.id = nd.GOODS0 and w.NAME0%I not like '%БУТЫЛКА%'  \n" +
						  " and w.TARA0 = 0 and w.WIDTH0>0 and w.ARTIKUL0%I like '%ОБОЛОНЬ%'  \n" +
						  " group by w.WIDTH0, nd.PRICE0, w.goodcharsvalues10,nd.GOODS0";
				}
				else
				{
					if (Configuration.DataClassTimeProfile)
						GLOBAL.SysLog.Write("set fix query", MessageType.Information);


					// form skaldRegM
					sql = " select w.WIDTH0, \n" +

							" iif(cast(w.WIDTH0 as varchar(8)) like '90%', " +
							" iif(w.GOODCHARSVALUES10=50,sum( IIF(sm.quan0%p<>0,sm.quan0%p,sm.quan0%m))/50, \n" +
							" iif(w.GOODCHARSVALUES10=30,sum( IIF(sm.quan0%p<>0,sm.quan0%p,sm.quan0%m))/30, \n" +
							" sum( IIF(sm.quan0%p<>0,sm.quan0%p,sm.quan0%m)))) ,sum( IIF(sm.quan0%p<>0,sm.quan0%p,sm.quan0%m)) ) , \n" +

							" iif(cast(w.WIDTH0 as varchar(8)) like '90%', " +
							" iif(w.GOODCHARSVALUES10=50,(select nd.PRICE0 from NAKLDETAIL0 nd where nd.id = sm.posref0 )*50, \n" +
							"  iif(w.GOODCHARSVALUES10=30,(select nd.PRICE0 from NAKLDETAIL0 nd where nd.id = sm.posref0 )*30, \n" +
							" (select nd.PRICE0  from NAKLDETAIL0 nd where nd.id = sm.posref0 ))) ,(select nd.PRICE0  from NAKLDETAIL0 nd where nd.id = sm.posref0 )) \n" +
							
							" ,sm.goods0 \n" +
							" from nakladna0 n, WAREHOUS0 w, skladreg0%m sm \n" +
							" where n.id = sm.docref0 \n" +
							" and n.NAME0%I = 'ВОЗВРАТНАЯ НАКЛАДНАЯ' \n" +
							" and sm.goods0 = w.id \n" +
							" and w.NAME0%I not like '%БУТЫЛКА%' \n" +
							" and w.ARTIKUL0%I like '%ОБОЛОНЬ%' \n" +
							" and w.TARA0 = 0 \n" +
							" and w.WIDTH0 > 0  \n" +
							" and n.id = '" + item2.ID + "' \n" +
							" group by  n.id, w.WIDTH0, w.GOODCHARSVALUES10, sm.posref0,sm.goods0 ";
				}

				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("Execute order item query", MessageType.Information);

				if (!GLOBAL.FbDatabase.ReaderExec(sql))
				{
					GLOBAL.SysLog.Write(GLOBAL.FbDatabase.LastError, MessageType.Error);
					return null;
				}

				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("Extract data", MessageType.Information);

				while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
				{
					try
					{
						int count = Int32.Parse(buf[1]);
						double price = double.Parse(buf[2]);

						foreach (var multipackItem in GLOBAL.MultipackList)
						{
							if (buf[3] == multipackItem.ProductId)
							{
								count *= multipackItem.MultipackCount;
								price /= multipackItem.MultipackCount;
							}
						}


						//foreach (var cutTaraCustomerItem in GLOBAL.CutTaraList.CutTaraCustomerList)
						//{
						//    if (cutTaraCustomerItem.ID == item2.CustomerRjId)
						//    {
						//        price = GLOBAL.CutTaraList.CutTaraPriceList.
						//            Where(cutTaraPriceItem => buf[3] == cutTaraPriceItem.ProductId).
						//            Aggregate(price, (current, cutTaraPriceItem) => current - (current - cutTaraPriceItem.Price));
						//    }
						//}

						var item3 = new ReturnSupplierProductItem
						{
							Code = int.Parse(buf[0]),
							Count = Math.Abs(count),
							Price = price
						};
						item2.ReturnSupplierProductList.Add(item3);
					}
					catch (Exception exception)
					{
						Configuration.ErrorMessage.Append(string.Concat(new object[] { "Error cast Return supplier order item. code:", buf[0], " Count:", buf[1], " Price:", buf[2], ".  <br /> <br />Ex:", exception }));
						GLOBAL.SysLog.Write(string.Concat(new object[] { "Error cast Return supplier order item. code:", buf[0], " Count:", buf[1], " Price:", buf[2], ". \n\n ex:", exception }), MessageType.Error);
						GLOBAL.FbDatabase.ReaderClose();
						return null;
					}
				}
				GLOBAL.FbDatabase.ReaderClose();
			}
			return list;
		}


		// #####################################################################################################
		// WorkMove        #####################################################################################
		// #####################################################################################################

		public class WorkMoveItem
		{
			public bool Deleted { get; set; }
			public string DocNumber { get; set; }
			public string Operation { get; set; }
			public int Quan { get; set; }
			public string SkladName { get; set; }
			public bool SkladUsed { get; set; }
		}

		public static List<WorkMoveItem> GetWorkMove(DateTime date, string productId)
		{
			GLOBAL.SysLog.Write("get work move ", MessageType.Information);

			List<string> buf;
			var workMoveList = new List<WorkMoveItem>();
			string sql = "select " +
				"\n cast(date_to_str(sg.date_) as date) as date_, " +
				"\n iif(sg.quanreal0%p > 0,sg.quanreal0%p,sg.quanreal0%m*-1) as ProductMovement, " +
				"\n " +
				"\n coalesce( " +
				"\n iif((select n.name0%i from nakladna0 n where n.id = sg.docref0 and n.deleted='0') is null, " +
				"\n iif((select o.name0%i from order0 o where o.id = sg.docref0 and o.deleted='0') is null, " +
				"\n (select m.name0%i from move0 m where m.id = sg.docref0 and m.deleted='0'), " +
				"\n (select o.name0%i from order0 o where o.id = sg.docref0 and o.deleted='0')), " +
				"\n (select n.name0%i from nakladna0 n where n.id = sg.docref0 and n.deleted='0')),'-') " +
				"\n ," +
				"\n coalesce( " +
				"\n iif((select n.number0%i from nakladna0 n where n.id = sg.docref0 and n.deleted='0') is null, " +
				"\n iif((select o.number0%i from order0 o where o.id = sg.docref0 and o.deleted='0') is null, " +
				"\n (select m.number0%i from move0 m where m.id = sg.docref0 and m.deleted='0'), " +
				"\n (select o.number0%i from order0 o where o.id = sg.docref0 and o.deleted='0')), " +
				"\n (select n.number0%i from nakladna0 n where n.id = sg.docref0 and n.deleted='0' )),'-') " +
				"\n " +
				"\n , sk.id, sk.name0%i, sg.goods0 " +
				"\n from  skladreg0%m sg , warehous0 wr, sklad00 sk " +
				"\n where sg.goods0 = wr.id and wr.width0 ='" + productId + "'  " +
				"\n and  date_to_str(sg.date_) = '" + date.ToString("dd.MM.yyyy") + "' and  sg.sklad0 = sk.id ";
			if (!GLOBAL.FbDatabase.ReaderExec(sql))
			{
				return null;
			}
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				string str2 = buf[5];
				string str3 = buf[4];
				bool flag = false;
				if (Configuration.SkladMain == str3)
				{
					flag = true;
				}
				if (Configuration.SkladSoh == str3)
				{
					flag = true;
				}
				if (Configuration.SkladReserve == str3)
				{
					flag = true;
				}
				if (buf[2] != "-")
				{
					foreach (var workMoveItem1 in workMoveList)
					{
						if (workMoveItem1.DocNumber == buf[3] && workMoveItem1.Operation == buf[2])
						{
							workMoveItem1.Quan += Int32.Parse(buf[1]);
						}
					}

					if (Int32.Parse(buf[1]) != 0)
					{

						int count = Int32.Parse(buf[1]);
						//double price = double.Parse(buf[2]);

						count = GLOBAL.MultipackList.Where(multipackItem => buf[6] ==
							multipackItem.ProductId).Aggregate(count, (current, multipackItem) =>
								current * multipackItem.MultipackCount);



						var workMoveItem = new WorkMoveItem
						{
							Operation = buf[2],
							DocNumber = buf[3],
							Quan = count,
							Deleted = false,
							SkladUsed = flag,
							SkladName = str2
						};
						workMoveList.Add(workMoveItem);
					}
				}
			}
			GLOBAL.FbDatabase.ReaderClose();
			return workMoveList;
		}


		// #####################################################################################################
		// Spisanie        #####################################################################################
		// #####################################################################################################

		public class DiscardedItem
		{
			public DiscardedItem()
			{
				this.DiscardedProductList = new List<DataClass.DiscardedProductItem>();
			}

			public List<DataClass.DiscardedProductItem> DiscardedProductList { get; set; }
			public string DocNumber { get; set; }
			public string ID { get; set; }
            public string Store { get; set; }
		}

		public class DiscardedProductItem
		{
			public int Code { get; set; }
			public int Count { get; set; }
			public double Price { get; set; }
		}

		public static List<DiscardedItem> GetDiscarded(string date, bool fix)
		{
			GLOBAL.SysLog.Write("Get discarted order ", MessageType.Information);
			List<string> buf;

			var list = new List<DiscardedItem>();
			string sql = string.Empty;

			if (!fix)
			{
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("Execute normal query", MessageType.Information);

				sql = " select distinct m.id, m.number0%i, " +

                      " iif( m.sklad_out0 = '" + Configuration.SkladMain + "','Основной'," +
                      " iif( m.sklad_out0 = '" + Configuration.SkladSoh + "', 'СОХ'," +
                      " iif( m.sklad_out0 = '" + Configuration.SkladSpoilage + "', 'Брак'," +
                      " iif( m.sklad_out0 = '" + Configuration.SkladReserve + "', 'РезервВип','ОЩИБКА')))) " +

					  "\n from move0 m where " +
					  "\n m.name0%i = 'НАКЛАДНАЯ НА ВНУТРЕННЕЕ ПЕРЕМЕЩЕНИЕ' " +
					  "\n and cast( date_to_str(m.date0) as date) = '" + date + "' and m.skl_prov0='Да' " +
					  "\n and m.sklad_out0 in ('" + Configuration.SkladMain + "','" +
					  Configuration.SkladSoh + "','" + Configuration.SkladReserve + "') " +
					  "\n and m.deleted =0";

			}
			else
			{
				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("Execute fix query", MessageType.Information);


				sql = " select distinct m.id, m.number0%i, " +

                      " iif( m.sklad_out0 = '" + Configuration.SkladMain + "','Основной'," +
                      " iif( m.sklad_out0 = '" + Configuration.SkladSoh + "', 'СОХ'," +
                      " iif( m.sklad_out0 = '" + Configuration.SkladSpoilage + "', 'Брак'," +
                      " iif( m.sklad_out0 = '" + Configuration.SkladReserve + "', 'РезервВип','ОЩИБКА')))) " +

					  "\n from move0 m, skladreg0%m sm where " +
					  "\n m.name0%i = 'НАКЛАДНАЯ НА ВНУТРЕННЕЕ ПЕРЕМЕЩЕНИЕ' " +
					  "\n and m.id = sm.docref0 " +
					  "\n and cast( date_to_str(m.date0) as date) = '" + date + "' and m.skl_prov0='Да' " +
					  "\n and m.sklad_out0 in ('" + Configuration.SkladMain + "','" +
					  Configuration.SkladSoh + "','" + Configuration.SkladReserve + "') " +
					  "\n and m.deleted =0";
			}

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Execute query", MessageType.Information);

			if (!GLOBAL.FbDatabase.ReaderExec(sql))
			{
				return null;
			}

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Extract data", MessageType.Information);


			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				var item = new DiscardedItem
				{
					DocNumber = buf[1],
					ID = buf[0],
                    Store = buf[2]
				};
				list.Add(item);
			}
			GLOBAL.FbDatabase.ReaderClose();
			GLOBAL.Interface.AddText("Найдено " + list.Count + " накладных списания");
			int num = 0;
			foreach (DiscardedItem item2 in list)
			{
				num++;
				GLOBAL.Interface.SetProgressBar((num * 100) / list.Count);

				if (!fix)
				{
					if (Configuration.DataClassTimeProfile)
						GLOBAL.SysLog.Write("set normal query", MessageType.Information);

					sql = "select w.width0, "+
						  " COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,sum(md.QUAN0)/50,iif(max(w.GOODCHARSVALUES10)=30,sum(md.QUAN0)/30,sum(md.QUAN0)) ), sum(md.QUAN0) ),0) , " +
						  " COALESCE(iif(cast(w.WIDTH0 as varchar(8)) like '90%', iif(max(w.GOODCHARSVALUES10)=50,min(md.postavprice0)*50,iif(max(w.GOODCHARSVALUES10)=30,min(md.postavprice0)*30,min(md.postavprice0))), min(md.postavprice0) ),0)" +

						  //" iif(w.goodcharsvalues10=50,sum(md.QUAN0)/50, " +
						  //" iif(w.goodcharsvalues10=30,sum(md.QUAN0)/30,sum(md.QUAN0)))," +
						  //" iif(w.goodcharsvalues10=50,min( md.postavprice0 )*50," +
						  //" iif(w.goodcharsvalues10=30,min( md.postavprice0 )*30,min( md.postavprice0 )))  " +
						  " ,md.goods0 \n" +
						  "\n from movedetail0 md \n join warehous0 w on w.id = md.goods0 " +
						  "\n where md.deleted = 0 and w.width0 > 0 and w.ARTIKUL0%I like '%ОБОЛОНЬ%' and md.invoce0 = '" + item2.ID + "' " +
						  "\n group by w.width0, w.goodcharsvalues10,md.goods0";
				}
				else
				{
					if (Configuration.DataClassTimeProfile)
						GLOBAL.SysLog.Write("set fix query", MessageType.Information);

					//sql = "select distinct (select wh.WIDTH0 from WAREHOUS0 wh where wh.id = sm.goods0), \n" +
					//      " iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.goods0)=50, \n" +
					//      " sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m))/50, \n" +
					//      " iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.goods0)=30, \n" +
					//      " sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m))/30, \n" +
					//      " sum(IIF(sm.quan0%p > 0, sm.quan0%p, sm.quan0%m)))), \n" +
					//      " iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.goods0)=50, \n" +
					//      " min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.posref0) )*50, \n" +
					//      " iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.goods0)=30, \n" +
					//      " min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.posref0) )*30, \n" +
					//      " min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.posref0) ))) \n" +
					//      " ,sm.goods0 \n" +
					//      " from skladreg0%m sm \n" +
					//      " where \n" +
					//      " (select wh.WIDTH0 from WAREHOUS0 wh \n" +
					//      " where wh.id = sm.goods0 and wh.ARTIKUL0%I like '%ОБОЛОНЬ%' ) > 0 \n" +
					//      " and sm.docref0 = '" + item2.ID + "' \n" +
					//      " group by sm.goods0, sm.quan0%p, sm.quan0%m,sm.goods0 ";

					sql =
						 " select distinct wh.WIDTH0,  \n" +
						 " iif(cast(wh.WIDTH0 as varchar(8)) like '90%', \n"+
						 " iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.GOODS0)=50,  \n"+
						 " sum(IIF(sm.QUAN0%P > 0, sm.QUAN0%P, sm.QUAN0%M))/50,  \n" +
						 " iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.GOODS0)=30,  \n"+
						 " sum(IIF(sm.QUAN0%P > 0, sm.QUAN0%P, sm.QUAN0%M))/30,  \n" +
						 " sum(IIF(sm.QUAN0%P > 0, sm.QUAN0%P, sm.QUAN0%M)))) , sum(IIF(sm.QUAN0%P > 0, sm.QUAN0%P, sm.QUAN0%M))), \n" +
						 "  iif(cast(wh.WIDTH0 as varchar(8)) like '90%', \n"+
						 " iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.GOODS0)=50,  \n"+
						 " min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.POSREF0) )*50,  \n"+
						 " iif((select wh.GOODCHARSVALUES10 from WAREHOUS0 wh where wh.id = sm.GOODS0)=30,  \n"+
						 " min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.POSREF0) )*30,  \n"+
						 " min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.POSREF0) )))  \n"+
						 " ,min( (select md.POSTAVPRICE0 from MOVEDETAIL0 md where md.id = sm.POSREF0) )) \n"+
						 " ,sm.GOODS0  \n"+
						 " from SKLADREG0%M sm, WAREHOUS0 wh \n" +
						 " where  \n"+
						 " wh.id = sm.GOODS0 \n"+
						 " and wh.ARTIKUL0%I like '%ОБОЛОНЬ%' \n"+
						 " and sm.DOCREF0 =  '" + item2.ID + "'  \n" +
						 " group by sm.GOODS0, sm.QUAN0%P, sm.QUAN0%M,sm.GOODS0,wh.WIDTH0 \n";
				}

				if (!GLOBAL.FbDatabase.ReaderExec(sql))
				{
					return null;
				}
				while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
				{
					try
					{
						int count = Int32.Parse(buf[1]);
						double price = double.Parse(buf[2]);

						foreach (var multipackItem in GLOBAL.MultipackList)
						{
							if (buf[3] == multipackItem.ProductId)
							{
								count *= multipackItem.MultipackCount;
								price /= multipackItem.MultipackCount;
							}
						}

						var item3 = new DiscardedProductItem
						{
							Code = int.Parse(buf[0]),
							Count = Math.Abs(count),
							Price = price
						};
						item2.DiscardedProductList.Add(item3);
					}
					catch (Exception exception)
					{
						Configuration.ErrorMessage.Append(string.Concat(new object[] { "Error cast Discarded order item. code:", buf[0], " Count:", buf[1], " Price:", buf[2], ".  <br /> <br />Ex:", exception }));
						GLOBAL.SysLog.Write(string.Concat(new object[] { "Error cast Discarded order item. code:", buf[0], " Count:", buf[1], " Price:", buf[2], ". \n\n ex:", exception }), MessageType.Error);
						GLOBAL.FbDatabase.ReaderClose();
						return null;
					}
				}
				GLOBAL.FbDatabase.ReaderClose();
			}
			return list;
		}

		// #####################################################################################################
		// RestVTRT        #####################################################################################
		// #####################################################################################################

		public class RestVTRTItem
		{
			public RestVTRTItem()
			{
				this.RestVTRTProductList = new List<RestVTRTProductItem>();
			}

			public string DocNumber { get; set; }

			public string EDRPOU { get; set; }
			public string CustomerId { get; set; }
			public string CustomerName { get; set; }

			public string ShopId { get; set; }
			public string ShopName { get; set; }
			public string TZ { get; set; }

			public List<RestVTRTProductItem> RestVTRTProductList { get; set; }

			public string ID { get; set; }
		}

		public class RestVTRTProductItem
		{
			public int Code { get; set; }
			public int Count { get; set; }
			public double Price { get; set; }
		}

		private class UserToTzItem
		{
			public string UserId { get; set; }
			public string TzCode { get; set; }
		}

		private class ProductIdToCodeItem
		{
			public string ProductId { get; set; }
			public string Code { get; set; }
		}


		public static List<RestVTRTItem> GetRestVTRT(DateTime date)
		{
			var restVtrtList = new List<RestVTRTItem>();
			var userToTzList = new List<UserToTzItem>();
			var productIdToCodeList = new List<ProductIdToCodeItem>();

			GLOBAL.SysLog.Write("Get rest VTRT", MessageType.Information);

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Extract user from MS", MessageType.Information);

			GLOBAL.FbDatabase.ReaderExec(" select wr.id, wr.index0 from workers0 wr \n" +
			" where wr.index0 like '___%'");
			List<string> buf;
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				userToTzList.Add(new UserToTzItem { UserId = buf[0], TzCode = buf[1] });
			}
			GLOBAL.FbDatabase.ReaderClose();

			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Extract product from MS", MessageType.Information);

			GLOBAL.FbDatabase.ReaderExec(" select wh.id, wh.width0 from warehous0 wh " +
			" where wh.width0 > 0");
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				productIdToCodeList.Add(new ProductIdToCodeItem { ProductId = buf[0], Code = buf[1] });
			}
			GLOBAL.FbDatabase.ReaderClose();


			var mssql = new Database();
			var msCon = mssql.ConnectMssql();
			var msCmd = msCon.CreateCommand();

			// extract order
			msCmd.CommandText = "select coalesce(oNumber,' '), \n" +
			" ( select c.cJuridicalOKPO from tblCustomers c, tblShops s where sCustomerId = c.id and s.id = oShopId ),\n" +
			" ( select sCustomerId from tblShops s where s.id = oShopId ), \n" +
			" ( select c.cName from tblCustomers c, tblShops s where sCustomerId = c.id and s.id = oShopId ), \n" +
			" oShopId, \n" +
			" (select sName from tblShops where id=oShopId)+'('+dbo.GetShopFullAddress(oShopId)+')', \n" +
            
			//" ( select sName+'('+ COALESCE(ss.smsShortName + ' ', '') + COALESCE(sm.smName + ', ', '') + \n"+
			//" COALESCE(d.dName + ' район, ', '') + sAddress+')' \n"+
			//" FROM tblShops s \n"+
			//" LEFT JOIN tblDistricts d ON s.sDistrictId = d.id \n"+
			//" LEFT JOIN tblSettlements sm ON sm.id = d.SettlementId  \n"+
			//" LEFT JOIN tblSettlementStatuses ss ON ss.id = sm.SettlementStatusId \n" +
			//" WHERE s.id = oShopId), "+  

			" (select uz.uZone from tblUserZones uz, tblUsers u WHERE u.id = oCreatorId and u.urId = uz.rUserId ), \n" +
			" o.id \n" +
			" from tblOrders o where \n" +
			" oAddTime > '" + date.ToString("yyyy-MM-dd") + " 00:00:00.000' \n" +
			" and oAddTime < '" + date.ToString("yyyy-MM-dd") + " 23:59:59.000' \n" +
			" and (select COUNT(odRest) from tblOrderDetails od where odOrderId = o.id and odRest >=0 ) > 0 ";


			if (Configuration.DataClassTimeProfile)
				GLOBAL.SysLog.Write("Extract rest from MS", MessageType.Information);

			var reader = msCmd.ExecuteReader();
			while (reader.Read())
			{
				if (reader[0].ToString().Length > 1)
				{
					restVtrtList.Add(new RestVTRTItem
					{
                 		DocNumber = "ОСТ-" + reader[0],
                 		EDRPOU = reader[1].ToString(),
                 		CustomerId = reader[2].ToString(),
                 		CustomerName = reader[3].ToString(),
                 		ShopId = reader[4].ToString(),
						ShopName = GLOBAL.ShopsMS.GetNameById(reader[4].ToString()), //  reader[5].ToString(),
						TZ = reader[6].ToString(),
						//	(from userToTzItem in userToTzList
						//	 where userToTzItem.UserId == reader[6].ToString()
						//	 select userToTzItem.TzCode).FirstOrDefault(),
						ID = reader[7].ToString()
					});
				}
				else
				{
					restVtrtList.Add(new RestVTRTItem
					{
						DocNumber = "ОСТ-" + reader[7].ToString().Replace("-", ""),
						EDRPOU = reader[1].ToString(),
						CustomerId = reader[2].ToString(),
						CustomerName = reader[3].ToString(),
						ShopId = reader[4].ToString(),
						ShopName = GLOBAL.ShopsMS.GetNameById(reader[4].ToString()),//reader[5].ToString(),
						TZ = reader[6].ToString(),
						ID = reader[7].ToString()
					});
				}

			}
			reader.Close();

			foreach (var restVTRTItem in restVtrtList)
			{

				// extract order product items
                msCmd.CommandText = "select (select paKodObolon from tblProductAddons pa where pa.id = odProductId ), odRest,odProductId \n" +
				" from tblOrderDetails where odOrderId='" + restVTRTItem.ID + "' and odRest>=0 ";

				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("Execute MS query: " + msCmd.CommandText, MessageType.Information);

				reader = msCmd.ExecuteReader();

				if (Configuration.DataClassTimeProfile)
					GLOBAL.SysLog.Write("Extract data", MessageType.Information);

				while (reader.Read())
				{
					string code = string.Empty;
					try
					{
						//code = (from productIdToCodeItem in productIdToCodeList
						//		where productIdToCodeItem.ProductId == reader[0].ToString()
						//			   select productIdToCodeItem.Code).FirstOrDefault();

						if (Int32.Parse(reader[0].ToString()) > 0)
							restVTRTItem.RestVTRTProductList.Add(new RestVTRTProductItem
							{
								Code = Int32.Parse(reader[0].ToString()),
								Count = Int32.Parse(reader[1].ToString()),
								Price = 0
							});
					}
					catch (Exception exception)
					{
                        Configuration.ErrorMessage.Append(string.Concat(new object[] { "Error cast Discarded order item. code:", reader[0].ToString(), " Count:", reader[1].ToString(), " MSProductId: ", reader[2].ToString(), ".  <br /> <br />Ex:", exception }));
                        GLOBAL.SysLog.Write(string.Concat(new object[] { "Error cast Discarded order item. code:", reader[0].ToString(), " Count:", reader[1].ToString(), " MSProductId: ", reader[2].ToString(), ". \n\n ex:", exception }), MessageType.Error);
						reader.Close();
						return null;
					}
				}
				reader.Close();
			}

			msCon.Close();

			return restVtrtList;
		}


		// #####################################################################################################
		// Multipack for dolboeb        #####################################################################
		// #####################################################################################################


		public class MultipackItem
		{
			public string ProductId { get; set; }
			public int MultipackCount { get; set; }
		}


		public static List<MultipackItem> GetMultipack()
		{
			var multicackList = new List<MultipackItem>();

#if !DEBUG
			try
			{
#endif

            GLOBAL.FbDatabase.ReaderExec(" select wrh.id from warehous0 wrh where wrh.name0%i like '%(4%' ");
			List<string> buf;
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				multicackList.Add(new MultipackItem() { ProductId = buf[0], MultipackCount = 4 });
			}
			GLOBAL.FbDatabase.ReaderClose();

            GLOBAL.FbDatabase.ReaderExec(" select wrh.id from warehous0 wrh where wrh.name0%i like '%(6%' ");
            while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
            {
                multicackList.Add(new MultipackItem() { ProductId = buf[0], MultipackCount = 6 });
            }
            GLOBAL.FbDatabase.ReaderClose();

#if !DEBUG
			}
			catch (Exception ex)
			{
				GLOBAL.SysLog.Write("Error extract Multipack product. Exception:" + ex,MessageType.Error);
				//Func.Log(, Func.LogType.Error);
				return null;
			}
#endif

			return multicackList;
		}

		// #####################################################################################################
		// CutTara for dolboeb        #####################################################################
		// #####################################################################################################

		public class CutTara
		{
			private List<CutTaraCustomerItem> _cutTaraCustomerList;
			private List<CutTaraPriceItem> _cutTaraPriceList;

			public CutTara()
			{
				_cutTaraCustomerList = new List<CutTaraCustomerItem>();
				_cutTaraPriceList = new List<CutTaraPriceItem>();
			}

			public List<CutTaraCustomerItem> CutTaraCustomerList
			{
				get { return _cutTaraCustomerList; }
				set { _cutTaraCustomerList = value; }
			}

			public List<CutTaraPriceItem> CutTaraPriceList
			{
				get { return _cutTaraPriceList; }
				set { _cutTaraPriceList = value; }

			}
		}

		public class CutTaraPriceItem
		{
			public string ProductId { get; set; }
			public double Price { get; set; }
		}

		public class CutTaraCustomerItem
		{
			public string ID { get; set; }
		}

		public static CutTara GetCutTara()
		{
			var cutTara = new CutTara();

#if !DEBUG
			try
			{
#endif

			GLOBAL.FbDatabase.ReaderExec(" select l.id from legal0 l where l.kodpodr0 like 'з пляшкою' ");
			List<string> buf;
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				cutTara.CutTaraCustomerList.Add(new CutTaraCustomerItem() { ID = buf[0] });
			}
			GLOBAL.FbDatabase.ReaderClose();
#if !DEBUG
			}
			catch (Exception ex)
			{
				GLOBAL.SysLog.Write("Error extract GetCutTara product. Exception:" + ex,MessageType.Error);
				//Func.Log(, Func.LogType.Error);
				return null;
			}
#endif


#if !DEBUG
			try
			{
			List<string> buf;
#endif

			GLOBAL.FbDatabase.ReaderExec("select wrh.id,wrh.goodcharsvalues40 from warehous0 wrh where wrh.goodcharsvalues40 <> '' ");
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				try
				{
					double price = double.Parse(buf[1]);
					cutTara.CutTaraPriceList.Add(new CutTaraPriceItem() { ProductId = buf[0], Price = price });
				}
				catch (Exception ex)
				{
					GLOBAL.SysLog.Write("Error cast string to double GetCutTara product. Exception:" + ex, MessageType.Error);
				}
			}
			GLOBAL.FbDatabase.ReaderClose();
#if !DEBUG
			}
			catch (Exception ex)
			{
				GLOBAL.SysLog.Write("Error extract GetCutTara product. Exception:" + ex,MessageType.Error);
				//Func.Log(, Func.LogType.Error);
				return null;
			}
#endif

			return cutTara;
		}

		// #####################################################################################################
		// MsSql shop name from code       #####################################################################
		// #####################################################################################################

		public class ShopMs
		{
			private List<ShopMsItem> _shopMsList = new List<ShopMsItem>();

			public List<ShopMsItem> ShopMsList
			{
				get { return _shopMsList; }
				set { _shopMsList = value; }
			}

			public string GetNameById(string id)
			{
				foreach (var shopMsitem in _shopMsList)
				{
					if (shopMsitem.Id == id) return shopMsitem.Name;
				}
				return "-1";
			}
		}

		public class ShopMsItem
		{
			public string Id { get; set; }
			public string Name { get; set; }
		}


		// #####################################################################################################
		// MsSql customer numerical        #####################################################################
		// #####################################################################################################

		public class CustMsId
		{
			private List<CustMsItem> _custMsList = new List<CustMsItem>();

			public List<CustMsItem> CustMsList
			{
				get { return _custMsList; }
				set { _custMsList = value; }
			}

			public string GetNumberById(string id)
			{
				foreach (var custMsItem in _custMsList)
				{
					if (custMsItem.Id == id) return custMsItem.Number;
				}
				return "-1";
			}
		}

		public class CustMsItem
		{
			public string Id { get; set; }
			public string Number { get; set; }
		}

	}
}
