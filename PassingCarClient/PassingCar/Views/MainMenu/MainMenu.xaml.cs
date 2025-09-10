using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PassingCar.Views
{

    [DesignTimeVisible(true)]
    public partial class MainMenu : Shell
    {
        public bool HaveMoreProfiles = false;
        public MainMenu()
        {
            try
            {
                InitializeComponent();
                HaveMoreProfiles = Api.HasMorePofiles;
                if (HaveMoreProfiles)
                {
                    ch_profile.IsVisible = true;
                }
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    double rightMargin = App.Current.MainPage.Width / 4;
                    footerGrid.Margin = new Thickness(0, 0, rightMargin, 0);
                }
                
                // CRITICAL FIX: Refresh header binding after a short delay to ensure profile data is loaded
                Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
                {
                    try
                    {
                        RefreshHeaderBinding();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[MainMenu] Error refreshing header: {ex.Message}");
                    }
                    return false; // Don't repeat
                });
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        
        public void RefreshHeaderBinding()
        {
            try
            {
                headerItem?.RefreshBindingContext();
                System.Diagnostics.Debug.WriteLine("[MainMenu] Header binding refreshed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainMenu] Error refreshing header binding: {ex.Message}");
                _ = ex.Handle();
            }
        }
        protected override void OnAppearing()
        {
            try
            {
                //var user = await Api.GetUserData();
                //if (user != null && user.JuridicDetails != null && !string.IsNullOrEmpty(user.JuridicDetails.CompanyName) && !user.JuridicDetails.IsShippingCompany)
                //{
                //    AdsPage.FlyoutItemIsVisible = false;
                //}
                AdsPage.IsVisible = true;
                base.OnAppearing();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void BackPage(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("everythinng_ok");
                await Shell.Current.Navigation.PushAsync(new MyAdsPage());
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public async Task SwitchProfile()
        {
            var userdata = await Api.GetUserData();
            List<ProfileType> types = new List<ProfileType>();
            if (userdata.Uber)
                types.Add(ProfileType.JuridicaTransporte);
            if (userdata.Customer)
                types.Add(ProfileType.Fisica);
            if (userdata.JuridicPerson)
                types.Add(ProfileType.Juridica);
            if (types.Any())
            {
                ProfileType profile;
                if (types.Count == 1)
                {
                    profile = types[0];
                }
                else
                {
                    string action = await App.Current.MainPage.DisplayActionSheet("Elige el perfil", null, null, types.Select(a => a.CustomToString()).ToArray());
                    profile = action.GetProfileType();
                }
                this.OpenPopUp();
                await Api.SetProfile(profile);
                await App.HeaderContext.LoadProfileName(userdata);
                
                // CRITICAL FIX: Clear all cached listings to prevent profile mixing
                try
                {
                    // FIXED: Only clear session cache when switching profiles, preserve user ads
                    await App.LocalDatabase.ClearCache(); // Now only clears session data, preserves ads
                    System.Diagnostics.Debug.WriteLine($"[MainMenu] Profile switched to {profile}, cleared session cache (ads preserved)");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MainMenu] Error clearing session cache: {ex.Message}");
                }
                
                Application.Current.MainPage = new MainMenu();
                this.ClosePopUp();
            }
        }
        private async void HelpPage(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.Navigation.PushAsync(new SupportPage());
                Shell.Current.FlyoutIsPresented = false;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void ChangeAccount(object sender, EventArgs e)
        {
            try
            {
                await SwitchProfile();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}