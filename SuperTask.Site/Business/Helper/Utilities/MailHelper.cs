using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
namespace Business.Helper
{

	public class MailHelper
	{
		private readonly SmtpClient _client = new SmtpClient();

		public MailHelper(string host,int port)
		{
			_client.Host = host;
			_client.Port = port;
		}


		public async Task<bool> SendMail(string from, string fromName, string[] toUser, string[] cc, string title, string body)
		{
			try
			{
				var to = string.Join(";", toUser);
				var mail = new MailMessage(from, to, title, body);

				if(cc!=null && cc.Length > 0)
				{
					foreach(var item in cc)
					{
						mail.CC.Add(item);
					}
				}

				await _client.SendMailAsync(mail);

				return true;
				
			}
			catch
			{
				return false;
			}
		}

	}

}