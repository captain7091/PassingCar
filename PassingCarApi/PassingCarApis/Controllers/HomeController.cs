using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PassingCarApis.Configuration;
using PassingCarApis.Extensions;
using PassingCarApis.Models;
using PassingCarApis.Models.API.Payment;
using Rotativa.AspNetCore;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PassingCarApis.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Redirect("https://www.passingcar.com/");
        }
        public IActionResult RemoveLogs()
        {
            if (AppConfiguration.CanDeleteLogs)
            {

                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = @$"Delete from dbo.[Log]";
                    _ = connection.Query(query);
                    $"Log was deleted".AddToLog();
                    return Json($"Log was deleted");
                }
                catch (Exception ex)
                {
                    ex.CatchIt();
                    return Json(JsonConvert.SerializeObject(ex));
                }
            }
            else
            {
                return Redirect("https://www.passingcar.com/");
            }
        }
        [HttpPost]
        public IActionResult PaymentProof(PaymentProofModel input)
        {
            ViewAsPdf pdf = new("PaymentProof", input)
            {
                FileName = "PaymentProof.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(0, 0, 0, 0)
            };
            return pdf;
        }
        public async Task<IActionResult> PaymentProofByOrderId(int offerId)
        {
            GetPaymentDetailsResponse response = new();
            _ = await HttpContext.GetTokenAsync("access_token");
            //int userId = GetUserIdFromToken(token);
            //if (userId > 0)
            //{
            using SqlConnection connection = AppConfiguration.GetConnection();
            connection.Open();
            try
            {
                string query = @$"Select Top (1) * from Payment
                                        where OfferId =  @OfferId";
                Payment payment = await connection.QueryFirstOrDefaultAsync<Payment>(query,
                    new
                    {
                        OfferId = offerId
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
            //}
            //else
            //{
            //    response.Success = false;
            //    response.ErrorMessage = $"You must be logged in!";
            //}
            ViewAsPdf pdf = new("PaymentProof", response.PaymentDetails.PaymentProofModel)
            {
                FileName = $"PassingCar_PayProof_{offerId}.pdf",
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(0, 0, 0, 0),
                ContentType = "application/pdf",
            };
            return pdf;
        }
        public async Task<IActionResult> GeneratePaymentProofByOrderId(int offerId)
        {
            try
            {
                GetPaymentDetailsResponse response = new();

                string? token = await HttpContext.GetTokenAsync("access_token");
                //int userId = GetUserIdFromToken(token);
                //if (userId > 0)
                //{
                using SqlConnection connection = AppConfiguration.GetConnection();
                connection.Open();
                try
                {
                    string query = @$"Select Top (1) * from Payment
                                        where OfferId =  @OfferId";
                    Payment payment = await connection.QueryFirstOrDefaultAsync<Payment>(query,
                        new
                        {
                            OfferId = offerId
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
                    response.Success = false;
                    response.ErrorMessage = ex.Message;
                    ex.CatchIt();
                }
                finally
                {
                    connection.Close();
                }
                ViewAsPdf pdf = new("GeneratedPaymentProof", response.PaymentDetails.PaymentProofFromTransporterModel)
                {
                    FileName = $"PayProof_{response.PaymentDetails.PaymentProofFromTransporterModel.NoFactura}.pdf",
                    PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    PageMargins = new Rotativa.AspNetCore.Options.Margins(0, 0, 0, 0),
                    ContentType = "application/pdf",
                };
                return pdf;
            }
            catch (Exception ex)
            {
                ex.CatchIt();
                return Redirect("https://passingcar.com");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
