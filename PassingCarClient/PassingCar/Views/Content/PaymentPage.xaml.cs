using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.ViewModels;
using Stripe;


namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PaymentPage : ContentPage
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentPage(int OrderId, decimal Amount)
        {
            try
            {
                this.OrderId = OrderId;
                this.Amount = Amount;
                InitializeComponent();
                amount_value.Text = Amount.ToString();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public void Pay(object sender, System.EventArgs e)
        {
            try
            {
                //check validators
                int check_pay = 0;

                //check card_number
                if (payment_cardnumber.Text != null)
                {
                    if (cardnumber_validator.IsValid)
                    {
                        //do nothing
                        payment_cardnumber_head.TextColor = Colors.Black;
                    }
                    else
                    {
                        payment_cardnumber_head.TextColor = Colors.IndianRed;
                        check_pay = 1;
                    }
                }
                else
                {
                    payment_cardnumber_head.TextColor = Colors.IndianRed;
                    check_pay = 1;
                }

                //check card_name
                if (payment_name.Text != null)
                {
                    if (payment_name.Text.Length > 1)
                    {
                        //do nothing
                        payment_name_head.TextColor = Colors.Black;
                    }
                    else
                    {
                        payment_name_head.TextColor = Colors.IndianRed;
                        check_pay = 1;
                    }
                }
                else
                {
                    payment_name_head.TextColor = Colors.IndianRed;
                    check_pay = 1;
                }

                //check exp_date
                if (payment_expirationdate.Text != null && payment_card_year.Text != null)
                {
                    if (cardnumber_validator.IsValid && payment_card_year.Text.Length > 3)
                    {
                        try
                        {
                            int year = int.Parse(payment_card_year.Text);
                            payment_expirationdate_head.TextColor = year < 2090 && year >= DateTime.Now.Year ? Colors.Black : Colors.IndianRed;
                        }
                        catch
                        {
                            payment_expirationdate_head.TextColor = Colors.IndianRed;
                        }
                        //do nothing
                        //bank_expirationdate_head.TextColor = Color.Black;
                    }
                    else
                    {
                        payment_expirationdate_head.TextColor = Colors.IndianRed;
                        check_pay = 1;
                    }
                }
                else
                {
                    payment_expirationdate_head.TextColor = Colors.IndianRed;
                    check_pay = 1;
                }

                //check cvc
                if (payment_cvc.Text != null)
                {
                    if (cvc_validator.IsValid)
                    {
                        //do nothing
                        payment_cvc_head.TextColor = Colors.Black;
                    }
                    else
                    {
                        payment_cvc_head.TextColor = Colors.IndianRed;
                        check_pay = 1;
                    }
                }
                else
                {
                    payment_cvc_head.TextColor = Colors.IndianRed;
                    check_pay = 1;
                }

                //if all_validators
                if (check_pay == 0)
                {
                    MakePayment();
                    //Console.WriteLine("all_okk");
                    //DoSomethingAsync();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async void MakePayment()
        {
            try
            {
                this.OpenPopUp();
                MakePaymentViewModel payment = new MakePaymentViewModel(OrderId, Amount);
                //
                User userLogged = await Api.GetUserData();
                TokenCardOptions cardOptions = new TokenCardOptions
                {
                    //Number = cardModel.Number,
                    Number = payment_cardnumber.Text,
                    //ExpMonth = cardModel.ExpMonth,
                    ExpMonth = payment_expirationdate.Text,
                    //ExpYear = cardModel.ExpYear,
                    ExpYear = payment_card_year.Text,
                    //Cvc = cardModel.Cvc,
                    Cvc = payment_cvc.Text,
                    //Currency = "EUR",
                    Currency = "ron",
                    //Name = cardModel.Name,
                    Name = payment_name.Text,
                    //AddressCity = cardModel.AddressCity,
                    AddressCity = (userLogged != null && userLogged.Address != null) ? userLogged.Address.City : string.Empty,
                    //AddressZip = cardModel.AddressZip,
                    AddressZip = (userLogged != null && userLogged.Address != null) ? userLogged.Address.ZipCode : string.Empty,
                    //AddressLine1 = cardModel.AddressLine1,
                    AddressLine1 = (userLogged != null && userLogged.Address != null) ? userLogged.Address.Address : string.Empty,
                    //AddressCountry = cardModel.AddressCountry
                    //AddressCountry = "RO"
                };

                await payment.Pay(cardOptions);
                _ = await Navigation.PopAsync();
                //payment.GetRefundForSpecificTransaction();
                //payment.GetRefundInformation();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void Backward(object sender, EventArgs e)
        {
            try
            {
                _ = await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            try
            {
                GoHome();
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }

        public async void GoHome()
        {
            try
            {
                _ = await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}