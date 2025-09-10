using Newtonsoft.Json;
using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API.Payment;
using Stripe;


namespace PassingCar.ViewModels
{
    public class MakePaymentViewModel
    {
        public string mycustomer;
        public string getchargedID;
        public string refundID;
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public int PaymentId { get; set; }
        public MakePaymentViewModel(int OrderId, decimal Amount)
        {
            this.OrderId = OrderId;
            this.Amount = Amount;
        }
        public async Task Pay(TokenCardOptions cardDetails)
        {
            try
            {
                //handle exception
                try
                {
                    StripeConfiguration.ApiKey = "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";
                    //Step 1 : Assign Card to Token Object and create Token

                    TokenCreateOptions token = new TokenCreateOptions
                    {
                        Card = cardDetails
                    };

                    TokenService serviceToken = new TokenService();
                    Token newToken = serviceToken.Create(token);

                    // Step 2 : Assign Token to the Source

                    SourceCreateOptions options = new SourceCreateOptions
                    {
                        Type = SourceType.Card,
                        Currency = "ron",
                        Token = newToken.Id
                    };
                    SourceService service = new SourceService();
                    Source source = service.Create(options);

                    //Step 3 : Now generate the customer who is doing the payment

                    Stripe.CustomerCreateOptions myCustomer = new Stripe.CustomerCreateOptions()
                    {
                        Name = "PassingCar",
                        Email = "patrutioan21@gmail.com",
                        Description = "PassingCar Payment",
                    };

                    CustomerService customerService = new Stripe.CustomerService();
                    Stripe.Customer stripeCustomer = customerService.Create(myCustomer);

                    mycustomer = stripeCustomer.Id; // Not needed

                    //Step 4 : Now Create Charge Options for the customer. 
                    User user = await Api.GetUserData();
                    ChargeCreateOptions chargeoptions = new Stripe.ChargeCreateOptions
                    {
                        Amount = Convert.ToInt16(Amount) * 100,
                        Currency = "ron",
                        ReceiptEmail = user.Email,
                        Customer = stripeCustomer.Id,
                        Source = source.Id

                    };

                    //Step 5 : Perform the payment by  Charging the customer with the payment. 
                    ChargeService service1 = new Stripe.ChargeService();
                    InsertPaymentResponse addPaymentResponse = await Api.AddPayment(new InsertPaymentInput()
                    {
                        ChargeRequest = new ChargeData()
                        {
                            Amount = chargeoptions.Amount,
                            ApplicationFeeAmount = chargeoptions.ApplicationFeeAmount,
                            Capture = chargeoptions.Capture,
                            Currency = chargeoptions.Currency,
                            Customer = chargeoptions.Customer,
                            Description = chargeoptions.Description,
                            ExchangeRate = chargeoptions.ExchangeRate,
                            Metadata = chargeoptions.Metadata,
                            ReceiptEmail = chargeoptions.ReceiptEmail,
                            StatementDescriptor = chargeoptions.StatementDescriptor,
                            StatementDescriptorSuffix = chargeoptions.StatementDescriptorSuffix,
                            TransferGroup = chargeoptions.TransferGroup,
                            ExtraParams = chargeoptions.ExtraParams,
                        },
                        OfferId = OrderId
                    });
                    if (addPaymentResponse != null && addPaymentResponse.Success && addPaymentResponse.PaymentId > 0)
                    {
                        PaymentId = addPaymentResponse.PaymentId;
                        string chargeId = string.Empty;
                                                try
                        {
                            Stripe.Charge charge = service1.Create(chargeoptions); // This will do the Payment
                            _ = await Api.UpdatePayResponse(new UpdatePayResponseRequest()
                            {
                                ChargeResponse = new ChargeResponse()
                                {
                                    Id = charge.Id,
                                    Object = charge.Object,
                                    Amount = charge.Amount,
                                    AmountRefunded = charge.AmountRefunded,
                                    ApplicationId = charge.ApplicationId,
                                    ApplicationFeeAmount = charge.ApplicationFeeAmount,
                                    CalculatedStatementDescriptor = charge.CalculatedStatementDescriptor,
                                    Captured = charge.Captured,
                                    Created = charge.Created,
                                    Currency = charge.Currency,
                                    Description = charge.Description,
                                    Disputed = charge.Disputed,
                                    FailureCode = charge.FailureCode,
                                    FailureMessage = charge.FailureMessage,
                                    Livemode = charge.Livemode,
                                    Metadata = charge.Metadata,
                                    Paid = charge.Paid,
                                    PaymentMethod = charge.PaymentMethod,
                                    ReceiptEmail = charge.ReceiptEmail,
                                    ReceiptNumber = charge.ReceiptNumber,
                                    ReceiptUrl = charge.ReceiptUrl,
                                    Refunded = charge.Refunded,
                                    StatementDescriptor = charge.StatementDescriptor,
                                    StatementDescriptorSuffix = charge.StatementDescriptorSuffix,
                                    Status = charge.Status,
                                    TransferGroup = charge.TransferGroup
                                },
                                PaymentId = PaymentId
                            });
                                                        _ = await Api.ChangeOfferState(new Models.API.Ads.ChangeOfferStateInput()
                            {
                                OfferId = OrderId,
                                State = OfferState.Accepted
                            });
                            Action redirectAction = new Action(async () =>
                            {
                                await Shell.Current.GoToAsync("//MyShippingPage");
                            });
                            this.ClosePopUp();
                            await Task.Delay(200);
                            this.OpeErrorPopUp("Todo está listo", "Su pago fue confirmado. Se desbloqueó el chat con el uber y ahora puedes ver detalles de Mi envío. Uber fue notificado y ¡recibirá el paquete en un futuro próximo!", "Ok", redirectAction);

                        }
                        catch (Exception ex)
                        {
                            this.OpeErrorPopUp("No se pudo realizar el pago", ex.Message, "Ok");
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.OpeErrorPopUp("No se pudo realizar el pago", ex.Message, "Ok");
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public void GetRefundForSpecificTransaction()
        {
            try
            {
                //handle exception
                RefundService refundService = new RefundService();
                RefundCreateOptions refundOptions = new RefundCreateOptions
                {
                    Charge = getchargedID,
                };
                Refund refund = refundService.Create(refundOptions);
                refundID = refund.Id;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }


        public void GetRefundInformation()
        {
            try
            {
                //handle exception
                RefundService service = new RefundService();
                Refund refund = service.Get(refundID);
                string serializedCustomer = JsonConvert.SerializeObject(refund);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}
