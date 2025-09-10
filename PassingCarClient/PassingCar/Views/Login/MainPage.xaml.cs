using PassingCar.Extensions;
using PassingCar.Hubs;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API.Ads;
using PassingCar.Models.API.NotificationNS;
using PassingCar.Models.API.User;
using PassingCar.ViewModels;



using System.Collections.ObjectModel;
using System.ComponentModel;

using Microsoft.Maui.Maps;
using PassingCar.Utils;
using Android.Content;


namespace PassingCar.Views
{

    public partial class MainPage : ContentPage, INotifyPropertyChanged, IDisposable
    {
        public MainPageViewModel ViewModel { get; set; }

        private int check_if_enable = 0;
        private DateTime statusDateTime;
        private bool ratingloaded = false;
        private bool ratingLoadingStopped = false;
        public MainPage()
        {
            try
            {
                InitializeComponent();
                DisplayCurrentLocation();
                ViewModel = App.HeaderContext;
                BindingContext = ViewModel;
                up_context_image.IsVisible = false;
                statusDateTime = DateTime.Now;
                _swipeGestureRecognizer = new SwipeGestureRecognizer();
                _swipeGestureRecognizer.Swiped += SwipeGestureRecognizer_Swiped;
                StatusFrame.GestureRecognizers.Add(_swipeGestureRecognizer);
                //up_context.IsVisible = false;
                _tapFeedback = new TapGestureRecognizer();
                _tapFeedback.Tapped += TapFeedback_Tapped;
                FeedbackSP.GestureRecognizers.Add(_tapFeedback);
                _tapShare = new TapGestureRecognizer();
                _tapShare.Tapped += TapShare_Tapped;
                ShareSP.GestureRecognizers.Add(_tapShare);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        protected override void OnAppearing()
        {
            try
            {
                var displayInfo = DeviceDisplay.MainDisplayInfo;
                var width = displayInfo.Width / displayInfo.Density;
                var height = displayInfo.Height / displayInfo.Density;

// Adjust Map and NeatFrame size based on the screen dimensions
                map.WidthRequest = width * 0.90;  // 75% of the screen width
                map.HeightRequest = height * 0.25; // 30% of the screen height
               
                
                DoRatingLoading();
                LoadData();
                if (notificationCollectionview.ItemsSource is ObservableCollection<NotificationExtended> items && items.Count > 0)
                {
                    // Scroll to the first item in the collection
                    notificationCollectionview.ScrollTo(0, position: ScrollToPosition.Start, animate: true);
                }
                base.OnAppearing();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void TapShare_Tapped(object sender, EventArgs e)
        {
            try
            {
                await Share.RequestAsync(new ShareTextRequest
                {
                    Uri = $"https://www.passingcar.com",
                    Title = "Compartir Passing Car con amig@s"
                });
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void TapFeedback_Tapped(object sender, EventArgs e)
        {
            try
            {
                await Browser.OpenAsync("https://www.passingcar.com/#contact");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
        {
            try
            {
                if (e.Direction == SwipeDirection.Right)
                {
                    TapGestureRecognizerLeft_Tapped(sender, null);
                }
                else if (e.Direction == SwipeDirection.Left)
                {
                    TapGestureRecognizerRight_Tapped(sender, null);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void OnMyAdsItemSelected(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Add null checks for event args and selection
                if (e?.CurrentSelection == null)
                    return;

                AdsDetailsExtened mydetails = e.CurrentSelection as AdsDetailsExtened;
                if (mydetails == null)
                    return;

                SingleAdsViewModel modelBinding = new SingleAdsViewModel(mydetails);
                await modelBinding.AsyncLoad();
                PassingCar.Views.Content.SingleAdsPage singleAdsPage = new PassingCar.Views.Content.SingleAdsPage(modelBinding);
                
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
                System.Diagnostics.Debug.WriteLine($"[MainPage] Error in OnMyAdsItemSelected: {ex.Message}");
            }
        }

        private async void LoadData()
        {
            try
            {
                // Optimize: Load all data in parallel instead of sequentially
                var loadRatingTask = LoadRating();
                var loadStatusTask = LoadStatus();
                var loadNotificationsTask = LoadNotifications();
                var loadReviewsTask = LoadReviews();
                var loadMyAdsTask = LoadMyAds();

                // Wait for all tasks to complete
                await Task.WhenAll(loadRatingTask, loadStatusTask, loadNotificationsTask, loadReviewsTask, loadMyAdsTask);

                // Setup UI interactions
                TapGestureRecognizer tapAds = new TapGestureRecognizer();
                tapAds.Tapped += (s, e) =>
                {
                    _ = Navigation.PushAsync(new MyAdsPage());
                };
                MyAdsFrame.GestureRecognizers.Add(tapAds);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async Task LoadReviews()
        {
            try
            {
                reviewsLoading.IsVisible = true;
                GetReviewsResponse reviews = await Api.GetReviews(new GetReviewsInput());
                if (reviews != null && reviews.Success && reviews.Reviews != null && reviews.Reviews.Count() > 0)
                {
                    while (ReviewsSP.Children.Count > 1)
                    {
                        ReviewsSP.Children.RemoveAt(1);
                    }
                    foreach (ReviewM item in reviews.Reviews)
                    {
                        ReviewsSP.Children.Add(new Content.Review(item));
                    }
                }
                reviewsLoading.IsVisible = false;
            }
            catch (Exception ex)
            {
                reviewsLoading.IsVisible = false;
                _ = ex.Handle();
            }
        }
        private async Task LoadNotifications()
        {
            try
            {
                GetNotificationsResponse notifs = await Api.GetNotifications();
                if (notifs != null && notifs.Notifications != null && notifs.Notifications.Count() > 0)
                {
                    ViewModel.Notifications.Clear();
                    notifs.Notifications = notifs.Notifications.OrderBy(o => o.CreatedAt);
                    foreach (Notification notif in notifs.Notifications)
                    {
                        ViewModel.Notifications.Insert(0, new NotificationExtended(notif, ViewModel.Notifications));
                    }
                    PassingCarHubs.UpdateEvent(PassingCarHubs.Ads.NewNotification, (input) =>
                    {
                        if (input is Notification item)
                        {
                            ViewModel.Notifications.Insert(0, new NotificationExtended(item, ViewModel.Notifications));
                          // CrossLocalNotifications.Current.Show(item.Title, $"{item.Message}");
                        }
                    });
                    SmallLoadingNotifs.IsVisible = false;
                }
                else
                {
                    SmallLoadingNotifs.IsVisible = false;
                    NotificationsSP.Children.Clear();
                    Label label = new Label()
                    {
                        Text = $"0 notificaitons",
                        TextColor = Colors.Black,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalOptions = LayoutOptions.Center,
                        FontAttributes = FontAttributes.Bold
                    };
                    NotificationsSP.Children.Add(label);

                    PassingCarHubs.UpdateEvent(PassingCarHubs.Ads.NewNotification, (input) =>
                    {
                        if (input is Notification item)
                        {
                            if (ViewModel.Notifications != null && ViewModel.Notifications.Count > 0)
                            {
                                ViewModel.Notifications.Insert(0, new NotificationExtended(item, ViewModel.Notifications));
                            }
                            else
                            {
                                NotificationsSP.Children.Clear();
                                ViewModel.Notifications = new ObservableCollection<NotificationExtended>();
                            }
                           // CrossLocalNotifications.Current.Show(item.Title, item.Message);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async Task LoadMyAds()
        {
            try
            {
                AdsFilter filter = new AdsFilter()
                {
                    OnlyOwnAds = true,
                    OnlyFavorites = false,
                };
                await ViewModel.AsyncLoad(filter, SmallLoadingMyAds, Navigation);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async Task LoadRating()
        {
            try
            {
                GetUserRatingAndSoldResponse ratingAndSold = await Api.GetUserRatingAndSol();

                ratingloaded = true;
                // Optimize: Remove artificial delay loop
                if (!ratingLoadingStopped)
                {
                    ratingLoadingStopped = true;
                }
                await Task.Delay(40);
                double rating = ratingAndSold != null && ratingAndSold.Rating > 0 ? ratingAndSold.Rating : 0f;
                RatingStar1.Source = GetStarImage(rating, 1);
                RatingStar2.Source = GetStarImage(rating, 2);
                RatingStar3.Source = GetStarImage(rating, 3);
                RatingStar4.Source = GetStarImage(rating, 4);
                RatingStar5.Source = GetStarImage(rating, 5);
                RatingValue.Text = ratingAndSold != null && ratingAndSold.Rating > 0 ? $"{rating:0.00}" : $"Sin reseñas";
                SoldValue.Text = (ratingAndSold != null && ratingAndSold.Sold.HasValue) ? $"{ratingAndSold.Sold:0.00} €" : $"0.00 €";
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private ImageSource GetStarImage(double userRating, int countStar)
        {
            try
            {
                return !(userRating > 0)
                    ? "a_star1.png"
                    : (userRating - countStar) >= 0
                        ? "a_star_filled1.png"
                        : (userRating - countStar) >= -0.5
                                            ? "a_star_half1.png"
                                            : "a_star1.png";
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return "a_star1.png";
            }
        }
        private void DoRatingLoading()
        {
            try
            {
                int count = 0;
                Device.StartTimer(TimeSpan.FromMilliseconds(350), () =>
                {
                    switch (count % 7)
                    {
                        case 0:
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                RatingStar1.Source = "a_star_filled1.png";
                                //RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                RatingStar5.Source = "a_star1.png";
                                // Removed Task.Delay(10) for immediate response
                            });
                            break;
                        case 1:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                //RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                RatingStar2.Source = "a_star_filled1.png";
                                //RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                await Task.Delay(10);
                            });
                            break;
                        case 2:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                //RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                //RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                RatingStar3.Source = "a_star_filled1.png";
                                //RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                await Task.Delay(10);
                            });
                            break;
                        case 3:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                RatingStar1.Source = "a_star1.png";
                                //RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                //RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                RatingStar4.Source = "a_star_filled1.png";
                                //RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                await Task.Delay(10);
                            });
                            break;
                        case 4:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                //RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                RatingStar2.Source ="a_star1.png";
                                //RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                //RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                RatingStar5.Source = "a_star_filled1.png";
                                await Task.Delay(50);
                            });
                            break;
                        case 5:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                //RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                RatingStar3.Source = "a_star1.png";
                                //RatingStar4.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                //RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                await Task.Delay(50);
                            });
                            break;
                        case 6:
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                //RatingStar1.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar2.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                //RatingStar3.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star1.png");
                                RatingStar4.Source = "a_star1.png";
                                //RatingStar5.Source = ImageSource.FromResource("PassingCar.Resources.Images.a_star_filled1.png");
                                if (RatingValue.Text.Contains("..."))
                                {
                                    RatingValue.Text = RatingValue.Text.Replace("...", "");
                                }
                                else
                                {
                                    RatingValue.Text += ".";
                                }
                                SoldValue.Text = RatingValue.Text;
                                await Task.Delay(50);
                            });
                            break;
                        default:
                            break;
                    }
                    count++;
                    ratingLoadingStopped = ratingloaded;
                    return !ratingloaded;
                });
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async Task LoadStatus()
        {
            try
            {
                StaticsRightArrow.IsVisible = statusDateTime.Month != DateTime.Now.Month || statusDateTime.Year != DateTime.Now.Year;
                if (StatusStackPanel.Children.Count > 0)
                {
                    int childrencount = StatusStackPanel.Children.Count;
                    for (int index = 0; index <= childrencount - 1; index++)
                    {
                        StatusStackPanel.Children.RemoveAt(0);
                    }
                    LoadingSmall loading = new LoadingSmall();
                    StatusStackPanel.Children.Add(loading);
                    await Task.Delay(200);
                    StatusLabelMonth.Text = $"Situación {statusDateTime.GetMonth()} {statusDateTime.Year}:";
                    await Task.Delay(200);
                    MonthlyStatus status = await Api.GetMonthlyStatus(new MonthlyStatusInput()
                    {
                        DateTime = statusDateTime
                    });
                    StatusStackPanel.Children.RemoveAt(StatusStackPanel.Children.Count - 1);
                    bool hasValue = status != null && status.Success && (
                        status.PublishedAds > 0
                        || status.SentPackages > 0
                        || status.TransportedPackages > 0
                        || status.CanceledDeliveries > 0
                        || status.OffersSent > 0
                        || status.SpentMoney > 0
                        || status.EarnedMpney > 0);
                    if (hasValue)
                    {
                        if (status.PublishedAds > 0)
                        {
                            AddStatusLabelToStacklayout($"{status.PublishedAds} anuncios publicados.");
                        }
                        if (status.SentPackages > 0)
                        {
                            AddStatusLabelToStacklayout($"{status.SentPackages} paquetes enviados.");
                        }
                        if (status.TransportedPackages > 0)
                        {
                            AddStatusLabelToStacklayout($"{status.TransportedPackages} paquetes entregados.");
                        }
                        if (status.CanceledDeliveries > 0)
                        {
                            AddStatusLabelToStacklayout($"{status.CanceledDeliveries} entregas canceladas.", Colors.DarkRed);
                        }
                        if (status.OffersSent > 0)
                        {
                            AddStatusLabelToStacklayout($"{status.OffersSent} ofertas enviadas.");
                        }
                        if (status.SpentMoney > 0)
                        {
                            AddStatusLabelToStacklayout($"{status.SpentMoney:0.00} € gastados.");
                        }
                        if (status.EarnedMpney > 0)
                        {
                            AddStatusLabelToStacklayout($"{status.EarnedMpney:0.00} € ganado.");
                        }
                    }
                    else
                    {
                        AddStatusLabelToStacklayout("Ninguna actividad");
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private void AddStatusLabelToStacklayout(string text, Color? textColor = null)
        {
            try
            {
                Label label = new Label()
                {
                    Text = text,
                    TextColor = textColor ?? Colors.Black,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontAttributes = FontAttributes.Bold
                };
                StatusStackPanel.Children.Add(label);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void LogOut_Clicked(object sender, EventArgs e)
        {
            try
            {
                _ = await Api.Logout();
                try
                {
                    // if (CrossFacebookClient.Current != null && CrossFacebookClient.Current.IsLoggedIn)
                    // {
                    //     CrossFacebookClient.Current.Logout();
                    // }
                }
                catch (Exception) { }
                try
                {
                    // if (CrossGoogleClient.Current != null && CrossGoogleClient.Current.IsLoggedIn)
                    // {
                    //     CrossGoogleClient.Current.Logout();
                    // }
                }
                catch (Exception) { }
                Application.Current.MainPage = new LoginInput();
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
                _ = myscrollview.ScrollToAsync(0, 0, true);
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
                {
                    hello_context.TextColor = Color.FromRgb(180, 180, 177);
                    menu_context.Source = "menu_icon.png";
                    up_context_image.IsVisible = false;
                    grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    //up_context.IsVisible = false;
                    check_if_enable = 0;
                    return;
                }
                double scrollingSpace = myscrollview.ContentSize.Height - myscrollview.Height;
                //if (scrollingSpace <= e.ScrollY)
                if (e.ScrollY >= 50)
                {
                    hello_context.TextColor = Colors.White;
                    if (check_if_enable == 0)
                    {
                        up_context_image.IsVisible = true;
                        menu_context.Source = "menu_icon_white.png";
                        up_context_image.BackgroundColor = Color.FromRgb(0, 0, 0);
                    }

                    grid_context.BackgroundColor = Color.FromRgb(0, 0, 0);
                    check_if_enable = 1;
                    //reached end
                }
                if (e.ScrollY == 0)
                {
                    hello_context.TextColor = Color.FromRgb(180, 180, 177);
                    menu_context.Source = "menu_icon.png";
                    up_context_image.IsVisible = false;
                    grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    //up_context.IsVisible = false;
                    check_if_enable = 0;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }


        public async Task DisplayCurrentLocation()
        {
            try
            {
                // Check the current permission status
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                // If permission is not granted, request it
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                // If permission is granted, proceed with location retrieval
                if (status == PermissionStatus.Granted)
                {
                    // Request geolocation with medium accuracy
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                    var location = await Geolocation.GetLocationAsync(request);

                    if (location != null)
                    {
                        // Move the map to the current location
                        var position = new Location(location.Latitude, location.Longitude);
                        MapSpan mapSpan = MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(0.444));

                        // Assuming you have a map object defined somewhere
                        map.MoveToRegion(mapSpan);

                        // Optionally, log or display the location
                        Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Unable to retrieve location.", "OK");
                    }
                }
                else if (status == PermissionStatus.Denied)
                {
                    // Permission was denied, inform the user and guide them to settings
                    await Application.Current.MainPage.DisplayAlert("Permission Denied",
                        "Location permission is required. Please enable it in your device's settings.", "OK");
                }
                else if (status == PermissionStatus.Restricted)
                {
                    // Permission is restricted, typically due to parental controls
                    await Application.Current.MainPage.DisplayAlert("Permission Restricted",
                        "Location permission is restricted. Please check your device's restrictions or parental controls.", "OK");
                }
            }
            catch (PermissionException ex)
            {
                // Handle the case when permission is denied
                await Application.Current.MainPage.DisplayAlert("Permission Error",
                    "Geolocation permission denied. Please grant permission in the app settings.", "OK");
            }
            catch (FeatureNotSupportedException)
            {
                // Handle the case when the device does not support geolocation
                await Application.Current.MainPage.DisplayAlert("Error", "Geolocation is not supported on this device.", "OK");
            }
            catch (FeatureNotEnabledException)
            {
                // Location services are not enabled on the device
                await Shell.Current.DisplayAlert("Location Required", "Please enable location services in device settings.", "OK");

#if ANDROID
                // Open the location settings page on Android
                var intent = new Android.Content.Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                intent.AddFlags(ActivityFlags.NewTask); // ✅ This is required
                Android.App.Application.Context.StartActivity(intent);
#endif

            }
            catch (Exception ex)
            {
                // Handle any other unexpected errors
                await Application.Current.MainPage.DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
            }
        }


        private void SwapMenuContent(object sender, EventArgs e)
        {
            try
            {
                Shell.Current.FlyoutIsPresented = true;
                //Shell.Current.FlyoutIsPresented = false;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void TapGestureRecognizerLeft_Tapped(object sender, EventArgs e)
        {
            try
            {
                statusDateTime = statusDateTime.AddMonths(-1);
                await LoadStatus();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void TapGestureRecognizerRight_Tapped(object sender, EventArgs e)
        {
            try
            {
                statusDateTime = statusDateTime.AddMonths(1);
                await LoadStatus();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        private async void CheckAdsPage(object sender, EventArgs e)
        {
            try
            {
                //await Shell.Current.Navigation.PushAsync(new AdsPage());
                await Shell.Current.GoToAsync($"//AdsPage");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async void InboxPage(object sender, EventArgs e)
        {
            try
            {
                //await Shell.Current.Navigation.PushAsync(new ChatsPage());
                await Shell.Current.GoToAsync($"//InboxPage");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        [Obsolete]
        private async void OpenProfile(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.Navigation.PushAsync(new MyAccountPage());
                //await Shell.Current.GoToAsync($"//InboxPage");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            // Display an alert asking if the user really wants to exit
            var result = DisplayAlert("Confirm Exit", "¿Realmente desea salir de Passing Car?", "SI", "NO").Result;

            if (result)
            {
                // Close the application
                Application.Current.Quit();
                return true; // Indicates that the back button press has been handled
            }

            return false; // Allows the default back button behavior
        }


        private async void TapGestureRecognizer_OnTapped(object? sender, TappedEventArgs e)
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

                SingleAdsViewModel modelBinding = new SingleAdsViewModel(mydetails);
                await modelBinding.AsyncLoad();
                PassingCar.Views.Content.SingleAdsPage singleAdsPage = new PassingCar.Views.Content.SingleAdsPage(modelBinding);
                
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
                System.Diagnostics.Debug.WriteLine($"[MainPage] Error in TapGestureRecognizer_OnTapped: {ex.Message}");
            }
        }

        #region IDisposable
        private bool _disposed = false;
        private SwipeGestureRecognizer _swipeGestureRecognizer;
        private TapGestureRecognizer _tapFeedback;
        private TapGestureRecognizer _tapShare;

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
                    System.Diagnostics.Debug.WriteLine("[MainPage] Disposing resources...");
                    
                    // Clean up gesture recognizers
                    if (_swipeGestureRecognizer != null)
                    {
                        _swipeGestureRecognizer.Swiped -= SwipeGestureRecognizer_Swiped;
                        StatusFrame?.GestureRecognizers?.Remove(_swipeGestureRecognizer);
                        _swipeGestureRecognizer = null;
                    }
                    
                    if (_tapFeedback != null)
                    {
                        _tapFeedback.Tapped -= TapFeedback_Tapped;
                        FeedbackSP?.GestureRecognizers?.Remove(_tapFeedback);
                        _tapFeedback = null;
                    }
                    
                    if (_tapShare != null)
                    {
                        _tapShare.Tapped -= TapShare_Tapped;
                        ShareSP?.GestureRecognizers?.Remove(_tapShare);
                        _tapShare = null;
                    }
                    
                    // Dispose ViewModel if it's disposable
                    if (ViewModel is IDisposable disposableViewModel)
                    {
                        disposableViewModel.Dispose();
                    }
                    
                    System.Diagnostics.Debug.WriteLine("[MainPage] Resources disposed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MainPage] Error during disposal: {ex.Message}");
                    _ = ex.Handle();
                }
                _disposed = true;
            }
        }
        #endregion
    }
}