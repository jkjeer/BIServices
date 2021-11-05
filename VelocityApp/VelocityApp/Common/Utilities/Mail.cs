using System;
using System.Configuration;
using System.Net.Mail;

namespace VelocityApp.Common.Utilities
{
    public class Mail
    {
        public static void SendHtmlAsync(string host, string from, string to, string bcc, string subject, string body)
        {
            try
            {

                var sc = new SmtpClient();
                sc.Host = ConfigurationManager.AppSettings["SmtpHost"];
                var msg = new MailMessage();
                msg.From = new MailAddress(from);
                msg.To.Add(to);
                if (!string.IsNullOrEmpty(bcc))
                    msg.Bcc.Add(bcc);
                msg.Subject = subject;
                msg.IsBodyHtml = true;
                msg.Body = body;
                //sc.SendMailAsync(msg);
                sc.Send(msg);
            }
            catch (ApplicationException e)
            {
                throw new ApplicationException(e.Message);
            }
        }
    }
}
