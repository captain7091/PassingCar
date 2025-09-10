using Newtonsoft.Json;
using PassingCarApis.Extensions;
using PassingCarApis.Models.API;
using RestSharp;
using System.Net;

namespace PassingCarApis.Services
{
    public class SmsServices
    {
        //private string accountSid = "AC403833a3d312b3d1352bb69e72c02332";
        //private string authToken = "4b759c858df4c7a1c13bf69c9d3afe79";
        private string apitoken;
        public SmsServices()
        {

        }
        public async Task<ApiBaseResponse> Send(string phoneNumberTo, string messsage)
        {
            ApiBaseResponse response;
            try
            {
                RestClient client = await CreateAuthorizedClientForWS();
                RestRequest apiRquest = new("/sms/", Method.Post);
                _ = apiRquest.AddParameter("token", apitoken, ParameterType.HttpHeader);
                _ = apiRquest.AddParameter("from", "PassingCar", ParameterType.GetOrPost);
                _ = apiRquest.AddParameter("to", phoneNumberTo, ParameterType.GetOrPost);
                _ = apiRquest.AddParameter("text", messsage, ParameterType.GetOrPost);
                //apiRquest.AddParameter("from", "+34642249777");
                //apiRquest.AddParameter("to", phoneNumberTo);
                //apiRquest.AddParameter("text", messsage);
                RestResponse apiResponse = await client.ExecuteAsync(apiRquest);
                if (apiResponse != null)
                {
                    if (apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed)
                    {
                        response = new ApiBaseResponse()
                        {
                            Success = false,
                            ErrorMessage = $"Eroare apelare api. StatusCode {apiResponse.StatusCode} ResponseStatus {apiResponse.ResponseStatus}"
                        };
                    }
                    else
                    {
                        SendSMSResult? smsResult = JsonConvert.DeserializeObject<SendSMSResult>(apiResponse.Content);
                        $"SEND Response: {apiResponse.Content}".AddToLog();
                        response = smsResult != null
                            ? new ApiBaseResponse()
                            {
                                Success = smsResult.Success,
                                ErrorMessage = smsResult.Message
                            }
                            : new ApiBaseResponse()
                            {
                                Success = false,
                                ErrorMessage = $"smsResult is null"
                            };
                    }
                }
                else
                {
                    response = new ApiBaseResponse()
                    {
                        Success = false,
                        ErrorMessage = $"Eroare apelare api. apiResponse is  NULL"
                    };
                }

            }
            catch (Exception e)
            {
                e.CatchIt();
                response = new ApiBaseResponse()
                {
                    ErrorMessage = e.Message,
                    Success = false
                };
            }
            _ = JsonConvert.SerializeObject(new
            {
                phoneNumberTo,
                messsage,
                response
            });
            response.Success = true;
            return response;
        }
        private async Task<RestClient> CreateAuthorizedClientForWS()
        {
            RestClient client = CreateClientForWS();
            RestRequest apiRquest = new("/auth/", Method.Post);
            _ = apiRquest.AddParameter("appkey", "d39c753f08144e7463", ParameterType.GetOrPost);
            _ = apiRquest.AddParameter("appsecret", "f9e35071571d28a4c272a3a9a83b15a4e7ed9dc30fe", ParameterType.GetOrPost);
            //apiRquest.AddJsonBody(new
            //{
            //    appkey = "d39c753f08144e7463",
            //    appsecret = "f9e35071571d28a4c272a3a9a83b15a4e7ed9dc30fe"
            //});
            //apiRquest.AddParameter("appkey", "d39c753f08144e7463");
            //apiRquest.AddParameter("appsecret", "f9e35071571d28a4c272a3a9a83b15a4e7ed9dc30fe");
            RestResponse apiResponse = await client.ExecuteAsync(apiRquest);
            if (apiResponse != null)
            {
                if (apiResponse.StatusCode != HttpStatusCode.OK || apiResponse.ResponseStatus != ResponseStatus.Completed)
                {
                    return CreateClientForWS();
                }
                else
                {
                    AuthResult? authResult = JsonConvert.DeserializeObject<AuthResult>(apiResponse.Content);
                    if (authResult == null)
                    {
                        return CreateClientForWS();
                    }
                    else
                    {
                        _ = client.AddDefaultHeader("token", $"{authResult.token}");
                        apitoken = authResult.token;
                        return client;
                    }
                }
            }
            else
            {
                return CreateClientForWS();
            }
        }
        private static RestClient CreateClientForWS()
        {
            return new RestClient("https://www.hostinet.com/api");
        }
    }
    public class SendSMSResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int Code { get; set; }
    }
    public class AuthResult
    {
        public string? token { get; set; }
        public bool success { get; set; }
    }
}
