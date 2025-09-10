
using Auth0.OidcClient;
using CommunityToolkit.Maui.Alerts;
using Newtonsoft.Json;
using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models.API;
using PassingCar.Models.API.User;
using System.Threading;
using System.Threading.Tasks;

namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Initial : ContentPage
    {
        private readonly Auth0Client _auth0Client;
        public delegate void ExecOnLoaded();
        public ExecOnLoaded ExecAfterAppearing { get; set; }
        public Initial(ExecOnLoaded ExecAfterAppearing = null)
        {
            try
            {
                InitializeComponent();
               // IsLocationServiceEnabled();
               _auth0Client=  this.Handler?.MauiContext?.Services.GetService<Auth0Client>();
                Routing.RegisterRoute("SupportPage", typeof(SupportPage));
               
                this.ExecAfterAppearing = ExecAfterAppearing;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        protected override async void OnAppearing()
        {
            try
            {
                LoadingArcs page = new LoadingArcs();
                LoadingContent.Content = page;
                base.OnAppearing();

                // Optimize: Show splash for minimal time, then navigate immediately
                _ = Task.Run(async () =>
                {
                    // Show splash for just 200ms to give visual feedback
                    await Task.Delay(200);
                    await InitializeAppAsync();
                });

                ExecAfterAppearing?.Invoke();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                // Fallback to login on any error
                Application.Current.MainPage = new LoginInput();
            }
        }

        private async Task InitializeAppAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("InitializeAppAsync started");

                // Check stored login data first (fastest operation)
                var loginData = await GetStoredLoginDataAsync();
                System.Diagnostics.Debug.WriteLine($"Login data retrieved: IsLoggedIn={loginData.IsLoggedIn}");

                if (loginData.IsLoggedIn && loginData.LoginRequest != null)
                {
                    System.Diagnostics.Debug.WriteLine("User is logged in, navigating to MainMenu");
                    // Navigate immediately to main menu, handle login in background
                    Dispatcher.Dispatch(() =>
                    {
                        try
                        {
                            Application.Current.MainPage = new MainMenu();
                            System.Diagnostics.Debug.WriteLine("MainMenu set successfully");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error creating MainMenu: {ex.Message}");
                            // Fallback to login page
                            Application.Current.MainPage = new LoginInput();
                        }
                    });

                    // Try to auto-login in background
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var loginResponse = await Api.Login(loginData.LoginRequest, false);
                            if (loginResponse != null && loginResponse.Success)
                            {
                                var lwm = new PassingCar.ViewModels.LoginViewModel(_auth0Client);
                                await lwm.InitMenuAfterLogin(loginResponse);
                            }
                        }
                        catch (Exception ex)
                        {
                            _ = ex.Handle();
                            // If background login fails, redirect to login page
                            Dispatcher.Dispatch(() =>
                            {
                                Application.Current.MainPage = new LoginInput();
                            });
                        }
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("User not logged in, navigating to LoginInput");
                    // Navigate to login immediately
                    Dispatcher.Dispatch(() =>
                    {
                        try
                        {
                            Application.Current.MainPage = new LoginInput();
                            System.Diagnostics.Debug.WriteLine("LoginInput set successfully");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error creating LoginInput: {ex.Message}");
                            // Ultimate fallback
                            Application.Current.MainPage = new ContentPage
                            {
                                Content = new Label
                                {
                                    Text = "Login Page Error - Please restart app",
                                    HorizontalOptions = LayoutOptions.Center,
                                    VerticalOptions = LayoutOptions.Center
                                }
                            };
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                System.Diagnostics.Debug.WriteLine($"Error in InitializeAppAsync: {ex.Message}");
                Dispatcher.Dispatch(() =>
                {
                    try
                    {
                        Application.Current.MainPage = new LoginInput();
                    }
                    catch
                    {
                        // Ultimate fallback
                        Application.Current.MainPage = new ContentPage
                        {
                            Content = new Label
                            {
                                Text = "App Error - Please restart",
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center
                            }
                        };
                    }
                });
            }
        }

        private async Task<bool> CheckConnectionWithTimeout()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Initial] Starting connection check with timeout");
                
                // Add timeout to prevent hanging - reduced to 3 seconds for faster response
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                
                // Check network connectivity first
                var connectivity = Connectivity.NetworkAccess;
                if (connectivity != NetworkAccess.Internet)
                {
                    System.Diagnostics.Debug.WriteLine($"[Initial] No internet connectivity: {connectivity}");
                    return false;
                }
                
                var connectionTask = Api.CheckConnection();
                var timeoutTask = Task.Delay(3000, cts.Token);

                var completedTask = await Task.WhenAny(connectionTask, timeoutTask);

                if (completedTask == connectionTask)
                {
                    cts.Cancel();
                    var result = await connectionTask;
                    bool isConnected = result != null && result.Success;
                    System.Diagnostics.Debug.WriteLine($"[Initial] Connection check completed: {isConnected}");
                    return isConnected;
                }

                // Timeout occurred
                System.Diagnostics.Debug.WriteLine("[Initial] Connection check timed out");
                return false;
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("[Initial] Connection check was cancelled");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Initial] Connection check error: {ex.Message}");
                // On error, return false to avoid assuming connectivity
                return false;
            }
        }

        private async Task<(bool IsLoggedIn, LoginRequest LoginRequest)> GetStoredLoginDataAsync()
        {
            try
            {
                var loggedInTask = SecureStorage.GetAsync("LoggedIn");
                var loginRequestTask = SecureStorage.GetAsync("LoginRequest");

                var results = await Task.WhenAll(loggedInTask, loginRequestTask);

                bool loggedIn = !string.IsNullOrEmpty(results[0]) && JsonConvert.DeserializeObject<bool>(results[0]);
                LoginRequest loginRequest = string.IsNullOrEmpty(results[1]) ? null : JsonConvert.DeserializeObject<LoginRequest>(results[1]);

                return (loggedIn, loginRequest);
            }
            catch
            {
                return (false, null);
            }
        }

       
    }
}