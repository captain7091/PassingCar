using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models.API.User;
using PassingCar.Views;


namespace PassingCar.ViewModels
{
    public class AccountPageViewModel : BaseViewModel
    {
        public Command DeleteAccount { get; set; }
        public AccountPageViewModel()
        {
            DeleteAccount = new Command(DeleteAccountAction);
        }
        private readonly INavigation navigation;
        private async void DeleteAccountAction()
        {
            try
            {
                //handle exception
                GetUserRatingAndSoldResponse curent_rev_balance = await Api.GetUserRatingAndSol();
                if (curent_rev_balance.Sold < 0)
                {
                    await App.Current.MainPage.DisplayAlert("Your cannot delete the account", "Your current balance is < 0.", "Ok");
                }
                else if (curent_rev_balance.Sold > 0)
                {
                    await App.Current.MainPage.DisplayAlert("Your cannot delete the account", "Your current balance is > 0.", "Ok");
                }
                else
                {
                    bool answer = await App.Current.MainPage.DisplayAlert("¿Estás seguro de que quieres eliminar la cuenta?",
                        "Anuncios, ofertas, chats, envíos, pagos, reseñas serán eliminados de PassingCar", "Si estoy seguro", "No");
                    if (answer)
                    {
                        Models.API.ApiBaseResponse result = await Api.DeleteAccount();
                        if (result.Success)
                        {
                            await App.Current.MainPage.DisplayAlert("Tu cuenta fue eliminada", "Puedes crear otra cuenta cada vez", "Ok");
                            _ = await Api.Logout();
                            Application.Current.MainPage = new LoginInput();
                        }
                        else
                        {
                            await App.Current.MainPage.DisplayAlert("Tu cuenta NO fue eliminada", result.ErrorMessage, "Ok");
                        }
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Your account was NOT deleted", $"Estamos felices de que sigas ahí", "Ok");
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}
