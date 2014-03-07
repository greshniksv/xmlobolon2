using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using fnLog2;
using ICSharpCode.SharpZipLib.Zip;

namespace XmlObolon_2._0.Classess
{
	static class Services
	{
		//public static void SendMail(string mailTo, string mailFrom, string attach, string head, string body)
		//{
		//    string[] masMail = mailTo.Split(';');

		//    foreach (var mailTo2 in masMail)
		//    {
		//        if (mailTo2.Length<3) continue;

		//        GLOBAL.SysLog.Write(@"Send mail to: " + mailTo2 + " from " + mailFrom, MessageType.Information);

		//        //Адрес SMTP-сервера
		//        String smtpHost = Configuration.SmtpHost;
		//        //Порт SMTP-сервера
		//        int smtpPort = Configuration.SmtpPort;
		//        //Логин
		//        String smtpUserName = Configuration.SmtpUser;
		//        //Пароль
		//        String smtpUserPass = Configuration.SmtpPassword;
		//        //Создание подключения
		//        var client = new SmtpClient(smtpHost, smtpPort);
		//        if (Configuration.SmtpSsl)
		//        {
		//            client.EnableSsl = true;
		//        }
		//        client.Credentials = new NetworkCredential(smtpUserName, smtpUserPass);

		//        //Адрес для поля "От"
		//        String msgFrom = mailFrom;
		//        //Адрес для поля "Кому" (адрес получателя)
		//        String msgTo = mailTo2;
		//        //Тема письма
		//        String msgSubject = head;
		//        //Текст письма
		//        string msgBody = body;
		//        //Вложение для письма
		//        //Если нужно больше вложений, для каждого вложения создаем свой объект Attachment с нужным путем к файлу
				
		//        //Создание сообщения
		//        var message = new MailMessage(msgFrom, msgTo, msgSubject, msgBody) { IsBodyHtml = true, ReplyTo = new MailAddress(msgFrom) };
		//        //Крепим к сообщению подготовленное заранее вложение
		//        if (attach != null)
		//        {
		//            var attachData = new Attachment(attach);
		//            message.Attachments.Add(attachData);
		//        }

		//        try
		//        {
		//            //Отсылаем сообщение
		//            client.Send(message);
		//            GLOBAL.SysLog.Write("Sending mail success", MessageType.Information);
		//        }
		//        catch (SmtpException ex)
		//        {
		//            //В случае ошибки при отсылке сообщения можем увидеть, в чем проблема
		//            GLOBAL.SysLog.Write("Error send mail: Detail:" + ex, MessageType.Error);
		//        }
		//    }
		//}



		public static void ReserveSendMail(string mailTo, string replyTo, string mailFrom, string attach, string head, string body)
		{
			GLOBAL.SysLog.Write("ReserveSend message to " + mailTo + " from " + mailFrom + " replayTo " + replyTo, MessageType.Information);

			string[] strArray = mailTo.Split(new char[] { ';' });
			foreach (string str in strArray)
			{
				if (str.Length >= 3)
				{
					GLOBAL.SysLog.Write("Reserve send mail to: " + str + " from " + mailFrom, 0);
					string resSmtpHost = Configuration.ResSmtpHost;
					int resSmtpPort = Configuration.ResSmtpPort;
					string resSmtpUser = Configuration.ResSmtpUser;
					string resSmtpPassword = Configuration.ResSmtpPassword;
					SmtpClient client = new SmtpClient(resSmtpHost, resSmtpPort);
					if (Configuration.SmtpSsl)
					{
						client.EnableSsl = true;
					}
					client.Credentials = new NetworkCredential(resSmtpUser, resSmtpPassword);
					string from = mailFrom;
					string to = str;
					string subject = head;
					string str8 = body;
					MailMessage message2 = new MailMessage(from, to, subject, str8)
					{
						IsBodyHtml = true,
						ReplyTo = new MailAddress(replyTo)
					};
					MailMessage message = message2;
					if (attach != null)
					{
						Attachment item = new Attachment(attach);
						message.Attachments.Add(item);
					}
					ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
					try
					{
						client.Send(message);
						GLOBAL.SysLog.Write("Sending mail success", 0);
					}
					catch (SmtpException exception)
					{
						GLOBAL.SysLog.Write("Error send mail: Detail:" + exception,MessageType.Error);
					}
				}
			}
		}

		public static void SendMail(string mailTo, string replyTo, string mailFrom, string attach, string head, string body)
		{
			GLOBAL.SysLog.Write("Send Mailmessage to "+mailTo+" from "+mailFrom+" replayTo "+replyTo,MessageType.Information);

			string[] strArray = mailTo.Split(new char[] { ';' });
			foreach (string str in strArray)
			{
				if (str.Length >= 3)
				{
					string smtpHost = Configuration.SmtpHost;
					int smtpPort = Configuration.SmtpPort;
					string smtpUser = Configuration.SmtpUser;
					string smtpPassword = Configuration.SmtpPassword;
					SmtpClient client = new SmtpClient(smtpHost, smtpPort);
					if (Configuration.SmtpSsl)
					{
						client.EnableSsl = true;
					}
					client.Credentials = new NetworkCredential(smtpUser, smtpPassword);
					string from = mailFrom;
					string to = str;
					string subject = head;
					string str8 = body;
					MailMessage message2 = new MailMessage(from, to, subject, str8)
					{
						IsBodyHtml = true,
						ReplyTo = new MailAddress(replyTo)
					};
					MailMessage message = message2;
					if (attach != null)
					{
						Attachment item = new Attachment(attach);
						message.Attachments.Add(item);
					}
					try
					{
						client.Send(message);
						GLOBAL.SysLog.Write("Sending mail success", 0);
					}
					catch (SmtpException exception)
					{
						GLOBAL.SysLog.Write("Error send mail: Detail:" + exception,MessageType.Error);
						ReserveSendMail(mailTo, replyTo, mailFrom, attach, head, body);
					}
				}
				else
				{
					GLOBAL.SysLog.Write("Error send message to "+str+" to few sumbol.",MessageType.Error);
				}
			}
		}


		public static void CreateZip(string file, string outfile)
		{
			try
			{
				var s = new ZipOutputStream(File.Create(outfile));
				s.SetLevel(9); // 0-9, 9 being the highest compression
				var buffer = new byte[4096];

				var entry = new ZipEntry(Path.GetFileName(file)) { DateTime = DateTime.Now };
				s.PutNextEntry(entry);

				FileStream fs = File.OpenRead(file);
				int sourceBytes;
				do
				{
					sourceBytes = fs.Read(buffer, 0, buffer.Length);
					s.Write(buffer, 0, sourceBytes);
				}
				while (sourceBytes > 0);
				fs.Dispose();
				fs.Close();

				s.Finish();
				s.Close();

				Thread.Sleep(1000);
			}
			catch (Exception ex) { GLOBAL.SysLog.Write("Error compress file. Detail:"+ex,MessageType.Error); }
		}



	}
}
