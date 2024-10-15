using System.Net;
using System.Net.Mail;

namespace WebShopping.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("tranchithanh0404@gmail.com", "npgunlkrxcqatoyp")
            };
            return client.SendMailAsync(new MailMessage(from: "tranchithanh0404@gmail.com", to: email, subject, message));
        }
    }
}
