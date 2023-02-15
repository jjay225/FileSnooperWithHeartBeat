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
        private readonly IConfiguration _config;
        private readonly string _emailFrom;
        private readonly string _emailTo;


        public SendGridService(
            ILogger<SendGridService> logger,
            ISendGridClient sendGridClient,
            IConfiguration config)
        {
            _logger = logger;
            _sendGridClient = sendGridClient;
            _config = config;
            _emailFrom = _config.GetValue<string>("EmailFrom");
            _emailTo = _config.GetValue<string>("EmailTo");
        }
        public async Task SendEmailTest()
        {
            var from = new EmailAddress(_emailFrom);
            var to = new EmailAddress(_emailTo);
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
