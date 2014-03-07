using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fnLog2;

namespace XmlObolon_2._0.Classess
{
	static class ValidateDataClass
	{

		/// <summary>
		/// Check data for incorrect 
		///  -- Return false if error exist
		/// </summary>
		/// <param name="restListMain"></param>
		/// <param name="restListSoh"></param>
		/// <param name="restListRes"></param>
		/// <param name="restListMainOld"></param>
		/// <param name="restListSohOld"></param>
		/// <param name="restListResOld"></param>
		/// <param name="comingOrder"></param>
		/// <param name="returnCustomer"></param>
		/// <param name="invoiceItems"></param>
		/// <param name="discardedItems"></param>
		/// <param name="orderItems"></param>
		/// <param name="moveItems"></param>
		/// <param name="returnSupplierItems"></param>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static bool Check(
            List<DataClass.RestItem> restListMain, List<DataClass.RestItem> restListSoh, List<DataClass.RestItem> restListSpoilage, List<DataClass.RestItem> restListRes,
            List<DataClass.RestItem> restListMainOld, List<DataClass.RestItem> restListSohOld, List<DataClass.RestItem> restListSpoilageOld, List<DataClass.RestItem> restListResOld,
			List<DataClass.ComingOrderItem> comingOrder, List<DataClass.ReturnCustomerItem> returnCustomer,
			List<DataClass.InvoiceItem> invoiceItems, List<DataClass.DiscardedItem> discardedItems,
			List<DataClass.OrderItem> orderItems, List<DataClass.MoveItem> moveItems,
			List<DataClass.ReturnSupplierItem> returnSupplierItems, DateTime dateTime, bool fix)
		{

			bool checkAnsver = true;
			var productCodeList = new List<int>();

			// ############################################################
			// ###  Get full product list


			// get ProductID from rest Main
			foreach (var restItem in restListMain)
			{
				bool exist = false;
				foreach (var productCodeItem in productCodeList)
				{
					if (productCodeItem == restItem.Code) exist = true;
				}
				if (!exist) productCodeList.Add(restItem.Code);
			}

			// get ProductID from rest Soh
			foreach (var restItem in restListSoh)
			{
				bool exist = false;
				foreach (var productCodeItem in productCodeList)
				{
					if (productCodeItem == restItem.Code) exist = true;
				}
				if (!exist) productCodeList.Add(restItem.Code);
			}

			// get ProductID  from rest Res
			if(restListRes!=null)
				foreach (var restItem in restListRes)
				{
					bool exist = false;
					foreach (var productCodeItem in productCodeList)
					{
						if (productCodeItem == restItem.Code) exist = true;
					}
					if (!exist) productCodeList.Add(restItem.Code);
				}

			// get ProductID  from rest Main Old
			foreach (var restItem in restListMainOld)
			{
				bool exist = false;
				foreach (var productCodeItem in productCodeList)
				{
					if (productCodeItem == restItem.Code) exist = true;
				}
				if (!exist) productCodeList.Add(restItem.Code);
			}

			// get ProductID  from rest Soh Old
			foreach (var restItem in restListSohOld)
			{
				bool exist = false;
				foreach (var productCodeItem in productCodeList)
				{
					if (productCodeItem == restItem.Code) exist = true;
				}
				if (!exist) productCodeList.Add(restItem.Code);
			}

			// get ProductID  from rest Res Old
			if (restListResOld!=null)
				foreach (var restItem in restListResOld)
				{
					bool exist = false;
					foreach (var productCodeItem in productCodeList)
					{
						if (productCodeItem == restItem.Code) exist = true;
					}
					if (!exist) productCodeList.Add(restItem.Code);
				}

			// get ProductID from coming order 
			foreach (var comingOrderItem in comingOrder)
			{
				foreach (var comingProductItem in comingOrderItem.ComingProductList)
				{
					bool exist = false;
					foreach (var productCodeItem in productCodeList)
					{
						if (productCodeItem == comingProductItem.Code) exist = true;
					}
					if (!exist) productCodeList.Add(comingProductItem.Code);
				}
			}

			// get ProductID from return customer
			foreach (var returnCustomerItem in returnCustomer)
			{
				foreach (var returnCustomerProductItem in returnCustomerItem.ReturnCustomerProductList)
				{
					bool exist = false;
					foreach (var productCodeItem in productCodeList)
					{
						if (productCodeItem == returnCustomerProductItem.Code) exist = true;
					}
					if (!exist) productCodeList.Add(returnCustomerProductItem.Code);
				}
			}

			// get ProductID from invoice items
			foreach (var invoiceItem in invoiceItems)
			{
				foreach (var invoiceProductItem in invoiceItem.InvoiceProductList)
				{
					bool exist = false;
					foreach (var productCodeItem in productCodeList)
					{
						if (productCodeItem == invoiceProductItem.Code) exist = true;
					}
					if (!exist) productCodeList.Add(invoiceProductItem.Code);
				}
			}

			// get ProductID from discarded items
			foreach (var discardedItem in discardedItems)
			{
				foreach (var discardedProductItem in discardedItem.DiscardedProductList)
				{
					bool exist = false;
					foreach (var productCodeItem in productCodeList)
					{
						if (productCodeItem == discardedProductItem.Code) exist = true;
					}
					if (!exist) productCodeList.Add(discardedProductItem.Code);
				}
			}

			// get ProductID from order items
			foreach (var orderItem in orderItems)
			{
				foreach (var orderProductItem in orderItem.OrderProductList)
				{
					bool exist = false;
					foreach (var productCodeItem in productCodeList)
					{
						if (productCodeItem == orderProductItem.Code) exist = true;
					}
					if (!exist) productCodeList.Add(orderProductItem.Code);
				}
			}

			// get ProductID from move items
			foreach (var moveItem in moveItems)
			{
				foreach (var moveProductItem in moveItem.MoveProductList)
				{
					bool exist = false;
					foreach (var productCodeItem in productCodeList)
					{
						if (productCodeItem == moveProductItem.Code) exist = true;
					}
					if (!exist) productCodeList.Add(moveProductItem.Code);
				}
			}


			// get ProductID from return supplier
			foreach (var returnSupplierItem in returnSupplierItems)
			{
				foreach (var returnSupplierProductItem in returnSupplierItem.ReturnSupplierProductList)
				{
					bool exist = false;
					foreach (var productCodeItem in productCodeList)
					{
						if (productCodeItem == returnSupplierProductItem.Code) exist = true;
					}
					if (!exist) productCodeList.Add(returnSupplierProductItem.Code);
				}
			}


			foreach (var productCodeItem in productCodeList)
			{
				int restEndDay = 0;
				int restBeginDay = 0;
				int currentProductCode = productCodeItem;

				restEndDay += restListMain.Where(restItem => restItem.Code == currentProductCode).Sum(restItem => restItem.Count);
				restEndDay += restListSoh.Where(restItem => restItem.Code == currentProductCode).Sum(restItem => restItem.Count);
				if (restListRes!=null)
					restEndDay += restListRes.Where(restItem => restItem.Code == currentProductCode).Sum(restItem => restItem.Count);

				restBeginDay += restListMainOld.Where(restItem => restItem.Code == currentProductCode).Sum(restItem => restItem.Count);
				restBeginDay += restListSohOld.Where(restItem => restItem.Code == currentProductCode).Sum(restItem => restItem.Count);
				if (restListResOld!=null)
					restBeginDay += restListResOld.Where(restItem => restItem.Code == currentProductCode).Sum(restItem => restItem.Count);
			
				int comingOrderSum = comingOrder.Sum(comingOrderItem => comingOrderItem.ComingProductList.Where(item => item.Code == currentProductCode).Sum(item => item.Count));
				int returnCustomersSum = returnCustomer.Sum(returnCustomerItem => returnCustomerItem.ReturnCustomerProductList.Where(item => item.Code == currentProductCode).Sum(item => item.Count));
				int invoiceItemsSum = invoiceItems.Sum(invoiceItem => invoiceItem.InvoiceProductList.Where(item => item.Code == currentProductCode).Sum(item => item.Count));
				int discardedItemsSum = discardedItems.Sum(discardedItem => discardedItem.DiscardedProductList.Where(item => item.Code == currentProductCode).Sum(item => item.Count));

				// not need
				int ordersItemsSum = orderItems.Sum(orderItem => orderItem.OrderProductList.Where(item => item.Code == currentProductCode).Sum(item => item.Count));
				int moveItemsSum = moveItems.Sum(moveItem => moveItem.MoveProductList.Where(item => item.Code == currentProductCode).Sum(item => item.Count));
				int returnSupplierItemsSum = returnSupplierItems.Sum(returnSupplierItem => returnSupplierItem.ReturnSupplierProductList.Where(item => item.Code == currentProductCode).Sum(item => item.Count));


				int expectedRestToEndDay = ((((restBeginDay - 
					invoiceItemsSum) - discardedItemsSum) - returnSupplierItemsSum) + 
					moveItemsSum) + comingOrderSum + returnCustomersSum;

				if(expectedRestToEndDay!=restEndDay)
				{
					checkAnsver = false;
					// Ebaniy v rot error !!!

					GLOBAL.Interface.AddText("Расхождение товара: " + currentProductCode);

					int maxNaklCount = 0;
					if (maxNaklCount < comingOrder.Count) maxNaklCount = comingOrder.Count;
					if (maxNaklCount < returnCustomer.Count) maxNaklCount = returnCustomer.Count;
					if (maxNaklCount < invoiceItems.Count) maxNaklCount = invoiceItems.Count;
					if (maxNaklCount < discardedItems.Count) maxNaklCount = discardedItems.Count;
					if (maxNaklCount < moveItems.Count) maxNaklCount = moveItems.Count;
					if (maxNaklCount < returnSupplierItems.Count) maxNaklCount = returnSupplierItems.Count;

					Configuration.ErrorMessage.Append("<table width='100%' border='0' cellpadding='6' cellspacing='1' style='border:solid; border-color:#CCC'><tr><td align='center' bgcolor='#CCCCCC'>");
					Configuration.ErrorMessage.Append("Расхождение товара за " + dateTime.ToString("dd.MM.yyyy") + "<br />");
					Configuration.ErrorMessage.Append("Торговая площадка: " + Configuration.ZoneNum + "<br /></td></tr></table>");
					Configuration.ErrorMessage.Append("<br/><br/><table border ='0' cellpadding='6' cellspacing='1' style='font-weight:normal;font-size: 11px;border:solid; border-color:#CCCCCC'> <tr bgcolor='#CCCCCC'><th>Товар</th><th align='left' colspan='" + maxNaklCount + "'>" + productCodeItem + "</th></tr>");

					Configuration.ErrorMessage.Append("<tr><td>Расхождение</td><td bgcolor='#FF0000' style='color:#FFF'> <b> " + (expectedRestToEndDay - restEndDay) + "</b> </td></tr>");
					Configuration.ErrorMessage.Append("<tr bgcolor='#EFEFEF'><td>Остаток_на_начало</td><td> "+restBeginDay+" </td></tr>");
					Configuration.ErrorMessage.Append("<tr><td>Остаток_на_конец</td><td> "+restEndDay+" </td></tr>");

					Configuration.ErrorMessage.Append("<tr bgcolor='#EFEFEF'><td>Приходный ордер [" + comingOrderSum + "]</td>");
					foreach (var comingOrderItem in comingOrder)
					{
						foreach (var comingProductItem in comingOrderItem.ComingProductList)
						{
							if (comingProductItem.Code == currentProductCode)
							{
								Configuration.ErrorMessage.Append("<td>" + comingOrderItem.DocNumber + "[" + comingProductItem.Count + "]</td>");
								break;
							}
						}
					}
					Configuration.ErrorMessage.Append("</tr>");

					Configuration.ErrorMessage.Append("<tr><td>Возврат клиенту [" + returnCustomersSum + "]</td>");
					foreach (var returnCustomerItem in returnCustomer)
					{
						foreach (var returnCustomerProductItem in returnCustomerItem.ReturnCustomerProductList)
						{
							if (returnCustomerProductItem.Code == currentProductCode)
							{
								Configuration.ErrorMessage.Append("<td>" + returnCustomerItem.DocNumber + "[" + returnCustomerProductItem.Count + "]</td>");
								break;
							}
						}
					}
					Configuration.ErrorMessage.Append("</tr>");

					Configuration.ErrorMessage.Append("<tr bgcolor='#EFEFEF'><td>Расходные_накладные_[" + invoiceItemsSum + "]</td>");
					foreach (var invoiceItem in invoiceItems)
					{
						foreach (var invoiceProductItem in invoiceItem.InvoiceProductList)
						{
							if(invoiceProductItem.Code==currentProductCode)
							{
								Configuration.ErrorMessage.Append("<td>" + invoiceItem.DocNumber + "[" + invoiceProductItem.Count + "]</td>");
								break;
							}
						}
					}
					Configuration.ErrorMessage.Append("</tr>");

					Configuration.ErrorMessage.Append("<tr><td>Списание [" + discardedItemsSum + "]</td>");
					foreach (var discardedItem in discardedItems)
					{
						foreach (var discardedProductItem in discardedItem.DiscardedProductList)
						{
							if (discardedProductItem.Code == currentProductCode)
							{
								Configuration.ErrorMessage.Append("<td>" + discardedItem.DocNumber + "[" + discardedProductItem.Count + "]</td>");
								break;
							}
						}
					}
					Configuration.ErrorMessage.Append("</tr>");

					Configuration.ErrorMessage.Append("<tr bgcolor='#EFEFEF'><td>Перемещение [" + moveItemsSum + "]</td>");
					foreach (var moveItem in moveItems)
					{
						foreach (var moveProductItem in moveItem.MoveProductList)
						{
							if (moveProductItem.Code == currentProductCode)
							{
								Configuration.ErrorMessage.Append("<td>" + moveItem.DocNumber + "[" + moveProductItem.Count + "]</td>");
								break;
							}
						}
					}
					Configuration.ErrorMessage.Append("</tr>");

					Configuration.ErrorMessage.Append("<tr><td>Возврат постащику [" + returnSupplierItemsSum + "]</td>");
					foreach (var returnSupplierItem in returnSupplierItems)
					{
						foreach (var returnSupplierProductItem in returnSupplierItem.ReturnSupplierProductList)
						{
							if (returnSupplierProductItem.Code == currentProductCode)
							{
								Configuration.ErrorMessage.Append("<td>" + returnSupplierItem.DocNumber + "[" + returnSupplierProductItem.Count + "]</td>");
								break;
							}
						}
					}
					Configuration.ErrorMessage.Append("</tr>");
					Configuration.ErrorMessage.Append("</table>");


					// #####################################################################################
					// #### Find broken order

					var brokenDocs = new List<string>();
					var mismathCountDocs = new List<string>();

					var workMove = DataClass.GetWorkMove(dateTime, currentProductCode.ToString());

					foreach (var comingOrderItem in comingOrder)
					{
						if(!comingOrderItem.ComingProductList.Any(Item => Item.Code == currentProductCode)) continue;

						bool existDocInRajahQuery = false;
						foreach (var workMoveItem in workMove)
						{

							if (comingOrderItem.DocNumber.ToLower() == workMoveItem.DocNumber.ToLower())
							{

								if (comingOrderItem.ComingProductList.
									Where(Item => Item.Code == currentProductCode).
									Any(Item => Math.Abs(Item.Count) != Math.Abs(workMoveItem.Quan)))
								{
									mismathCountDocs.Add("<tr><td>" + workMoveItem.DocNumber + "</td><td>" + workMoveItem.Operation + "</td><td>" + workMoveItem.Quan + "</td><td>" + workMoveItem.SkladName + "</td></tr>");
								}

								workMoveItem.Deleted = true;
								existDocInRajahQuery = true;
							}
						}

						if(!existDocInRajahQuery)
						{
							// DocNumber not exist in rajah query :(
							//brokenDocs.Add("Ненайден ПРИХОДНЫЙ ОРДЕР в радже. Номер:" + comingOrderItem.DocNumber + "  ");
							brokenDocs.Add("<tr><td>" + comingOrderItem.DocNumber + "</td><td> ПРИХОДНЫЙ ОРДЕР </td><tr> ");
						}
					}

					foreach (var returnCustomerItem in returnCustomer)
					{
						if (!returnCustomerItem.ReturnCustomerProductList.Any(item => item.Code == currentProductCode)) continue;

						bool existDocInRajahQuery = false;
						foreach (var workMoveItem in workMove)
						{
							if (returnCustomerItem.DocNumber.ToLower() == workMoveItem.DocNumber.ToLower())
							{

								if (returnCustomerItem.ReturnCustomerProductList.
									Where(item => item.Code == currentProductCode).
									Any(item => Math.Abs(item.Count) != Math.Abs(workMoveItem.Quan)))
								{
									mismathCountDocs.Add("<tr><td>" + workMoveItem.DocNumber + "</td><td>" + workMoveItem.Operation + "</td><td>" + workMoveItem.Quan + "</td><td>" + workMoveItem.SkladName + "</td></tr>");
								}

								workMoveItem.Deleted = true;
								existDocInRajahQuery = true;
							}
						}

						if (!existDocInRajahQuery)
						{
							// DocNumber not exist in rajah query :(
							//brokenDocs.Add("Ненайден ВОЗВРАТ ОТ КЛИЕНТА в радже. Номер:" + returnCustomerItem.DocNumber + "  ");
							brokenDocs.Add("<tr><td>" + returnCustomerItem.DocNumber + "</td><td> ВОЗВРАТ ОТ КЛИЕНТА </td><tr> ");
						}
					}


					foreach (var invoiceItem in invoiceItems)
					{
						if (!invoiceItem.InvoiceProductList.Any(item => item.Code == currentProductCode)) continue;

						bool existDocInRajahQuery = false;
						foreach (var workMoveItem in workMove)
						{
							if (invoiceItem.DocNumber.ToLower() == workMoveItem.DocNumber.ToLower())
							{
								if (invoiceItem.InvoiceProductList.
									Where(item => item.Code == currentProductCode).
									Any(item => Math.Abs(item.Count) != Math.Abs(workMoveItem.Quan)))
								{
									mismathCountDocs.Add("<tr><td>" + workMoveItem.DocNumber + "</td><td>" + workMoveItem.Operation + "</td><td>" + workMoveItem.Quan + "</td><td>" + workMoveItem.SkladName + "</td></tr>");
								}

								workMoveItem.Deleted = true;
								existDocInRajahQuery = true;
							}
						}

						if (!existDocInRajahQuery)
						{
							// DocNumber not exist in rajah query :(
							//brokenDocs.Add("Ненайдена РАСХОДНАЯ НАКЛАДНАЯ в радже. Номер:" + invoiceItem.DocNumber + "  ");
							brokenDocs.Add("<tr><td>" + invoiceItem.DocNumber + "</td><td> РАСХОДНАЯ НАКЛАДНАЯ </td><tr> ");
						}
					}

					foreach (var discardedItem in discardedItems)
					{
						if (!discardedItem.DiscardedProductList.Any(Item => Item.Code == currentProductCode)) continue;
						bool existDocInRajahQuery = false;
						foreach (var workMoveItem in workMove)
						{
							if (discardedItem.DocNumber.ToLower() == workMoveItem.DocNumber.ToLower())
							{

								if (discardedItem.DiscardedProductList.
									Where(item => item.Code == currentProductCode).
									Any(item => Math.Abs(item.Count) != Math.Abs(workMoveItem.Quan)))
								{
									mismathCountDocs.Add("<tr><td>" + workMoveItem.DocNumber + "</td><td>" + workMoveItem.Operation + "</td><td>" + workMoveItem.Quan + "</td><td>" + workMoveItem.SkladName + "</td></tr>");
								}

								workMoveItem.Deleted = true;
								existDocInRajahQuery = true;
							}
						}

						if (!existDocInRajahQuery)
						{
							// DocNumber not exist in rajah query :(
							//brokenDocs.Add("Ненайдено СПИСАНИЕ в радже. Номер:" + discardedItem.DocNumber + "  ");
							brokenDocs.Add("<tr><td>" + discardedItem.DocNumber + "</td><td> СПИСАНИЕ </td><tr> ");
						}
					}

					foreach (var moveItem in moveItems)
					{
						if (!moveItem.MoveProductList.Any(item => item.Code == currentProductCode)) continue;
						bool existDocInRajahQuery = false;
						foreach (var workMoveItem in workMove)
						{
							if (moveItem.DocNumber.ToLower() == workMoveItem.DocNumber.ToLower())
							{

								if (moveItem.MoveProductList.
									Where(item => item.Code == currentProductCode).
									Any(item => Math.Abs(item.Count) != Math.Abs(workMoveItem.Quan)))
								{
									mismathCountDocs.Add("<tr><td>" + workMoveItem.DocNumber + "</td><td>" + workMoveItem.Operation + "</td><td>" + workMoveItem.Quan + "</td><td>" + workMoveItem.SkladName + "</td></tr>");
								}

								workMoveItem.Deleted = true;
								existDocInRajahQuery = true;
							}
						}

						if (!existDocInRajahQuery)
						{
							// DocNumber not exist in rajah query :(
							//brokenDocs.Add("Ненайдено ПЕРЕМЕЩЕНИЕ в радже. Номер:" + moveItem.DocNumber + "  ");
							brokenDocs.Add("<tr><td>" + moveItem.DocNumber + "</td><td> ПЕРЕМЕЩЕНИЕ </td><tr> ");
						}
					}

					foreach (var returnSupplierItem in returnSupplierItems)
					{
						if (!returnSupplierItem.ReturnSupplierProductList.Any(item => item.Code == currentProductCode)) continue;
						bool existDocInRajahQuery = false;
						foreach (var workMoveItem in workMove)
						{
							if (returnSupplierItem.DocNumber.ToLower() == workMoveItem.DocNumber.ToLower())
							{

								if (returnSupplierItem.ReturnSupplierProductList.
									Where(item => item.Code == currentProductCode).
									Any(item => Math.Abs(item.Count) != Math.Abs(workMoveItem.Quan)))
								{
									mismathCountDocs.Add("<tr><td>" + workMoveItem.DocNumber + "</td><td>" + workMoveItem.Operation + "</td><td>" + workMoveItem.Quan + "</td><td>" + workMoveItem.SkladName + "</td></tr>");
								}
								
								
								workMoveItem.Deleted = true;
								existDocInRajahQuery = true;
							}
						}

						if (!existDocInRajahQuery)
						{
							
							// DocNumber not exist in rajah query :(
							brokenDocs.Add("<tr><td>" + returnSupplierItem.DocNumber + "</td><td> ВОЗВРАТ ПОСТАВЩИКУ </td><tr> ");
							//brokenDocs.Add("Ненайден ВОЗВРАТ ПОСТАВЩИКУ в радже. Номер:" + returnSupplierItem.DocNumber + "  ");
						}
					}

					//Configuration.ErrorMessage.Append("<br><br>");
					//Configuration.ErrorMessage.Append("<font size='2px'>");
					//foreach (var brokenDoc in brokenDocs)
					//{
					//    Configuration.ErrorMessage.Append(brokenDoc + "<br>");
					//}
					//Configuration.ErrorMessage.Append("</font>");

					Configuration.ErrorMessage.Append("<br><br>");

					Configuration.ErrorMessage.Append("<table style='font-weight:normal;font-size: 11px;' width='500'><tr bgcolor='#CCCCCC'><th colspan='4'> Ненайденые документы в радже </th></tr>");
					Configuration.ErrorMessage.Append("<tr><th>Номер накладной</th><th>Операция</th></tr>");
					bool highlighting = false;
					foreach (var brokenDoc in brokenDocs)
					{
						highlighting = !highlighting;
						Configuration.ErrorMessage.Append(highlighting ? brokenDoc.Replace("<tr>", "<tr bgcolor='#EFEFEF'>") : brokenDoc);
					}
					Configuration.ErrorMessage.Append("</table><br><br>");



					Configuration.ErrorMessage.Append("<table style='font-weight:normal;font-size: 11px;' width='500'><tr><th colspan='4' bgcolor='#CCCCCC'> Несовпадает кол-во товара в накладной </th></tr>");
					Configuration.ErrorMessage.Append("<tr><th>Номер накладной</th><th>Операция</th><th>Кол-во</th><th>Склад</th></tr>");
					highlighting = false;
					foreach (var mismathCountDoc in mismathCountDocs)
					{
						highlighting = !highlighting;
						Configuration.ErrorMessage.Append(highlighting ? mismathCountDoc.Replace("<tr>", "<tr bgcolor='#EFEFEF'>") : mismathCountDoc);
					}
					Configuration.ErrorMessage.Append("</table><br><br>");



					Configuration.ErrorMessage.Append("<table style='font-weight:normal;font-size: 11px;' width='500'><tr><th colspan='4' bgcolor='#CCCCCC'> Ненайдено документов в запросе </th></tr>");
					Configuration.ErrorMessage.Append("<tr><th>Номер накладной</th><th>Операция</th><th>Кол-во</th><th>Склад</th></tr>");
					highlighting = false;
					foreach (var workMoveItem in workMove.Where(item => !item.Deleted))
					{
						highlighting = !highlighting;
						if (workMoveItem.SkladUsed)
							Configuration.ErrorMessage.Append("<tr "+(highlighting?"bgcolor='#EFEFEF'":"")+"><td><i>" + workMoveItem.DocNumber + "</i></td><td><i>" + workMoveItem.Operation + "</i></td><td><i>" + workMoveItem.Quan + "</i></td><td><i>" + workMoveItem.SkladName + "</i></td></tr> ");
						else
							Configuration.ErrorMessage.Append("<tr " + (highlighting ? "bgcolor='#EFEFEF'" : "") + "><td>" + workMoveItem.DocNumber + "</td><td> " + workMoveItem.Operation + " </td><td> " + workMoveItem.Quan + " </td><td> " + workMoveItem.SkladName + "</td></tr> ");
					}
					Configuration.ErrorMessage.Append("</table><br><br>");

				}


			}


			if (Configuration.ErrorMessage.Length > 10 && !fix)
			{
				if (DataClass.FindError(dateTime.ToString("dd.MM.yyyy"))) checkAnsver = false;
				if (DataClass.FindPPOrder(dateTime.ToString("dd.MM.yyyy"))) checkAnsver = false;
				if (DataClass.FindErrorInVozvrat(dateTime.ToString("dd.MM.yyyy"))) checkAnsver = false;
			}


			return checkAnsver;
		}

	}

	// ########################################################################################################
	// ########################################################################################################
	// ########################################################################################################
	// ########################################################################################################
	// ########################################################################################################
	// Check rajah data error

	enum DocType
	{
		Invoice, IncomingOrder, ReturnSypplier, ReturnCustomer, Move, Default
	}

	class RajahDoc
	{
		public bool Checked { get; set; }
		public string DocId { get; set; }
		public string DocNum { get; set; }
		public DocType Operation { get; set; }
		public string Date { get; set; }
		public string Manager { get; set; }
		public string Payer { get; set; }
		public List<RajahDocDetail> RajahDocDetailList { get; set; }
	}

	class RajahDocDetail
	{
		public bool Checked { get; set; }
		public string PosId { get; set; }
		public string ProductId { get; set; }
		public string SkladId { get; set; }
		public string PartyId { get; set; }
		public string BasePartyId { get; set; }
		public string Count { get; set; }
	}

	class RajahSkladRegTItem
	{
		public bool Checked { get; set; }
		public string Date { get; set; }
		public string BaseParty { get; set; }
		public string ProductId { get; set; }
		public string SkladId { get; set; }
		public string PartyId { get; set; }
		public string Count { get; set; }
	}


	/// <summary>
	/// Validate data in table SKLADREG%M with -> ( NAKLADNA... , MOVE , ORDER... )
	/// And 
	/// </summary>
	class CheckRajahDataError
	{
		private readonly List<RajahSkladRegTItem> _skladRegDocT = new List<RajahSkladRegTItem>();
		public string CheckDateStart { get; set; }
		public string CheckDateEnd { get; set; }

		bool LoadSkladRegT()
		{
			string query = 
				"select cast(date_to_str(ttt.date_) as date),ttt.party0,ttt.baseparty0,ttt.sklad0, \n"+
				" IIF(ttt.quan0%m <> '0', ttt.quan0%m,ttt.quan0%p) as quan, ttt.goods0 \n" +
				" from skladreg0%t ttt \n"+
				" where cast(date_to_str(ttt.date_) as date) >='" + CheckDateStart + "' \n" +
				" and cast(date_to_str(ttt.date_) as date) <='" + CheckDateEnd + "' ";
			GLOBAL.FbDatabase.ReaderExec(query);
			List<string> buf;
			long count = 0;
			int update = 0;
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				count++;
				update++;
				
				if (update == 500)
				{
					update = 0;
					Console.SetCursorPosition(0, Console.CursorTop);
					Console.Write("SkladRegT processed " + count + "row        ");
				}

				_skladRegDocT.Add(new RajahSkladRegTItem
				                  	{
				                  		Date = buf[0],
                                        PartyId = buf[1],
										BaseParty = buf[2],
										SkladId = buf[3],
										Count = buf[5],
                                        ProductId = buf[6],
                                        Checked = false
				                  	});
			}
			GLOBAL.FbDatabase.ReaderClose();
			Console.Write("\n");
			return true;
		}


		private readonly List<RajahDoc> _skladRegDoc = new List<RajahDoc>();

		bool LoadSkladRegM()
		{
			string query =
			" select sc.name0%i,sc.number0%i, skm.baseparty0, \n" +
			" skm.sklad0, skm.posref0, skm.docref0, \n" +
			" IIF(skm.quan0%m<>'0',skm.quan0%m,skm.quan0%p), \n" +
			"  date_to_str(skm.date_),sc.payer0, sc.manager0,skm.goods0,skm.party0 \n" +
			" from skladreg0%m skm, simplecap0 sc \n" +
			" where sc.id = skm.docref0 \n" +
			" and sc.deleted='0' \n" + 
			" and cast( date_to_str(skm.date_) as date ) >='" + CheckDateStart + "' \n" +
			" and cast( date_to_str(skm.date_) as date ) <='" + CheckDateEnd + "' \n" +
			" order by sc.number0%i ";

			GLOBAL.FbDatabase.ReaderExec(query);
			List<string> buf;
			//long count = 0;
			//int update = 0;
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				//count++;
				//update++;
				//ВОЗВРАТНАЯ НАКЛАДНАЯ
				//ВОЗВРАТНЫЙ ОРДЕР
				//НАКЛАДНАЯ НА ВНУТРЕННЕЕ ПЕРЕМЕЩЕНИЕ
				//ПРИХОДНЫЙ ОРДЕР
				//РАСХОДНАЯ НАКЛАДНАЯ

				//if (update == 500)
				//{
				//    update = 0;
				//    //Console.SetCursorPosition(0, Console.CursorTop);
				//    //Console.Write("Обработка SkladRegM " + count + "row        ");
				//}

				DocType type = DocType.Default;

				switch (buf[0])
				{
					case "ВОЗВРАТНЫЙ ОРДЕР":
						type = DocType.ReturnCustomer; break;

					case "НАКЛАДНАЯ НА ВНУТРЕННЕЕ ПЕРЕМЕЩЕНИЕ":
						type = DocType.Move; break;

					case "ПРИХОДНЫЙ ОРДЕР":
						type = DocType.IncomingOrder; break;

					case "РАСХОДНАЯ НАКЛАДНАЯ":
						type = DocType.ReturnCustomer; break;

					case "ВОЗВРАТНАЯ НАКЛАДНАЯ":
						type = DocType.ReturnSypplier; break;

					default: type = DocType.Default; break;
				}

				


				bool exist = false;

				for (int i = _skladRegDoc.Count-1; i >0 ; i--)
				{
					if (_skladRegDoc[i].DocId == buf[5])
					{
						
						exist = true;
						_skladRegDoc[i].RajahDocDetailList.Add(new RajahDocDetail
						{
							Count = buf[6].Replace('-', ' ').Trim(),
							BasePartyId = buf[2],
							PosId = buf[4],
							ProductId = buf[10],
							SkladId = buf[3],
                            PartyId = buf[11],
							Checked = false
						});
						break;
					}
				}


				if(!exist)
				{
					var det = new List<RajahDocDetail>
	          		{
	          			new RajahDocDetail
	          				{
	          					Count = buf[6].Replace('-',' ').Trim(),
	          					BasePartyId = buf[2],
	          					PosId = buf[4],
	          					ProductId = buf[10],
	          					SkladId = buf[3],
								PartyId = buf[11],
	          					Checked = false
	          				}
	          		};

					_skladRegDoc.Add(new RajahDoc
					{
						Date = DateTime.Parse(buf[7]).ToString("dd.MM.yyyy"),
						DocId = buf[5],
						DocNum = buf[1],
						Checked = false,
						Manager = buf[9],
						Operation = type,
						Payer = buf[8],
						RajahDocDetailList = det
					});
				}

			}

			GLOBAL.FbDatabase.ReaderClose();
			//Console.Write("\n");
			return true;
		}


		// -----------------------------------------------------
		// Invoice Doc

		private readonly List<RajahDoc> _invoiceDoc = new List<RajahDoc>();

		bool LoadInvoices()
		{
			string query =
				" select nk.id, nk.number0%i, cast( date_to_str(nk.date0) as date), \n" +
				" nk.manager0,nk.payer0, \n" +
				" nkd.id, nkd.goods0, nkd.sklad0, nkd.baseparty0, nkd.quan0 \n" +
				" from nakladna0 nk, nakldetail0 nkd \n" +
				" where nk.id = nkd.invoce0 \n" +
				" and nk.deleted='0' and nkd.deleted='0' \n" + 
				" and cast( date_to_str(nk.date0) as date) >= '" + CheckDateStart + "' "+
				" and cast( date_to_str(nk.date0) as date) <= '" + CheckDateEnd + "' ";

			GLOBAL.FbDatabase.ReaderExec(query);
			List<string> buf;
			//int count = 0;
			//int update = 0;
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				//update++;
				//count++;

				//if (update == 500)
				//{
				//    update = 0;
				//    Console.SetCursorPosition(0, Console.CursorTop);
				//    Console.Write("Обработка Invoice " + count + "row        ");
				//}

				const DocType type = DocType.Invoice;

				bool exist = false;
				for (int i = _invoiceDoc.Count - 1; i > 0; i--)
				{
					if (_invoiceDoc[i].DocId == buf[0])
					{
						exist = true;
						_invoiceDoc[i].RajahDocDetailList.Add(new RajahDocDetail
						{
							Count = buf[9],
							BasePartyId = buf[8],
							PosId = buf[5],
							ProductId = buf[6],
							SkladId = buf[7],
							Checked = false
						});
						break;
					}
				}


				if(!exist)
				{
					var det = new List<RajahDocDetail>
	          		{
	          			new RajahDocDetail
	          				{
	          					Count = buf[9],
								BasePartyId = buf[8],
								PosId = buf[5],
								ProductId = buf[6],
								SkladId = buf[7],
	          					Checked = false
	          				}
	          		};

					_invoiceDoc.Add(new RajahDoc
					{
						Date = DateTime.Parse(buf[2]).ToString("dd.MM.yyyy"),
						DocId = buf[0],
						DocNum = buf[1],
						Checked = false,
						Manager = buf[3],
						Operation = type,
						Payer = buf[4],
						RajahDocDetailList = det
					});
				}


			}
			GLOBAL.FbDatabase.ReaderClose();
			return true;
		}



		// -----------------------------------------------------
		// Orders Doc

		private readonly List<RajahDoc> _orderDoc = new List<RajahDoc>();

		bool LoadOrders()
		{
			string query =
				" select orr.id, orr.number0%i, date_to_str(orr.date0), \n" +
				" orr.manager0, orr.payer0, \n" +
				" ord.id, ord.goods0,ord.sklad0,ord.baseparty0,ord.quan0 \n" +
				" from order0 orr, orddetail0 ord \n" +
				" where ord.invoce0 = orr.id \n" +
				" and orr.deleted='0' and ord.deleted='0' \n" + 
				" and cast( date_to_str(orr.date0) as date) >= '" + CheckDateStart + "' \n"+
				" and cast( date_to_str(orr.date0) as date) <= '" + CheckDateEnd + "' \n";

			GLOBAL.FbDatabase.ReaderExec(query);
			List<string> buf;
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				const DocType type = DocType.Invoice;

				if (!_orderDoc.Any(rajahDoc => rajahDoc.DocId == buf[0]))
				{
					_orderDoc.Add(new RajahDoc
					{
						Date = DateTime.Parse(buf[2]).ToString("dd.MM.yyyy"),
						DocId = buf[0],
						Checked = false,
						DocNum = buf[1],
						Manager = buf[3],
						Operation = type,
						Payer = buf[4],
						RajahDocDetailList = new List<RajahDocDetail>()
					});
				}


				foreach (var rajahDoc in _orderDoc)
				{
					if (rajahDoc.DocId == buf[5])
					{
						rajahDoc.RajahDocDetailList.Add(new RajahDocDetail
						{
							Count = buf[9],
							BasePartyId = buf[8],
							PosId = buf[5],
							ProductId = buf[6],
							SkladId = buf[7]
						});

						break;
					}
				}

			}

			GLOBAL.FbDatabase.ReaderClose();

			return true;
		}

		// -----------------------------------------------------
		// Move Doc

		private readonly List<RajahDoc> _moveDoc = new List<RajahDoc>();

		bool LoadMoves()
		{
			string query =
				" select mv.id, mv.number0%i, date_to_str(mv.date0), \n" +
				" mv.manager0,mv.payer0, \n" +
				" mvd.id,mvd.goods0,mvd.sklad0,mvd.baseparty0,mvd.quan0 \n" +
				" from move0 mv, movedetail0 mvd \n" +
				" where mvd.invoce0 = mv.id \n" +
                " and mv.deleted='0' and mvd.deleted='0' \n"+ 
				" and cast( date_to_str(mv.date0) as date) >= '" + CheckDateStart + "' \n"+
				" and cast( date_to_str(mv.date0) as date) <= '" + CheckDateEnd + "' \n";

			GLOBAL.FbDatabase.ReaderExec(query);
			List<string> buf;
			while ((buf = GLOBAL.FbDatabase.ReaderNext()) != null)
			{
				const DocType type = DocType.Invoice;

				if (!_orderDoc.Any(rajahDoc => rajahDoc.DocId == buf[0]))
				{
					_orderDoc.Add(new RajahDoc
					{
						Date = DateTime.Parse(buf[2]).ToString("dd.MM.yyyy"),
						DocId = buf[0],
						Checked = false,
						DocNum = buf[1],
						Manager = buf[3],
						Operation = type,
						Payer = buf[4],
						RajahDocDetailList = new List<RajahDocDetail>() 
					});
				}


				foreach (var rajahDoc in _orderDoc)
				{
					if (rajahDoc.DocId == buf[5])
					{
						rajahDoc.RajahDocDetailList.Add(new RajahDocDetail
						{
							Count = buf[9],
							BasePartyId = buf[8],
							PosId = buf[5],
							ProductId = buf[6],
							SkladId = buf[7]
						});

						break;
					}
				}

			}
			GLOBAL.FbDatabase.ReaderClose();

			return true;
		}


		public string CheckData(DateTime checkDateStart,DateTime checkDateEnd)
		{
			var result = new StringBuilder();
			var dayCounts = checkDateEnd - checkDateStart ;
			var dayCount = dayCounts.Days;

			for (int i = 0; i < dayCount; i++)
			{
				_skladRegDocT.Clear();
				_moveDoc.Clear();
				_orderDoc.Clear();
				_invoiceDoc.Clear();

				var res = CheckDataDetail(checkDateStart.AddDays(i).ToString("dd.MM.yyyy"));
				if(res.Length>5)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("----------------------------------------------------");
					Console.WriteLine(res);
					Console.WriteLine("----------------------------------------------------");
					Console.ResetColor();
				}
			}

			return result.ToString();
		}

		public string CheckDataDetail(string checkDate)
		{
			Console.WriteLine("\n * Запуск проверки за: "+checkDate);
			CheckDateStart = checkDate;
			CheckDateEnd = checkDate;

			var report = new StringBuilder();
			Console.Write("Загрузка SkladRegM - ");
			LoadSkladRegM();
			//Console.WriteLine("Loading SkladRegT");
			//LoadSkladRegT();
			Console.Write("Invoice - ");
			LoadInvoices();
			Console.Write("Orders - ");
			LoadOrders();
			Console.Write("Move");
			LoadMoves();

			// ---------------------------------------------------
			// INVOICE

			//int count = 0;
			//int update = 0;
			foreach (var rajahDocG in _skladRegDoc)
			{
				//count++;
				//update++;

				//if (update >= 500)
				//{
				//    update = 0;
				//    Console.SetCursorPosition(0, Console.CursorTop);
				//    Console.Write("Обрабатываю: " + (count*100/_skladRegDoc.Count) + "%        ");
				//}

				foreach (var rajahDocI in _invoiceDoc)
				{

					if(rajahDocG.DocId == rajahDocI.DocId)
					{
						//Console.WriteLine("MATCH");


						bool allMatch = true;

						if (rajahDocG.DocNum != rajahDocI.DocNum)
						{
							allMatch = false;
							report.Append("Не совпадает НОМЕР у накладной: " + rajahDocI.DocId + " | " + rajahDocI.DocNum + "\n");
						}

						if(rajahDocG.Date != rajahDocI.Date)
						{
							allMatch = false;
							//report.Append("Не совпадает ДАТА у накладной: " + rajahDocI.DocId + " | " + rajahDocI.DocNum + "\n");
							report.Append("Не совпадает ДАТА у накладной: " + rajahDocI.DocId + " | " + rajahDocI.DocNum + " Даты: " + rajahDocG.Date + "|" + rajahDocI.Date + " \n");
						}

						if (rajahDocG.Manager != rajahDocI.Manager)
						{
							allMatch = false;
							report.Append("Не совпадает МЕНЕДЖЕР у накладной: " + rajahDocI.DocId + " | " + rajahDocI.DocNum + "\n");
						}

						if (rajahDocG.Payer != rajahDocI.Payer)
						{
							allMatch = false;
							report.Append("Не совпадает ПОКУПАТЕЛЬ у накладной: " + rajahDocI.DocId + " | " + rajahDocI.DocNum + "\n");
						}

						bool detailMatch = true;
						foreach (var rajahDocDetail in rajahDocI.RajahDocDetailList)
						{
							foreach (var docDetail in rajahDocG.RajahDocDetailList)
							{
								
								if(rajahDocDetail.PosId==docDetail.PosId)
								{
									bool allDetailMatch = true;

									if(rajahDocDetail.Count !=docDetail.Count )
									{
										allDetailMatch = false;
										report.Append("Не совпадает КОЛ-ВО у накладной: " + rajahDocI.DocId + " | " + rajahDocI.DocNum + "Позиция: " + rajahDocDetail.PosId + " Кол-во: " + rajahDocDetail.Count +"|"+ docDetail.Count + " \n");
									}

									if (rajahDocDetail.BasePartyId != docDetail.BasePartyId)
									{
										allDetailMatch = false;
										report.Append("Не совпадает ПАРТИЯ ТОВАРА у накладной: " + rajahDocI.DocId + " | " + rajahDocI.DocNum + "Позиция: " + rajahDocDetail.PosId + "\n");
									}

									if (rajahDocDetail.ProductId != docDetail.ProductId)
									{
										allDetailMatch = false;
										report.Append("Не совпадает ИД ПРОДУКТА у накладной: " + rajahDocI.DocId + " | " + rajahDocI.DocNum + "Позиция: " + rajahDocDetail.PosId + "\n");
									}

									if (rajahDocDetail.ProductId != docDetail.ProductId)
									{
										allDetailMatch = false;
										report.Append("Не совпадает ИД ПРОДУКТА у накладной: " + rajahDocI.DocId + " | " + rajahDocI.DocNum + "Позиция: " + rajahDocDetail.PosId + "\n");
									}

									if (rajahDocDetail.SkladId != docDetail.SkladId)
									{
										allDetailMatch = false;
										report.Append("Не совпадает СКЛАД у накладной: " + rajahDocI.DocId + " | " + rajahDocI.DocNum + "Позиция: " + rajahDocDetail.PosId + " Склад: " + rajahDocDetail.SkladId +"|"+ docDetail.SkladId + " INVOICE \n");
									}

									if(allDetailMatch)
									{
										rajahDocDetail.Checked = true;
										docDetail.Checked = true;
									}
									else
									{
										detailMatch = false;
									}

								}


							}
						}


						if(!detailMatch)
						{
							allMatch = false;
							report.Append("Не совпали ПОЗИЦИИ у накладной: " + rajahDocI.DocId + " | " + rajahDocI.DocNum + "\n");
						}

						if (allMatch)
						{
							rajahDocG.Checked = true;
							rajahDocI.Checked = true;
						}

					}
				}


				// ---------------------------------------------------
				// MOVE


				foreach (var rajahDocM in _moveDoc)
				{

					Console.WriteLine("[" + rajahDocG.DocId + "] - [" + rajahDocM.DocId + "]");
					Console.ReadKey();

					if (rajahDocG.DocId == rajahDocM.DocId)
					{
						Console.WriteLine("MATCH");
						bool allMatch = true;

						if (rajahDocG.DocNum != rajahDocM.DocNum)
						{
							allMatch = false;
							report.Append("Не совпадает НОМЕР у накладной: " + rajahDocM.DocId + " | " + rajahDocM.DocNum + "\n");
						}

						if (rajahDocG.Date != rajahDocM.Date)
						{
							allMatch = false;
							//report.Append("2Не совпадает ДАТА у накладной: " + rajahDocM.DocId + " | " + rajahDocM.DocNum + "\n");
							report.Append("Не совпадает ДАТА у накладной: " + rajahDocM.DocId + " | " + rajahDocM.DocNum + " Даты: " + rajahDocG.Date + "|" + rajahDocM.Date + " \n");
						}

						if (rajahDocG.Manager != rajahDocM.Manager)
						{
							allMatch = false;
							report.Append("Не совпадает МЕНЕДЖЕР у накладной: " + rajahDocM.DocId + " | " + rajahDocM.DocNum + "\n");
						}

						if (rajahDocG.Payer != rajahDocM.Payer)
						{
							allMatch = false;
							report.Append("Не совпадает ПОКУПАТЕЛЬ у накладной: " + rajahDocM.DocId + " | " + rajahDocM.DocNum + "\n");
						}

						bool detailMatch = true;
						foreach (var rajahDocDetail in rajahDocM.RajahDocDetailList)
						{
							foreach (var docDetail in rajahDocG.RajahDocDetailList)
							{

								if (rajahDocDetail.PosId == docDetail.PosId)
								{
									bool allDetailMatch = true;

									if (rajahDocDetail.Count != docDetail.Count)
									{
										allDetailMatch = false;
										report.Append("Не совпадает КОЛ-ВО у накладной: " + rajahDocM.DocId + " | " + rajahDocM.DocNum + "Позиция: " + rajahDocDetail.PosId + "\n");
									}

									if (rajahDocDetail.BasePartyId != docDetail.BasePartyId)
									{
										allDetailMatch = false;
										report.Append("Не совпадает ПАРТИЯ ТОВАРА у накладной: " + rajahDocM.DocId + " | " + rajahDocM.DocNum + "Позиция: " + rajahDocDetail.PosId + "\n");
									}

									if (rajahDocDetail.ProductId != docDetail.ProductId)
									{
										allDetailMatch = false;
										report.Append("Не совпадает ИД ПРОДУКТА у накладной: " + rajahDocM.DocId + " | " + rajahDocM.DocNum + "Позиция: " + rajahDocDetail.PosId + "\n");
									}

									if (rajahDocDetail.ProductId != docDetail.ProductId)
									{
										allDetailMatch = false;
										report.Append("Не совпадает ИД ПРОДУКТА у накладной: " + rajahDocM.DocId + " | " + rajahDocM.DocNum + "Позиция: " + rajahDocDetail.PosId + "\n");
									}

									if (rajahDocDetail.SkladId != docDetail.SkladId)
									{
										allDetailMatch = false;
										report.Append("Не совпадает СКЛАД у накладной: " + rajahDocM.DocId + " | " + rajahDocM.DocNum + "Позиция: " + rajahDocDetail.PosId + " Склад: " + rajahDocDetail.SkladId + "|" + docDetail.SkladId + " MOVE \n");
									}

									if (allDetailMatch)
									{
										rajahDocDetail.Checked = true;
										docDetail.Checked = true;
									}
									else
									{
										detailMatch = false;
									}

								}


							}
						}


						if (!detailMatch)
						{
							allMatch = false;
							report.Append("Не совпали ПОЗИЦИИ у накладной: " + rajahDocM.DocId + " | " + rajahDocM.DocNum + "\n");
						}

						if (allMatch)
						{
							rajahDocG.Checked = true;
							rajahDocM.Checked = true;
						}

					}	
				}

				// ---------------------------------------------------
				// ORDER

				foreach (var rajahDocO in _orderDoc)
				{

					if (rajahDocG.DocId == rajahDocO.DocId)
					{
						bool allMatch = true;

						if (rajahDocG.DocNum != rajahDocO.DocNum)
						{
							allMatch = false;
							report.Append("Не совпадает НОМЕР у накладной: " + rajahDocO.DocId + " | " + rajahDocO.DocNum + "\n");
						}

						if (rajahDocG.Date != rajahDocO.Date)
						{
							allMatch = false;
							report.Append("Не совпадает ДАТА у накладной: " + rajahDocO.DocId + " | " + rajahDocO.DocNum + " Даты: " + rajahDocG.Date +"|"+ rajahDocO.Date + " \n");
						}

						if (rajahDocG.Manager != rajahDocO.Manager)
						{
							allMatch = false;
							report.Append("Не совпадает МЕНЕДЖЕР у накладной: " + rajahDocO.DocId + " | " + rajahDocO.DocNum + "\n");
						}

						if (rajahDocG.Payer != rajahDocO.Payer)
						{
							allMatch = false;
							report.Append("Не совпадает ПОКУПАТЕЛЬ у накладной: " + rajahDocO.DocId + " | " + rajahDocO.DocNum + "\n");
						}

						bool detailMatch = true;
						foreach (var rajahDocDetail in rajahDocO.RajahDocDetailList)
						{
							foreach (var docDetail in rajahDocG.RajahDocDetailList)
							{

								if (rajahDocDetail.PosId == docDetail.PosId)
								{
									bool allDetailMatch = true;

									if (rajahDocDetail.Count != docDetail.Count)
									{
										allDetailMatch = false;
										report.Append("Не совпадает КОЛ-ВО у накладной: " + rajahDocO.DocId + " | " + rajahDocO.DocNum + "Позиция: " + rajahDocDetail.PosId + "\n");
									}

									if (rajahDocDetail.BasePartyId != docDetail.BasePartyId)
									{
										allDetailMatch = false;
										report.Append("Не совпадает ПАРТИЯ ТОВАРА у накладной: " + rajahDocO.DocId + " | " + rajahDocO.DocNum + "Позиция: " + rajahDocDetail.PosId + "\n");
									}

									if (rajahDocDetail.ProductId != docDetail.ProductId)
									{
										allDetailMatch = false;
										report.Append("Не совпадает ИД ПРОДУКТА у накладной: " + rajahDocO.DocId + " | " + rajahDocO.DocNum + "Позиция: " + rajahDocDetail.PosId + "\n");
									}

									if (rajahDocDetail.ProductId != docDetail.ProductId)
									{
										allDetailMatch = false;
										report.Append("Не совпадает ИД ПРОДУКТА у накладной: " + rajahDocO.DocId + " | " + rajahDocO.DocNum + "Позиция: " + rajahDocDetail.PosId + "\n");
									}

									if (rajahDocDetail.SkladId != docDetail.SkladId)
									{
										allDetailMatch = false;
										report.Append("Не совпадает СКЛАД у накладной: " + rajahDocO.DocId + " | " + rajahDocO.DocNum + "Позиция: " + rajahDocDetail.PosId + " Склад: " + rajahDocDetail.SkladId + "|" + docDetail.SkladId + " ORDER \n");
									}

									if (allDetailMatch)
									{
										rajahDocDetail.Checked = true;
										docDetail.Checked = true;
									}
									else
									{
										detailMatch = false;
									}

								}


							}
						}


						if (!detailMatch)
						{
							allMatch = false;
							report.Append("Не совпали ПОЗИЦИИ у накладной: " + rajahDocO.DocId + " | " + rajahDocO.DocNum + "\n");
						}

						if (allMatch)
						{
							rajahDocG.Checked = true;
							rajahDocO.Checked = true;
						}

					}	

				}

				// --------------------------------------------------
				// Check SKLADREG%T

				//rajahDocG

				foreach (var rajahDocDetail in rajahDocG.RajahDocDetailList)
				{

					foreach (var rajahSkladRegTItem in _skladRegDocT)
					{
						if(rajahSkladRegTItem.PartyId==rajahDocDetail.PartyId &&
							rajahSkladRegTItem.ProductId==rajahDocDetail.ProductId &&
							rajahSkladRegTItem.BaseParty==rajahDocDetail.BasePartyId &&
							rajahSkladRegTItem.Date==rajahDocG.Date &&
							rajahSkladRegTItem.SkladId == rajahDocDetail.SkladId)
						{
							if(rajahSkladRegTItem.Count != rajahDocDetail.Count)
							{
								report.Append("Между SKLADREG..M&T не совпали кол-во. Номер: " + rajahDocG.DocId + " | " + rajahDocG.DocNum + "\n");
							}
							else
							{
								rajahSkladRegTItem.Checked = true;
							}

						}
					}

				}

				

			}

			foreach (var rajahSkladRegTItem in _skladRegDocT.Where(item => !item.Checked))
			{
				report.Append("НеИспольз SKLADREG%T ! Party:" + rajahSkladRegTItem.PartyId + "  BaseParty: " + rajahSkladRegTItem.BaseParty + " Date:" + rajahSkladRegTItem.Date + " Count: " + rajahSkladRegTItem.Count + "  \n");
			}

			foreach (var rajahDoc in _skladRegDoc.Where(item=>!item.Checked))
			{
				report.Append("НеИспольз SKLADREG%M. Номер:" + rajahDoc.DocId + " | " + rajahDoc.DocNum + " Операция:" + rajahDoc.Operation+"\n");
			}

			foreach (var rajahDoc in _invoiceDoc.Where(item => !item.Checked))
			{
				report.Append("НеИспольз INVOICE. Номер:" + rajahDoc.DocId + " | " + rajahDoc.DocNum + " Операция:" + rajahDoc.Operation + "\n");
			}

			foreach (var rajahDoc in _moveDoc.Where(item => !item.Checked))
			{
				report.Append("НеИспольз MOVE. Номер:" + rajahDoc.DocId + " | " + rajahDoc.DocNum + " Операция:" + rajahDoc.Operation + "\n");
			}

			foreach (var rajahDoc in _orderDoc.Where(item => !item.Checked))
			{
				report.Append("НеИспольз ORDER. Номер:" + rajahDoc.DocId + " | " + rajahDoc.DocNum + " Операция:" + rajahDoc.Operation + "\n");
			}


			//-------------------------------------------------------------------------------
			// Check SKLADREG%T for <> SKLADREG%M

			Console.WriteLine("");

			return report.ToString();
		}


	}





	// ########################################################################################################
	// ########################################################################################################
	// ########################################################################################################
	// ########################################################################################################
	// ########################################################################################################
	// Convert data to XML format

	static class DataToXML
	{
		public static string Convert(
			List<DataClass.RestItem> restListMain, List<DataClass.RestItem> restListSoh, List<DataClass.RestItem> restListRes,
			List<DataClass.RestItem> restListMainOld, List<DataClass.RestItem> restListSohOld, List<DataClass.RestItem> restListResOld,
			List<DataClass.ComingOrderItem> comingOrder, List<DataClass.ReturnCustomerItem> returnCustomer,
			List<DataClass.InvoiceItem> invoiceItems, List<DataClass.DiscardedItem> discardedItems,
			List<DataClass.OrderItem> orderItems, List<DataClass.MoveItem> moveItems,
			List<DataClass.ReturnSupplierItem> returnSupplierItems, List<DataClass.RestVTRTItem> restVTRTItems, string date)
		{

			var xmlFileBody = new StringBuilder();


			// ********************************  REST  **************************************
			xmlFileBody.Append("<Остатки>\n");
			GLOBAL.Interface.SetState(ActionState.GettingRest);
			xmlFileBody.Append("<Склад Тип=\"Основной\">\n");
			//

			//var restListMain = DataClass.GetRest(Configuration.SkladMain, dateTime.ToString("dd.MM.yyyy"));

			foreach (var restItem in restListMain)
			{
				xmlFileBody.Append("<Товар КодО=\"" + restItem.Code +
								   "\" Количество=\"" + restItem.Count + "\" Цена=\"" + restItem.Price.ToString().Replace(",", ".") + "\"/>\n");
			}
			xmlFileBody.Append("</Склад>\n");

			if (Configuration.SkladSoh.Length > 3)
			{
				xmlFileBody.Append("<Склад Тип=\"СОХ\">\n");
				//
				//var restListSoh = DataClass.GetRest(Configuration.SkladSoh, dateTime.ToString("dd.MM.yyyy"));
				foreach (var restItem in restListSoh)
				{
					xmlFileBody.Append("<Товар КодО=\"" + restItem.Code +
									   "\" Количество=\"" + restItem.Count + "\" Цена=\"" + restItem.Price.ToString().Replace(",", ".") + "\"/>\n");
				}
				xmlFileBody.Append("</Склад>\n");
			}
			else
			{
				xmlFileBody.Append("<Склад Тип=\"СОХ\"/>\n");
			}


            if (Configuration.SkladSpoilage.Length > 3)
            {
                xmlFileBody.Append("<Склад Тип=\"Брак\">\n");
                //
                //var restListSoh = DataClass.GetRest(Configuration.SkladSoh, dateTime.ToString("dd.MM.yyyy"));
                foreach (var restItem in restListSoh)
                {
                    xmlFileBody.Append("<Товар КодО=\"" + restItem.Code +
                                       "\" Количество=\"" + restItem.Count + "\" Цена=\"" + restItem.Price.ToString().Replace(",", ".") + "\"/>\n");
                }
                xmlFileBody.Append("</Склад>\n");
            }
            else
            {
                xmlFileBody.Append("<Склад Тип=\"БРАК\"/>\n");
            }


			if (Configuration.SkladReserve.Length > 3)
			{
				xmlFileBody.Append("<Склад Тип=\"РезервВИП\">\n");
				//var restListRes = DataClass.GetRest(Configuration.SkladReserve, dateTime.ToString("dd.MM.yyyy"));
				foreach (var restItem in restListRes)
				{
					xmlFileBody.Append("<Товар КодО=\"" + restItem.Code +
									   "\" Количество=\"" + restItem.Count + "\" Цена=\"" + restItem.Price.ToString().Replace(",", ".") + "\"/>\n");
				}
				xmlFileBody.Append("</Склад>\n");
			}
			else
			{
				xmlFileBody.Append("<РезервВИП/>\n");
			}

			
			xmlFileBody.Append("</Остатки>");



			// ********************************  ComingOrder  **************************************


			xmlFileBody.Append("<Приход>");
			foreach (var comingOrderItem in comingOrder)
			{
				if (comingOrderItem.ComingProductList.Count <= 0) continue;
				xmlFileBody.Append("<Документ Номер=\"" + comingOrderItem.DocNumber + "\" Операция=\"Приход\">\n");
				xmlFileBody.Append("<НаклПост Номер=\"" + comingOrderItem.NaklPostNumber + "\" Дата=\"" + comingOrderItem.Date +
								   "\"/>\n");
                xmlFileBody.Append("<Склад Тип=\"" + comingOrderItem.Store.Trim() + "\"/>\n");
				xmlFileBody.Append("<Контрагент ЕДРПОУ=\"" + comingOrderItem.EDRPOU + "\" Код=\"" + comingOrderItem.CustomerId +
								   "\">" + comingOrderItem.CustomerName + "<ТРТ Код=\"\"/></Контрагент>\n");
				xmlFileBody.Append("<ТЗ></ТЗ>\n");

				foreach (var comingProductItem in comingOrderItem.ComingProductList)
				{
					xmlFileBody.Append("<Товар КодО=\"" + comingProductItem.Code + "\" Количество=\"" + comingProductItem.Count +
									   "\" Цена=\"" + comingProductItem.Price.ToString().Replace(",", ".") + "\"/>\n");
				}

				xmlFileBody.Append("</Документ>\n");
			}




			// ********************************  Return product from customers  **************************************


			//var returnCustomer = DataClass.GetReturnCustomer(dateTime.ToString("dd.MM.yyyy"));

			foreach (var returnCustomerItem in returnCustomer)
			{
				if (returnCustomerItem.ReturnCustomerProductList.Count <= 0) continue;

				xmlFileBody.Append("<Документ Номер=\"" + returnCustomerItem.DocNumber + "\" Операция=\"ВозвратПокупателя\">\n");
				xmlFileBody.Append("<НаклПост Номер=\"\" Дата=\"" + returnCustomerItem.Date + "\"/>\n");
                xmlFileBody.Append("<Склад Тип=\"" + returnCustomerItem.Store.Trim() + "\"/>\n");
				xmlFileBody.Append("<Контрагент ЕДРПОУ=\"" + returnCustomerItem.EDRPOU + "\" Код=\"" +
								   returnCustomerItem.CustomerId + "\">" + returnCustomerItem.CustomerName + " " +
								   (returnCustomerItem.ShopName != ""
										? "<ТРТ Код=\"" + returnCustomerItem.ShopId + "\">" + returnCustomerItem.ShopName + "</ТРТ>"
										: "<ТРТ Код=\"\"/>") + " </Контрагент>\n");
				xmlFileBody.Append("<ТЗ>" + returnCustomerItem.TZ + "</ТЗ>\n");

				foreach (var customerProductItem in returnCustomerItem.ReturnCustomerProductList)
				{
					xmlFileBody.Append("<Товар КодО=\"" + customerProductItem.Code + "\" Количество=\"" + customerProductItem.Count +
									   "\" Цена=\"" + customerProductItem.Price.ToString().Replace(",", ".") + "\"/>\n");
				}

				xmlFileBody.Append("</Документ>\n");
			}




			// ********************************  Move orders  **************************************


			//var moveItems = DataClass.GetMove(dateTime.ToString("dd.MM.yyyy"));

			foreach (var moveItem in moveItems)
			{
				if (moveItem.MoveProductList.Count <= 0) continue;

				xmlFileBody.Append("<Документ Номер=\"" + moveItem.DocNumber + "\" Операция=\"Перемещение\">\n");
				xmlFileBody.Append("<НаклПост Номер=\"\" Дата=\"" + date + "\"/>\n");
                xmlFileBody.Append("<Склад Тип=\"" + moveItem.Store.Trim() + "\"/>\n");
				xmlFileBody.Append("<Контрагент ЕДРПОУ=\"\" Код=\"" + moveItem.CustomerId + "\">" + moveItem.CustomerName +
								   "<ТРТ Код=\"\"/> </Контрагент>\n");
				xmlFileBody.Append("<ТЗ/>\n");

				foreach (var moveProductItem in moveItem.MoveProductList)
				{
					xmlFileBody.Append("<Товар КодО=\"" + moveProductItem.Code + "\" Количество=\"" + moveProductItem.Count +
									   "\" Цена=\"" + moveProductItem.Price.ToString().Replace(",", ".") + "\"/>\n");
				}
				xmlFileBody.Append("</Документ>\n");
			}

			xmlFileBody.Append("</Приход>\n");




			// ********************************  Invoice  **************************************

			xmlFileBody.Append("<Расход>\n");
			//var invoiceItems = DataClass.GetInvoice(dateTime.ToString("dd.MM.yyyy"));

			foreach (var invoiceItem in invoiceItems)
			{
				if (invoiceItem.InvoiceProductList.Count <= 0) continue;

				xmlFileBody.Append("<Документ Номер=\"" + invoiceItem.DocNumber + "\" Операция=\"Продажа\">\n");
                xmlFileBody.Append("<Склад Тип=\"" + invoiceItem.Store.Trim() + "\"/>\n");
				xmlFileBody.Append("<Контрагент ЕДРПОУ=\"" + invoiceItem.EDRPOU + "\" Код=\"" + invoiceItem.CustomerId + "\">" +
								   invoiceItem.CustomerName + " " +
								   ((invoiceItem.ShopName != "")
										? ("<ТРТ Код=\"" + invoiceItem.ShopId + "\">" + invoiceItem.ShopName + "</ТРТ>")
										: "<ТРТ Код=\"\"/>") + " </Контрагент>\n");
				xmlFileBody.Append(invoiceItem.Note.IndexOf("ВОЗМЕЩЕНИЕ") != -1 ? ("<ТЗ/>") : ("<ТЗ>" + invoiceItem.TZ + "</ТЗ>\n"));
				
				foreach (var invoiceProductItem in invoiceItem.InvoiceProductList)
				{
					xmlFileBody.Append("<Товар КодО=\"" + invoiceProductItem.Code + "\" Количество=\"" + invoiceProductItem.Count +
						"\" Цена=\"" + (invoiceItem.Note.IndexOf("ВОЗМЕЩЕНИЕ") != -1 ?"0":(invoiceProductItem.Price.ToString().Replace(",", "."))) + "\"/>\n");
				}
				xmlFileBody.Append("</Документ>\n");
			}



			// ********************************  Spisanie  **************************************

			//var discardedItems = DataClass.GetDiscarded(dateTime.ToString("dd.MM.yyyy"));

			foreach (var discardedItem in discardedItems)
			{
				if (discardedItem.DiscardedProductList.Count <= 0) continue;

				xmlFileBody.Append("<Документ Номер=\"" + discardedItem.DocNumber + "\" Операция=\"Списание\">\n");
                xmlFileBody.Append("<Склад Тип=\"" + discardedItem.Store.Trim() + "\"/>\n"); 
                xmlFileBody.Append("<Контрагент ЕДРПОУ=\"\" Код=\"\">Списание<ТРТ Код=\"\"/></Контрагент>\n");
                xmlFileBody.Append("<ТЗ/>\n");

				foreach (var discardedProductItem in discardedItem.DiscardedProductList)
				{
					xmlFileBody.Append("<Товар КодО=\"" + discardedProductItem.Code + "\" Количество=\"" +
									   discardedProductItem.Count + "\" Цена=\"" + discardedProductItem.Price.ToString().Replace(",", ".") + "\"/>\n");
				}
				xmlFileBody.Append("</Документ>\n");
			}





			// ********************************  ReturnSupplier  **************************************

			//var returnSupplierItems = DataClass.GetReturnSupplier(dateTime.ToString("dd.MM.yyyy"));

			foreach (var returnSupplierItem in returnSupplierItems)
			{
                if (returnSupplierItem.ReturnSupplierProductList.Count <= 0) continue;
                
				xmlFileBody.Append("<Документ Номер=\"" + returnSupplierItem.DocNumber + "\" Операция = \"ВозвратПоставщику\">\n");
                xmlFileBody.Append("<Склад Тип=\"" + returnSupplierItem.Store.Trim() + "\"/>\n");
				xmlFileBody.Append("<Контрагент ЕДРПОУ=\"\" Код=\"" + returnSupplierItem.CustomerId + "\">" +
								   returnSupplierItem.CustomerName + " " +
								   ((returnSupplierItem.ShopName != "")
										? ("<ТРТ Код=\"" + returnSupplierItem.ShopId + "\">" + returnSupplierItem.ShopName +
										   "</ТРТ>")
										: "<ТРТ Код=\"\"/>") + " </Контрагент>\n");
				xmlFileBody.Append("<ТЗ/>\n");

				foreach (var returnSupplierProductItem in returnSupplierItem.ReturnSupplierProductList)
				{
					xmlFileBody.Append("<Товар КодО=\"" + returnSupplierProductItem.Code + "\" Количество=\"" +
									   returnSupplierProductItem.Count + "\" Цена=\"" + returnSupplierProductItem.Price.ToString().Replace(",", ".") + "\"/>\n");
				}
				xmlFileBody.Append("</Документ>\n");

			}

			xmlFileBody.Append("</Расход>\n");



			// ********************************  Orders  **************************************

			//var orderItems = DataClass.GetOrders(dateTime);

			xmlFileBody.Append((orderItems.Count > 0) ? "<Заявки>" : "<Заявки/>");
			foreach (var orderItem in orderItems)
			{
				if (orderItem.OrderProductList.Count <= 0) continue;
				xmlFileBody.Append("<Заявка Номер=\"" + orderItem.DocNumber + "\" ДатаДоставки = \"" + orderItem.CreateDate + "\">\n");
				xmlFileBody.Append("<Контрагент ЕДРПОУ=\"" + orderItem.EDRPOU + "\" Код=\"" + orderItem.CustomerId + "\">" +
								   orderItem.CustomerName + " " +
								   ((orderItem.ShopName != "")
										? ("<ТРТ Код=\"" + orderItem.ShopId + "\">" + orderItem.ShopName + "</ТРТ>")
										: "<ТРТ Код=\"\"/>") + " </Контрагент>\n");
                //xmlFileBody.Append("<Склад Тип=\"" + orderItem.Store + "\"/>\n");
				xmlFileBody.Append("<ТЗ>" + orderItem.TZ + "</ТЗ>\n");
				foreach (var orderProductItem in orderItem.OrderProductList)
				{
					xmlFileBody.Append("<Товар КодО=\"" + orderProductItem.Code + "\" Количество=\"" + orderProductItem.Count +
									   "\" Цена=\"" + orderProductItem.Price.ToString().Replace(",", ".") + "\"/>\n");
				}
				xmlFileBody.Append("</Заявка>\n");
			}
			if (orderItems.Count > 0)
			{
				xmlFileBody.Append("</Заявки>\n");
			}

			// ********************************  RestVTRT  **************************************

			xmlFileBody.Append((restVTRTItems.Count > 0) ? "<ОстаткиВТРТ>\n" : "<ОстаткиВТРТ/>\n");
			foreach (var restVTRTItem in restVTRTItems)
			{
				//if (restVTRTItem..Count <= 0) continue;
				xmlFileBody.Append("<СнятиеОстатков Номер=\"" + restVTRTItem.DocNumber + "\" >\n");
				xmlFileBody.Append("<Контрагент ЕДРПОУ=\"" + restVTRTItem.EDRPOU + "\" Код=\"" + restVTRTItem.CustomerId + "\">\n" +
								   restVTRTItem.CustomerName + " " +
								   ((restVTRTItem.ShopName != "")
										? ("<ТРТ Код=\"" + restVTRTItem.ShopId + "\">" + restVTRTItem.ShopName + "</ТРТ>\n")
										: "<ТРТ Код=\"\"/>") + " </Контрагент>\n");
				xmlFileBody.Append("<ТЗ>" + restVTRTItem.TZ + "</ТЗ>\n");
				foreach (var restVTRTProductItem in restVTRTItem.RestVTRTProductList)
				{
					xmlFileBody.Append("<Товар КодО=\"" + restVTRTProductItem.Code + "\" Количество=\"" + restVTRTProductItem.Count +
									   "\" Цена=\"\"/>\n");
				}
				xmlFileBody.Append("</СнятиеОстатков>\n");
			}
			if (restVTRTItems.Count > 0)
			{
				xmlFileBody.Append("</ОстаткиВТРТ>\n");
			}

			return xmlFileBody.ToString();
		}

	}


}
