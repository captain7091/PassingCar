using CommunityToolkit.Maui.Extensions;
using PassingCar.Extensions;
using PassingCar.Hubs;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API.Ads;
using PassingCar.ViewModels;
using PassingCar.Views.Intitial;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


namespace PassingCar.Views
{
    public partial class MyShippingPage : ContentPage, IDisposable
    {
        public int check_if_enable = 0;
        public ListShippingsViewModel ShippingsLibrary { get; }
        public View ContentPage { get; set; }
        private bool isDataLoaded = false; // Track if data has been loaded to prevent unnecessary reloads

        public MyShippingPage()
        {
            try
            {
                InitializeComponent();
                ShippingsLibrary = new ListShippingsViewModel(SmallLoading);
                BindingContext = ShippingsLibrary;
                ContentPage = myshipping_page;
                AsyncLoad();
              
                PassingCarHubs.UpdateEvent(PassingCarHubs.Ads.ShippingStateChanged, async (input) =>
                {
                    ChangesShippingStateInput item = input as ChangesShippingStateInput;
                    if (item != null)
                    {
                        // Find the specific shipping item and update only that item instead of reloading everything
                        var shippingToUpdate = ShippingsLibrary.Shippingss.FirstOrDefault(s => s.Id == item.ShippingId);
                        if (shippingToUpdate != null)
                        {
                            // Update only the specific item's state to avoid full reload and prevent ads from disappearing
                            shippingToUpdate.State = item.State;
                        }
                    }
                });
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
                // Set initial tab state (Mis Ofertas is default active)
                Publicados.BackgroundColor = Color.FromHex("#fe3f40");
                En_Ruta.BackgroundColor = Color.FromHex("#c3c3c3");
                Enviados.BackgroundColor = Color.FromHex("#c3c3c3");
                Publicados2.BackgroundColor = Color.FromHex("#fe3f40");
                En_Ruta2.BackgroundColor = Color.FromHex("#c3c3c3");
                Enviados2.BackgroundColor = Color.FromHex("#c3c3c3");
                
                // AUTO-LOAD Mis Ofertas tab data when screen appears (since it's the default active tab)
                await LoadMisOfertasTabData();
                
                base.OnAppearing();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            // Update tab button colors for both CollectionViews
            this.En_Ruta.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            Enviados.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            await Publicados.BackgroundColorTo(Color.FromHex("#fe3f40"));
            
            // Update tab button colors for collectionview_ads
            this.En_Ruta2.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            Enviados2.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            await Publicados2.BackgroundColorTo(Color.FromHex("#fe3f40"));

            // Show loading spinner
            SmallLoading.IsVisible = true;

            // Load Mis Ofertas tab data
            await LoadMisOfertasTabData();
            
            // Hide loading spinner after data is loaded
            SmallLoading.IsVisible = false;
        }

        private async Task LoadMisOfertasTabData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MyShippingPage] LoadMisOfertasTabData called - loading offers for sender");
                
                // Call GetMyOffersAds API (returns complete ad details)
                GetNextAdsResponse response = await Api.GetMyOffersAds();
                
                if (response != null && response.Success && response.AdsItem != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[MyShippingPage] GetMyOffersAds API returned {response.AdsItem.Count()} ads with offers for Mis Ofertas tab");
                    
                    // Create a temporary loading control
                    var tempLoading = new LoadingSmall();
                    
                    // Create a new ListAdsViewModel for offers display (uses AdsDetailsExtened which matches XAML)
                    var adsViewModel = new ListAdsViewModel(tempLoading);
                    
                    // Clear existing ads
                    adsViewModel.Adss.Clear();
                    
                    // Convert API response to AdsDetailsExtened and populate the UI
                    foreach (var item in response.AdsItem)
                    {
                        System.Diagnostics.Debug.WriteLine($"[MyShippingPage] Processing ad with offers: {item.AdsTitle}, AdsId: {item.AdsId}");
                        
                        var adDetails = new AdsDetailsExtened(false)
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
                            UserName = item.UserName,
                            UserRating = item.UserRating,
                            PostedTime = item.PostedTime,
                            UserId = item.UserId,
                            UserProfile = item.UserProfile,
                            ModifiedAt = item.ModifiedAt,
                        };
                        
                        adsViewModel.Adss.Add(adDetails);
                        
                        System.Diagnostics.Debug.WriteLine($"[MyShippingPage] Added ad to viewmodel: {adDetails.AdsTitle}, Count: {adsViewModel.Adss.Count}");
                    }
                    
