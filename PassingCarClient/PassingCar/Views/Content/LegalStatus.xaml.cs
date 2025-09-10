using PassingCar.Extensions;
using PassingCar.Models;


namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LegalStatus : ContentPage
    {
        public User current_user;

        public LegalStatus(User user = null)
        {
            try
            {
                current_user = user;
                InitializeComponent();
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
                GoBack();
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }

        public async void GoBack()
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

        private async void SelectFisica(object sender, EventArgs e)
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushModalAsync(new PhoneNumber(current_user, "fisica"));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void SelectJuridica(object sender, EventArgs e)
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushModalAsync(new PhoneNumber(current_user, "juridica", false));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void SelectTransporterJuridica(object sender, EventArgs e)
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushModalAsync(new PhoneNumber(current_user, "juridica", true));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}