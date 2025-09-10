
using MauiEx;
using Newtonsoft.Json;
using PassingCar.Extensions;
using PassingCar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

using static PassingCar.Models.Address;

namespace PassingCar.ViewModels
{
    public class GooglePlacesGetData : BaseViewModel, INotifyPropertyChanged
    {
        private List<string> adr = new List<string>();
        private static HttpClient _httpClientInstance;
        public static HttpClient HttpClientInstance => _httpClientInstance ?? (_httpClientInstance = new HttpClient());

        public const string GooglePlacesApiAutoCompletePath = "https://maps.googleapis.com/maps/api/place/autocomplete/json?key={0}&input={1}&components=country:es"; //Adding country:us limits results to es
        public const string GooglePlacesApiKey = "AIzaSyClobyfPOyAcYe5i8pNIwRhTlKAPr4BO5Q";
        public const string GooglePlacesApiPlaceDetails = "https://maps.googleapis.com/maps/api/place/details/json?key={0}&placeid={1}"; //Adding

        public string placeId = "";
        public string PostalCode = "";
        public string Lat = "";
        public string Lng = "";
        public string Maps_Url = "";

        private string _addressText;

        public string AddressText
        {
            get => _addressText;
            set
            {
                try
                {
                    //handle exception
                    if (_addressText != value)
                    {
                        _addressText = value;
                        OnPropertyChanged();
                    }
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                }
            }
        }

        public async Task GetPlacesPredictionsAsync(string text, AutoSuggestBox sender)
        {
            try
            {
                //handle exception
                placeId = "";
                PostalCode = "";
                Lat = "";
                Lng = "";
                Maps_Url = "";
                // TODO: Add throttle logic, Google begins denying requests if too many are made in a short amount of time

                CancellationToken cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(2)).Token;

                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, string.Format(GooglePlacesApiAutoCompletePath, GooglePlacesApiKey, WebUtility.UrlEncode(text))))
                { //Be sure to UrlEncode the search term they enter
                    Console.WriteLine(request.ToString());
                    using (HttpResponseMessage message = await HttpClientInstance.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false))
                    {
                        if (message.IsSuccessStatusCode)
                        {
                            string json = await message.Content.ReadAsStringAsync().ConfigureAwait(false);

                            PlacesLocationPredictions predictionList = await Task.Run(() => JsonConvert.DeserializeObject<PlacesLocationPredictions>(json)).ConfigureAwait(false);

                            if (predictionList.Status == "OK")
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {



                                    adr = null;
                                    adr = new List<string>();
                                    if (predictionList.Predictions.Count > 0)
                                    {
                                        foreach (Prediction prediction in predictionList.Predictions)
                                        {
                                            //adr.Add(prediction.Description);                                       
                                            placeId = prediction.PlaceId;
                                            //ViewModel.Addresses = ViewModel.Addresses;
                                            adr.Add(prediction.Description);
                                        }
                                        sender.ItemsSource = adr;
                                    }
                                });
                            }
                            else
                            {
                                //throw new Exception(predictionList.Status);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public async Task GetDataAsync()
        {
            CancellationToken cancellationToken2 = new CancellationTokenSource(TimeSpan.FromMinutes(2)).Token;

            try
            {
                //handle exception
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, string.Format(GooglePlacesApiPlaceDetails, GooglePlacesApiKey, WebUtility.UrlEncode(placeId))))
                { //Be sure to UrlEncode the search term they enter
                    Console.WriteLine(request.ToString());
                    using (HttpResponseMessage message = await HttpClientInstance.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken2).ConfigureAwait(false))
                    {
                        if (message.IsSuccessStatusCode)
                        {
                            string json = await message.Content.ReadAsStringAsync().ConfigureAwait(false);

                            PlacesLocationDetails predictionList = await Task.Run(() => JsonConvert.DeserializeObject<PlacesLocationDetails>(json)).ConfigureAwait(false);

                            if (predictionList.Status == "OK")
                            {

                                //Addresses.Clear();

                                //adr.Clear();
                                if (predictionList.Result.Address_Components.Count > 0)
                                {
                                    foreach (Address.Components prediction in predictionList.Result.Address_Components)
                                    {
                                        foreach (string comp in prediction.Types)
                                        {
                                            if (comp == "postal_code")
                                            {
                                                PostalCode = prediction.LongName;
                                            }
                                        }
                                        //placeId = prediction.PlaceId;

                                    }
                                    //search_box.ItemsSource = adr;
                                    Maps_Url = predictionList.Result.Url;
                                    Lat = predictionList.Result.Geometry.Location.Lat.ToString();
                                    Lng = predictionList.Result.Geometry.Location.Lng.ToString();
                                }

                                //if (Device.RuntimePlatform == Device.iOS)
                                //{
                                // https://developer.apple.com/library/ios/featuredarticles/iPhoneURLScheme_Reference/MapLinks/MapLinks.html
                                //    await Launcher.OpenAsync("http://maps.apple.com/?q=394+Pacific+Ave+San+Francisco+CA");
                                //}
                                //else if (Device.RuntimePlatform == Device.Android)
                                //{
                                // open the maps app directly
                                //   await Launcher.OpenAsync("http://maps.google.com/maps?z=12&t=m&q=loc:" + Lat + "+" + Lng);
                                //   await Launcher.OpenAsync(Maps_Url);
                                //}
                            }
                            else
                            {
                                //throw new Exception(predictionList.Status);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
    }
}
