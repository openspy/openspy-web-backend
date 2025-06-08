using System;
using System.Threading.Tasks;
using CoreWeb.Models;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CoreWeb.Services
{
    public class EmailService
    {
        private ISendGridClient sendGridClient;
        private IConfiguration config;
        public EmailService(IConfiguration config, ISendGridClient sendGridClient)
        {
            this.config = config;
            this.sendGridClient = sendGridClient;
        }

        public async Task<bool> SendEmailVerification(User user, String guid)
        {
            var to = new EmailAddress(user.Email);
            var from = new EmailAddress(config.GetValue<string>("SendGrid:FromAddress"), config.GetValue<string>("SendGrid:FromName"));
            var dynamicTemplateData = new
            {
                guid = guid,
                userid = user.Id
            };

            var msg = MailHelper.CreateSingleTemplateEmail(from, to, config.GetValue<string>("SendGrid:EmailVerifyTemplate"), dynamicTemplateData);
            var sendgridResposne = await sendGridClient.SendEmailAsync(msg);
            if (!sendgridResposne.IsSuccessStatusCode)
            {
                return false;
            }
            return true;
        }
        public async Task<bool> SendPasswordReset(User user, String guid)
        {
            var to = new EmailAddress(user.Email);
            var from = new EmailAddress(config.GetValue<string>("SendGrid:FromAddress"), config.GetValue<string>("SendGrid:FromName"));
            var dynamicTemplateData = new
            {
                guid = guid,
                userid = user.Id
            };

            var msg = MailHelper.CreateSingleTemplateEmail(from, to, config.GetValue<string>("SendGrid:PasswordResetTemplate"), dynamicTemplateData);
            var sendgridResposne = await sendGridClient.SendEmailAsync(msg);
            if (!sendgridResposne.IsSuccessStatusCode)
            {
                return false;
            }
            return true;
        }
    }
}