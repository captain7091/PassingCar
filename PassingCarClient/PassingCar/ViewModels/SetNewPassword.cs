using PassingCar.Extensions;
using PassingCar.Models;
using PassingCar.Models.API;
using PassingCar.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassingCar.ViewModels
{
    internal class SetNewPasswordViewmodel : BaseViewModel
    {
        private User user_;
        public SetNewPasswordViewmodel(User user)
        {
            user_ = user;
        }

        public async void SetNewPasswordClicked(string newPassword, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(newPassword.Trim()) || string.IsNullOrEmpty(confirmPassword.Trim()))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Please fill in all fields.", "OK");
                    return;
                }
                if (newPassword.Trim() != confirmPassword.Trim())
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Passwords do not match.", "OK");
                    return;
                }
                // Call API to set new password
                // Assuming Api.SetNewPassword is a method that sets the new password

                var user = await IntegrationsWithApi.Api.GetUserFromApiValid(user_.PhoneNumber);



                user.HashedPassword = newPassword.Trim().EncryptString();


                var response = await IntegrationsWithApi.Api.UpdateUser(user);

                if (response.Success)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Password has been set successfully.", "OK");
                    // Navigate to login or another page
                    Application.Current.MainPage = new LoginInput();

                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", response.ErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}
