using System.Net;
using System.Net.Mail;
namespace ParkingManagement.Services
{
    public class EmailSender
    {
        private readonly IConfiguration config;
        public EmailSender(IConfiguration config)
        {
            this.config = config;
        }

        public async Task<bool> SendEmail(string sendto, string subject, string mailbody)
        {
            MailMessage message = new MailMessage("sandeepyadav75932@gmail.com", sendto, subject, mailbody);
            message.IsBodyHtml = true;

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new NetworkCredential(config["Email:emailId"], config["Email:password"]);
            try
            {
                client.Send(message);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}

