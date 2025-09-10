using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PassingCar.Extensions;
using PassingCar.Hubs;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API.User;
using PassingCar.Services;
using PassingCar.Utils;
using PassingCar.Views;



using RestSharp;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Auth0.OidcClient;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace PassingCar.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly ICustomNotification notification;
        private IAppleSignIn appleSignInService;
        private readonly Auth0Client _auth0Client;
        public string EmailPhoneNumber { get; set; }
        public string Password { get; set; }
        private bool _loading;
        public bool Loading 
        { 
            get => _loading; 
            set 
            { 
                _loading = value; 
                OnPropertyChanged(); 
            } 
        }
        private bool isAppleLogoVisible;

        public bool IsAppleLogoVisible
        {
            get { return isAppleLogoVisible; }
            set
            {
                isAppleLogoVisible = value;
                OnPropertyChanged();
            }
        }

        public Command LoginCommand { get; }
        public Command LoginFacebookCommand { get; }
        public Command LoginGoogleCommand { get; }
        public Command LoginAppleCommand { get; }
        public Command ForgotPasswordCommand { get; }
        public Command RegisterCommand { get; }

        // private readonly IGoogleClientManager _googleClientManager;
        public bool IsLoggedIn { get; set; }
        public string Token { get; set; }
        public User CurrentUser { get; set; }
        public LoadingSmall Smallloading { get; set; }

        private readonly string[] permisions = new string[] { "email", "public_profile" };

        public LoginViewModel(Auth0Client auth0Client)
        {
            IsAppleLogoVisible = DeviceInfo.Platform == DevicePlatform.iOS;
            LoginCommand = new Command(OnLoginClicked);
            LoginFacebookCommand = new Command(OnFacebookLoginClick);
            LoginGoogleCommand = new Command(OnGoogleLoginClick);
            LoginAppleCommand = new Command(OnAppleLoginClick);
            RegisterCommand = new Command(OnRegisterClick);
            ForgotPasswordCommand = new Command(OnForgotPasswordClick);
            _auth0Client = Application.Current.Handler.MauiContext.Services.GetService<Auth0Client>();

            //Loading = false;
            // _googleClientManager = CrossGoogleClient.Current;
            appleSignInService = Application.Current.Handler.MauiContext.Services.GetService<IAppleSignIn>();

            IsLoggedIn = false;
            Smallloading = new LoadingSmall();
            //smallloading = new LoadingSmall();          
        }

        private async void OnLoginClicked(object obj)
        {
            try
            {
                //handle exception
                this.OpenPopUp(true);
                Loading = true;
                
                // Validate input fields
                if (string.IsNullOrWhiteSpace(EmailPhoneNumber) || string.IsNullOrWhiteSpace(Password))
                {
                    ShowErrorMessage("Login Failed", "Please enter both email/phone and password");
                    return;
                }
                
                // CRITICAL FIX: Add connection check before attempting login
                System.Diagnostics.Debug.WriteLine("[LoginViewModel] Starting login process...");
                
                string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                bool isValid = Regex.IsMatch(EmailPhoneNumber, pattern);

                var loginRequest = new LoginRequest()
                {
                    Email = isValid ? EmailPhoneNumber : "",
                    PhoneNumber = !isValid ? EmailPhoneNumber : "",
                    Password = Password,
                };
                
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Login request created for: {(isValid ? "Email" : "Phone")} = {EmailPhoneNumber}");

                // CRITICAL FIX: Direct API call with connection pre-warming for first-attempt reliability
                System.Diagnostics.Debug.WriteLine("[LoginViewModel] Calling API.Login with pre-warming enabled...");
                LoginResponse result = await Api.Login(loginRequest);

                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Login API response received: Success={result?.Success}, HasUserData={result?.UserData != null}");

                if (result != null)
                {
                    await ProcessLoginResponse(result);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LoginViewModel] Login result is null - this indicates a critical API issue");
                    ShowErrorMessage("Login Failed", "Login failed. Please check your internet connection and try again.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Login exception: {ex.Message}");
                ShowErrorMessage("Login Failed", $"Login failed: {ex.Message}");
                _ = ex.Handle();
            }
            finally
            {
                Loading = false;
                // Don't close popup here - let the success/error popup handle it
                // this.ClosePopUp();
                System.Diagnostics.Debug.WriteLine("[LoginViewModel] Login process completed");
            }
        }

        public async Task ProcessLoginResponse(LoginResponse result)
        {
            System.Diagnostics.Debug.WriteLine($"[LoginViewModel] ProcessLoginResponse - Success: {result?.Success}, ErrorMessage: {result?.ErrorMessage}");
            
            if (result.Success)
            {
                List<ProfileType> types = new List<ProfileType>();

                // Skip additional API call if user data is already present
                if (result.UserData != null)
                {
                    if (result.UserData.Uber)
                        types.Add(ProfileType.JuridicaTransporte);
                    if (result.UserData.Customer)
                        types.Add(ProfileType.Fisica);
                    if (result.UserData.JuridicPerson)
                        types.Add(ProfileType.Juridica);

                    if (types.Any())
                    {
                        // Show success message only when login is completely successful
                        ShowSuccessMessage("Login Successful", "Welcome! You have successfully logged in.");

                        ProfileType profile;
                        if (types.Count == 1)
                        {
                            profile = types[0];
                        }
                        else
                        {
                            Api.HasMorePofiles = true;
                            string action = await App.Current.MainPage.DisplayActionSheet("Elige el perfil", null, null,
                                types.Select(a => a.CustomToString()).ToArray());
                            profile = action.GetProfileType();
                        }

                        // Set profile immediately and navigate
                        await Api.SetProfile(profile);

                        // Navigate to main menu immediately
                        Application.Current.MainPage = new MainMenu();

                        // Initialize menu in background after navigation
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await InitMenuAfterLogin(result);
                            }
                            catch (Exception ex)
                            {
                                _ = ex.Handle();
                            }
                        });
                    }
                    else
                    {
                        ShowErrorMessage("Login Failed", "No profile registered for this account. Please contact support.");
                    }
                }
                else
                {
                    ShowErrorMessage("Login Failed", "User data not available. Please try again.");
                }
            }
            else
            {
                // Show failure message with backend error
                ShowErrorMessage("Login Failed", result.ErrorMessage ?? "Unknown error occurred. Please try again.");
            }
        }

        private void ShowErrorMessage(string title, string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Showing error popup: {title} - {message}");
                //handle exception - ensure it runs on main thread
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.OpeErrorPopUp(title, message, "OK");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Error showing error popup: {ex.Message}");
                _ = ex.Handle();
            }
        }

        private void ShowSuccessMessage(string title, string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Showing success popup: {title} - {message}");
                // Show green success popup - ensure it runs on main thread
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.OpeSuccessPopUp(title, message, "OK");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Error showing success popup: {ex.Message}");
                _ = ex.Handle();
            }
        }

        private async void OnRegisterClick(object obj)
        {
            try
            {
                //handle exception
                try
                {
                    await App.Current.MainPage.Navigation.PushModalAsync(new LegalStatus());
                    //await App.Current.MainPage.Navigation.PushModalAsync(new RegisterInput(new User()));
                }
                catch (Exception ex)
                {
                    this.OpeErrorPopUp("Failed OnRegisterClick", $"{JsonConvert.SerializeObject(ex)}", "OK");
                }
                //Application.Current.MainPage = new RegisterInput($"+40743469259");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }

        }

        private async void OnForgotPasswordClick(object obj)
        {
            try
            {
                //handle exception
                await Application.Current.MainPage.Navigation.PushModalAsync(  new ForgotPhoneNumber());

            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }

        }

        private async void OnAppleLoginClick()
        {
            try
            {
                this.OpenPopUp(true);
                Loading = true;
                var account = await appleSignInService.SignInWithAppleAsync();
                if (account != null)
                {
                    LoginResponse loginResponse = await Api.Login(new LoginRequest()
                    {
                        Email = account.Email,
                        AppleId = account.UserId,
                    });
                    if (loginResponse != null && loginResponse.Success)
                    {
                        if (!string.IsNullOrEmpty(loginResponse.UserData.PhoneNumber))
                        {
                            await ProcessLoginResponse(loginResponse);
                        }
                        else
                        {
                            loginResponse.UserData.AppleId = account.UserId;
                            await Application.Current.MainPage.Navigation.PushModalAsync(
                                new LegalStatus(loginResponse.UserData));
                        }
                    }
                    else
                    {
                        CurrentUser = new User()
                        {
                            Name = account.Name,
                            Email = account.Email,
                            Surname = "",
                            AppleId = account.UserId,
                            RegistrationType = RegistrationType.Apple,
                            CreatedAt = DateTime.Now
                        };
                        //await Api.Register(CurrentUser);
                        // Log the current User email
                        Debug.WriteLine(CurrentUser.Email);
                        IsLoggedIn = true;

                        await Application.Current.MainPage.Navigation.PushModalAsync(new LegalStatus(CurrentUser));
                    }

                    //string token = CrossGoogleClient.Current.AccessToken;
                    //Token = token;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void OnGoogleLoginClick()
        {

            //handle exception
            this.OpenPopUp(true);
            Loading = true;
            try
            {
                var loginResult = await _auth0Client.LoginAsync();

                if (loginResult.IsError)
                {
                    // Handle error
                    Console.WriteLine($"Error: {loginResult.Error}");
                    return;
                }

                // Access the tokens
                var accessToken = loginResult.AccessToken;
                var idToken = loginResult.IdentityToken;
                var refreshToken = loginResult.RefreshToken;
                var user = loginResult.User;
                var name = user.FindFirst(c => c.Type == "name")?.Value;
                var email = user.FindFirst(c => c.Type == "email")?.Value;
                var picture = user.FindFirst(c => c.Type == "picture")?.Value;



                LoginResponse loginResponse = await Api.Login(new LoginRequest()
                {
                    Email = email,
                    GoogleId = idToken,
                });
                if (loginResponse != null && loginResponse.Success)
                {
                    if (!string.IsNullOrEmpty(loginResponse.UserData.PhoneNumber))
                    {
                        //if (loginResponse.UserData != null && string.IsNullOrEmpty(loginResponse.UserData.GoogleId))
                        //{
                        //    loginResponse.UserData.GoogleId = googleUser.Id;

                        //    var client = new RestClient(googleUser.Picture);
                        //    byte[] photoBytes = client.DownloadData(new RestRequest("#", Method.Get));
                        //    if (photoBytes != null && photoBytes.Length > 0)
                        //    {
                        //        loginResponse.UserData.Photo = photoBytes;
                        //    }

                        //    await Api.UpdateUser(loginResponse.UserData);
                        //}
                        await ProcessLoginResponse(loginResponse);
                    }
                    else
                    {
                        loginResponse.UserData.GoogleId = idToken;
                        await Application.Current.MainPage.Navigation.PushModalAsync(
                            new LegalStatus(loginResponse.UserData));
                    }
                }
                else
                {
                    CurrentUser = new User()
                    {
                        Name = name,
                        Email = email,
                        Surname = name,
                        GoogleId = idToken,
                        RegistrationType = RegistrationType.Google,
                        CreatedAt = DateTime.Now
                    };
                    RestClient client = new RestClient(picture);
                    CurrentUser.Photo = client.DownloadData(new RestRequest("#", Method.Get));
                    //await Api.Register(CurrentUser);
                    // Log the current User email
                    Debug.WriteLine(CurrentUser.Email);
                    IsLoggedIn = true;

                    await Application.Current.MainPage.Navigation.PushModalAsync(
                        new LegalStatus(CurrentUser));
                }

                string token = accessToken;
                Token = token;




                this.ClosePopUp();


            }
            catch (Exception ex)
            {
                this.ClosePopUp();
                this.OpeErrorPopUp("Failed to login with Google", $"{JsonConvert.SerializeObject(ex)}", "OK");
            }
            finally
            {
                Loading = false;
            }
        }





        private async void OnFacebookLoginClick()
        {
            try
            {
                //handle exception
                this.OpenPopUp(true);
                Loading = true;


                //await CrossFacebookClient.Current.RequestUserDataAsync(
                //    new string[] { "email", "first_name", "gender", "last_name" }, new string[] { }
                //    );
                try
                {
                    // FacebookResponse<bool> response = await CrossFacebookClient.Current.LoginAsync(permisions);
                    // this.ClosePopUp();
                    // switch (response.Status)
                    // {
                    //     case FacebookActionStatus.Completed:
                    //         IsLoggedIn = true;
                    //         await LoadData();
                    //         break;
                    // case FacebookActionStatus.Canceled:
                    //
                    //     break;
                    // case FacebookActionStatus.Unauthorized:
                    //     this.OpeErrorPopUp("Unauthorized", response.Message, "Ok");
                    //     break;
                    // case FacebookActionStatus.Error:
                    //     this.OpeErrorPopUp("Error", response.Message, "Ok");
                    //     break;
                    // }
                }
                catch (Exception ex)
                {
                    this.OpeErrorPopUp("Failed to login with Facebook", ex.Message, "Ok");
                }

                //this.OpeErrorPopUp("Not implemented", $"LoginViewModel > OnFacebookLoginClick", "OK");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
            finally
            {
                Loading = false;
            }

        }

        public async Task LoadData()
        {
            try
            {
                //handle exception
                try
                {
                    //  FacebookResponse<string> jsonData = await CrossFacebookClient.Current.RequestUserDataAsync
                    // (
                    //       new string[] { "id", "name", "email", "picture" }, new string[] { }
                    // );

                    // JObject data = JObject.Parse(jsonData.Data);
                    // LoginResponse loginResponse = await Api.Login(new LoginRequest()
                    // {
                    //     Email = data["email"].ToString(),
                    //     FacebookId = data["id"].ToString(),
                    // });
                    // if (loginResponse != null && loginResponse.Success)
                    // {
                    //     if (!string.IsNullOrEmpty(loginResponse.UserData.PhoneNumber))
                    //     {

                    // await ProcessLoginResponse(loginResponse);
                    //     }
                    //     else
                    //     {
                    //         loginResponse.UserData.FacebookId = data["id"].ToString();
                    //         await Application.Current.MainPage.Navigation.PushModalAsync(new LegalStatus(loginResponse.UserData));
                    //     }
                    //}
                    // else
                    // {
                    //     CurrentUser = new User()
                    //     {
                    //         Name = data["name"].ToString(),
                    //         CreatedAt = DateTime.Now,
                    //         Email = data["email"].ToString(),
                    //         FacebookId = data["id"].ToString(),
                    //         RegistrationType = RegistrationType.Facebook
                    //     };
                    //     string name = data["name"].ToString();
                    //     string[] nameSplited = name.Split(' ');
                    //     if (nameSplited.Length > 0)
                    //     {
                    //         if (nameSplited.Length > 1)
                    //         {
                    //             CurrentUser.Name = nameSplited[nameSplited.Length - 1];
                    //             for (int index = 0; index < nameSplited.Length - 1; index++)
                    //             {
                    //                 CurrentUser.Surname = nameSplited[index];
                    //             }
                    //         }
                    //     }
                    //     RestClient client = new RestClient($"{data["picture"]["data"]["url"]}");
                    //     CurrentUser.Photo = client.DownloadData(new RestRequest("#", Method.Get));
                    //     //await Api.Register(CurrentUser);
                    //     //Debug.WriteLine(CurrentUser.Email);
                    //
                    //     await Application.Current.MainPage.Navigation.PushModalAsync(new LegalStatus(CurrentUser));
                    // }
                }
                catch (Exception ex)
                {
                    this.OpeErrorPopUp("Failed to load data after login with Facebook", JsonConvert.SerializeObject(ex), "Ok");
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public async Task InitMenuAfterLogin(LoginResponse loginResponse)
        {
            try
            {
                // CRITICAL FIX: Clear only session data, preserve user ads and content
                App.CurrentUser = null;
                App.HeaderContext = null;
                
                // FIXED: Only clear session-specific cache, NOT user ads
                try
                {
                    await App.LocalDatabase.ClearCache(); // Now only clears session data, preserves ads
                    System.Diagnostics.Debug.WriteLine("[LoginViewModel] Cleared session cache (user ads preserved)");
                }
                catch (Exception cacheEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Error clearing session cache: {cacheEx.Message}");
                }
                
                // Initialize header context immediately with fresh instance
                App.HeaderContext = new MainPageViewModel();

                if (loginResponse.UserData != null)
                {
                    // CRITICAL FIX: Set App.CurrentUser so user ID is available throughout the app
                    App.CurrentUser = loginResponse.UserData;
                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] App.CurrentUser set: ID={App.CurrentUser.Id}, Name='{App.CurrentUser.Name}'");

                    // Also set the local CurrentUser for this ViewModel
                    CurrentUser = loginResponse.UserData;
                    // Set profile image asynchronously without blocking
                    if (loginResponse.UserData.Photo != null && loginResponse.UserData.Photo.Length > 0)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            try
                            {
                                App.HeaderContext.ProfileImageSource = ImageSource.FromStream(() => new MemoryStream(loginResponse.UserData.Photo));
                            }
                            catch (Exception ex)
                            {
                                _ = ex.Handle();
                            }
                        });
                    }

                    // Load profile name in background
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await App.HeaderContext.LoadProfileName(loginResponse.UserData);
                            
                            // CRITICAL FIX: Refresh header binding after profile data is loaded
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                try
                                {
                                    if (Application.Current.MainPage is MainMenu mainMenu)
                                    {
                                        mainMenu.RefreshHeaderBinding();
                                        System.Diagnostics.Debug.WriteLine("[LoginViewModel] Header binding refreshed after profile load");
                                    }
                                }
                                catch (Exception refreshEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Error refreshing header: {refreshEx.Message}");
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            _ = ex.Handle();
                        }
                    });

                    // Initialize hubs in background without blocking login
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine("[LoginViewModel] Starting hub initialization");
                            
                            // Check network connectivity before initializing hubs
                            var connectivity = Connectivity.NetworkAccess;
                            if (connectivity != NetworkAccess.Internet)
                            {
                                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] No internet connectivity for hub initialization: {connectivity}");
                                return;
                            }
                            
                            // Add timeout for hub initialization
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                            
                            await PassingCarHubs.InitHubs(loginResponse.Token);
                            System.Diagnostics.Debug.WriteLine("[LoginViewModel] Hub initialization completed successfully");

                            PassingCarHubs.UpdateEvent(PassingCarHubs.Chats.NewMessage, (input) =>
                            {
                                try
                                {
                                    ChatMessageWithOtherUserId item = input as ChatMessageWithOtherUserId;
                                    if (item?.UserId != loginResponse.UserData.Id)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"[LoginViewModel] New message received from user {item?.UserId}");
                                        // CrossLocalNotifications.Current.Show("Tienes nuevo mensaje", $"{item.Message}");
                                    }
                                }
                                catch (Exception msgEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Error handling new message event: {msgEx.Message}");
                                }
                            });
                        }
                        catch (OperationCanceledException)
                        {
                            System.Diagnostics.Debug.WriteLine("[LoginViewModel] Hub initialization was cancelled due to timeout");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Error initializing hubs: {ex.Message}");
                            _ = ex.Handle();
                            // Don't show error popup for hub connection failures during login
                        }
                    });

                    // Set profile types
                    List<ProfileType> types = new List<ProfileType>();
                    if (loginResponse.UserData.Uber)
                        types.Add(ProfileType.JuridicaTransporte);
                    if (loginResponse.UserData.Customer)
                        types.Add(ProfileType.Fisica);
                    if (loginResponse.UserData.JuridicPerson)
                        types.Add(ProfileType.Juridica);

                    Api.HasMorePofiles = types.Any() && types.Count > 1;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
    public interface ICustomNotification
    {
        void Send(string Title, string Message);
    }
}
