using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartBeatSnooperReader.Services
{
    public class SendGridService : ISendGridService
    {
        private readonly ILogger _logger;
        private readonly ISendGridClient _sendGridClient;

        public SendGridService(
            ILogger<SendGridService> logger,
            ISendGridClient sendGridClient)
        {
            _logger = logger;
            _sendGridClient = sendGridClient;
        }
        public async Task SendEmailTest()
        {
            var from = new EmailAddress("");
            var to = new EmailAddress("");
            var msg = new SendGridMessage
            {
                From = from,
                Subject = "Sending with Twilio SendGrid is Fun"
            };
            msg.AddContent(MimeType.Text, "and easy to do anywhere, even with C#");
            msg.AddTo(to);

            var response = await _sendGridClient.SendEmailAsync(msg);
            _logger.LogDebug("Status code from SendGrid is: {statusCode}", response.StatusCode);
        }
    }
}
