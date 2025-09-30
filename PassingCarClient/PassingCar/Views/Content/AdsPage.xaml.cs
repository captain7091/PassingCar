using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API.Ads;
using PassingCar.Utils;
using PassingCar.ViewModels;
using PassingCar.Views.Content;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PassingCar.Views
{
    public partial class AdsPage : ContentPage, IDisposable
    {
        //public ICommand ForgotPasswordCommand { get; }
        public ListAdsViewModel AdsLibrary { get; }
        public bool Favorites { get; set; }
        private int check_if_enable = 0;
        private AdsFilter _adsFilter;

        public AdsPage()
        {
            try
            {
                InitializeComponent();
                AdsLibrary = new ListAdsViewModel(SmallLoading, navigation: Navigation);
                BindingContext = AdsLibrary;
               
                List<string> province_pick_from = new List<string>();
                List<string> province_pick_to = new List<string>();
                province_pick_from.Add("A Coruña");
                province_pick_to.Add("A Coruña");
                province_pick_from.Add("Álava");
                province_pick_to.Add("Álava");
                province_pick_from.Add("Albacete");
                province_pick_to.Add("Albacete");
                province_pick_from.Add("Alicante");
                province_pick_to.Add("Alicante");
                province_pick_from.Add("Almería");
                province_pick_to.Add("Almería");
                province_pick_from.Add("Asturias");
                province_pick_to.Add("Asturias");
                province_pick_from.Add("Ávila");
                province_pick_to.Add("Ávila");
                province_pick_from.Add("Badajoz");
                province_pick_to.Add("Badajoz");
                province_pick_from.Add("Islas Baleares");
                province_pick_to.Add("Islas Baleares");
                province_pick_from.Add("Barcelona");
                province_pick_to.Add("Barcelona");
                province_pick_from.Add("Vizcaya");
                province_pick_to.Add("Vizcaya");
                province_pick_from.Add("Burgos");
                province_pick_to.Add("Burgos");
                province_pick_from.Add("Cáceres");
                province_pick_to.Add("Cáceres");
                province_pick_from.Add("Cádiz");
                province_pick_to.Add("Cádiz");
                province_pick_from.Add("Cantabria");
                province_pick_to.Add("Cantabria");
                province_pick_from.Add("Castellón");
                province_pick_to.Add("Castellón");
                province_pick_from.Add("Ciudad Real");
                province_pick_to.Add("Ciudad Real");
                province_pick_from.Add("Córdoba");
                province_pick_to.Add("Córdoba");
                province_pick_from.Add("Cuenca");
                province_pick_to.Add("Cuenca");
                province_pick_from.Add("Guipúzcoa");
                province_pick_to.Add("Guipúzcoa");
                province_pick_from.Add("Gerona");
                province_pick_to.Add("Gerona");
                province_pick_from.Add("Granada");
                province_pick_to.Add("Granada");
                province_pick_from.Add("Guadalajara");
                province_pick_to.Add("Guadalajara");
                province_pick_from.Add("Huelva");
                province_pick_to.Add("Huelva");
                province_pick_from.Add("Huesca");
                province_pick_to.Add("Huesca");
                province_pick_from.Add("Jaén");
                province_pick_to.Add("Jaén");
                province_pick_from.Add("La Rioja");
                province_pick_to.Add("La Rioja");
                province_pick_from.Add("Las Palmas");
                province_pick_to.Add("Las Palmas");
                province_pick_from.Add("León");
                province_pick_to.Add("León");
                province_pick_from.Add("Lérida");
                province_pick_to.Add("Lérida");
                province_pick_from.Add("Lugo");
                province_pick_to.Add("Lugo");
                province_pick_from.Add("Madrid");
                province_pick_to.Add("Madrid");
                province_pick_from.Add("Málaga");
                province_pick_to.Add("Málaga");
                province_pick_from.Add("Murcia");
                province_pick_to.Add("Murcia");
                province_pick_from.Add("Navarra");
                province_pick_to.Add("Navarra");
                province_pick_from.Add("Orense");
                province_pick_to.Add("Orense");
                province_pick_from.Add("Palencia");
                province_pick_to.Add("Palencia");
                province_pick_from.Add("Pontevedra");
                province_pick_to.Add("Pontevedra");
                province_pick_from.Add("Salamanca");
                province_pick_to.Add("Salamanca");
                province_pick_from.Add("Santa Cruz de Tenerife");
                province_pick_to.Add("Santa Cruz de Tenerife");
                province_pick_from.Add("Segovia");
                province_pick_to.Add("Segovia");
                province_pick_from.Add("Sevilla");
                province_pick_to.Add("Sevilla");
                province_pick_from.Add("Soria");
                province_pick_to.Add("Soria");
                province_pick_from.Add("Tarragona");
                province_pick_to.Add("Tarragona");
                province_pick_from.Add("Teruel");
                province_pick_to.Add("Teruel");
                province_pick_from.Add("Toledo");
                province_pick_to.Add("Toledo");
                province_pick_from.Add("Valencia");
                province_pick_to.Add("Valencia");
                province_pick_from.Add("Valladolid");
                province_pick_to.Add("Valladolid");
                province_pick_from.Add("Zamora");
                province_pick_to.Add("Zamora");
                province_pick_from.Add("Zaragoza");
                province_pick_to.Add("Zaragoza");
                provincie_picker_from.ItemsSource = province_pick_from;
                provincie_picker_to.ItemsSource = province_pick_from;
                fromLastXDays_picker.ItemsSource = new List<string>()
            {
                "Hoy",
                "Los últimos 7 días",
                "Los últimos 30 días",
            };
                relevance_picker.ItemsSource = new List<string>()
            {
                "El más reciente",
                "El más antiguo",
                "El más caro",
                "El más barato"
            };

                // Add price range picker for enhanced filtering
                price_range_picker.ItemsSource = new List<string>()
            {
                "Cualquier precio",
                "0€ - 50€",
                "50€ - 100€",
                "100€ - 200€",
                "200€ - 500€",
                "500€+"
            };

                _adsFilter = new AdsFilter()
                {
                    OnlyOwnAds = false,
                    OnlyFavorites = false,
                    Active = true,
                    ShowAllUsers = true // Show ads from all users for Anuncios module
                };
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
                SmallLoading.IsVisible = true;
                AdsLibrary.KeepLoading = true;
                
                // Use new lightweight API for Anuncios screen
                await LoadAnunciosAds();
                //base.OnAppearing();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        protected override void OnDisappearing()
        {
            try
            {
                AdsLibrary.KeepLoading = false;
                base.OnDisappearing();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async Task LoadAnunciosAds()
        {
            try
            {
                
                // Call new lightweight API
                GetNextAdsResponse response = await Api.GetAllUsersAds();
                
                if (response?.Success == true && response.AdsItem != null && response.AdsItem.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[AdsPage] Received {response.AdsItem.Count()} ads from GetAllUsersAds API");
                    
                    // Clear existing ads
                    AdsLibrary.Adss.Clear();
                    AdsLibrary.AllAds.Clear();
                    
                    // Convert API response to local ads format
                    foreach (var item in response.AdsItem)
                    {
                        AdsDetailsExtened adsDetails = new AdsDetailsExtened(false)
                        {
                            FirstAdsImage = item.FirstAdsImage,
                            AdsTitle = item.AdsTitle,
                            IsFavorite = item.IsFavorite,
                            AdsId = item.AdsId,
                            AdsFrom = item.AdsFrom,
                            AdsTo = item.AdsTo,
                            AdsPrice = item.AdsPrice,
                            State = item.State.Espana(),
                            UserProfilePhoto = item.UserProfilePhoto,
                            UserProfile = item.UserProfile,
                            UserName = item.UserName,
                            UserRating = item.UserRating,
                            PostedTime = item.PostedTime,
                            UserId = item.UserId,
                            ModifiedAt = item.ModifiedAt,
                        };
                        
                        // DEBUG: Log each ad's state for Anuncios screen
                        try
                        {
                            var stateValue = adsDetails.State.ReplaceEspana();
                            var adsState = (AdsState)Enum.Parse(typeof(AdsState), stateValue);
                            var stateInt = (int)adsState;
                            System.Diagnostics.Debug.WriteLine($"[AdsPage] Anuncios - Ad {adsDetails.AdsId}: State='{adsDetails.State}' -> Parsed='{stateValue}' -> StateInt={stateInt}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[AdsPage] Anuncios - Error parsing state for ad {adsDetails.AdsId}: {ex.Message}");
                        }
                        
                        AdsLibrary.Adss.Add(adsDetails);
                        AdsLibrary.AllAds.Add(adsDetails);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"[AdsPage] Successfully loaded {AdsLibrary.Adss.Count} ads into UI");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[AdsPage] No ads received or API failed: {response?.ErrorMessage}");
                    AdsLibrary.Adss.Clear();
                    AdsLibrary.AllAds.Clear();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdsPage] Error loading Anuncios ads: {ex.Message}");
                _ = ex.Handle();
            }
            finally
            {
                SmallLoading.IsVisible = false;
            }
        }




        private ViewCell lastCell;
        private void ViewCell_Tapped(object sender, System.EventArgs e)
        {
            try
            {
                if (lastCell != null)
                {
                    //lastCell.View.BackgroundColor = Color.Transparent;
                }

                ViewCell viewCell = (ViewCell)sender;
                if (viewCell.View != null)
                {
                    //viewCell.View.BackgroundColor = Color.Transparent;
                    lastCell = viewCell;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void BackMainMenu(object obj)
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(MainMenu));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        //protected void AsyncLoad()
        //{
        //    try
        //    {
        //        AdsLibrary.KeepSync();
        //    }
        //    catch (Exception ex)
        //    {
        //        _ = ex.Handle();
        //    }
        //}
        private async void OnItemSelected(object sender, ItemTappedEventArgs e)
        {
            try
            {
                // Add null checks for event args and item
                if (e?.Item == null)
                    return;

                AdsDetailsExtened mydetails = e.Item as AdsDetailsExtened;
                if (mydetails == null)
                    return;

                SingleAdsViewModel modelBinding = new SingleAdsViewModel(mydetails);
                await modelBinding.AsyncLoad();
                SingleAdsPage singleAdsPage = new SingleAdsPage(modelBinding);
                
                // Increment view counter without blocking navigation
                _ = Task.Run(async () =>
                {
                    try
                    {
                        _ = await Api.IncrementViewCounter(new IncrementViewCounterInput()
                        {
                            AdsId = mydetails.AdsId
                        });
                    }
                    catch (Exception ex)
                    {
                        _ = ex.Handle();
                    }
                });

                // Ensure navigation happens on main thread
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Navigation.PushAsync(singleAdsPage);
                    this.ClosePopUp();
                });
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                System.Diagnostics.Debug.WriteLine($"[AdsPage] Error in OnItemSelected: {ex.Message}");
            }
        }

        public void ScrollUp(object sender, EventArgs e)
        {
            try
            {
                //offers_listview.ScrollTo(AdsLibrary, ScrollToPosition.Start, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void Backward(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync($"//HomePage");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            try
            {
                GoHome();
                return true;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }

        public async void GoHome()
        {
            try
            {
                await Shell.Current.GoToAsync($"//HomePage");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

       

        [Obsolete]
        private async void add_ads(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.Navigation.PushAsync(new AddAds("AdsPage"));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void ResetClicked(object sender, EventArgs e)
        {
            try
            {
                _adsFilter = new AdsFilter()
                {
                    OnlyOwnAds = false,
                    OnlyFavorites = false,
                    Active = true,
                    ShowAllUsers = true
                };
                provincie_picker_from.SelectedIndex = -1;
                provincie_picker_to.SelectedIndex = -1;
                fromLastXDays_picker.SelectedIndex = -1;
                relevance_picker.SelectedIndex = -1;
                price_range_picker.SelectedIndex = -1;
                this.OpenPopUp();
                AdsLibrary.Adss.Clear();
                
                // Use lightweight API for reset
                await LoadAnunciosAds();
                this.ClosePopUp();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void ApplyClicked(object sender, EventArgs e)
        {
            try
            {
                _adsFilter.ProvincieFrom = provincie_picker_from.SelectedItem != null ? provincie_picker_from.SelectedItem.ToString() : string.Empty;
                _adsFilter.ProvincieTo = provincie_picker_to.SelectedItem != null ? provincie_picker_to.SelectedItem.ToString() : string.Empty;

                // Convert string selection to integer for FromLastXDays
                var selectedDays = fromLastXDays_picker.SelectedItem?.ToString();
                _adsFilter.FromLastXDays = selectedDays switch
                {
                    "Hoy" => 1,
                    "Los últimos 7 días" => 7,
                    "Los últimos 30 días" => 30,
                    _ => 0
                };

                _adsFilter.Relevance = relevance_picker.SelectedItem != null ? relevance_picker.SelectedItem.ToString() : string.Empty;

                // Handle price range filtering
                var priceRange = price_range_picker.SelectedItem?.ToString();
                _adsFilter.PriceRange = priceRange;
                SetPriceRangeFilter(_adsFilter, priceRange);

                this.OpenPopUp();
                AdsLibrary.Adss.Clear();
                
                // Use lightweight API for apply filters
                await LoadAnunciosAds();
                this.ClosePopUp();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void SetPriceRangeFilter(AdsFilter filter, string priceRange)
        {
            switch (priceRange)
            {
                case "0€ - 50€":
                    filter.MinPrice = 0;
                    filter.MaxPrice = 50;
                    break;
                case "50€ - 100€":
                    filter.MinPrice = 50;
                    filter.MaxPrice = 100;
                    break;
                case "100€ - 200€":
                    filter.MinPrice = 100;
                    filter.MaxPrice = 200;
                    break;
                case "200€ - 500€":
                    filter.MinPrice = 200;
                    filter.MaxPrice = 500;
                    break;
                case "500€+":
                    filter.MinPrice = 500;
                    filter.MaxPrice = null;
                    break;
                default:
                    filter.MinPrice = null;
                    filter.MaxPrice = null;
                    break;
            }
        }

        void collectionview_ads_Scrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            try
            {
                // Add null checks for event args and UI elements
                if (e == null || hello_context == null || menu_context == null || grid_context == null)
                    return;
               
                // Handling for iOS
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        hello_context.TextColor = Color.FromRgb(180, 180, 177);
                        menu_context.Source = "back_arrow.png";
                        grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    });
                    check_if_enable = 0;
                    return; // Exit early for iOS-specific behavior
                }

                // Check the center item index and update the UI accordingly
                if (e.FirstVisibleItemIndex == 0)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        hello_context.TextColor = Color.FromRgb(180, 180, 177);
                        menu_context.Source = "back_arrow.png";
                        grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    });
                    check_if_enable = 0;
                }

                // When center item is the 5th, adjust UI elements
                if (e.CenterItemIndex == 5)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        hello_context.TextColor = Colors.White;
                        if (check_if_enable == 0)
                        {
                            menu_context.Source = "back_arrow_white.png"; // Change the arrow image
                        }
                        grid_context.BackgroundColor = Color.FromRgb(0, 0, 0); // Change the background
                    });
                    check_if_enable = 1;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions with proper error handling
                _ = ex.Handle();
                System.Diagnostics.Debug.WriteLine($"[AdsPage] Error in collection view scrolling: {ex.Message}");
            }
        }

       
        private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            try
            {
                // Retrieve the tapped item from the sender
                var tappedFrame = sender as NeatFrame; // Assuming NeatFrame is the frame you're tapping
                var mydetails = tappedFrame?.BindingContext as AdsDetailsExtened;

                if (mydetails != null)
                {
                    // Load the details and initialize the SingleAdsViewModel
                    SingleAdsViewModel modelBinding = new SingleAdsViewModel(mydetails);
                    await modelBinding.AsyncLoad();

                    // Create a new instance of the SingleAdsPage and pass the ViewModel
                    SingleAdsPage singleAdsPage = new SingleAdsPage(modelBinding);

                    // Increment the view counter for the selected ad
                    _ = await Api.IncrementViewCounter(new IncrementViewCounterInput()
                    {
                        AdsId = mydetails.AdsId
                    });

                    // Navigate to the SingleAdsPage
                    await Navigation.PushAsync(singleAdsPage);

                    // Optionally, close the pop-up if needed
                    this.ClosePopUp();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        /// <summary>
        /// Quick filter for carrier-specific ads (all users)
        /// </summary>
        private async void ShowAllAdsClicked(object sender, EventArgs e)
        {
            try
            {
                _adsFilter = new AdsFilter()
                {
                    OnlyOwnAds = false,
                    OnlyFavorites = false,
                    Active = true,
                    ShowAllUsers = true
                };

                this.OpenPopUp();
                AdsLibrary.Adss.Clear();
                await AdsLibrary.CustomLoad(_adsFilter);
                this.ClosePopUp();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        public void Dispose()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[AdsPage] Disposing resources...");
                
                // Dispose ViewModel if it implements IDisposable
                if (AdsLibrary is IDisposable disposableViewModel)
                {
                    disposableViewModel.Dispose();
                    System.Diagnostics.Debug.WriteLine("[AdsPage] AdsLibrary disposed");
                }
                
                System.Diagnostics.Debug.WriteLine("[AdsPage] Resources disposed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdsPage] Error during disposal: {ex.Message}");
                _ = ex.Handle();
            }
        }

    }
}
