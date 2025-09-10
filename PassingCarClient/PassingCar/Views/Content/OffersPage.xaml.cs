using PassingCar.Extensions;
using PassingCar.Utils;
using PassingCar.ViewModels;

namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OffersPage : ContentPage, IDisposable
    {
        public ListOffersViewModel ListOffersViewModel { get; }
        public bool Favorites { get; set; }
        public bool reloadAds;
        private int check_if_enable = 0;
        public OffersPage()
        {
            try
            {
                InitializeComponent();
                ListOffersViewModel = new ListOffersViewModel();
                BindingContext = ListOffersViewModel;
                reloadAds = true;
    
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        // Back Button Click Handler
        private async void BackButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Navigate back to Mis Repartos page
                await Shell.Current.GoToAsync("//MainPage");
            }
            catch (Exception ex)
            {
                try
                {
                    // Alternative: Use Navigation.PopAsync
                    await Navigation.PopAsync();
                }
                catch (Exception ex2)
                {
                    // Handle navigation errors
                    System.Diagnostics.Debug.WriteLine($"Back navigation error: {ex2.Message}");
                    _ = ex2.Handle();
                }
            }
        }

        protected override async void OnAppearing()
        {
            try
            {
                await ListOffersViewModel.AsyncLoad();
                await Task.Delay(100);
                await ListOffersViewModel.LoadPhotos();
                ListOffersViewModel.UpdateEvent();
                await Task.Delay(100);

                base.OnAppearing();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
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

        private void BackOffersPage(object sender, EventArgs e)
        {
            try
            {
                //base.OnBackButtonPressed();
                Application.Current.MainPage = new MainMenu();
                //Shell.Current.FlyoutIsPresented = false;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }



        //private ViewCell lastCell;
        private void ViewCell_Tapped(object sender, System.EventArgs e)
        {
            try
            {
                //if (lastCell != null)
                //{
                //lastCell.View.BackgroundColor = Color.Transparent;
                //}

                //var viewCell = (ViewCell)sender;
                //if (viewCell.View != null)
                //{
                //    viewCell.View.BackgroundColor = Color.Transparent;
                //    lastCell = viewCell;
                //}
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void OnItemSelected(object sender, ItemTappedEventArgs e)
        {
            try
            {
                //reloadAds = false;
                //var mydetails = e.Item as AdsDetailsExtened;
                //await Navigation.PushAsync(new SingleAdsPage(mydetails));
                //await Api.IncrementViewCounter(new IncrementViewCounterInput()
                //{
                //    AdsId = mydetails.AdsId
                //});
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
        void collectionview_ads_Scrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            try
            {
                // Add null checks for event args
                if (e == null)
                    return;

                // Handling for iOS
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    check_if_enable = 0;
                    return; // Exit early for iOS-specific behavior
                }

                // Check the center item index and update the UI accordingly
                if (e.FirstVisibleItemIndex == 0)
                {
                    check_if_enable = 0;
                }

                // When center item is the 5th, adjust UI elements
                if (e.CenterItemIndex == 5)
                {
                    check_if_enable = 1;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions with proper error handling
                _ = ex.Handle();
                System.Diagnostics.Debug.WriteLine($"[OffersPage] Error in collection view scrolling: {ex.Message}");
            }
        }

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
                    System.Diagnostics.Debug.WriteLine("[OffersPage] Disposing resources...");
                    
                    // Dispose ViewModel if it's disposable
                    if (ListOffersViewModel is IDisposable disposableViewModel)
                    {
                        disposableViewModel.Dispose();
                    }
                    
                    System.Diagnostics.Debug.WriteLine("[OffersPage] Resources disposed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[OffersPage] Error during disposal: {ex.Message}");
                    _ = ex.Handle();
                }
                _disposed = true;
            }
        }
        #endregion
    }
}