namespace ChatRoom.API.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendWelcomeEmailAsync(string email, string firstName, string lastName);
        Task<bool> SendEmailAsync(string to, string subject, string body);
    }
}