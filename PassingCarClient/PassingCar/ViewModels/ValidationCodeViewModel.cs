using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API;
using PassingCar.Models.API.User;
using PassingCar.Models.API.Validation;
using PassingCar.Views;
using PassingCar.Views.Register;

namespace PassingCar.ViewModels
{
    public class ValidationCodeViewModel : BaseViewModel
    {
        public string ValidationCode { get; set; }
        public Command BackCommand { get; }
        public Command SendAgainCommand { get; set; }
        private readonly User user;
        public string current_legal_entity;
        public string password;
        private readonly bool isShippingCompany;
        bool isRegistration = true;
        public ValidationCodeViewModel(User user, string legal_entity = null, bool isShippingCompany = false, string password = null , bool isregistration = true)
        {
            current_legal_entity = legal_entity;
            BackCommand = new Command(BackClicked);
            SendAgainCommand = new Command(SendAgain);
            this.user = user;
            this.isShippingCompany = isShippingCompany;
            this.password = password;
            this.isRegistration = isregistration;
        }

        [Obsolete]
        public async void CheckCodeClicked()
        {
            try
            {
                //handle exception
                CheckPhoneValidationCodeResponse result = await Api.CheckPhoneValidationCode(new CheckPhoneValidationCodeRequest()
                {
                    PhoneNumber = user.PhoneNumber,
                    ValidationCode = ValidationCode
                });
                if (result != null)
                {
                    if (result.IsValid)
                    {
                        this.OpenPopUp();
                        if (!string.IsNullOrEmpty(user.GoogleId) || !string.IsNullOrEmpty(user.FacebookId))
                        {
                            LoginResponse loginResponse = await Api.Login(new LoginRequest()
                            {
                                GoogleId = user.GoogleId,
                                FacebookId = user.FacebookId
                            });
                            if (loginResponse != null && loginResponse.Success && loginResponse.UserData != null)
                            {
                                loginResponse.UserData.PhoneNumber = user.PhoneNumber;
                                ApiBaseResponse response = await Api.UpdateUser(loginResponse.UserData);
                                if (response != null && response.Success)
                                {
                                    LoginViewModel lwm = new LoginViewModel(null);
                                    await lwm.InitMenuAfterLogin(loginResponse);

                                    Application.Current.MainPage = new MainMenu();
                                }
                                else
                                {
                                    Application.Current.MainPage = new RegisterInput(user, current_legal_entity);
                                }
                            }
                        }
                        else
                        {
                        }


                        if (!isRegistration)
                        {
                            await Application.Current.MainPage.Navigation.PushModalAsync(new SetNewPassword(user));

                        }
                        else
                        {
                            Application.Current.MainPage = new RegisterInput(user, current_legal_entity, isShippingCompany, password);
                        }
                        this.ClosePopUp();
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
                _ = ex.Handle();
            }
        }
        private void BackClicked()
        {
            try
            {
                //handle exception
               // Application.Current.MainPage = new PhoneNumber();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void SendAgain()
        {
            try
            {
                //handle exception
                if (user != null && !string.IsNullOrEmpty(user.PhoneNumber))
                {
                    SendValidationCodeOnPhoneResponse response = await Api.SendValidationCodeToPhone(new SendValidationCodeOnPhoneRequest()
                    {
                        PhoneNumber = user.PhoneNumber
                    });
                    if (response != null && response.Sent)
                    {
                        this.OpeErrorPopUp("Code was sent", $"Le enviamos otro código de validación el {user.PhoneNumber}", "Ok");
                    }
                    else
                    {
                this.OpeErrorPopUp("Algo salió mal", $"Inténtalo de nuevo", "Intentar otra vez");
                    }
                }
                else
                {
       this.OpeErrorPopUp("Algo salió mal", $"Perdimos el número de teléfono para enviar el código.. Inténtalo de nuevo", "INTENTALO DE NUEVO");
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
             //   this.OpeErrorPopUp("No se pudo verificar el código de validación", message, "INTENTALO DE NUEVO");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}
