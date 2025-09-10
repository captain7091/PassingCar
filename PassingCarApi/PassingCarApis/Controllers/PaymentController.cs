using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PassingCarApis.Configuration;
using PassingCarApis.Extensions;
using PassingCarApis.Models;
using PassingCarApis.Models.API;
using PassingCarApis.Models.API.Payment;
using PassingCarApis.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PassingCarApis.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    [ApiController]
    public class PaymentController : PassingCarBaseController
    {
        private UserIdAndProfile? userIdAndProfile = null;
        public PaymentController(INotificationService notificationService, IHubService hubService, ILoginService loginService) : base(notificationService, hubService, loginService)
        {

        }
        private async Task<UserIdAndProfile> GetUserIdAndProfileAsync()
        {
            if (userIdAndProfile == null)
            {
                string? token = await HttpContext.GetTokenAsync("access_token");
                userIdAndProfile = loginService.GetUserProfileFromToken(token ?? string.Empty);
            }
            return userIdAndProfile;
        }
        [HttpPost(Name = "AddPayment")]
        public async Task<InsertPaymentResponse> Add(InsertPaymentInput input)
        {
            InsertPaymentResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = @$"INSERT INTO [dbo].[Payment]
                                       ([UserId]
                                       ,[OfferId]
                                       ,[ChargeRequestJson]
                                       ,[CreatedAt])
		                               OUTPUT INSERTED.Id
                                 VALUES
                                       (@UserId
                                       ,@OfferId
                                       ,@ChargeRequestJson
                                       ,GETDATE())";
                    response.PaymentId = await connection.QueryFirstOrDefaultAsync<int>(query,
                        new
                        {
                            UserId = loggedUser.Id,
                            input.OfferId,
                            ChargeRequestJson = JsonConvert.SerializeObject(input.ChargeRequest)
                        });

                    response.Success = true;
                    response.ErrorMessage = $"";
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "UpdatePayResponse")]
        public async Task<ApiBaseResponse> UpdatePayResponse(UpdatePayResponseRequest input)
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = @$"UPDATE [dbo].[Payment]
                                       SET 
                                          [ChargeResponseJson] = @ChargeResponseJson
                                          ,[ModifiedAt] = GETDATE()
                                     WHERE 
                                     Id = @PaymentId";
                    _ = await connection.QueryAsync(query,
                        new
                        {
                            input.PaymentId,
                            ChargeResponseJson = JsonConvert.SerializeObject(input.ChargeResponse)
                        });
                    if (input.ChargeResponse != null && input.ChargeResponse.Paid)
                    {
                        query = $@"Select
	                                CONCAT(u.Name,' ',u.Surname) as ClientName,
	                                u.Address,
	                                u.PersonalInfo,
	                                u.JuridicDetails,
                                    u.PhoneNumber,
	                                a.Title as Title,
	                                a.[From],
	                                a.[To],
                                    o.Id as OfferId,
	                                o.Message as Price
	                                from 
                                Offer o
                                inner join Payment p
                                on o.Id = p.OfferId
                                inner join Ads a
                                on o.AdId = a.Id
                                inner join [User] u
                                on a.UserId = u.Id
                                where p.Id = @PaymentId";
                        PaymentProofDBDetails paymentProofDBDetails = await connection.QueryFirstOrDefaultAsync<PaymentProofDBDetails>(query, new
                        {
                            input.PaymentId
                        });
                        AddressInfo? address = (!string.IsNullOrEmpty(paymentProofDBDetails.Address)) ? JsonConvert.DeserializeObject<AddressInfo>(paymentProofDBDetails.Address) : new AddressInfo();
                        PersonalInfo? personalInfo = (!string.IsNullOrEmpty(paymentProofDBDetails.PersonalInfo)) ? JsonConvert.DeserializeObject<PersonalInfo>(paymentProofDBDetails.PersonalInfo) : new PersonalInfo();
                        JuridicDetails? juridicDetails = (!string.IsNullOrEmpty(paymentProofDBDetails.JuridicDetails)) ? JsonConvert.DeserializeObject<JuridicDetails>(paymentProofDBDetails.JuridicDetails) : new JuridicDetails();
                        AddressInfo? addressFrom = (!string.IsNullOrEmpty(paymentProofDBDetails.From)) ? JsonConvert.DeserializeObject<AddressInfo>(paymentProofDBDetails.From) : new AddressInfo();
                        AddressInfo? addressTo = (!string.IsNullOrEmpty(paymentProofDBDetails.To)) ? JsonConvert.DeserializeObject<AddressInfo>(paymentProofDBDetails.To) : new AddressInfo();
                        TaxDetails taxDetails = new(paymentProofDBDetails.Price);
                        PaymentProofModel proofModel = new()
                        {
                            ClientName = paymentProofDBDetails.ClientName,
                            Address = address.Address,
                            City = address.City,
                            State = address.Provincie,
                            ZipCode = address.ZipCode,
                            NIF = juridicDetails.NIF,
                            PhoneNumer = paymentProofDBDetails.PhoneNumber,
                            DNI = personalInfo.DNI,
                            NoFactura = input.PaymentId.ToString(),
                            CreateDate = DateTime.Now,
                            Packet = paymentProofDBDetails.Title,
                            Origin = $"{addressFrom.Provincie}, {addressFrom.City}, {addressFrom.ZipCode}",
                            Destination = $"{addressTo.Provincie}, {addressTo.City}, {addressTo.ZipCode}",
                            Price = taxDetails.Price,
                            RestPrice = taxDetails.RestPrice,
                            BaseImposit = taxDetails.BaseImposit,
                            Imposit = taxDetails.Imposit,
                            RestClientPrice = taxDetails.RestClientPrice,
                        };
                        ApiBaseResponse insertPaymentProofModel = await UpdatePaymentProofModel(new UpdatePaymentProofRequest()
                        {
                            PaymentId = input.PaymentId,
                            ProofModel = proofModel
                        });
                        proofModel.NoFactura = string.Empty;
                        proofModel.ClientName = juridicDetails.CompanyName;
                        insertPaymentProofModel = await UpdatePaymentProofModelForTransporter(new UpdatePaymentProofRequest()
                        {
                            PaymentId = paymentProofDBDetails.OfferId,
                            ProofModel = proofModel
                        });
                    }

                    response.Success = true;
                    response.ErrorMessage = $"";
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "UploadNoFactura")]
        public async Task<ApiBaseResponse> UploadNoFactura(UploadNoFacturaRequest input)
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = @$"Select Top (1) * from Payment
                                        where OfferId =  @OfferId";
                    Payment payment = await connection.QueryFirstOrDefaultAsync<Payment>(query,
                        new
                        {
                            input.OfferId,
                        });
                    if (payment != null)
                    {
                        PaymentDetails paymentDetails = new()
                        {
                            PaymentId = payment.Id,
                            UserId = payment.UserId,
                            OfferId = payment.OfferId,
                            CreatedAt = payment.CreatedAt,
                            ModifiedAt = payment.ModifiedAt,
                        };
                        if (!string.IsNullOrEmpty(payment.PaymentProofFromTransporterModelJson))
                        {
                            paymentDetails.PaymentProofFromTransporterModel = JsonConvert.DeserializeObject<PaymentProofModel>(payment.PaymentProofFromTransporterModelJson) ?? new();
                        }
                        paymentDetails.PaymentProofFromTransporterModel!.NoFactura = input.NoFactura;
                        return await UpdatePaymentProofModelForTransporter(new UpdatePaymentProofRequest()
                        {
                            PaymentId = input.OfferId,
                            ProofModel = paymentDetails.PaymentProofFromTransporterModel
                        });
                    }
                    else
                    {
                        response.Success = false;
                        response.ErrorMessage = $"Eroare identificare plata";
                    }
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "UpdateRefund")]
        public async Task<ApiBaseResponse> UpdateRefund(UpdateRefundRequest input)
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = @$"UPDATE [dbo].[Payment]
                                       SET 
                                          [RefundRequestJson] = @RefundRequestJson
                                          ,[ModifiedAt] = GETDATE()
                                     WHERE 
                                     Id = @PaymentId";
                    _ = await connection.QueryAsync(query,
                        new
                        {
                            input.PaymentId,
                            RefundRequestJson = JsonConvert.SerializeObject(input.Refund)
                        });
                    response.Success = true;
                    response.ErrorMessage = $"";
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "UpdateRefundResponse")]
        public async Task<ApiBaseResponse> UpdateRefundResponse(UpdateRefundResponseRequest input)
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = @$"UPDATE [dbo].[Payment]
                                       SET 
                                          [RefundResponseJson] = @RefundResponseJson
                                          ,[ModifiedAt] = GETDATE()
                                     WHERE 
                                     Id = @PaymentId";
                    _ = await connection.QueryAsync(query,
                        new
                        {
                            input.PaymentId,
                            RefundRequestJson = JsonConvert.SerializeObject(input.Refund)
                        });
                    response.Success = true;
                    response.ErrorMessage = $"";
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        //[HttpPost(Name = "UpdatePaymentProofModel")]
        private async Task<ApiBaseResponse> UpdatePaymentProofModel(UpdatePaymentProofRequest input)
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = @$"UPDATE [dbo].[Payment]
                                       SET 
                                          [PaymentProofModelJson] = @PaymentProofModel
                                          ,[ModifiedAt] = GETDATE()
                                     WHERE 
                                     Id = @PaymentId";
                    _ = await connection.QueryAsync(query,
                        new
                        {
                            input.PaymentId,
                            PaymentProofModel = JsonConvert.SerializeObject(input.ProofModel)
                        });
                    response.Success = true;
                    response.ErrorMessage = $"";
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        private async Task<ApiBaseResponse> UpdatePaymentProofModelForTransporter(UpdatePaymentProofRequest input)
        {
            ApiBaseResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = @$"UPDATE [dbo].[Payment]
                                       SET 
                                          [PaymentProofFromTransporterModelJson] = @PaymentProofModel
                                          ,[ModifiedAt] = GETDATE()
                                     WHERE 
                                     OfferId = @PaymentId";
                    _ = await connection.QueryAsync(query,
                        new
                        {
                            input.PaymentId,
                            PaymentProofModel = JsonConvert.SerializeObject(input.ProofModel)
                        });
                    response.Success = true;
                    response.ErrorMessage = $"";
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
        [HttpPost(Name = "GetPaymentDetails")]
        public async Task<GetPaymentDetailsResponse> GetPaymentDetails(GetPaymentDetailsInput input)
        {
            GetPaymentDetailsResponse response = new();
            UserIdAndProfile loggedUser = await GetUserIdAndProfileAsync();
            if (loggedUser.Id > 0)
            {
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = @$"Select Top (1) * from Payment
                                        where OfferId =  @OfferId";
                    Payment payment = await connection.QueryFirstOrDefaultAsync<Payment>(query,
                        new
                        {
                            input.OfferId
                        });
                    if (payment != null)
                    {
                        PaymentDetails paymentDetails = new()
                        {
                            PaymentId = payment.Id,
                            UserId = payment.UserId,
                            OfferId = payment.OfferId,
                            CreatedAt = payment.CreatedAt,
                            ModifiedAt = payment.ModifiedAt,
                        };
                        if (!string.IsNullOrEmpty(payment.ChargeRequestJson))
                        {
                            paymentDetails.ChargeRequest = JsonConvert.DeserializeObject<ChargeData>(payment.ChargeRequestJson);
                        }
                        if (!string.IsNullOrEmpty(payment.ChargeResponseJson))
                        {
                            paymentDetails.ChargeResponse = JsonConvert.DeserializeObject<ChargeResponse>(payment.ChargeResponseJson);
                        }
                        if (!string.IsNullOrEmpty(payment.RefundRequestJson))
                        {
                            paymentDetails.RefundRequest = JsonConvert.DeserializeObject<RefundData>(payment.RefundRequestJson);
                        }
                        if (!string.IsNullOrEmpty(payment.RefundResponseJson))
                        {
                            paymentDetails.RefundResponse = JsonConvert.DeserializeObject<RefundResponse>(payment.RefundResponseJson);
                        }
                        if (!string.IsNullOrEmpty(payment.PaymentProofModelJson))
                        {
                            paymentDetails.PaymentProofModel = JsonConvert.DeserializeObject<PaymentProofModel>(payment.PaymentProofModelJson);
                        }
                        if (!string.IsNullOrEmpty(payment.PaymentProofFromTransporterModelJson))
                        {
                            paymentDetails.PaymentProofFromTransporterModel = JsonConvert.DeserializeObject<PaymentProofModel>(payment.PaymentProofFromTransporterModelJson);
                        }
                        response.PaymentDetails = paymentDetails;
                    }
                    else
                    {
                        response.PaymentDetails = new PaymentDetails();
                    }
                    response.Success = true;
                    response.ErrorMessage = $"";
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = $"You must be logged in!";
            }
            return response;
        }
    }
}
