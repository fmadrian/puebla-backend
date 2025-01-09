using PueblaApi.Exceptions;
using PueblaApi.Services.Interfaces;
using PueblaApi.Settings;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace PueblaApi.Helpers;
/// <summary>
/// Implementation of the email service using SendGrid.
/// Singleton so we can reuse the same configuration throughout the application.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailConfiguration _emailConfiguration;
    private readonly ILogger<EmailService> _logger;
    private readonly SendGridClient client;
    public EmailService(EmailConfiguration emailConfiguration, ILogger<EmailService> logger)
    {
        this._emailConfiguration = emailConfiguration;
        this._logger = logger;
        try
        {
            this.client = new SendGridClient(this._emailConfiguration.ApiKey);
        }
        catch (Exception)
        {
            // Log error and rethrow exception.
            this._logger.LogError("Could not create SendGrid client.");
            throw;
        }
    }
    public async Task SendEmail(string to, string subject, string htmlContent)
    {

        var msg = MailHelper.CreateSingleEmail(new EmailAddress(this._emailConfiguration.Sender), new EmailAddress(to), subject, "", htmlContent);
        Response response = await client.SendEmailAsync(msg);

        if (response.IsSuccessStatusCode)
            this._logger.LogInformation("Email sent.");
        else
        {
            string error = "";
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.BadRequest:
                    error = "Error interno al enviar el correo al servicio de correo.";
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    error = "Error al autorizarse con SendGrid.";
                    break;
                case System.Net.HttpStatusCode.Forbidden:
                    error = "Muchos intentos fallidos, espere un momento y vuelva a intentar.";
                    break;
                case System.Net.HttpStatusCode.NotAcceptable:
                    error = "Error interno al recibir la respuesta del servicio de correo.";
                    break;
                case System.Net.HttpStatusCode.TooManyRequests:
                    error = "No se pudo enviar el correo. se ha excedido el límite mensual de uso del servicio de correo.";
                    break;
                case System.Net.HttpStatusCode.InternalServerError:
                    error = "Error por parte del servicio de correo, espere un momento y vuelva a intentar.";
                    break;
                default:
                    break;
            }
            this._logger.LogError("SendGrid Error - {StatusCode}. Check SendGrid internal documentation to understand the cause of the error.", response.StatusCode);
            throw new EmailException(error);
        }
    }
}