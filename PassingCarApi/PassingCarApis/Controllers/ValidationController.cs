using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PassingCarApis.Configuration;
using PassingCarApis.Extensions;
using PassingCarApis.Models;
using PassingCarApis.Models.API;
using PassingCarApis.Models.API.Validation;
using PassingCarApis.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;




namespace PassingCarApis.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ValidationController : PassingCarBaseController
    {
        public ValidationController(INotificationService notificationService, IHubService hubService, ILoginService loginService) : base(notificationService, hubService, loginService)
        {

        }
        [HttpPost(Name = "SendCodeToPhone")]
        public async Task<SendValidationCodeOnPhoneResponse> SendCodeToPhone(SendValidationCodeOnPhoneRequest request)
        {
            SendValidationCodeOnPhoneResponse response;
            if (request != null)
            {
                if (!string.IsNullOrEmpty(request.PhoneNumber))
                {
                    using SqlConnection connection = AppConfiguration.GetConnection();
                    connection.Open();
                    try
                    {
                        int userId = 0;
                        string userEmail = string.Empty;
                        string username = string.Empty;
                        if (request.ForRegister)
                        {
                            if (!string.IsNullOrEmpty(request.PhoneNumber))
                            {
                                IEnumerable<UserDB> thisNumberUsers = connection.Query<UserDB>($"Select * from [User] where PhoneNumber = @PhoneNumber", new { request.PhoneNumber });
                                if (thisNumberUsers != null && thisNumberUsers.Count() > 0)
                                {
                                    UserDB user = thisNumberUsers.First();
                                    userId = user.Id;
                                    userEmail = user.Email ?? string.Empty;
                                    username = $"{user.Surname} {user.Name}";
                                    if (request.Customer && user.Customer)
                                    {
                                        return new SendValidationCodeOnPhoneResponse()
                                        {
                                            Sent = false,
                                            ErrorMessage = $"Este número de teléfono ya existe como persona física",
                                        };
                                    }
                                    if (request.JuridicPerson && user.JuridicPerson)
                                        return new SendValidationCodeOnPhoneResponse()
                                        {
                                            Sent = false,
                                            ErrorMessage = $"Este número de teléfono ya existe como persona jurídica",
                                        };
                                    if (request.Uber && user.Uber)
                                        return new SendValidationCodeOnPhoneResponse()
                                        {
                                            Sent = false,
                                            ErrorMessage = $"Este número de teléfono ya existe como persona jurídica del transportador",
                                        };
                                }
                            }
                        }
                        ValidationCode validationCode = new()
                        {
                            PhoneNumber = request.PhoneNumber,
                            Code = new Random().Next(10000, 99999).ToString(),
                            CreatedAt = DateTime.Now,
                            ExpiresAt = DateTime.Now.AddMinutes(5),
                        };
                        long id = connection.Insert(validationCode);
                        if (id > 0)
                        {
                            try
                            {

                                var accountSid = "ACb944af4f3ed3a80723e94b714a241bce";
                                var authToken = "28e040b5d2cb77b4fe231662b928c810";

                                var from = "whatsapp:+17624417605";
                                var to = $"whatsapp:{validationCode.PhoneNumber}";
                                var contentSid = "HXadbf6752619ccc26e8bfd566e7c3c766";
                                var contentVariables = $"{{\"1\":\"{validationCode.Code}\"}}";

                                using var client = new HttpClient();

                                var byteArray = Encoding.ASCII.GetBytes($"{accountSid}:{authToken}");
                                client.DefaultRequestHeaders.Authorization =
                                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                                var values = new Dictionary<string, string>
        {
            { "From", from },
            { "To", to },
            { "ContentSid", contentSid },
            { "ContentVariables", contentVariables }
        };

                                var content = new FormUrlEncodedContent(values);

                                var url = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";

                                var response_ = await client.PostAsync(url, content);
                                var responseString = await response_.Content.ReadAsStringAsync();

                                JsonDocument jsonDocument = JsonDocument.Parse(responseString);
                                JsonElement jsonElement = jsonDocument.RootElement;

                                // Access properties from the JsonElement
                                string status = jsonElement.GetProperty("status").GetString();
                                string error = jsonElement.GetProperty("error_message").GetString();
                                response = new SendValidationCodeOnPhoneResponse()
                                {
                                    Sent =  status == "queued" ? true : false,
                                    ExpirationDate = validationCode.ExpiresAt,
                                    ErrorMessage = error == null ? null : error,
                                    UserId = userId,
                                    Useremail = userEmail,
                                    Username = username,
                                };
                            }
                            catch (Exception ex)
                            {
                                ex.CatchIt();
                                response = new SendValidationCodeOnPhoneResponse()
                                {
                                    Sent = false,
                                    ErrorMessage = ex.Message,
                                };
                            }
                        }
                        else
                        {
                            response = new SendValidationCodeOnPhoneResponse()
                            {
                                Sent = false,
                                ErrorMessage = $"Invalid returned ID",
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.CatchIt();
                        response = new SendValidationCodeOnPhoneResponse()
                        {
                            Sent = false,
                            ErrorMessage = ex.Message,
                        };
                    }
                    finally
                    {
                        connection.Close();
                    }

                }
                else
                {
                    response = new SendValidationCodeOnPhoneResponse()
                    {
                        Sent = false,
                        ErrorMessage = $"PhoneNumber is required"
                    };
                }
            }
            else
            {
                response = new SendValidationCodeOnPhoneResponse()
                {
                    Sent = false,
                    ErrorMessage = $"Request is null"
                };
            }
            return response;
        }
        [HttpPost(Name = "CheckPhoneValidationCode")]
        public CheckPhoneValidationCodeResponse CheckPhoneValidationCode(CheckPhoneValidationCodeRequest request)
        {
            CheckPhoneValidationCodeResponse response;
            if (request != null)
            {
                if (!string.IsNullOrEmpty(request.PhoneNumber) && !string.IsNullOrEmpty(request.ValidationCode))
                {
                    using SqlConnection connection = AppConfiguration.GetConnection();
                    connection.Open();
                    try
                    {
                        try
                        {
                            IEnumerable<ValidationCode> validationCodesForThisNumber = connection.Query<ValidationCode>($"Select * from [ValidationCode] where PhoneNumber = @PhoneNumber", new { request.PhoneNumber });
                            if (validationCodesForThisNumber.HasValues())
                            {
                                validationCodesForThisNumber = validationCodesForThisNumber.Where(x => x.Code.Equals(request.ValidationCode));
                                if (validationCodesForThisNumber.HasValues() /*true*/)
                                {
                                    ValidationCode validationCode = validationCodesForThisNumber.First();
                                    response = validationCode.ExpiresAt > DateTime.Now
                                        ? new CheckPhoneValidationCodeResponse()
                                        {
                                            IsValid = true,
                                            ErrorMessage = string.Empty,
                                        }
                                        : new CheckPhoneValidationCodeResponse()
                                        {
                                            IsValid = false,
                                            ErrorMessage = $"Expired validation code",
                                        };



                                    response = new CheckPhoneValidationCodeResponse()
                                    {
                                        IsValid = true,
                                        ErrorMessage = string.Empty,
                                    };
                                }
                                else
                                {
                                    response = new CheckPhoneValidationCodeResponse()
                                    {
                                        IsValid = false,
                                        ErrorMessage = $"Invalid code",
                                    };
                                }
                            }
                            else
                            {
                                response = new CheckPhoneValidationCodeResponse()
                                {
                                    IsValid = false,
                                    ErrorMessage = $"This phone number has no validation codes",
                                };
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.CatchIt();
                            response = new CheckPhoneValidationCodeResponse()
                            {
                                IsValid = false,
                                ErrorMessage = ex.Message,
                            };
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.CatchIt();
                        response = new CheckPhoneValidationCodeResponse()
                        {
                            IsValid = false,
                            ErrorMessage = ex.Message,
                        };
                    }
                    finally
                    {
                        connection.Close();
                    }

                }
                else
                {
                    response = new CheckPhoneValidationCodeResponse()
                    {
                        IsValid = false,
                        ErrorMessage = $"PhoneNumber and Validation code is required"
                    };
                }
            }
            else
            {
                response = new CheckPhoneValidationCodeResponse()
                {
                    IsValid = false,
                    ErrorMessage = $"Request is null"
                };
            }
            return response;
        }
    }
}
