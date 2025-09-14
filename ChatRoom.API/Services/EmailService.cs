using System.Net;
using System.Net.Mail;
using ChatRoom.API.Services.Interfaces;

namespace ChatRoom.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string firstName, string lastName)
        {
            var subject = "Welcome to ChatRoom!";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to ChatRoom, {firstName} {lastName}!</h2>
                    <p>Thank you for registering with our platform. You can now start managing your tasks and collaborating with your team.</p>
                    <p>If you have any questions, please don't hesitate to contact our support team.</p>
                    <br>
                    <p>Best regards,<br>The ChatRoom Team</p>
                </body>
                </html>";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = emailSettings["SmtpHost"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");
                var username = emailSettings["Username"];
                var password = emailSettings["Password"];
                var fromEmail = emailSettings["FromEmail"];
                var fromName = emailSettings["FromName"];

                // Skip sending if SMTP settings are not configured
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Email settings not configured. Skipping email to {Email}", to);
                    return true; // Return true to not block registration
                }

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = enableSsl;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(username, password);

                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(fromEmail!, fromName);
                mailMessage.To.Add(to);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                return false;
            }
        }
    }
}