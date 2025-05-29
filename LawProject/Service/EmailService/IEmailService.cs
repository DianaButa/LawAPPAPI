namespace LawProject.Service.EmailService
{
  public interface IEmailService
  {
    Task SendConfirmationEmail(string email, string name, string fileNumber);
    Task SendNotificatonEmail(string email, string name, string fileNumber);

    Task SendEmailResetare(string toEmail, string subject, string body);
  }


}
