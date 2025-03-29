using Phongkham.Email;
using System.Net.Mail;
using System.Net;

public class EmailSender : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(ConstantHelper.emailSender, ConstantHelper.passwordSender),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(ConstantHelper.emailSender),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);  // Awaiting the email send to handle async correctly
    }
}
