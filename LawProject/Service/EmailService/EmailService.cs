using MailKit.Net.Smtp;
using MimeKit;


namespace LawProject.Service.EmailService
{
  public class EmailService : IEmailService

  {
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public EmailService(IConfiguration configuration)
    {
      _smtpServer = configuration["EmailSettings:SmtpServer"];
      _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
      _smtpUser = configuration["EmailSettings:SmtpUsername"];
      _smtpPass = configuration["EmailSettings:SmtpPassword"];
      _senderEmail = configuration["EmailSettings:SenderAddress"];
      _senderName = configuration["EmailSettings:SenderName"];
    }

    public async Task SendConfirmationEmail(string email, string name, string fileNumber)
    {
      var subject = "Confirmation Email";
      var body = $@"
            <h1>Salut, !</h1>
            <p> A fost introdus dosarul cu numarul {fileNumber}, pentru {name}.</p>";

      await SendEmailAsync(email, subject, body);
    }

    public async Task SendNotificatonEmail(string email, string name, string fileNumber)
    {
      var subject = "Notification Email";
      var body = $@"
        <h1>Salut, !</h1>
        <p>A fost actualizat dosarul cu numarul {fileNumber}. Click aici pentru mai multe detaliiSend.</p>";

      await SendEmailAsync(email, subject, body);
    }


    private async Task SendEmailAsync(string to, string subject, string body)
    {
      var email = new MimeMessage();
      email.From.Add(new MailboxAddress(_senderName, _senderEmail));
      email.To.Add(MailboxAddress.Parse("diana.c.farcas@gmail.com")); // Adresa fixă
      email.Subject = subject;
      email.Body = new TextPart("html") { Text = body };

      using var smtpClient = new SmtpClient();
      await smtpClient.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
      await smtpClient.AuthenticateAsync(_smtpUser, _smtpPass);
      await smtpClient.SendAsync(email);
      await smtpClient.DisconnectAsync(true);
    }

    public async Task SendEmailResetare(string toEmail, string subject, string body)
    {
      // Logica de trimitere email (ex: SMTP, SendGrid, etc.)
    }
  }
}
