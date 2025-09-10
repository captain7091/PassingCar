using System;
using Mopups.Pages; // Using Mopups plugin for Popups
using Mopups.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics; // For Colors in MAUI
using PassingCar.Views.Intitial;
using PassingCar.Views.ContentPopUp;

namespace PassingCar.Extensions
{
    public static class LoadingPopUpMethods
    {
        private static LoadingPopUp popup;
        private static ContentPopUp contentPopUp;

        public static async void OpenPopUp(this object source, bool onlyAndroid = false)
        {
            if (popup == null)
            {
                if (DeviceInfo.Platform != DevicePlatform.iOS || !onlyAndroid)
                {
                    popup = new LoadingPopUp();
                    await MopupService.Instance.PushAsync(popup);
                }
            }
        }

        public static async void ClosePopUp(this object source)
        {
            if (popup != null)
            {
                await MopupService.Instance.PopAsync();
                popup = null;
            }
        }

        public static async void OpeContentPopUp(this object source, ContentPage page)
        {
            contentPopUp = new ContentPopUp(page.BindingContext)
            {
                // Define background and dismissal behavior for the content popup
            };

            var contentGrid = new Grid
            {
                HeightRequest = Application.Current.MainPage.Height - 160,
                WidthRequest = Application.Current.MainPage.Width - 140,
                BackgroundColor = Colors.White
            };

            // Add the page content into the grid
            page.WidthRequest = Application.Current.MainPage.Width - 140;
            page.HeightRequest = Application.Current.MainPage.Height - 160;
            contentGrid.Children.Add(page.Content);

            // Add close button with a gesture
            var closeButton = new Button
            {
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(15),
                CornerRadius = 50,
                TextColor = Colors.Red,
                Padding = new Thickness(1),
                ImageSource = "x-symbol.png", // Use image from resource
                BackgroundColor = Colors.White
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                new { }.CloseContentPopUp();
            };
            closeButton.GestureRecognizers.Add(tapGesture);
            contentGrid.Children.Add(closeButton);

            // Create a neat frame for the popup
            var contentFrame = new Frame
            {
                BackgroundColor = Colors.White,
                CornerRadius = 20,
                Padding = new Thickness(20),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Content = contentGrid
            };

            var popupFrame = new Frame
            {
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(10),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Content = contentFrame
            };

            // Set the popup content
            contentPopUp.Content = popupFrame;

            try
            {
                await MopupService.Instance.PushAsync(contentPopUp); // Show the popup
            }
            catch (Exception ex)
            {
                new { }.OpeErrorPopUp("Cannot open this page", ex.Message, "Ok");
            }
        }

        public static async void PushPageAsync(this object app, ContentPage page)
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(page);
            }
            catch (Exception ex)
            {
                new { }.OpeErrorPopUp("Cannot open this page", ex.Message, "Ok");
            }
        }

        public static async void CloseContentPopUp(this object obj)
        {
            await MopupService.Instance.PopAsync(); // Close popup
        }

        public static void OpeErrorPopUp(this object app, string title, string message, string closeText, Action execOnClick = null)
        {
            app.ClosePopUp();

            var popUp = new ContentPopUp(null);
            void tmp()
            {
                MopupService.Instance.PopAsync();
                execOnClick?.Invoke();
            }

            popUp.Content = new CustomErrorPopUp(title, message, closeText, tmp).Content;
            MopupService.Instance.PushAsync(popUp);
        }

        public static void OpeSuccessPopUp(this object app, string title, string message, string closeText, Action execOnClick = null)
        {
            app.ClosePopUp();

            var popUp = new ContentPopUp(null);
            void tmp()
            {
                MopupService.Instance.PopAsync();
                execOnClick?.Invoke();
            }

            popUp.Content = new CustomSuccessPopUp(title, message, closeText, tmp).Content;
            MopupService.Instance.PushAsync(popUp);
        }

        public static void ShowNotificationPopUp(this object source, Grid gridContent, Action tappedAction)
        {
            var popUp = new ContentPopUp(null);
            // Assuming you add notification-specific content here
            MopupService.Instance.PushAsync(popUp);
        }
    }
}
