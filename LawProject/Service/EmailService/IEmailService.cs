namespace LawProject.Service.EmailService
{
  public interface IEmailService
  {
    Task SendConfirmationEmail(string email, string name, string fileNumber);
    Task SendNotificatonEmail( int id, string email, string name, string fileNumber = null, string subject = null, string body = null);

    Task SendEmailResetare(string toEmail, string subject, string body);
  }


}
