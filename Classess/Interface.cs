using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace XmlObolon_2._0.Classess
{
	public enum ActionState
	{
		Init, CheckForError, GettingRest, Prihod, VozvratPok, Peremeshenie, Prodaga, Spisanie, VozvratPost, Order, SendingMail
	}

	class MainInterface
	{
		private const int Top = 0;
		private const int Left = 1;
		private const int ScreenX = 76;
		private const int ScreenY = 20;
		private int _progressBar = 0;
		private string _progressBarInfo = string.Empty;
		private int _lastProgressBar = 0;
		private bool _hightlight = false;
		private ActionState _haveAction = ActionState.Init;
		private ActionState _lastAction;
		private Thread _mainThread;
		private int _togleHightlight = 50;
		private readonly List<string> _scrollList = new List<string>();
		private bool _executeError = false;
		public string FileCreation { get; set; }
		private string _timeToFinish;
		public bool WebInterface { get; set; }

		public void TimeToFinish(string time)
		{
			this._timeToFinish = time;
		}

		public void ExecuteError(bool error)
		{
			_executeError = error;
		}

		public void AddText(string text)
		{

			


			if (_mainThread != null)
			{
				if (!_mainThread.IsAlive)
					return;
			}
			else
			{
				//Console.Write(Console.OutputEncoding.EncodingName);

				if (WebInterface)
				{
					Console.WriteLine(" * Execute: " + text + "<br>");
				}
				else
				{
					Console.WriteLine(" * Execute: " + text);
				}
			}

			text = text.Length > 40 ? text.Substring(0, 40) : text;
			for (int i = text.Length; i < 40; i++) text += " ";

			if (_scrollList.Count < 14)
			{
				_scrollList.Add(text);
			}
			else
			{
				_scrollList.Remove(_scrollList[0]);
				_scrollList.Add(text);
			}
		}

		public void SetProgressBar(int progress)
		{
			_progressBar = progress;
		}

		public void SetProgressBar(int progress, string info)
		{
			_progressBar = progress;
			_progressBarInfo = info;
		}


		public void SetState(ActionState state)
		{
			_progressBar = 100;
			Thread.Sleep(100);
			_lastAction = _haveAction;
			_haveAction = state;
		}

		public void Start()
		{
			WebInterface = false;
			_executeError = false;
			_progressBar = 0;
			_lastProgressBar = 0;
			_haveAction = ActionState.Init;
			_scrollList.Clear();

			_mainThread = new Thread((Draw)) { IsBackground = true };
			_mainThread.Start();
		}

		public void Stop()
		{
			try
			{
				_mainThread.Abort();
			}
			catch (Exception)
			{
				Console.WriteLine("WARNING. Exception in try stop Draw thread.");
			}
		}


		private void Draw()
		{
			Console.ForegroundColor = ConsoleColor.DarkGray;

			if (_hightlight)
			{
				if (_executeError)
					Console.BackgroundColor = ConsoleColor.Red;
			}


			Console.SetCursorPosition(Left, Top);
			Console.Write("+");
			for (int i = 0; i < (ScreenX) - 1; i++) Console.Write("-");
			Console.Write("+");

			Console.SetCursorPosition(Left, ScreenY);
			Console.Write("+");
			for (int i = 0; i < (ScreenX) - 1; i++) Console.Write("-");
			Console.Write("+");

			Console.SetCursorPosition(Left, ScreenY - 4);
			Console.Write("+");
			for (int i = 0; i < (ScreenX) - 1; i++) Console.Write("-");
			Console.Write("+");


			for (int i = 1; i < ScreenY; i++)
			{
				Console.SetCursorPosition(Left, i); Console.Write("|");
			}

			for (int i = 1; i < ScreenY; i++)
			{
				Console.SetCursorPosition(ScreenX + Left, i); Console.Write("|");
			}

			for (int i = 1; i < ScreenY - 4; i++)
			{
				Console.SetCursorPosition((ScreenX + Left) / 2 - 5, i); Console.Write("|");
			}

			Console.ResetColor();
			Console.ForegroundColor = ConsoleColor.DarkGray;

			// if ProgressBar were some changes
			if (_lastProgressBar != _progressBar)
			{
				_lastProgressBar = _progressBar;
				int presentPosition = (ScreenX + Left) / 2 - 2;
				string presentText = string.Concat(new object[] { "Всего ", _progressBar, "% Осталось: ", _timeToFinish });
				presentPosition = 0x26 - (presentText.Length / 2);

				Console.SetCursorPosition(Left + 2, ScreenY - 2);
				int cur = _progressBar * (ScreenX - 3) / 100;
				for (int i = 0; i < (ScreenX) - 3; i++)
				{
					string writeBlock = " ";
					if (presentPosition <= i && presentPosition + presentText.Length > i)
					{
						writeBlock = presentText.Substring(i - presentPosition, 1);
					}

					if (i <= cur)
					{
						Console.ForegroundColor = ConsoleColor.Black;
						Console.BackgroundColor = ConsoleColor.Gray;
						Console.Write(writeBlock);
						Console.ResetColor();
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Black;
						Console.BackgroundColor = ConsoleColor.DarkGray;
						Console.Write(writeBlock);
						Console.ResetColor();
					}
				}
			}

			Console.ResetColor();
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.SetCursorPosition(Left + 3, Top + 2);
			Console.Write("Обрабатываются: "+_progressBarInfo);


			switch (_haveAction)
			{
				case ActionState.Init:

					Console.SetCursorPosition(Left + 4, Top + 4);
					if (!_hightlight)
					{
						Console.ForegroundColor = ConsoleColor.DarkGreen;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					Console.Write(" Подготовка");
					Console.ResetColor();
					break;

				case ActionState.CheckForError:

					Console.SetCursorPosition(Left + 4, Top + 5);
					if (!_hightlight)
					{
						Console.ForegroundColor = ConsoleColor.DarkGreen;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					Console.Write(" Проверка ошибок");
					Console.ResetColor();
					break;


				case ActionState.GettingRest:

					Console.SetCursorPosition(Left + 4, Top + 6);
					if (!_hightlight)
					{
						Console.ForegroundColor = ConsoleColor.DarkGreen;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					Console.Write(" Остатки");
					Console.ResetColor();
					break;


				case ActionState.Prihod:

					Console.SetCursorPosition(Left + 4, Top + 7);
					if (!_hightlight)
					{
						Console.ForegroundColor = ConsoleColor.DarkGreen;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					Console.Write(" Приходы");
					Console.ResetColor();
					break;

				case ActionState.VozvratPok:

					Console.SetCursorPosition(Left + 4, Top + 8);
					if (!_hightlight)
					{
						Console.ForegroundColor = ConsoleColor.DarkGreen;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					Console.Write(" Возвраты от покупателя");
					Console.ResetColor();
					break;

				case ActionState.Peremeshenie:

					Console.SetCursorPosition(Left + 4, Top + 9);
					if (!_hightlight)
					{
						Console.ForegroundColor = ConsoleColor.DarkGreen;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					Console.Write(" Перемещение");
					Console.ResetColor();
					break;


				case ActionState.Prodaga:

					Console.SetCursorPosition(Left + 4, Top + 10);
					if (!_hightlight)
					{
						Console.ForegroundColor = ConsoleColor.DarkGreen;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					Console.Write(" Продажи");
					Console.ResetColor();
					break;


				case ActionState.Spisanie:

					Console.SetCursorPosition(Left + 4, Top + 11);
					if (!_hightlight)
					{
						Console.ForegroundColor = ConsoleColor.DarkGreen;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					Console.Write(" Списания");
					Console.ResetColor();
					break;


				case ActionState.VozvratPost:

					Console.SetCursorPosition(Left + 4, Top + 12);
					if (!_hightlight)
					{
						Console.ForegroundColor = ConsoleColor.DarkGreen;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					Console.Write(" Возвраты поставщику");
					Console.ResetColor();
					break;

				case ActionState.SendingMail:

					Console.SetCursorPosition(Left + 4, Top + 13);
					if (!_hightlight)
					{
						Console.ForegroundColor = ConsoleColor.DarkGreen;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Write("*");
						Console.ForegroundColor = ConsoleColor.Gray;
					}
					Console.Write(" Отправка писем");
					Console.ResetColor();
					break;


			}



			switch (_lastAction)
			{
				case ActionState.Init:

					Console.SetCursorPosition(Left + 4, Top + 4);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("*");
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.Write(" Подготовка");
					Console.ResetColor();
					break;


				case ActionState.CheckForError:

					Console.SetCursorPosition(Left + 4, Top + 5);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("*");
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.Write(" Проверка ошибок");
					Console.ResetColor();
					break;

				case ActionState.GettingRest:

					Console.SetCursorPosition(Left + 4, Top + 6);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("*");
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.Write(" Остатки");
					Console.ResetColor();
					break;


				case ActionState.Prihod:

					Console.SetCursorPosition(Left + 4, Top + 7);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("*");
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.Write(" Приходы");
					Console.ResetColor();
					break;

				case ActionState.VozvratPok:

					Console.SetCursorPosition(Left + 4, Top + 8);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("*");
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.Write(" Возвраты от покупателя");
					Console.ResetColor();
					break;

				case ActionState.Peremeshenie:

					Console.SetCursorPosition(Left + 4, Top + 9);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("*");
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.Write(" Перемещение");
					Console.ResetColor();
					break;


				case ActionState.Prodaga:

					Console.SetCursorPosition(Left + 4, Top + 10);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("*");
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.Write(" Продажи");
					Console.ResetColor();
					break;


				case ActionState.Spisanie:

					Console.SetCursorPosition(Left + 4, Top + 11);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("*");
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.Write(" Списания");
					Console.ResetColor();
					break;


				case ActionState.VozvratPost:

					Console.SetCursorPosition(Left + 4, Top + 12);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("*");
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.Write(" Возвраты поставщику");
					Console.ResetColor();
					break;

				case ActionState.SendingMail:

					Console.SetCursorPosition(Left + 4, Top + 13);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("*");
					Console.ForegroundColor = ConsoleColor.Gray;
					Console.Write(" Отправка писем");
					Console.ResetColor();
					break;


			}



			if (_haveAction == ActionState.Init)
			{
				Console.SetCursorPosition(Left + 4, Top + 5);
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.Write("*");
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write(" Проверка ошибок");

				Console.SetCursorPosition(Left + 4, Top + 6);
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.Write("*");
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write(" Остатки");

				Console.SetCursorPosition(Left + 4, Top + 7);
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.Write("*");
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write(" Приходы");

				Console.SetCursorPosition(Left + 4, Top + 8);
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.Write("*");
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write(" Возвраты от покупателя ");

				Console.SetCursorPosition(Left + 4, Top + 9);
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.Write("*");
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write(" Перемещение ");

				Console.SetCursorPosition(Left + 4, Top + 10);
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.Write("*");
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write(" Продажы ");

				Console.SetCursorPosition(Left + 4, Top + 11);
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.Write("*");
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write(" Списания ");

				Console.SetCursorPosition(Left + 4, Top + 12);
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.Write("*");
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write(" Возвраты поставщику ");

				Console.SetCursorPosition(Left + 4, Top + 13);
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.Write("*");
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write(" Отправка писем ");
			}

			Console.ForegroundColor = ConsoleColor.Gray;
			Console.SetCursorPosition((ScreenX + Left) / 2 - 3, Top + 2);
			Console.Write(FileCreation);

			for (int i = 0; i < _scrollList.Count; i++)
			{
				Console.SetCursorPosition((ScreenX + Left) / 2 - 3, (Top + 1) + i);
				Console.Write(_scrollList[i]);
			}


			Console.ResetColor();

			if (_togleHightlight < 5) _togleHightlight++;
			{
				_togleHightlight = 0;
				_hightlight = !_hightlight;
			}
			Thread.Sleep(100);
			Draw();
		}


		//#######################################################################################################
		//#######################################################################################################
		//#######################################################################################################
		//## Select Date Form


		public List<DateTime> SelectDateFunc()
		{
			Console.Clear();
			var selDate = new List<DateTime>();
			const int maxConsRow = 10;
			bool sendMail = true;
			bool cursorDeep = false;
			int countDeep = 0;

			var visibleList = new List<DateTime>();
			DateTime sDateTime = DateTime.Now;
			sDateTime = sDateTime.AddDays(1);
			int cursor = maxConsRow - 1;
			for (int i = 0; i < maxConsRow; i++)
			{
				visibleList.Add(sDateTime.AddDays(-maxConsRow + i));
				//sDateTime = ;
			}

			while (true)
			{

				// -------------- Rendering -----------------------

				Console.SetCursorPosition(2, 2);
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("XML_OBOLON 2.0 (" + Configuration.ZoneNum + ")");
				Console.ResetColor();

				//Console.SetCursorPosition(2, 5);
				//Console.Write("Select Date count: " + selDate.Count + "  ");
				Console.SetCursorPosition(2, 6);
				Console.Write("Send Mail: " + (sendMail ? "Да" : "Нет") + "  ");

				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.SetCursorPosition(2, 8);
				Console.Write("* Согласиться `A` или ");
				Console.SetCursorPosition(2, 9);
				Console.Write("                `Enter` ");

				Console.SetCursorPosition(2, 11);
				Console.Write("* Выйти  `Esc`");

				Console.SetCursorPosition(2, 13);
				Console.Write("* Отправлять Меил `M`");

				Console.SetCursorPosition(2, 15);
				Console.Write("* Помощь `H`");

				Console.SetCursorPosition(2, 17);
				Console.Write("* Переотправить файл `R`");

				Console.ResetColor();

				Console.SetCursorPosition(2, 20);
				Console.Write("Copyright © by GreshnikSV ");
				Console.SetCursorPosition(2, 21);
				Console.Write("version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + " ");

				Console.SetCursorPosition(30, 1);
				Console.Write("+-------- Выберите начальную дату ------+");
				Console.SetCursorPosition(30, 2);
				Console.Write("|                                       |");
				for (int i = 0; i < visibleList.Count; i++)
				{
					Console.SetCursorPosition(30, i + 3);
					string day = string.Empty;
					switch (visibleList[i].DayOfWeek)
					{
						case DayOfWeek.Sunday:
							day = "Воскресенье"; break;

						case DayOfWeek.Monday:
							day = "Понедельник"; break;

						case DayOfWeek.Tuesday:
							day = "Вторник"; break;

						case DayOfWeek.Friday:
							day = "Пятница"; break;

						case DayOfWeek.Saturday:
							day = "Субота"; break;

						case DayOfWeek.Wednesday:
							day = "Среда"; break;

						case DayOfWeek.Thursday:
							day = "Четверг"; break;
					}

					bool check = selDate.Any(dateTime => visibleList[i] == dateTime);

					const string wr = "|    ";
					string wr2 = "  " + (check ? "[*]" : "[ ]") + " " + visibleList[i].ToString("dd-MM-yyyy") + " - " + day + "  ";
					Console.Write(wr);
					if (!cursorDeep) if (cursor == i) Console.BackgroundColor = ConsoleColor.DarkGray;
					Console.Write(wr2);
					Console.ResetColor();
					for (int j = wr.Length + wr2.Length; j < 40; j++) { Console.Write(" "); }
					Console.Write("|");
				}
				Console.SetCursorPosition(30, maxConsRow + 3);
				Console.Write("|                                       |");
				Console.SetCursorPosition(30, maxConsRow + 4);
				Console.Write("+---------------------------------------+");
				Console.SetCursorPosition(0, 22);


				// Draw DEEP section
				Console.SetCursorPosition(30, maxConsRow + 6);
				Console.Write("+--------------- Глубь: ----------------+");
				Console.SetCursorPosition(30, maxConsRow + 7);
				Console.Write("|                                       |");

				Console.SetCursorPosition(30, maxConsRow + 8);
				Console.Write("| ");
				if (cursorDeep) Console.BackgroundColor = ConsoleColor.DarkGray;
				DateTime finalDate = DateTime.Now;
				
				if (countDeep>0)
					if (selDate.Count>0)
						finalDate = selDate[0].AddDays((-1) * countDeep);

				string shift = string.Empty;
				for(int i=countDeep.ToString().Length;i<3;i++) shift += " ";
				Console.Write("< " + countDeep + shift + "дней > " + (selDate.Count>0?selDate[0].ToString("dd.MM.yyyy"):DateTime.Now.ToString("dd.MM.yyyy")) + " - " + finalDate.ToString("dd.MM.yyyy"));//20
				//Console.Write(" ");//allLine=37
				Console.ResetColor();
				Console.Write("   |");

				Console.SetCursorPosition(30, maxConsRow + 9);
				Console.Write("|                                       |");
				Console.SetCursorPosition(30, maxConsRow + 10);
				Console.Write("+---------------------------------------+");

				// -------------- Reaction on send kay -----------------------

				ConsoleKeyInfo retKey = Console.ReadKey();

				if (retKey.Key == ConsoleKey.LeftArrow)
				{
					if(cursorDeep)
						if (countDeep>0)
							countDeep--;
				}

				if (retKey.Key == ConsoleKey.RightArrow)
				{
					if (cursorDeep)
						if (countDeep < 40)
							countDeep++;
				}

				if (retKey.Key == ConsoleKey.UpArrow)
				{
					if (cursor > 0) cursor--;
					else
					{
						visibleList.Remove(visibleList[visibleList.Count - 1]);
						visibleList.Insert(0, visibleList[0].AddDays(-1));
					}
				}


				if (retKey.Key == ConsoleKey.DownArrow)
				{
					if (maxConsRow - 1 > cursor) cursor++;
					else
					{
						visibleList.Remove(visibleList[0]);
						visibleList.Add(visibleList[visibleList.Count - 1].AddDays(1));
					}
				}

				if (retKey.Key == ConsoleKey.Spacebar)
				{
					bool exist = false;
					foreach (var dateTime in selDate)
					{
						if (dateTime != visibleList[cursor]) continue;
						exist = true;
					}

					if (!exist) { selDate.Clear(); selDate.Add(visibleList[cursor]); countDeep = 1;
					}
					else { selDate.Remove(visibleList[cursor]); countDeep = 0;
					}
				}

				if (retKey.Key == ConsoleKey.M)
				{
					if (!sendMail)
					{
						Configuration.SendMessage = true;
						sendMail = true;
					}
					else
					{
						Configuration.SendMessage = false;
						sendMail = false;
					}
				}

				if (retKey.Key == ConsoleKey.R)
				{
					// start mail resend form
					MailResendForm();
					//break;
				}

				if (retKey.Key == ConsoleKey.H)
				{
					HelpForm();
				}
				

				if (retKey.Key == ConsoleKey.Tab)
				{
					cursorDeep = !cursorDeep;
				}

				if (retKey.Key == ConsoleKey.A || retKey.Key == ConsoleKey.Enter)
				{
					if (selDate.Count < 1) return null;
					Console.Write("\n\n");
					var startDt = selDate[0];
					selDate.Clear();
					for (int i = 0; i < countDeep; i++)
					{ selDate.Add(startDt.AddDays(-i)); }

					return selDate;
				}
				if (retKey.Key == ConsoleKey.Escape) { Console.Write("\n\n"); break; }
			}

			return null;
		}

		//#######################################################################################################
		//#######################################################################################################
		//#######################################################################################################
		//## mail resend form

		class FileObject
		{
			public string Name { get; set; }
			public string Path { get; set; }
			public bool Selected { get; set; }
			public bool Fail { get; set; }
			public int Number { get; set; } 
		}

		private void SortFileObjectList(ref List<FileObject> fileObjectList)
		{
			for (int i = 0; i < fileObjectList.Count; i++)
			{
				for (int j = 0; j < fileObjectList.Count; j++)
				{
					if(fileObjectList[j].Number<fileObjectList[i].Number)
					{
						FileObject buf = fileObjectList[i];
						fileObjectList[i] = fileObjectList[j];
						fileObjectList[j] = buf;
					}
				}
			}
		}


		public void MailResendForm()
		{
			Console.Clear();
			var fileList = Directory.GetFiles(Configuration.ReportingDir, "*zip", SearchOption.AllDirectories);

			var fileObjectList = new List<FileObject>();

			for (int i = 0; i < fileList.Length; i++)
			{
				fileObjectList.Add(new FileObject
				{
					Name = Path.GetFileName(fileList[i]),
					Path = fileList[i],
					Selected = false,
					Fail = (fileList[i].ToLower().IndexOf("fail") != -1 ? true : false),
					Number = Int32.Parse(fileList[i].IndexOf("_n")==-1?"0":fileList[i].Substring(fileList[i].IndexOf("_n") + 2, fileList[i].IndexOf(".xml") - (fileList[i].IndexOf("_n") + 2)))
				}); 
			}

			SortFileObjectList(ref fileObjectList);

			// rendering vars
			int cursorPosition = 0;
			int listPosition = 0;
			const int visibleItemListCount = 20;
			const int visibleItemListLegth = 45;
			const int visibleItemListX = 33;
			const int visibleItemListY = 1;


			// rendering
			Console.SetCursorPosition(1, 2);
			Console.Write(" * Чтобы выйти `ESC`");

			Console.SetCursorPosition(1, 4);
			Console.Write(" * Чтобы выбрать `SpaceBar`");

			Console.SetCursorPosition(1, 6);
			Console.Write(" * Чтобы отправить `ENTER`");


			Console.SetCursorPosition(visibleItemListX, visibleItemListY);
			for (int i = 0; i < visibleItemListLegth; i++)
			{
				Console.SetCursorPosition(visibleItemListX + i, visibleItemListY);
				Console.Write("-");
			}

			Console.SetCursorPosition(visibleItemListX, visibleItemListY + visibleItemListCount);
			for (int i = 0; i < visibleItemListLegth; i++)
			{
				Console.SetCursorPosition(visibleItemListX + i, visibleItemListY + visibleItemListCount);
				Console.Write("-");
			}

			Console.SetCursorPosition(visibleItemListX, visibleItemListY);
			for (int i = 0; i < visibleItemListCount; i++)
			{
				Console.SetCursorPosition(visibleItemListX, visibleItemListY + i);
				Console.Write("|");
			}

			Console.SetCursorPosition(visibleItemListX + visibleItemListLegth, visibleItemListY);
			for (int i = 0; i < visibleItemListCount; i++)
			{
				Console.SetCursorPosition(visibleItemListX + visibleItemListLegth, visibleItemListY + i);
				Console.Write("|");
			}

			Console.SetCursorPosition(visibleItemListX + visibleItemListLegth, visibleItemListY); Console.Write("+");
			Console.SetCursorPosition(visibleItemListX, visibleItemListY); Console.Write("+");
			Console.SetCursorPosition(visibleItemListX + visibleItemListLegth, visibleItemListY + visibleItemListCount); Console.Write("+");
			Console.SetCursorPosition(visibleItemListX, visibleItemListY + visibleItemListCount); Console.Write("+");
			
			while (true)
			{
				// draw file list
				int blockItemNumber = 0;
				for (int i = listPosition; i < visibleItemListCount + listPosition - 1; i++)
				{
					Console.SetCursorPosition(visibleItemListX + 1, visibleItemListY + 1 + blockItemNumber);
					if (fileObjectList.Count<=i) continue;

					Console.Write(" ");
					if (cursorPosition == blockItemNumber) Console.BackgroundColor = ConsoleColor.DarkGray;

					if (fileObjectList[i].Fail)
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.Write(" " + (fileObjectList[i].Selected ? "[*]" : "[ ]") + " " + fileObjectList[i].Name + "");
						
					}
					else
					{
						Console.Write(" " + (fileObjectList[i].Selected ? "[*]" : "[ ]") + " " + fileObjectList[i].Name + " ");
						Console.Write("         ");
					}
					Console.ResetColor();
					
					blockItemNumber++;
				}



				// get key
				ConsoleKeyInfo keyInfo = Console.ReadKey();

				switch (keyInfo.Key)
				{
					case ConsoleKey.Enter:
						
						break;

					case ConsoleKey.Spacebar:
						
						fileObjectList.Any(file => file.Selected = false);
						fileObjectList[listPosition + cursorPosition].Selected = 
							!fileObjectList[listPosition + cursorPosition].Selected;
						break;

					case ConsoleKey.DownArrow:
						if (cursorPosition < visibleItemListCount-2)
						{
							cursorPosition++;
						}
						else
						{
							if (listPosition < fileObjectList.Count-visibleItemListCount)
							listPosition++;
						}
						break;

					case ConsoleKey.UpArrow:
						if (cursorPosition > 0)
						{
							cursorPosition--;
						}
						else
						{
							if (listPosition > 0)
								listPosition--;
						}
						break; 
				}

				if(keyInfo.Key==ConsoleKey.Enter)
				{
					string xmlFile = string.Empty;

					foreach (var fileObject in fileObjectList)
					{
						if (fileObject.Selected) xmlFile = fileObject.Path;
					}

					Services.SendMail(Configuration.MailTo, Configuration.MailToError, Configuration.MailFrom, xmlFile, Path.GetFileName(xmlFile), "XML Report");
					Thread.Sleep(500);
					string mailHead = "Переотправка XML ( " + Path.GetFileName(xmlFile) + " ) в OBOLON "+
					DateTime.Now.ToString("dd.MM.yyyy")+ " ( ДатаСтарта: "+ " ????? Глубь: ????? )";
					Services.SendMail(Configuration.MailToError, Configuration.MailToError, Configuration.MailFrom, null, mailHead, "XML Report");
					Thread.Sleep(500);
					Console.Clear();
					break;
				}

				if (keyInfo.Key == ConsoleKey.Escape) { Console.Clear(); break; }

			}


		}


		//#######################################################################################################
		//#######################################################################################################
		//#######################################################################################################
		//## mail resend form

		public void HelpForm()
		{
			Console.Clear();
			var drawThread = new Thread(HelpDraw);
			drawThread.Start();

			while (true)
			{

				var key = Console.ReadKey();

				if (key.Key == ConsoleKey.Escape || key.Key == ConsoleKey.Q)
				{
					drawThread.Abort();
					break;
				}
			}
			Console.Clear();
		}

		public void HelpDraw()
		{
			Console.Clear();
			const string head = " ! ! ! Привет пользователь ! ! ! ";
			var rand = new Random();
			int headBigSymbol = 0;
			int headDrawPosition = 0;
			ConsoleColor textColor = ConsoleColor.DarkGray;

			Console.ForegroundColor = ConsoleColor.DarkGray;
			for (int i = 3; i < 77; i++)
			{
				Console.SetCursorPosition(i,4);
				Console.Write("-");
			}

			for (int i = 3; i < 77; i++)
			{
				Console.SetCursorPosition(i, 22);
				Console.Write("-");
			}
			Console.ResetColor();

			//Console.ForegroundColor = ConsoleColor.White;
			Console.SetCursorPosition(4, 6);
			Console.Write("Для того чтобы выбрать начальную дату, необходимо стрелочками вверх и вниз");
			Console.SetCursorPosition(4, 7);
			Console.Write("нанести выдления на нужную дату и нажать ПРОБЕЛ. Для того чтобы выбрать ");
			Console.SetCursorPosition(4, 8);
			Console.Write(" глубину необходимо нажать ТАБ и тогда выделния перенесутся ниже, в раздел ");
			Console.SetCursorPosition(4, 9);
			Console.Write(" глубь и далле кнопочками в лево в право выбрать необходимую глубину. ");
			//Console.SetCursorPosition(4, 10);
			//Console.Write("");

			
			Console.SetCursorPosition(4, 11);
			Console.ForegroundColor = ConsoleColor.DarkGray; 
			Console.Write("Параметры командной строки:"); 
			Console.ResetColor();
			Console.SetCursorPosition(4, 12);
			Console.Write(" -s = (int) Задается дата старта \" 0 - сегодня, 1 - завтра, -1 - вчера \" ");
			Console.SetCursorPosition(4, 13);
			Console.Write(" -d = (int) Задается глубина");
			Console.SetCursorPosition(4, 14);
			Console.Write(" -n = если есть этот параметр то письмо с файлом не отсылается. ");
			Console.SetCursorPosition(4, 15);
			Console.Write(" -chk = запуск функции проверки движения товара. ");
			Console.SetCursorPosition(4, 16);
			Console.Write(" -excs \"fileName\" = создание списка клиентов и магазинов с номерами. ");
			Console.SetCursorPosition(4, 17);
			Console.Write(" -xmlt = терминал для работы с файлами. ");
			


			Console.SetCursorPosition(4, 18);
			Console.ForegroundColor = ConsoleColor.DarkGray; 
			Console.Write(" Примечание: ");
			Console.ResetColor();
			Console.SetCursorPosition(4, 19);
			Console.Write("- 15го числа если запускать программу из коммандной строки утановится ");
			Console.SetCursorPosition(4, 20);
			Console.Write("глубина в 15 дней, а если будет 30 чисто то утановится 30 соостветственно.");

			Console.ResetColor();


			while (true)
			{
				Console.ForegroundColor = textColor;
				Console.SetCursorPosition(24 + headDrawPosition, 2);
				headDrawPosition++;
				if (headDrawPosition >= head.Length)
				{
					headDrawPosition = 0; headBigSymbol++;
					switch (rand.Next(1, 10))
					{
						case 1: textColor = ConsoleColor.Black; break;
						case 2: textColor = ConsoleColor.Yellow; break;
						case 3: textColor = ConsoleColor.White; break;
						case 4: textColor = ConsoleColor.Red; break;
						case 5: textColor = ConsoleColor.DarkGray; break;
						case 6: textColor = ConsoleColor.DarkCyan; break;
						case 7: textColor = ConsoleColor.DarkMagenta; break;
						case 8: textColor = ConsoleColor.DarkRed; break;
						case 9: textColor = ConsoleColor.Gray; break;
						case 10: textColor = ConsoleColor.Green; break;
					}
				}
				if (headBigSymbol >= head.Length) headBigSymbol = 0;

				if (headBigSymbol == headDrawPosition)
					Console.Write(head[headDrawPosition].ToString().ToUpper());
				else
					Console.Write(head[headDrawPosition]);
				Console.ResetColor();

				Thread.Sleep(50);
			}
		}


	}





}