                    // Set the binding context to show ads in collectionview_ads
                    collectionview_ads.BindingContext = adsViewModel;
                    collectionview_ads.IsVisible = true;
                    
                    // Keep collectionview_shippings visible (for tabs) but empty
                    ShippingsLibrary.Shippingss.Clear();
                    collectionview_shippings.IsVisible = true;
                    Entregados.IsVisible = false;
                    
                    System.Diagnostics.Debug.WriteLine($"[MyShippingPage] Successfully loaded {adsViewModel.Adss.Count} ads with offers into Mis Ofertas tab");
                    System.Diagnostics.Debug.WriteLine($"[MyShippingPage] CollectionView_ads Visible: {collectionview_ads.IsVisible}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[MyShippingPage] GetMyOffersAds API failed: Success={response?.Success}, Error='{response?.ErrorMessage}'");
                    // Show empty state or error message
                    collectionview_ads.IsVisible = true;
                    ShippingsLibrary.Shippingss.Clear();
                    collectionview_shippings.IsVisible = true;
                    Entregados.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MyShippingPage] Error loading offers for Mis Ofertas tab: {ex.Message}");
                _ = ex.Handle();
                collectionview_ads.IsVisible = true;
                ShippingsLibrary.Shippingss.Clear();
                collectionview_shippings.IsVisible = true;
                Entregados.IsVisible = false;
            }
        }

        private async Task LoadEnRutaTabData()
        {
            try
            {
                // Load shipping items instead of ads for En Ruta tab
                await ShippingsLibrary.CustomLoad();
                
                // FIXED: Ensure collectionview_shippings is visible (BindingContext already set in constructor)
                collectionview_shippings.IsVisible = true;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                collectionview_ads.IsVisible = true;
            }
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            // Update tab button colors for both CollectionViews
            Publicados.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            Enviados.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            await En_Ruta.BackgroundColorTo(Color.FromHex("#fe3f40"));
            
            // Update tab button colors for collectionview_ads
            Publicados2.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            Enviados2.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            await En_Ruta2.BackgroundColorTo(Color.FromHex("#fe3f40"));
         
            // Show shipping items for En Ruta tab (not ads)
            collectionview_shippings.IsVisible = true;
            collectionview_ads.IsVisible = false;
            Entregados.IsVisible = false;
            
            // Ensure proper binding context
            if (collectionview_shippings.BindingContext == null)
            {
                collectionview_shippings.BindingContext = ShippingsLibrary;
            }

            // Show loading spinner
            SmallLoading.IsVisible = true;

            // FIXED: Load driver's offers with payment completed (state >= 3) for En Ruta tab
            await LoadEnRutaTabData();
            
            // Hide loading spinner after data is loaded
            SmallLoading.IsVisible = false;
        }



        private async void Button_Clicked_2(object sender, EventArgs e)
        {
            // Update tab button colors for both CollectionViews
            Publicados.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            En_Ruta.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            await Enviados.BackgroundColorTo(Color.FromHex("#fe3f40"));
            
            // Update tab button colors for collectionview_ads
            Publicados2.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            En_Ruta2.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            await Enviados2.BackgroundColorTo(Color.FromHex("#fe3f40"));
          

            // Show loading spinner
            SmallLoading.IsVisible = true;

            try
            {
                // Create a temporary loading control for Entregados
                var tempLoading = new LoadingSmall();
                
                Entregados.BindingContext = new ListAdsViewModel(tempLoading);

                var bind = (Entregados.BindingContext as ListAdsViewModel);

                bind.Adss.Clear();

                await bind.CustomLoad(new AdsFilter()
                {
                    OnlyOwnAds = true,
                    OnlyFavorites = false,
                    Active = false,
                });

                var alladds = bind.AllAds;

                var username = await Api.GetUserData();
                var s = alladds.Where(x => x.State == "Entregado" && x.UserName == username.Name).ToList();

                bind.Adss.Clear();

                if (s.Count > 0)
                {
                    foreach (var item in s)
                    {
                        // Images removed from UI - no longer loading image data
                        bind.Adss.Add(item);
                    }
                    // Show Entregados content when data exists
                    collectionview_shippings.IsVisible = false;
                    collectionview_ads.IsVisible = false;
                    Entregados.IsVisible = true;
                }
                else
                {
                    // Show empty state card when no data (like other tabs)
                    collectionview_shippings.IsVisible = true;
                    collectionview_ads.IsVisible = false;
                    Entregados.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                // Handle any errors gracefully
                System.Diagnostics.Debug.WriteLine($"Error in Entregados tab: {ex.Message}");
                // Show empty state on error
                collectionview_shippings.IsVisible = true;
                collectionview_ads.IsVisible = false;
                Entregados.IsVisible = false;
            }
            finally
            {
                // Hide loading spinner after data is loaded
                SmallLoading.IsVisible = false;
            }
        }

        protected void AsyncLoad()
        {
            try
            {
                ShippingsLibrary.KeepSync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        // Method to manually refresh data when needed
        public async Task RefreshData()
        {
            try
            {
                SmallLoading.IsVisible = true;
                await ShippingsLibrary.CustomLoad();
                
                // Images removed from UI - no longer loading images
                
                isDataLoaded = true;
                SmallLoading.IsVisible = false;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                SmallLoading.IsVisible = false;
            }
        }

        // ViewCell_Tapped method removed - CollectionView doesn't use ViewCell

        public void ScrollUp(object sender, EventArgs e)
        {
            try
            {
                //myscrollview.ScrollToAsync(0, 0, true);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void OnScrolled(object sender, ScrolledEventArgs e)
        {
            try
            {
                if (Device.RuntimePlatform == Device.iOS)
                    return;
                //double scrollingSpace = myscrollview.ContentSize.Height - myscrollview.Height;
                //if (scrollingSpace <= e.ScrollY)
                if (e.ScrollY >= 50)
                {
                    collectionview_shippings.BackgroundColor = Colors.Red;
                    //hello_context.TextColor = Color.White;
                    if (check_if_enable == 0)
                    {
                        //up_context_image.IsVisible = true;
                        //menu_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow_white.png");
                        //up_context_image.BackgroundColor = Color.FromRgb(0, 0, 0);
                        //help_context.BackgroundColor = Color.FromRgb(0, 0, 0);
                        //help_image_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.help_icon_border.png");
                        //grid_context.BackgroundColor = Color.FromRgb(0, 0, 0);
                    }
                    check_if_enable = 1;
                    //reached end
                }
                if (e.ScrollY == 0)
                {
                    //hello_context.TextColor = Color.FromRgb(180, 180, 177);
                    //menu_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow.png");
                    //up_context_image.IsVisible = false;
                    //help_context.BackgroundColor = Color.LightGray;
                    //help_image_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.help_icon.png");
                    //grid_context.BackgroundColor = Color.Transparent;
                    //up_context.IsVisible = false;
                    check_if_enable = 0;
                }
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

        private async void GoToInbox(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.Navigation.PushAsync(new ChatsPageNewConcept("MyShippingPage"));
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

        private void Handle_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            try
            {
                if (Device.RuntimePlatform == Device.iOS)
                {
                    help_context.BackgroundColor = Colors.LightGray;
                    help_image_context.Source = "help_icon.png";
                    check_if_enable = 0;
                    return;
                }
                if (e.ItemIndex == 0)
                {
                    help_context.BackgroundColor = Colors.LightGray;
                    help_image_context.Source = "help_icon.png";
                    check_if_enable = 0;
                }
                if (e.ItemIndex == 2)
                {
                    if (check_if_enable == 0)
                    {
                        help_context.BackgroundColor = Color.FromRgb(0, 0, 0);
                        help_image_context.Source = "help_icon_border.png";
                    }
                    check_if_enable = 1;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void GoHelpPage(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.Navigation.PushAsync(new SupportPage());
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }


        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {

        }



        // EnsureImagesLoaded and LoadImageForShipping methods removed - images no longer displayed in UI

        #region IDisposable
        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[MyShippingPage] Disposing resources...");
                    
                    // Dispose ViewModel if it's disposable
                    if (ShippingsLibrary is IDisposable disposableViewModel)
                    {
                        disposableViewModel.Dispose();
                    }
                    
                    System.Diagnostics.Debug.WriteLine("[MyShippingPage] Resources disposed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MyShippingPage] Error during disposal: {ex.Message}");
                    _ = ex.Handle();
                }
                _disposed = true;
            }
        }
        #endregion
    }
}
