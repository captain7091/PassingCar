using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using Stripe;


using Application = Microsoft.Maui.Controls.Application;

namespace PassingCar.ViewModels
{
    public class TokenWithdrawViewModel
    {
        public TokenWithdrawViewModel()
        {

        }

        public async Task<string> GetToken(TokenCardOptions cardDetails)
        {
            try
            {
                //handle exception
                try
                {
                    StripeConfiguration.ApiKey = "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";


                    /*var options = new AccountCreateOptions
                    {
                        Country = "RO",
                        Type = "express",
                        Capabilities = new AccountCapabilitiesOptions
                        {
                            CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                            Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                        },
                        BusinessType = "individual",
                        BusinessProfile = new AccountBusinessProfileOptions { 
                            Url = "https://passingcar.com",
                            Mcc = "4789",
                            ProductDescription = "Mobile App - PassingCar\nTransport\nCurier",
                            SupportPhone = "+40742826784"
                        },
                        Email = "stefanana1802@gmail.com",


                    };
                    var service = new AccountService();
                    Account acc = service.Create(options);

                    //var options = new AccountCreateOptions { Type = "express" };
                    //var service = new AccountService();
                    //Account acc = service.Create(options);

                    var options3 = new TokenCreateOptions
                    {
                        BankAccount = new TokenBankAccountOptions
                        {
                            Country = "RO",
                            Currency = "ron",
                            AccountNumber = "RO18INGB0000999908190200",
                        },
                    };
                    var service3 = new TokenService();
                    Token stripeToken = service3.Create(options3);

                    var options4 = new ExternalAccountCreateOptions
                    {
                        ExternalAccount = stripeToken.Id,
                    };
                    var service4 = new ExternalAccountService();
                    IExternalAccount sv = service4.Create(acc.Id, options4);

                    var options2 = new AccountLinkCreateOptions
                    {
                        Account = acc.Id,
                        RefreshUrl = "https://connect.stripe.com/express/oauth/authorize?response_type=code&client_id=ca_NF49plsCWrDuahzUxOE1ayBRFsm3dX8R",
                        ReturnUrl = "https://example.com/return",
                        Type = "account_onboarding",
                    };
                    var service2 = new AccountLinkService();
                    AccountLink ser = service2.Create(options2);*/

                    return "";


                    //
                    //var options2 = new ExternalAccountCreateOptions
                    //{
                    //    ExternalAccount = stripeToken.Id,
                    //};
                    //var service2 = new ExternalAccountService();
                    //IExternalAccount sv = service2.Create("acct_1M5EGCAlEN2DneBN", options2);

                    //return stripeToken.BankAccount.Id.ToString();

                    //var token = new TokenCreateOptions
                    //{
                    //    Card = cardDetails
                    //};
                    //var serviceToken = new TokenService();
                    //Token stripeToken = serviceToken.Create(token);
                    //return stripeToken.Card.Id.ToString();
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return string.Empty;
            }

        }



        public async Task<Transfer> CreatePayout(decimal? amount, string currency, string account)
        {

            var transfer = new TransferCreateOptions
            {
                Amount = (long)(amount), // Convert to cents
                Currency = currency,
                Destination = account
            };

            var service = new TransferService();
            return await service.CreateAsync(transfer);
        }

        public async Task<string> CheckIBAN(string iban, User user_data)
        {
            StripeConfiguration.ApiKey = "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";
            var data_iban = user_data.PersonalInfo.IBAN;
            //data_iban = "acct_1NEZaHPAkJAb4sAx";
            if (string.IsNullOrEmpty(data_iban))
            {
                //create express account for user
                var usserAccount = await CreateExpressAccount(iban, "RO", "ron");
                //add usserAccount.Id to database;
                user_data.PersonalInfo.IBAN = usserAccount.Id;
                await Api.UpdateUser(user_data);
                //generate link to register
                var accountlink = await CreateAccountLink(usserAccount.Id);
                await Launcher.OpenAsync(new Uri(accountlink.Url));
            }
            if (!string.IsNullOrEmpty(data_iban))
            {
                var connectedAccount = await GetConnectedAccount(data_iban);
                Console.WriteLine($"Connected Account Status: {connectedAccount.PayoutsEnabled}");
                Console.WriteLine($"Connected Account Status: {connectedAccount.ChargesEnabled}");
                if (connectedAccount.PayoutsEnabled == false || connectedAccount.ChargesEnabled == false) 
                {
                    await Application.Current.MainPage.DisplayAlert("Alert!", "Su información de retiro está incompleta. Serás redirigido para completar", "Redirigir");
                    var accountlink = await CreateAccountLink(data_iban);
                    await Launcher.OpenAsync(new Uri(accountlink.Url));
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Succes!", "¡Tus informaciones revisadas! Puedes pagar tu dinero", "Ok");
                    return "200";
                }               
            }
            return "0";
        }

        public async Task<Account> GetConnectedAccount(string connectedAccountId)
        {
            var service = new AccountService();
            return await service.GetAsync(connectedAccountId);
        }

        public async Task Payout(User user, decimal? amount)
        {
            StripeConfiguration.ApiKey = "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";
            var amountt = amount; // The amount you want to pay out
            var currency = "ron"; // The currency of the payout
            
            var balance = await GetBalance();
            // forme Output the available and pending amounts for each currency
            foreach (var balanceAmount in balance.Available)
            {
                Console.WriteLine($"Currency: {balanceAmount.Currency.ToUpper()}");
                Console.WriteLine($"Available Amount: {balanceAmount.Amount / 100}"); // Convert the amount from cents to the appropriate currency unit
                Console.WriteLine();
            }

            foreach (var balanceAmount in balance.Pending)
            {
                Console.WriteLine($"Currency: {balanceAmount.Currency.ToUpper()}");
                Console.WriteLine($"Pending Amount: {balanceAmount.Amount / 100}"); // Convert the amount from cents to the appropriate currency unit
                Console.WriteLine();
            }

            var payout = await CreatePayout(amountt, currency, user.PersonalInfo.IBAN);
            var balanceTransactionId = payout.BalanceTransactionId;

            var service = new BalanceTransactionService();
            var balanceTransaction = service.Get(balanceTransactionId);

            if (balanceTransaction.Status == "succeeded")
            {
                //empty sold (0)
                await Application.Current.MainPage.DisplayAlert("Succes", "Transfer was successful. (in 3-5 working days the money will arrive)", "Ok");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Alert!", "Transfer failed, Intentar otra vez or contact an administrator!", "Ok");
            }

            try
            {
                //handle exception
                try
                {
                    /*StripeConfiguration.ApiKey = "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";
                    var options = new PayoutCreateOptions
                    {
                        Amount = Convert.ToInt16(amount) * 100,
                        Currency = "ron",
                        Destination = token,
                        SourceType = "bank_account",
                    };
                    var service = new PayoutService();
                    service.Create(options);*/
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Alert!", "Algo salió mal. Intentar otra vez later or contact the Administrator! " + ex.ToString(), "Ok");
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async Task<Balance> GetBalance()
        {
            var service = new BalanceService();
            return await service.GetAsync();
        }

        public async Task<Account> CreateExpressAccount(string iban, string country, string currency)
        {
            var options = new AccountCreateOptions
            {
                Type = "express",
                ExternalAccount = new AccountBankAccountOptions
                {
                    Country = country,
                    Currency = currency,
                    AccountHolderType = "individual",
                    AccountNumber = iban
                }
            };
            var service = new AccountService();
            return await service.CreateAsync(options);
        }

        public async Task<AccountLink> CreateAccountLink(string accountId)
        {
            var options = new AccountLinkCreateOptions
            {
                Account = accountId,
                RefreshUrl = "https://connect.stripe.com/express/oauth/authorize?response_type=code&client_id=ca_NF49plsCWrDuahzUxOE1ayBRFsm3dX8R",
                ReturnUrl = "https://www.passingcar.com/index.php/withdraw/",
                Type = "account_onboarding",
            };

            var service = new AccountLinkService();
            return await service.CreateAsync(options);
        }

    }
}
