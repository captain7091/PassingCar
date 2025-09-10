using Newtonsoft.Json;
using PassingCarApis.Extensions;
using PassingCarApis.Models.API;
using System.Net;
using System.Net.Mail;

namespace PassingCarApis.Services
{
    public class EmailService
    {
        public ApiBaseResponse Send(EmailSendReq req)
        {
            ApiBaseResponse response = new()
            {
                Success = true,
                ErrorMessage = string.Empty
            };
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                string username = "noreply@passingcar.com";
                string password = "passingcar@2022";

                string host = "host204.hostinet.com";
                int port = 25;

                SmtpClient smtp = new()
                {
                    Host = host,
                    Port = port,
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(username, password),
                    Timeout = 20000
                };

                using MailMessage message = new(username, req.To)
                {
                    Subject = "PassingCar ",
                    IsBodyHtml = true,
                    Body = req.Body
                };
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                response.Success = false;
                response.ErrorMessage = JsonConvert.SerializeObject(ex);
            }
            _ = JsonConvert.SerializeObject(new
            {
                Method = "Send Email",
                req.To,
                response
            });
            return response;
        }
    }
    public class EmailSendReq
    {
        public string? To { get; set; }
        public int OfferId { get; set; }
        public string? Body { get; set; }
    }
}
