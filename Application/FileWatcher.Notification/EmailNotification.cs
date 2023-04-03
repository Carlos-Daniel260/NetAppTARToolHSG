using System.Collections.Generic;
using System.Net.Mail;

namespace FileWatcher.Notification
{
    public class EmailNotification
    {
        public static void Send(string @from, List<string> to, string subject, string body)
        {
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress(from),
                    Subject = subject,
                    Body = "<table><tr><td style='text-align: center'> FileWatcher.Service Error </td></tr><tr><td>" +
                       body + "</td></tr></table>",
                    IsBodyHtml = true
                };

                to.ForEach(e => { mail.To.Add(new MailAddress(e)); });

                var client = new SmtpClient
                {
                    Port = 25,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Timeout = 1000,
                    Host = "Corimc04",
                };

                client.Send(mail);
            }
            catch (System.Exception)
            {
            }
            
        }
    }
}