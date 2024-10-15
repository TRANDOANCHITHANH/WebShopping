namespace WebShopping.Areas.Admin.Repository
{
    public interface IEmailSender
    {
       public Task SendEmailAsync(string email, string subject,string message); // gui mail
    }
}
