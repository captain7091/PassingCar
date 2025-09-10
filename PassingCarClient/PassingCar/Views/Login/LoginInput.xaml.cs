
using Auth0.OidcClient;
using PassingCar.Extensions;
using PassingCar.ViewModels;

namespace PassingCar.Views
{
    
    public partial class LoginInput : ContentPage
    {
        //public LoadingSmall page;
        //public LoadingSmall page2;
        private readonly Auth0Client? _auth0Client;
        public LoginInput()
        {
            try
            {
                InitializeComponent();
                _auth0Client = this.Handler?.MauiContext?.Services.GetService<Auth0Client>();

                BindingContext = new LoginViewModel(_auth0Client!);
                //page = new LoadingSmall();
                //page2 = new LoadingSmall();
                //ActivateLoading();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public void ActivateLoading()
        {
            try
            {
                //LoadingLoginView.Content = page;
                //LoadingLoginView2.Content = page2;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public void CloseLoading()
        {
            try
            {
                //LoadingLoginView.Content = null;
                //LoadingLoginView2.Content = null;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void login_email_phone_TextChanged(object sender, TextChangedEventArgs e)
        {
            emailCross.IsVisible = !string.IsNullOrEmpty(e.NewTextValue);
        }

        private void login_password_TextChanged(object sender, TextChangedEventArgs e)
        {
            passwordCross.IsVisible = !string.IsNullOrEmpty(e.NewTextValue);
        }

        private void emailCross_Clicked(object sender, EventArgs e)
        {
            emailCross.IsVisible = false;
            login_email_phone.Text = string.Empty;
        }

        private void passwordCross_Clicked(object sender, EventArgs e)
        {
            passwordCross.IsVisible = false;
            login_password.Text = string.Empty;
        }

        private void passwordEye_Clicked(object sender, EventArgs e)
        {
            login_password.IsPassword = !login_password.IsPassword;
            passwordEye.Source = login_password.IsPassword ? "close_eye.png" : "open_eye.png";
        }
    }
}