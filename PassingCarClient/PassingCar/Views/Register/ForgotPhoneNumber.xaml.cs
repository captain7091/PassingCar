using PassingCar.Extensions;
using PassingCar.Models;
using PassingCar.ViewModels;
using PhoneNumbers;


namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ForgotPhoneNumber : ContentPage
    {
        private int check_if_enable = 0;
        private readonly PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();
        public ForgotPhoneNumber(User user = null)
        {
            try
            {
                InitializeComponent();
                BindingContext = new PhoneNumberViewModel(user, Navigation , isRegistration: false);
                topbar_register_logo.IsVisible = false;
                country_code.PropertyChanged += Country_code_PropertyChanged;
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
                if (IsValidNumber(country_code.Text.Replace("(", "").Replace(")", "") + number.Text))
                {
                    var phonenumber = country_code.Text.Replace("(", "").Replace(")", "") + number.Text;



                    var bind = BindingContext as PhoneNumberViewModel;

                    bind.PhoneNumber = phonenumber;
                    bind.SendCodeCommand.Execute(null);





                }
                else
                {
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