using System.Net;
using System.Net.Mail;
using System.Text;
using bakery_web_api.Interfaces.User;
using bakery_web_api.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace bakery_web_api.Services.User;

public class ContactFormService : IContactForm
{
    private readonly IConfiguration _configuration;

    public ContactFormService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IActionResult> SendMessage(ContactFormDto contactFormDto)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

            var smtpClient = new SmtpClient(smtpSettings?.SmtpServer)
            {
                Port = smtpSettings.Port,
                Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password),
                EnableSsl = true
            };

            // Tworzenie maila
            var mailMessage = new MailMessage
            {
                From = new MailAddress("kontakt@trzebachleba.pl", "trzebachleba.pl"),
                Subject = "Wiadomość z formularza kontaktowego",
                IsBodyHtml = true
            };
            mailMessage.To.Add("kontakt@trzebachleba.pl");

            // Tworzenie treści maila
            var emailContent = new StringBuilder();
            emailContent.AppendLine(
                "<h1 style='font-family: Arial, sans-serif; color: #333; font-size: 20px;'>Wiadomość wysłana z formularza strony kontaktu.</h1>");
            emailContent.AppendLine(
                $"<p style='font-family: Arial, sans-serif; color: #333; font-size: 16px;'>{contactFormDto.FirstName} {contactFormDto.LastName} <br>{contactFormDto.Email}</br>  <br>{contactFormDto.Phone}</br></p>");
            emailContent.AppendLine(
                $"<p style='font-family: Arial, sans-serif; color: #333; font-size: 14px;'>{contactFormDto.Message}</p>");

            mailMessage.Body = emailContent.ToString();

            // Wysłanie maila
            Task.Run(() => smtpClient.SendMailAsync(mailMessage));

            return new OkObjectResult("Wiadomość została pomyślnie wysłana!");
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new { error = ex.Message });
        }
    }
}