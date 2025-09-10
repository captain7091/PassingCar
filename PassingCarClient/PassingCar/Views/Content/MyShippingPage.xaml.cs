using CommunityToolkit.Maui.Extensions;
using PassingCar.Extensions;
using PassingCar.Hubs;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models.API.Ads;
using PassingCar.ViewModels;
using PassingCar.Views.Intitial;
using System.Collections.Generic;
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
                // Only load data on first appearance or if explicitly needed, not on every tab switch
                if (!isDataLoaded)
                {
                    SmallLoading.IsVisible = true;
                    await ShippingsLibrary.CustomLoad();
                    
                    // Images removed from UI - no longer loading images
                    
                    isDataLoaded = true;
                }
                base.OnAppearing();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            this.En_Ruta.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            Enviados.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            await Publicados.BackgroundColorTo(Color.FromHex("#fe3f40"));

            collectionview_shippings.IsVisible = true;
            Entregados.IsVisible = false;

            // Show loading spinner
            SmallLoading.IsVisible = true;

            // Set OnlyActive to false for Publicados tab to show all shipping items
            // This prevents ads from disappearing when their state changes
            ShippingsLibrary.OnlyActive = false;
            
            // Reload data for Publicados tab
            await ShippingsLibrary.CustomLoad();
            
            // Hide loading spinner after data is loaded
            SmallLoading.IsVisible = false;
        }

        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            Publicados.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            Enviados.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            await En_Ruta.BackgroundColorTo(Color.FromHex("#fe3f40"));
         
            collectionview_shippings.IsVisible = true;
            Entregados.IsVisible = false;

            // Show loading spinner
            SmallLoading.IsVisible = true;

            // Set OnlyActive to true for En Ruta tab to show only active shipments
            ShippingsLibrary.OnlyActive = true;
            
            // Reload data for En Ruta tab
            await ShippingsLibrary.CustomLoad();
            
            // Hide loading spinner after data is loaded
            SmallLoading.IsVisible = false;
        }



        private async void Button_Clicked_2(object sender, EventArgs e)
        {
             Publicados.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            En_Ruta.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            await Enviados.BackgroundColorTo(Color.FromHex("#fe3f40"));
          

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
                    Entregados.IsVisible = true;
                }
                else
                {
                    // Show empty state card when no data (like other tabs)
                    collectionview_shippings.IsVisible = true;
                    Entregados.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                // Handle any errors gracefully
                System.Diagnostics.Debug.WriteLine($"Error in Entregados tab: {ex.Message}");
                // Show empty state on error
                collectionview_shippings.IsVisible = true;
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

        // OFERTAS CONDUCTORES BUTTON CLICK HANDLER - ADDED FROM OLD VERSION
        private async void OfertasConductoresBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Navigate to Offers page
                await Shell.Current.GoToAsync("OffersPage");
            }
            catch (Exception ex)
            {
                try
                {
                    // Alternative: Direct navigation
                    await Navigation.PushAsync(new OffersPage());
                }
                catch (Exception ex2)
                {
                    // Handle navigation errors
                    System.Diagnostics.Debug.WriteLine($"Navigation error: {ex2.Message}");
                    _ = ex2.Handle();
                }
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
