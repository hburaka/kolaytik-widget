namespace Kolaytik.Core.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string verificationLink);
    Task SendPasswordResetAsync(string toEmail, string resetLink);
    Task SendTicketStatusChangedAsync(string toEmail, string subject, string newStatus);
}
