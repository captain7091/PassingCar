using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models.API.Ads;
using PassingCar.Utils;
using PassingCar.ViewModels;
using PassingCar.Views.Content;


namespace PassingCar.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FavoriteAdsPage : ContentPage, IDisposable
    {
        //public ICommand ForgotPasswordCommand { get; }
        public ListAdsViewModel AdsLibrary { get; }
        public bool Favorites { get; set; }
        private int check_if_enable = 0;
        public FavoriteAdsPage()
        {
            try
            {
                InitializeComponent();
                AdsLibrary = new ListAdsViewModel(SmallLoading, true, navigation: Navigation);
                BindingContext = AdsLibrary;
                AsyncLoad();
                //ForgotPasswordCommand = new Command(BackMainMenu);    
                AdsFilter filter = new AdsFilter()
                {
                    OnlyOwnAds = false,
                    OnlyFavorites = true,
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
                await AdsLibrary.LoadFavorites();
                base.OnAppearing();
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
                //Application.Current.MainPage = new MainPage();
                await Shell.Current.GoToAsync(nameof(MainMenu));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        protected void AsyncLoad()
        {
            try
            {
                AdsLibrary.KeepSync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        //protected override async void OnAppearing()
        //{
        //base.OnAppearing();

        //adsView.ItemsSource = AdsLibrary.Adss;
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

                if (mydetails.AdIsEnabled)
                {
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
                else
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        this.OpeErrorPopUp($"This ads is no more active", $"", $"Ok");
                    });
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                System.Diagnostics.Debug.WriteLine($"[FavoriteAdsPage] Error in OnItemSelected: {ex.Message}");
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

        private void Handle_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            try
            {
                if (Device.RuntimePlatform == Device.iOS)
                    return;
                if (e.ItemIndex == 0)
                {
                    //hello_context.TextColor = Color.FromRgb(180, 180, 177);
                    //menu_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow.png");
                    //grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    check_if_enable = 0;
                }
                if (e.ItemIndex == 5)
                {
                    //hello_context.TextColor = Color.White;
                    if (check_if_enable == 0)
                    {
                        //menu_context.Source = ImageSource.FromResource($"PassingCar.Resources.Images.back_arrow_white.png");
                    }
                    //grid_context.BackgroundColor = Color.FromRgb(0, 0, 0);
                    check_if_enable = 1;
                }
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
                System.Diagnostics.Debug.WriteLine($"[FavoriteAdsPage] Error in collection view scrolling: {ex.Message}");
            }
        }


        private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            try
            {
                // Retrieve the tapped item from the sender with null checks
                var tappedFrame = sender as NeatFrame;
                if (tappedFrame?.BindingContext == null)
                    return;

                var mydetails = tappedFrame.BindingContext as AdsDetailsExtened;
                if (mydetails == null)
                    return;

                // Load the details and initialize the SingleAdsViewModel
                SingleAdsViewModel modelBinding = new SingleAdsViewModel(mydetails);
                await modelBinding.AsyncLoad();

                // Create a new instance of the SingleAdsPage and pass the ViewModel
                SingleAdsPage singleAdsPage = new SingleAdsPage(modelBinding);

                // Increment the view counter without blocking navigation
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
                System.Diagnostics.Debug.WriteLine($"[FavoriteAdsPage] Error in TapGestureRecognizer_Tapped: {ex.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[FavoriteAdsPage] Disposing resources...");
                
                // Dispose ViewModel if it implements IDisposable
                if (AdsLibrary is IDisposable disposableViewModel)
                {
                    disposableViewModel.Dispose();
                    System.Diagnostics.Debug.WriteLine("[FavoriteAdsPage] AdsLibrary disposed");
                }
                
                System.Diagnostics.Debug.WriteLine("[FavoriteAdsPage] Resources disposed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FavoriteAdsPage] Error during disposal: {ex.Message}");
                _ = ex.Handle();
            }
        }
    }
}