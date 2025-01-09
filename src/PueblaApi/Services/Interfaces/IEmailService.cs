namespace PueblaApi.Services.Interfaces;

public interface IEmailService
{
    public Task SendEmail(string to, string subject, string htmlContent);
}