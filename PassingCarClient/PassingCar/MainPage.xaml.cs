using Auth0.OidcClient;

namespace PassingCar;

public partial class MainPage : ContentPage
{
    int count = 0;
    // 👇 new code
    private readonly Auth0Client auth0Client;
    // 👆 new code

    // 👇 changed code
    public MainPage(Auth0Client client)
        // 👆 changed code
    {
        InitializeComponent();
        auth0Client = client;    // 👈 new code
    }

    //...existing code...
  
    // 👇 new code
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var loginResult =await  auth0Client.LoginAsync();

        if (!loginResult.IsError)
        {
            LoginView.IsVisible = false;
            HomeView.IsVisible = true;
        }
        else
        {
            await DisplayAlert("Error", loginResult.ErrorDescription, "OK");
        }
    }
}