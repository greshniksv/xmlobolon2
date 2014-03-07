using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace XmlObolon_2._0.Classess
{
	class XmlFile
	{
		public string FileName { get; set; }
		public int Number { get; set; }
	}


	public class XmlAnalize
	{
		private List<XmlFile> _xmlFileList;

		private string GetParamStr(string cmdData,string param)
		{

			if (cmdData.IndexOf(param) != -1)
			{
				var cmdDataList = cmdData.Split(' ');
				for (int i = 0; i < cmdDataList.Length; i++)
				{
					try
					{
						if (cmdDataList[i] == param) return cmdDataList[i + 1];
					}
					catch (Exception)
					{
						Console.WriteLine("incorrect parameter [" + param + "] ");
						return "ERROR";
					}
				}
			}

			return "";
		}

		private bool GetParamExist(string cmdData, string param)
		{

			if (cmdData.IndexOf(param) != -1)
			{
				var cmdDataList = cmdData.Split(' ');
				for (int i = 0; i < cmdDataList.Length; i++)
				{
					try
					{
						if (cmdDataList[i] == param) return true;
					}
					catch (Exception)
					{
						Console.WriteLine("incorrect parameter [" + param + "] ");
						return false;
					}
				}
			}

			return false;
		}


		private int GetParamInt(string cmdData, string param)
		{

			if (cmdData.IndexOf(param) != -1)
			{
				var cmdDataList = cmdData.Split(' ');
				for (int i = 0; i < cmdDataList.Length; i++)
				{
					try
					{
						if (cmdDataList[i] == param) return Int32.Parse(cmdDataList[i + 1]);
					}
					catch (Exception)
					{
						Console.WriteLine("incorrect parameter [" + param + "]");
						return -1;
					}
				}
			}
			return -1;
		}	

		public void StartTerminal()
		{
			_xmlFileList = new List<XmlFile>();
			Console.Write("XML Termital: starting\n");
			var xmlFileList = Directory.GetFiles(Configuration.ReportingDir, "*.xml", SearchOption.AllDirectories);

			foreach (var xmlFileItem in xmlFileList)
			{
				var number = Int32.Parse(Path.GetFileNameWithoutExtension(xmlFileItem).Split('_')[3].Replace('n', '0'));
				_xmlFileList.Add(new XmlFile { FileName = xmlFileItem, Number = number });
			}

			for (int i = 0; i < _xmlFileList.Count; i++)
			{
				for (int j = 0; j < _xmlFileList.Count; j++)
				{
					if(_xmlFileList[i].Number>_xmlFileList[j].Number)
					{
						var temp = new XmlFile {FileName = _xmlFileList[i].FileName, Number = _xmlFileList[i].Number};
						_xmlFileList[i] = _xmlFileList[j];
						_xmlFileList[j] = temp;
					}
				}
			}
			

			Console.Write("XML Termital: found " + _xmlFileList.Count + " files \n");

			Console.Write("XML Termital: activated\n");
			while (true)
			{
				Console.Write("\nxml>");
				var rez = Command(Console.ReadLine().ToLower());
				if (rez == "quit") break;
				Console.Write(rez);
			}
		}


		private string Command(string inCmd)
		{
			inCmd = inCmd.Trim();
			if (inCmd == "quit") return inCmd;

			var cmd = inCmd.Split(' ')[0];
			var cmdData = inCmd.Substring(cmd.Length, inCmd.Length - cmd.Length).Trim();
			
			switch (cmd)
			{

				// ################################################
				// FindDocNum
				// 

				case "find":

					int useFirstFile = 0;
					int fileNumber = 0;
					string doctype = "Документ";
					string docNum = "";
					string customer = "";
					string customerNum = "";
					string trt = "";
					string tz = "";

					if (GetParamExist(cmdData, "doc"))
					{
						doctype = "Документ";
					}

					if (GetParamExist(cmdData, "rest"))
					{
						doctype = "СнятиеОстатков";
					}

					if (GetParamExist(cmdData, "ord"))
					{
						doctype = "Заявка";
					}

					tz = GetParamStr(cmdData, "tz");
					customer = GetParamStr(cmdData, "customer");
					customerNum = GetParamStr(cmdData, "customernum");
					trt = GetParamStr(cmdData, "trt");
					docNum = GetParamStr(cmdData, "num");
					useFirstFile = GetParamInt(cmdData, "first");
					fileNumber = GetParamInt(cmdData, "filenum");


					cmdData = cmdData.Split(' ')[0];

					int anal = 0;
					foreach (var xmlFileItem in _xmlFileList)
					{
						if (useFirstFile > 0 && anal >= useFirstFile) break;
						if (fileNumber >0 && xmlFileItem.Number != fileNumber) continue;

						anal++;
						Console.SetCursorPosition(0,Console.CursorTop);
						Console.Write("                                 ");
						Console.SetCursorPosition(0, Console.CursorTop);
						Console.Write("File analize [" + xmlFileItem.Number + "] " + anal + "/" + (useFirstFile > 0 ? useFirstFile : _xmlFileList.Count));


						var doc = new XmlDocument();
						TextReader textReader = new StreamReader(xmlFileItem.FileName, Encoding.GetEncoding(1251));
						
						var xml = textReader.ReadToEnd();
						xml = xml.Replace("<!DOCTYPE ФайлЭкспортаДанных SYSTEM \"export.dtd\">", "");

						doc.LoadXml(xml);


						XmlNodeList items = doc.GetElementsByTagName(doctype);
						foreach (XmlNode x in items)
						{
							bool existTz = false;
							bool existCustomerNum = false;
							bool existCustomer = false;
							bool existTrt = false;
							bool existDocNum = false;
							string showMask = "";
							//Console.WriteLine("Item {0} = {1}", x.Attributes[0].Name, x.Attributes[0].Value);	


							//###########################################
							// DOC NUM
							
							if (docNum.Length > 0)
							{
								if (x.Attributes != null && x.Attributes[0].Value.IndexOf(docNum) != -1)
								{
									existDocNum = true;
								}
								if (existDocNum) showMask += "1"; else showMask += "0";
							}


							//###########################################
							// CUSTOMERS
							
							if (customer.Length > 1)
							{
								foreach (XmlNode xc in x.ChildNodes)
								{
									if (xc.Name == "Контрагент" 
										&& xc.FirstChild!=null
										&& xc.FirstChild.Value.ToLower().IndexOf(customer.ToLower())!=-1)
									{
										existCustomer = true;
									}
								}
								if (existCustomer) showMask += "1"; else showMask += "0";
							}

							//###########################################
							// CUSTOMERS NUM

							if (customerNum.Length > 1)
							{
								existCustomerNum = x.ChildNodes.Cast<XmlNode>().Where(xc => xc.Name == "Контрагент").Any(xc => xc.Attributes[1].Value == customerNum);
								if (existCustomerNum) showMask += "1"; else showMask += "0";
							}

							//###########################################
							// TRT

							if (trt.Length > 1)
							{
								foreach (XmlNode xc in x.ChildNodes)
								{
									if (xc.Name == "Контрагент")
									{
										existTrt = xc.ChildNodes.Cast<XmlNode>().Any(xcc => xcc.Name == "ТРТ" && xcc.Attributes[0].Value == trt);
									}
								}
								if (existTrt) showMask += "1"; else showMask += "0";
							}


							//###########################################
							// TZ
							if (tz.Length>1)
							{
								foreach (XmlNode xc in x.ChildNodes)
								{
									if (xc.Name == "ТЗ"
										&& xc.FirstChild != null
										&& xc.FirstChild.Value==tz)
									{
										existTz = true;
									}
								}
								if (existTz) showMask += "1"; else showMask += "0";
							}


							if (showMask.IndexOf("0")==-1)
							{
								Console.SetCursorPosition(0, Console.CursorTop);
								Console.Write("                                                   ");
								Console.SetCursorPosition(0, Console.CursorTop);

								Console.ForegroundColor = ConsoleColor.Green;
								Console.Write(" - File: " + Path.GetFileName(xmlFileItem.FileName)+"\n\n");
								Console.ResetColor();

								
								Console.Write(doctype + " ");
								foreach (XmlNode attribute in x.Attributes)
								{
								    Console.Write(attribute.Name + ": " + attribute.Value + " ");
								}

								Console.Write(x.InnerXml.Replace("<", "\n<").Replace("\n</", "</") + "\n\n\n");


								//foreach (XmlNode xc in x.ChildNodes)
								//{
								//    Console.Write("\n{0} : {1}", xc.Name, xc.InnerText);

								//    if (xc.Name == "Контрагент")
								//    {
								//        Console.Write("\n");
								//        foreach (XmlNode attribute in xc.Attributes)
								//        {
								//            Console.Write(attribute.Name + ": " + attribute.Value + " ");
								//        }

								//        Console.Write("\nТРТ " + xc.LastChild.Attributes[0].Name + ":" + xc.LastChild.Attributes[0].Value + 
								//            " Адресс: " + xc.LastChild.InnerText);
								//    }
								//    else
								//    {
								//        if (xc.Attributes.Count > 0)
								//        {
								//            Console.Write("[");
								//            foreach (XmlNode attribute in xc.Attributes)
								//            {
								//                Console.Write("\t"+attribute.Name + ": " + attribute.Value + " ");
								//            }
								//            Console.Write("]");
								//        }
								//    }

								//}
								//Console.Write("\n\n\n");
							}
							
						}
						

					}
					break;

				case "help":
						
					Console.Write("\n  FIND: \n   [doc|rest|ord] - тип документа \n"+
						"   [fileNumber] - номер xml файла "+
						"\n   [num] - номер документа " +
						"\n   [tz] - использовать торговую зону 'tz 6421100' " +
						"\n   [customer] - использовать клинета 'customer луганск' "+
						"\n   [customernum] - номер клиента "+
						"\n   [trt] - номер торговой точки "+
						"\n   [first] - указать сколько файлов сканировать " +
						"\n\n  QUIT - выйти\n ");

				break;

				default: return "Command not found";
			}


			return "";
		}



		

	}
}
