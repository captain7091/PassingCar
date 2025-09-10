
using PassingCar.Extensions;
using PassingCar.Models;
using PassingCar.ViewModels;
using PhoneNumbers;


namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PhoneNumber : ContentPage
    {
        private int check_if_enable = 0;
        private readonly string current_legal_entity;
        private readonly PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();
        private string lastPhoneNumber;
        public PhoneNumberViewModel ViewModel { get; set; }

        public PhoneNumber(User user = null, string legal_entity = null, bool isShippingCompany = false)
        {
            try
            {
                InitializeComponent();
                ViewModel = new PhoneNumberViewModel(user, Navigation, legal_entity, isShippingCompany);
                BindingContext = ViewModel;
                topbar_register_logo.IsVisible = false;
                number.TextChanged += Number_TextChanged;
                lastPhoneNumber = string.Empty;
                country_code.PropertyChanged += Country_code_PropertyChanged;
                if (!string.IsNullOrEmpty(country_code.Text))
                {
                    countryInit.Text = country_code.Text.Replace("(", "").Replace(")", "");
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void Country_code_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                countryInit.Text = country_code.Text.Replace("(", "").Replace(")", "");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void Number_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(number.Text))
                {
                    if (number.Text.Length < lastPhoneNumber.Length)
                    {
                        if (number.Text.Length == 3)
                        {
                            number.Text = string.Concat(number.Text.Take(2).ToList());
                        }
                        if (number.Text.Length == 7)
                        {
                            number.Text = string.Concat(number.Text.Take(6).ToList());
                        }
                    }
                    else
                    {
                        if (number.Text.Length == 3 || number.Text.Length == 7)
                        {
                            number.Text += $" ";
                        }
                        if (number.Text.Length > 11)
                        {
                            number.Text = string.Concat(number.Text.Take(11).ToList());
                        }
                    }
                }
                lastPhoneNumber = number.Text;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void OnScrolled(object sender, ScrolledEventArgs e)
        {
            try
            {
                if (Device.RuntimePlatform == Device.iOS)
                {
                    topbar_register_logo.IsVisible = false;
                    register_logo.IsVisible = true;
                    back_button.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow.png");
                    grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    check_if_enable = 0;
                    return;
                }
                //double scrollingSpace = this.ContentSize.Height - offers_listview.Height;
                //if (scrollingSpace <= e.ScrollY)
                if (e.ScrollY >= 100)
                {
                    register_logo.IsVisible = false;
                    topbar_register_logo.IsVisible = true;
                    if (check_if_enable == 0)
                    {
                        back_button.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow_white.png");
                        grid_context.BackgroundColor = Color.FromRgb(0, 0, 0);
                    }
                    check_if_enable = 1;
                    //reached end
                }
                if (e.ScrollY == 0)
                {
                    topbar_register_logo.IsVisible = false;
                    register_logo.IsVisible = true;
                    back_button.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow.png");
                    grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    check_if_enable = 0;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private bool IsValidNumber(string aNumber)
        {
            try
            {
                bool result = false;

                aNumber = aNumber.Trim();

                if (aNumber.StartsWith("00"))
                {
                    // Replace 00 at beginning with +
                    aNumber = "+" + aNumber.Remove(0, 2);
                }

                try
                {
                    result = phoneUtil.IsValidNumber(phoneUtil.Parse(aNumber, ""));
                }
                catch
                {
                    Console.WriteLine("errornotvalid");
                    // Exception means is no valid number
                }

                return result;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                string phoneNumber = $"{country_code.Text.Replace("(", "").Replace(")", "")}{number.Text.Replace(" ", string.Empty).Replace("-", string.Empty)}";
                if (IsValidNumber(phoneNumber))
                {
                    ViewModel.PhoneNumber = phoneNumber;
                    ViewModel.SendCodeCommand.Execute(null);
                    Console.WriteLine(country_code.Text + number.Text);
                }
                else
                {
                    this.OpeErrorPopUp($"Invalid phone number", $"Please recheck phone number and Intentar otra vez", $"OK");
                    Console.WriteLine(country_code.Text + number.Text);
                    Console.WriteLine("not valid");
                }
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
                _ = await Application.Current.MainPage.Navigation.PopModalAsync();
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
                _ = await Application.Current.MainPage.Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void GoHelpPage(object sender, EventArgs e)
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushModalAsync(new SupportPage("modal"));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}