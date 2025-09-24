using Newtonsoft.Json;
using PassingCar;
using PassingCar.IntegrationsWithApi;
using PassingCar.LocalDatabase;
using PassingCar.Models.API.Ads;
using PassingCar.Models.API;
using PassingCar.ViewModels;
using PassingCar.Views.Content;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace PassingCar.Extensions
{
    public static class Extensions
    {
        
        public static bool HasValues(this IEnumerable<object> enumerable)
        {
            return enumerable != null && enumerable.Count() > 0;
        }
        public class DatetimeToStringConverter : IValueConverter
        {
            #region IValueConverter implementation

            public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
            {
                if (value == null)
                {
                    return string.Empty;
                }

                DateTime datetime = (DateTime)value;
                if (datetime.Date == DateTime.Today)
                {
                    return $"Hoy {datetime.ToLocalTime():HH:mm}";
                }
                if (datetime.Date == DateTime.Today.AddDays(-1))
                {
                    return $"Ayer {datetime.ToLocalTime():HH:mm}";
                }
                if (datetime.Date.AddDays(7) > DateTime.Today)
                {
                    switch (datetime.DayOfWeek)
                    {
                        case DayOfWeek.Monday:
                            return $"Lunes {datetime.ToLocalTime():HH:mm}";
                        case DayOfWeek.Tuesday:
                            return $"Martes {datetime.ToLocalTime():HH:mm}";
                        case DayOfWeek.Wednesday:
                            return $"Miércoles {datetime.ToLocalTime():HH:mm}";
                        case DayOfWeek.Thursday:
                            return $"Jueves {datetime.ToLocalTime():HH:mm}";
                        case DayOfWeek.Friday:
                            return $"Viernes {datetime.ToLocalTime():HH:mm}";
                        case DayOfWeek.Saturday:
                            return $"Sábado {datetime.ToLocalTime():HH:mm}";
                        case DayOfWeek.Sunday:
                            return $"Domingo {datetime.ToLocalTime():HH:mm}";
                        default:
                            break;
                    }
                }
                //put your custom formatting here
                string datetimeString = (datetime.AddYears(1) > DateTime.Now) ? datetime.ToLocalTime().ToString($"dd/MM/ HH:mm") : datetime.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
                return datetimeString
                    .Replace("/01/", " Enero ")
                    .Replace("/02/", " Febrero ")
                    .Replace("/03/", " Marzo ")
                    .Replace("/04/", " Abril ")
                    .Replace("/05/", " Mayo ")
                    .Replace("/06/", " Junio ")
                    .Replace("/07/", " Julio ")
                    .Replace("/08/", " Agosto ")
                    .Replace("/09/", " Septiembre ")
                    .Replace("/10/", " Octubre ")
                    .Replace("/11/", " Noviembre ")
                    .Replace("/12/", " Diciembre ");
            }

            public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }

            #endregion
        }
        public static void AddHideGesture(this Grid parent, Image arrowImg, Grid content)
        {
            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                content.IsVisible = !content.IsVisible;
                arrowImg.Source = content.IsVisible
                    ? ImageSource.FromResource("PassingCar.Resources.Images.down_item_arrow.png")
                    : ImageSource.FromResource("PassingCar.Resources.Images.right_item_arrow.png");
            };
            parent.GestureRecognizers.Add(tap);
        }
        public static string GetMonth(this DateTime time)
        {
            switch (time.Month)
            {
                case 1:
                    return "Enero";
                case 2:
                    return "Febrero";
                case 3:
                    return "Marzo";
                case 4:
                    return "Abril";
                case 5:
                    return "Mayo";
                case 6:
                    return "Junio";
                case 7:
                    return "Julio";
                case 8:
                    return "Agosto";
                case 9:
                    return "Septiembre";
                case 10:
                    return "Octubre";
                case 11:
                    return "Noviembre";
                case 12:
                    return "Diciembre";
                default:
                    return string.Empty;
            }
        }
        public static async Task GoToAds(this object source, int adsId)
        {
            try
            {
                new { }.OpenPopUp();
                LocalAd item = await App.LocalDatabase.GetAdByID(adsId);
                if (item != null)
                {
                    AdsDetailsExtened adsDetailsExtened = new AdsDetailsExtened(false)
                    {
                        FirstAdsImage = item.GetFirstPhoto(),
                        AdsTitle = item.AdsTitle,
                        IsFavorite = item.IsFavorite,
                        AdsId = item.Id,
                        AdsFrom = item.AdsFrom,
                        AdsTo = item.AdsTo,
                        AdsPrice = item.AdsPrice,
                        State = item.State.ToString(),
                        UserProfilePhoto = item.GetUserProfilePhoto(),
                        UserName = item.Username,
                        UserRating = item.UserRating,
                        PostedTime = item.PostedTime,
                        UserProfile = item.UserProfile,
                        UserId = item.UserId,
                        ModifiedAt = item.ModifiedAt,
                    };

                    SingleAdsViewModel modelBinding = new SingleAdsViewModel(adsDetailsExtened);
                    await modelBinding.AsyncLoad();
                    SingleAdsPage singleAdsPage = new SingleAdsPage(modelBinding);

                    new { }.ClosePopUp();
                    await App.Current.MainPage.Navigation.PushAsync(singleAdsPage);
                }
                else
                {
                    // Ad not found in local database - fetch from API
                    System.Diagnostics.Debug.WriteLine($"[GoToAds] Ad {adsId} not found in local database, fetching from API...");
                    
                    // Try to get ad details from GetAllUsersAds API
                    var allUsersAdsResponse = await Api.GetAllUsersAds();
                    
                    if (allUsersAdsResponse != null && allUsersAdsResponse.Success && allUsersAdsResponse.AdsItem != null)
                    {
                        var adFromApi = allUsersAdsResponse.AdsItem.FirstOrDefault(ad => ad.AdsId == adsId);
                        
                        if (adFromApi != null)
                        {
                            // Create AdsDetailsExtened from API response
                            var adsDetailsExtened = new AdsDetailsExtened(false)
                            {
                                FirstAdsImage = adFromApi.FirstAdsImage,
                                AdsTitle = adFromApi.AdsTitle,
                                IsFavorite = adFromApi.IsFavorite,
                                AdsId = adFromApi.AdsId,
                                AdsFrom = adFromApi.AdsFrom,
                                AdsTo = adFromApi.AdsTo,
                                AdsPrice = adFromApi.AdsPrice,
                                State = adFromApi.State.Espana(),
                                UserProfilePhoto = adFromApi.UserProfilePhoto,
                                UserName = adFromApi.UserName,
                                UserRating = adFromApi.UserRating,
                                PostedTime = adFromApi.PostedTime,
                                UserProfile = adFromApi.UserProfile,
                                UserId = adFromApi.UserId,
                                ModifiedAt = adFromApi.ModifiedAt,
                            };

                            SingleAdsViewModel modelBinding = new SingleAdsViewModel(adsDetailsExtened);
                            await modelBinding.AsyncLoad();
                            SingleAdsPage singleAdsPage = new SingleAdsPage(modelBinding);

                            new { }.ClosePopUp();
                            await App.Current.MainPage.Navigation.PushAsync(singleAdsPage);
                        }
                        else
                        {
                            new { }.ClosePopUp();
                            await Task.Delay(500);
                            new { }.OpeErrorPopUp($"Ad not found", $"This ad is no longer available", $"Ok");
                        }
                    }
                    else
                    {
                        new { }.ClosePopUp();
                        await Task.Delay(500);
                        new { }.OpeErrorPopUp($"Unable to get data for this Ads", $"Inténtalo de nuevo", $"Ok");
                    }
                }
            }
            catch (Exception ex)
            {
                _ = await Api.HandleException(new Models.API.HandleExceptionInput()
                {
                    Exception = JsonConvert.SerializeObject(ex)
                });
                new { }.ClosePopUp();
                await Task.Delay(500);
                new { }.OpeErrorPopUp($"Unable to get data for this Ads", $"Inténtalo de nuevo", $"Ok");
            }
        }
        public static bool Handle(this Exception ex)
        {
            try
            {
                // Log exception details for debugging
                System.Diagnostics.Debug.WriteLine($"[EXCEPTION HANDLER] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[EXCEPTION HANDLER] Stack Trace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"[EXCEPTION HANDLER] Source: {ex.Source}");
                
                // Get caller information for better debugging
                StackTrace stackTrace = new StackTrace();
                var frame = stackTrace.GetFrame(1);
                var method = frame?.GetMethod();
                var className = method?.DeclaringType?.Name ?? "Unknown";
                var methodName = method?.Name ?? "Unknown";
                
                System.Diagnostics.Debug.WriteLine($"[EXCEPTION HANDLER] Called from: {className}.{methodName}");
                
                // Send exception to API for tracking (in background, don't block UI)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        _ = await Api.HandleException(new Models.API.HandleExceptionInput()
                        {
                            Exception = JsonConvert.SerializeObject(new 
                            { 
                                ex, 
                                stName = className, 
                                stMet = methodName,
                                timestamp = DateTime.UtcNow
                            })
                        });
                    }
                    catch
                    {
                        // If API call fails, just continue - don't cause more exceptions
                        System.Diagnostics.Debug.WriteLine("[EXCEPTION HANDLER] Failed to send exception to API");
                    }
                });
                
                // For critical exceptions, show user notification
                if (IsCriticalException(ex))
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        try
                        {
                            if (Application.Current?.MainPage != null)
                            {
                                await Application.Current.MainPage.DisplayAlert(
                                    "Error", 
                                    "An error occurred. Please try again.", 
                                    "OK");
                            }
                        }
                        catch
                        {
                            // If we can't show alert, just continue
                        }
                    });
                }
                
                return true;
            }
            catch
            {
                // If exception handling itself fails, just log and return
                System.Diagnostics.Debug.WriteLine("[EXCEPTION HANDLER] Critical error in exception handler");
                return false;
            }
        }
        
        private static bool IsCriticalException(Exception ex)
        {
            // Determine if this is a critical exception that should show user notification
            return ex is NullReferenceException || 
                   ex is ArgumentNullException || 
                   ex is InvalidOperationException ||
                   ex is UnauthorizedAccessException ||
                   ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase);
        }
        private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("PassingCarKeyEncryptDevelop");
        private static readonly ThreadLocal<HMACSHA256> Algorithm = new(() => new HMACSHA256(EncryptionKey));

        public static string EncryptString(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashedBytes = Algorithm.Value.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes);
        }
    }
    public class DatetimeToStringConverter : IValueConverter
    {
        #region IValueConverter implementation

        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            DateTime datetime = (DateTime)value;
            if (datetime.Date == DateTime.Today)
            {
                return $"Hoy {datetime.ToLocalTime():HH:mm}";
            }
            if (datetime.Date == DateTime.Today.AddDays(-1))
            {
                return $"Ayer {datetime.ToLocalTime():HH:mm}";
            }
            if (datetime.Date.AddDays(7) > DateTime.Today)
            {
                switch (datetime.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        return $"Lunes {datetime.ToLocalTime():HH:mm}";
                    case DayOfWeek.Tuesday:
                        return $"Martes {datetime.ToLocalTime():HH:mm}";
                    case DayOfWeek.Wednesday:
                        return $"Miércoles {datetime.ToLocalTime():HH:mm}";
                    case DayOfWeek.Thursday:
                        return $"Jueves {datetime.ToLocalTime():HH:mm}";
                    case DayOfWeek.Friday:
                        return $"Viernes {datetime.ToLocalTime():HH:mm}";
                    case DayOfWeek.Saturday:
                        return $"Sábado {datetime.ToLocalTime():HH:mm}";
                    case DayOfWeek.Sunday:
                        return $"Domingo {datetime.ToLocalTime():HH:mm}";
                    default:
                        break;
                }
            }
            //put your custom formatting here
            string datetimeString = (datetime.AddYears(1) > DateTime.Now) ? datetime.ToLocalTime().ToString($"dd/MM/ HH:mm") : datetime.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            return datetimeString
                .Replace("/01/", " Enero ")
                .Replace("/02/", " Febrero ")
                .Replace("/03/", " Marzo ")
                .Replace("/04/", " Abril ")
                .Replace("/05/", " Mayo ")
                .Replace("/06/", " Junio ")
                .Replace("/07/", " Julio ")
                .Replace("/08/", " Agosto ")
                .Replace("/09/", " Septiembre ")
                .Replace("/10/", " Octubre ")
                .Replace("/11/", " Noviembre ")
                .Replace("/12/", " Diciembre ");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
