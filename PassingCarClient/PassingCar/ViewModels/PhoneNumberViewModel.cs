
using Mopups.Services;
using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API.Validation;
using PassingCar.Popups;
using PassingCar.Utils;
using PassingCar.Views;
using Stripe;

using System.Windows.Input;
using Application = Microsoft.Maui.Controls.Application;

namespace PassingCar.ViewModels
{
    public class PhoneNumberViewModel : BaseViewModel
    {
        private CountryModel _selectedCountry;
        public string PhoneNumber { get; set; }
        public ICommand ShowPopupCommand { get; }
        public ICommand CountrySelectedCommand { get; }
        public Command BackCommand { get; }
        public Command SendCodeCommand { get; }
        private readonly User user;
        private readonly INavigation navigation;
        private readonly string current_legal_entity;
        private readonly bool isShippingCompany;
        bool isRegistration = true;
        public PhoneNumberViewModel(User user, INavigation navigation, string legal_entity = null, bool isShippingCompany = false, bool isRegistration = true)
        {
            try
            {
                //handle exception

                if (isRegistration == false)
                {
                    current_legal_entity = "";

                }
                else
                {
                    current_legal_entity = legal_entity;
                }


                BackCommand = new Command(OnBackClicked);
                SendCodeCommand = new Command(SendCodeClicked);
                this.user = user ?? new User();
                SelectedCountry = CountryUtils.GetCountryModelByName("Spain");//Spain
                ShowPopupCommand = new Command(async _ => await ExecuteShowPopupCommand());
                CountrySelectedCommand = new Command(country => ExecuteCountrySelectedCommand(country as CountryModel));
                this.navigation = navigation;
                this.isShippingCompany = isShippingCompany;
                this.isRegistration = isRegistration;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public CountryModel SelectedCountry
        {
            get => _selectedCountry;
            set => SetProperty(ref _selectedCountry, value);
        }

        private Task ExecuteShowPopupCommand()
        {
            try
            {
                ChooseCountryPopup popup = new ChooseCountryPopup(SelectedCountry)
                {
                    CountrySelectedCommand = CountrySelectedCommand
                };
                return MopupService.Instance.PushAsync(popup);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return null;
            }

        }

        private void ExecuteCountrySelectedCommand(CountryModel country)
        {
            try
            {
                SelectedCountry = country;
            }
            catch (Exception e)
            {
                _ = e.Handle();
            }

        }

        private async void OnBackClicked()
        {
            try
            {
                //handle exception
                await navigation.PushAsync(new LoginInput());
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void SendCodeClicked()
        {
            try
            {
                //handle exception
                try
                {
                    SendValidationCodeOnPhoneResponse result = await Api.SendValidationCodeToPhone(new SendValidationCodeOnPhoneRequest()
                    {
                        PhoneNumber = $"{PhoneNumber.Trim()}",
                        ForRegister = isRegistration,
                        Customer = current_legal_entity.Equals("fisica"),
                        JuridicPerson = current_legal_entity.Equals("juridica") && !isShippingCompany,
                        Uber = current_legal_entity.Equals("juridica") && isShippingCompany,
                    });
                    if (result != null)
                    {
                        if (result.Sent)
                        {
                            user.PhoneNumber = $"{PhoneNumber.Trim()}";
                            if (result.UserId > 0)
                            {
                                bool answer = await App.Current.MainPage.DisplayAlert($"¿Eres {result.Username}?", $"En caso afirmativo, necesita la contraseña de la cuenta {result.Useremail}", "Bien, continuando", "No, estoy probando con otro número");
                                if (answer)
                                {
                                    string password = await App.Current.MainPage.DisplayPromptAsync($"Bienvenido, {result.Username}", $"Por favor ingrese la contraseña de su cuenta para continuar");
                                    if (string.IsNullOrEmpty(password))
                                    {
                                        PhoneNumber = string.Empty;
                                        return;
                                    }
                                    new { }.OpenPopUp();
                                    var loginResult = await Api.Login(new Models.API.User.LoginRequest { Email = result.Useremail, Password = password });
                                    this.ClosePopUp();
                                    await Task.Delay(100);
                                    if (loginResult.Success)
                                    {
                                        await Application.Current.MainPage.Navigation.PushModalAsync(new CheckValidationCode(result, loginResult.UserData, current_legal_entity, isShippingCompany, password));
                                    }
                                    else
                                    {
                                        password = await App.Current.MainPage.DisplayPromptAsync($"Contraseña incorrecta, {result.Username}", $"Número de intentos restantes: 2");
                                        if (string.IsNullOrEmpty(password))
                                        {
                                            PhoneNumber = string.Empty;
                                            return;
                                        }
                                        new { }.OpenPopUp();
                                        loginResult = await Api.Login(new Models.API.User.LoginRequest { Email = result.Useremail, Password = password });
                                        this.ClosePopUp();
                                        await Task.Delay(100);
                                        if (loginResult.Success)
                                        {
                                            await Application.Current.MainPage.Navigation.PushModalAsync(new CheckValidationCode(result, loginResult.UserData, current_legal_entity, isShippingCompany, password));
                                        }
                                        else
                                        {
                                            password = await App.Current.MainPage.DisplayPromptAsync($"Contraseña incorrecta, {result.Username}", $"Número de intentos restantes: 1");
                                            if (string.IsNullOrEmpty(password))
                                            {
                                                PhoneNumber = string.Empty;
                                                return;
                                            }
                                            new { }.OpenPopUp();
                                            loginResult = await Api.Login(new Models.API.User.LoginRequest { Email = result.Useremail, Password = password });
                                            this.ClosePopUp();
                                            await Task.Delay(100);
                                            if (loginResult.Success)
                                            {
                                                await Application.Current.MainPage.Navigation.PushModalAsync(new CheckValidationCode(result, loginResult.UserData, current_legal_entity, isShippingCompany, password));
                                            }
                                            else
                                            {
                                                password = await App.Current.MainPage.DisplayPromptAsync($"Contraseña incorrecta, {result.Username}", $"Será redirigido de vuelta si la contraseña es incorrecta");
                                                if (string.IsNullOrEmpty(password))
                                                {
                                                    PhoneNumber = string.Empty;
                                                    return;
                                                }
                                                new { }.OpenPopUp();
                                                loginResult = await Api.Login(new Models.API.User.LoginRequest { Email = result.Useremail, Password = password });
                                                this.ClosePopUp();
                                                await Task.Delay(100);
                                                if (loginResult.Success)
                                                {
                                                    await Application.Current.MainPage.Navigation.PushModalAsync(new CheckValidationCode(result, loginResult.UserData, current_legal_entity, isShippingCompany, password));
                                                }
                                                else
                                                {
                                                    PhoneNumber = string.Empty;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    PhoneNumber = string.Empty;
                                }

                            }
                            else
                            {


                                if (isRegistration == false)
                                {


                                    if (string.IsNullOrEmpty(user.PhoneNumber))
                                    {
                                        user.PhoneNumber = $"{PhoneNumber.Trim()}";
                                    }
                                    await Application.Current.MainPage.Navigation.PushModalAsync(new ForgotCheckValidationCode(user));

                                }
                                else
                                {
                                    await Application.Current.MainPage.Navigation.PushModalAsync(new CheckValidationCode(result, user, current_legal_entity, isShippingCompany));
                                }
                            }

                        }
                        else
                        {
                            ShowErrorMessage(result.ErrorMessage);
                        }
                    }
                    else
                    {
                        ShowErrorMessage($"Result is null");
                    }

                }
                catch (Exception ex)
                {
                    ShowErrorMessage(ex.Message);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private void ShowErrorMessage(string message)
        {
            try
            {
                //handle exception
                navigation.OpeErrorPopUp("No se pudo enviar el código de validación", message, "INTENTALO DE NUEVO");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}

