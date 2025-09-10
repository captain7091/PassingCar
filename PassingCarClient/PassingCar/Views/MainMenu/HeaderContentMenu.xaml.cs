using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.ViewModels;


namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HeaderContentView : ContentView
    {
        public static MainPageViewModel HeaderContext { get; set; }
        public HeaderContentView()
        {
            try
            {
                InitializeComponent();
                RefreshBindingContext();
                myHeader.HeightRequest = Application.Current.MainPage.Height / 3.5;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        
        public void RefreshBindingContext()
        {
            try
            {
                // CRITICAL FIX: Always use the current App.HeaderContext to ensure fresh data
                HeaderContext = App.HeaderContext;
                BindingContext = App.HeaderContext;
                System.Diagnostics.Debug.WriteLine($"[HeaderContentView] Binding context refreshed with UserName: '{App.HeaderContext?.UserName}'");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HeaderContentView] Error refreshing binding context: {ex.Message}");
                _ = ex.Handle();
            }
        }
        private async void LogOut(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    // Show confirmation alert
                    bool answer = await App.Current.MainPage.DisplayAlert(
                        "Logout",                          // Title
                        "Do you really want to logout?",  // Message
                        "Yes",                             // Accept button text
                        "No"                               // Cancel button text
                    );

                    // Check the user's response
                    if (answer)
                    {
                        _ = await Api.Logout();
                        // if (CrossFacebookClient.Current.IsLoggedIn)
                        // {
                        //     CrossFacebookClient.Current.Logout();
                        // }
                        // if (Plugin.Maui.GoogleClient.CrossGoogleClient.Current.IsLoggedIn)
                        // {
                        //     CrossGoogleClient.Current.Logout();
                        // }

                        Application.Current.MainPage = new LoginInput();
                    }
                }
                catch (Exception ex)
                {
                    this.OpeErrorPopUp("Error occured", ex.Message, "Ok");
                    Application.Current.MainPage = new LoginInput();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        [Obsolete]
        private async void OpenProfile(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.Navigation.PushAsync(new MyAccountPage());
                Shell.Current.FlyoutIsPresented = false;
                //await Shell.Current.GoToAsync($"//InboxPage");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void HomePage(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync($"//HomePage");
                Shell.Current.FlyoutIsPresented = false;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        [Obsolete]
        private async void ExitApp(object sender, EventArgs e)
        {
            try
            {
                // Display a confirmation alert to the user
                bool result = await Application.Current.MainPage.DisplayAlert(
                    title: "",
                    message: "¿Realmente desea salir de Passing Car?",
                    cancel: "NO",
                    accept: "SI");

                if (result)
                {
                    // Close the application gracefully
                    // This method will work for Android and iOS
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                // Handle exception (you might want to log it or show a message)
                _ = ex.Handle();
            }
        }


    }
}