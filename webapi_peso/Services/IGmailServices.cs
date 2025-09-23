using webapi_peso.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using static QRCoder.PayloadGenerator;

namespace PESOServerAPI.Services
{
    public interface IGmailServices
    {
        Task SendEmailConfimation(MailContent mail);
        Task SendEmail(string emailTo, string emailMessage);
    }
    public class GmailServices : IGmailServices
    {
        private MailSettings _mailConfig = new MailSettings()
        {
            Host = "smtp.gmail.com",
            Port = 587,
            FromEmail = "pesomisamisoriental@gmail.com",
            Username = "pesomisamisoriental@gmail.com",
            Password = "wbpoigwgueiigesb"
        };
        public async Task SendEmail(string emailTo, string emailMessage)
        {
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress(_mailConfig.FromEmail);
            message.To.Add(new MailAddress(emailTo));
            message.Subject = "[Reply] PESO Misamis Oriental";
            message.IsBodyHtml = true;
            message.Body = emailMessage;
            smtp.Port = _mailConfig.Port;
            smtp.Host = _mailConfig.Host;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(_mailConfig.Username, _mailConfig.Password);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            await smtp.SendMailAsync(message);

        }
        public async Task SendEmailConfimation(MailContent mail)
        {
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            message.From = new MailAddress(_mailConfig.FromEmail);
            message.To.Add(new MailAddress(mail.MailTo));
            message.Subject = "[PESO MisOr SYSTEM] Confirmation";
            message.IsBodyHtml = true;
            message.Body = "Please confirm your email by clicking the link below.";
            smtp.Port = _mailConfig.Port;
            smtp.Host = _mailConfig.Host;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(_mailConfig.Username, _mailConfig.Password);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            await smtp.SendMailAsync(message);
        }
    }
}
