using SendGrid.Helpers.Mail;
using SendGrid;
using System.Net;
namespace UmbracoGaloSfera.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string message)
    {
        var apiKey = _configuration["SendGrid:ApiKey"];
        var fromEmail = _configuration["SendGrid:FromEmail"];
        var fromName = _configuration["SendGrid:FromName"];

        try
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, message, message);

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                return true;
            }
            else
            {
                // Log the response body for detailed error info
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send email. Status Code: {StatusCode}, Response: {ResponseBody}", response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending email.");
            return false;
        }
    }
}
