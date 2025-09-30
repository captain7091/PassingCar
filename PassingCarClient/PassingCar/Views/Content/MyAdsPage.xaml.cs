
using CommunityToolkit.Maui.Extensions;
using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models.API;
using PassingCar.Models.API.Ads;
using PassingCar.Utils;
using PassingCar.ViewModels;
using PassingCar.Views.Content;
using System.Threading.Tasks;
using PassingCar.Models;
using PassingCar.LocalDatabase;



namespace PassingCar.Views
{
    public partial class MyAdsPage : ContentPage, IDisposable
    {
        //public ICommand ForgotPasswordCommand { get; }
        public ListAdsViewModel AdsLibrary { get; }
        public View ContentPage { get; set; }
        public bool Favorites { get; set; }
        private int check_if_enable = 0;
        bool togglebutton = false;
        public MyAdsPage()
         {
             try
             {
                 InitializeComponent();
            AdsLibrary = new ListAdsViewModel(SmallLoading, navigation: Navigation, onlyMyAds: true);
            BindingContext = AdsLibrary;
            

            
            // Subscribe to user initialization event for better synchronization
            App.UserInitialized += OnUserInitialized;

            AsyncLoad();
            //AdsLibrary.CustomLoad( new AdsFilter()
            //{
            //    OnlyOwnAds = true,
            //    OnlyFavorites = false,
            //});
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        
        private async void OnUserInitialized(PassingCar.Models.User user)
        {
            
            // Ensure we're on the main thread and this page is visible
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Check if this page is currently visible in the Shell
                    if (Application.Current.MainPage is MainMenu && 
                        Shell.Current?.CurrentPage == this)
                    {
                        await ForceRefreshAds();
                    }
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                }
            });
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
                _ = ex.Handle();
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
                _ = ex.Handle();
            }
        }
        protected override async void OnAppearing()
        {
            try
            {

                // CRITICAL FIX: Wait for user initialization if needed
                await EnsureUserIsInitialized();

                // Verify we have a valid user after initialization
                if (App.CurrentUser == null || App.CurrentUser.Id <= 0)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert("Login Required", "Please log in again to view your ads.", "OK");
                    });
                    return;
                }


                // NEW: Use GetMyAds API directly instead of local database approach
                SmallLoading.IsVisible = true;
                AdsLibrary.KeepLoading = true;

                // Call new GetMyAds API
                await LoadMyAdsFromAPI();

                base.OnAppearing();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                _ = ex.Handle();
            }
        }

        /// <summary>
        /// Loads user's ads using the new GetMyAds API
        /// </summary>
        private async Task LoadMyAdsFromAPI()
        {
            try
            {

                // Call the new GetMyAds API
                GetNextAdsResponse response = await Api.GetMyAds();
                
                if (response != null && response.Success && response.AdsItem != null)
                {
                    
                    // Clear existing ads
                    AdsLibrary.Adss.Clear();
                    AdsLibrary.AllAds.Clear();
                    
                    // Convert API response to local ads and populate the UI
                    foreach (var item in response.AdsItem)
                    {
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
                            UserProfile = item.UserProfile,
                            UserName = item.UserName,
                            UserRating = item.UserRating,
                            PostedTime = item.PostedTime,
                            UserId = item.UserId,
                            ModifiedAt = item.ModifiedAt,
                        };
                        
                        // Always add to AllAds for filtering purposes
                        AdsLibrary.AllAds.Add(adDetails);
                        
                        // FIXED: Only add to Adss if state < 3 (for Publicados tab by default)
                        try
                        {
                            var stateValue = adDetails.State.ReplaceEspana();
                            var adsState = (AdsState)Enum.Parse(typeof(AdsState), stateValue);
                            var stateInt = (int)adsState;
                            
                            if (stateInt < 3) // Only show ads with state < 3 in Publicados tab by default
                            {
                                AdsLibrary.Adss.Add(adDetails);
                            }
                        }
                        catch (Exception ex)
                        {
                            _ = ex.Handle();
                            // If parsing fails, add to Adss to be safe
                            AdsLibrary.Adss.Add(adDetails);
                        }
                    }
                    
                }
                else
                {
                    // Show error message to user if needed
                    if (!string.IsNullOrEmpty(response?.ErrorMessage))
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            DisplayAlert("Error", response.ErrorMessage, "OK");
                        });
                    }
                }
                
                SmallLoading.IsVisible = false;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                SmallLoading.IsVisible = false;
                _ = ex.Handle();
            }
        }

        /// <summary>
        /// Ensures App.CurrentUser is properly initialized before proceeding
        /// </summary>
        private async Task EnsureUserIsInitialized()
        {
            try
            {
                
                // If App.CurrentUser is already set, we're good
                if (App.CurrentUser != null && App.CurrentUser.Id > 0)
                {
                    return;
                }
                
                
                // Wait a bit for background login to complete
                for (int i = 0; i < 10; i++) // Wait up to 5 seconds
                {
                    await Task.Delay(500);
                    if (App.CurrentUser != null && App.CurrentUser.Id > 0)
                    {
                        return;
                    }
                }
                
                // If still null, try to get user from API as fallback
                try
                {
                    int userId = await Api.GetUserId();
                    if (userId > 0)
                    {
                        // Create a minimal user object for now
                        App.CurrentUser = new User { Id = userId, Name = "Current User" };
                    }
                }
                catch (Exception apiEx)
                {
                    _ = apiEx.Handle();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        private async Task ForceRefreshAds()
        {
            try
            {

                // Check if we have existing ads in database before clearing UI
                var existingAds = await App.LocalDatabase.GetAllLocalAds();

                // Load fresh ads from API (but don't clear UI collections yet)
                // Progressive timeout strategy: Try multiple approaches
                bool loaded = false;

                // Approach 1: Try with minimal filter (fastest)
                var minimalFilter = new AdsFilter()
                {
                    OnlyOwnAds = true,
                    Active = true
                };
                loaded = await App.LocalDatabase.GetNewAds(int.MinValue, minimalFilter);

                if (!loaded)
                {
                    // Approach 2: Try with extended timeout and more specific filter
                    var extendedFilter = new AdsFilter()
                    {
                        OnlyOwnAds = true,
                        FromLastXDays = 90, // Extend to 90 days to catch more ads
                        Active = true,
                        OnlyFavorites = false,
                        OnlyNextOne = false
                    };
                    loaded = await App.LocalDatabase.GetNewAds(int.MinValue, extendedFilter);
                }

                if (!loaded)
                {
                    // Approach 3: Try without date filter (get all user ads)
                    var allUserAdsFilter = new AdsFilter()
                    {
                        OnlyOwnAds = true,
                        FromLastXDays = 0, // No date restriction
                        Active = true
                    };
                    loaded = await App.LocalDatabase.GetNewAds(int.MinValue, allUserAdsFilter);
                }

                if (!loaded)
                {
                    // Attempt 4: Show ALL ads as fallback (better than showing nothing)
                    var allAdsFilter = new AdsFilter()
                    {
                        OnlyOwnAds = false, // Show all ads
                        FromLastXDays = 30, // Last 30 days
                        Active = true
                    };
                    loaded = await App.LocalDatabase.GetNewAds(int.MinValue, allAdsFilter);

                    if (loaded)
                    {
                        // Show a message to user that we're showing all ads due to network issues
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            DisplayAlert("Network Issue", "Showing all ads due to network connectivity issues. Your ads may be included.", "OK");
                        });
                    }
                }

                // Clear UI collections only after we've tried to load new data
                var bind = BindingContext as ListAdsViewModel;
                bind?.Adss.Clear();
                bind?.AllAds.Clear();

                if (!loaded)
                {
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] All API attempts failed, checking local database...");

                    // CRITICAL FIX: Always load all available ads from database when API fails
                    var allLocalAds = await App.LocalDatabase.GetAllLocalAds();
                    if (allLocalAds.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Database is empty - user needs to create ads");
                        await ShowNoAdsMessage();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Database has {allLocalAds.Count} ads - loading ALL ads from local database");
                        await ShowAllAdsFromDatabase();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] API call successful - new ads loaded from server");
                }

                System.Diagnostics.Debug.WriteLine("[MyAdsPage] ForceRefreshAds completed");
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] ForceRefreshAds error: {ex.Message}");
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

        protected async void AsyncLoad()
        {
            try
            {
                // FIXED: Don't remove test ads - they should be available for display when no real ads exist
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] AsyncLoad started - keeping test ads for fallback display");

                AdsLibrary.KeepSync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }



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
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Error in OnItemSelected: {ex.Message}");
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
                {
                    hello_context.TextColor = Color.FromRgb(180, 180, 177);
                    menu_context.Source = "back_arrow.png";
                    grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    check_if_enable = 0;
                    return;
                }
                if (e.ItemIndex == 0)
                {
                    hello_context.TextColor = Color.FromRgb(180, 180, 177);
                    menu_context.Source = "back_arrow.png";
                    grid_context.BackgroundColor = Color.FromRgb(255, 255, 255);
                    check_if_enable = 0;
                }
                if (e.ItemIndex == 5)
                {
                    hello_context.TextColor = Colors.White;
                    if (check_if_enable == 0)
                    {
                        menu_context.Source = "back_arrow_white.png";
                    }
                    grid_context.BackgroundColor = Color.FromRgb(0, 0, 0);
                    check_if_enable = 1;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }



        private async void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Reset all button colors
                this.En_Ruta.BackgroundColorTo(Color.FromHex("#c3c3c3"));
                Enviados.BackgroundColorTo(Color.FromHex("#c3c3c3"));
                await Publicados.BackgroundColorTo(Color.FromHex("#fe3f40"));

                SmallLoading.IsVisible = true;
                AdsLibrary.KeepLoading = true;

                var bind = BindingContext as ListAdsViewModel;
                bind.Adss.Clear();
                await ForceRefreshAds();
                await AdsLibrary.CustomLoad(new AdsFilter()
                {
                    OnlyOwnAds = true,
                    OnlyFavorites = false,
                    Active = true
                });

                // FIXED: Filter out ads with state >= 3 (payment completed) from Publicados tab
                // DEBUG: Add debugger statements to check ad states
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Publicados tab - Total ads before filtering: {bind.Adss.Count}");
                foreach (var ad in bind.Adss)
                {
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Ad ID: {ad.AdsId}, Title: {ad.AdsTitle}, State: {ad.State}");
                }
                
                var filteredAds = bind.Adss.Where(ad => 
                {
                    // Parse the state to check if it's >= 3
                    try
                    {
                        var stateValue = ad.State.ReplaceEspana();
                        var adsState = (AdsState)Enum.Parse(typeof(AdsState), stateValue);
                        var stateInt = (int)adsState;
                        var shouldShow = stateInt < 3; // Only show ads with state < 3
                        
                        // DEBUG: Log each ad's filtering decision
                        System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Ad {ad.AdsId}: State='{ad.State}' -> Parsed='{stateValue}' -> StateInt={stateInt} -> ShouldShow={shouldShow}");
                        
                        return shouldShow;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Error parsing state for ad {ad.AdsId}: {ex.Message}");
                        return true; // If parsing fails, show the ad
                    }
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Publicados tab - Ads after filtering: {filteredAds.Count}");
                
                bind.Adss.Clear();
                foreach (var ad in filteredAds)
                {
                    bind.Adss.Add(ad);
                }

                SmallLoading.IsVisible = false;
                AdsLibrary.KeepLoading = false;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Error in Button_Clicked: {ex.Message}");
                SmallLoading.IsVisible = false;
                AdsLibrary.KeepLoading = false;
            }
        }



        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            Publicados.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            Enviados.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            await En_Ruta.BackgroundColorTo(Color.FromHex("#fe3f40"));
            SmallLoading.IsVisible = true;
            AdsLibrary.KeepLoading = true;

            var bind = BindingContext as ListAdsViewModel;
            
            bind.Adss.Clear();
            
            // FIXED: Load data from API instead of local database for En Ruta tab
            System.Diagnostics.Debug.WriteLine("[MyAdsPage] En Ruta tab - Loading data from API");
            await LoadMyAdsFromAPI();
            
            var alladds = bind.AllAds;

            // DEBUG: Add debugger statements to check ad states in En Ruta tab
            System.Diagnostics.Debug.WriteLine($"[MyAdsPage] En Ruta tab - Total ads before filtering: {alladds.Count}");
            foreach (var ad in alladds)
            {
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] En Ruta - Ad ID: {ad.AdsId}, Title: {ad.AdsTitle}, State: {ad.State}, UserName: {ad.UserName}");
            }

            // FIXED: Show ads with state >= 3 (payment completed) in En Ruta tab
            var username = await Api.GetUserData();
            System.Diagnostics.Debug.WriteLine($"[MyAdsPage] En Ruta tab - Current username: {username.Name}");
            
            var s = alladds.Where(x => 
            {
                try
                {
                    var stateValue = x.State.ReplaceEspana();
                    var adsState = (AdsState)Enum.Parse(typeof(AdsState), stateValue);
                    var stateInt = (int)adsState;
                    var isCurrentUser = x.UserName == username.Name;
                    var shouldShow = stateInt >= 3 && isCurrentUser; // Show ads with state >= 3
                    
                    // DEBUG: Log each ad's filtering decision
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] En Ruta - Ad {x.AdsId}: State='{x.State}' -> Parsed='{stateValue}' -> StateInt={stateInt} -> IsCurrentUser={isCurrentUser} -> ShouldShow={shouldShow}");
                    
                    return shouldShow;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] En Ruta - Error parsing state for ad {x.AdsId}: {ex.Message}");
                    return false; // If parsing fails, don't show the ad
                }
            }).ToList();

            System.Diagnostics.Debug.WriteLine($"[MyAdsPage] En Ruta tab - Ads after filtering: {s.Count}");

            bind.Adss.Clear();

            foreach (var item in s)
            {
                bind.Adss.Add(item);
            }
            SmallLoading.IsVisible = false;
            AdsLibrary.KeepLoading = false;

        }

        private async void Button_Clicked_2(object sender, EventArgs e)
        {
            Publicados.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            En_Ruta.BackgroundColorTo(Color.FromHex("#c3c3c3"));
            await Enviados.BackgroundColorTo(Color.FromHex("#fe3f40"));
            SmallLoading.IsVisible = true;
            AdsLibrary.KeepLoading = true;

            var bind = BindingContext as ListAdsViewModel;
            bind.Adss.Clear();

            // FIXED: Load data from API instead of local database for Enviados tab
            System.Diagnostics.Debug.WriteLine("[MyAdsPage] Enviados tab - Loading data from API");
            await LoadMyAdsFromAPI();



            var alladds = bind.AllAds;

            // FIXED: Show only delivered ads (state = 5) in Enviados tab
            var username = await Api.GetUserData();
            var s = alladds.Where(x => 
            {
                try
                {
                    var stateValue = x.State.ReplaceEspana();
                    var adsState = (AdsState)Enum.Parse(typeof(AdsState), stateValue);
                    return (int)adsState == 5 && x.UserName == username.Name; // Show only delivered ads (state = 5)
                }
                catch
                {
                    return false; // If parsing fails, don't show the ad
                }
            }).ToList();

            bind.Adss.Clear();


            foreach (var item in s)
            {
                bind.Adss.Add(item);
            }
            SmallLoading.IsVisible = false;
            AdsLibrary.KeepLoading = false;
        }

        private async void offerList_Loaded(object sender, EventArgs e)
        {

             offerList.BindingContext = new ListOffersViewModel();
            await (offerList.BindingContext as ListOffersViewModel) .AsyncLoad();
        }

        private async void add_ads(object sender, TappedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MyAdsPage] Add ads button clicked - navigating to AddAds page");
                await Shell.Current.Navigation.PushAsync(new AddAds("MyAdsPage"));
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        // OFERTAS CONDUCTORES BUTTON CLICK HANDLER - MOVED FROM MIS REPARTOS
        private async void OfertasConductoresBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MyAdsPage] Ofertas Conductores button clicked");
                // Navigate to Offers page
                await Shell.Current.GoToAsync("OffersPage");
            }
            catch (Exception ex)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Shell navigation failed, trying direct navigation: {ex.Message}");
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



        /// <summary>
        /// Show message when no ads exist and guide user to create ads (CRITICAL FIX: Check database first)
        /// </summary>
        private async Task ShowNoAdsMessage()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MyAdsPage] Checking if we should show 'no ads' message...");

                // CRITICAL FIX: Check if there are ANY ads in the database before showing "no ads" message
                var allLocalAds = await App.LocalDatabase.GetAllLocalAds();
                if (allLocalAds.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] CRITICAL FIX: Found {allLocalAds.Count} ads in database - loading them instead of showing 'no ads' message");
                    await ShowAllAdsFromDatabase();
                    return;
                }

                System.Diagnostics.Debug.WriteLine("[MyAdsPage] Database is truly empty - showing guidance message");

                bool createAd = await DisplayAlert(
                    "📦 No tienes anuncios",
                    "Aún no has creado ningún anuncio. ¿Te gustaría crear tu primer anuncio ahora?",
                    "✨ Crear Anuncio",
                    "⏰ Más tarde"
                );

                if (createAd)
                {
                    System.Diagnostics.Debug.WriteLine("[MyAdsPage] User chose to create ad - navigating to AddAds page");
                    await Shell.Current.Navigation.PushAsync(new AddAds("MyAdsPage"));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[MyAdsPage] User chose to create ad later");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Error showing no ads message: {ex.Message}");
                _ = ex.Handle();
            }
        }

        /// <summary>
        /// Show ALL ads from local database when API fails (CRITICAL FIX)
        /// </summary>
        private async Task ShowAllAdsFromDatabase()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Loading ALL ads from local database...");

                // Load ALL ads from local database, not just user-specific ones
                var localAds = await App.LocalDatabase.GetAllLocalAds();
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Found {localAds.Count} total ads in local database");

                if (localAds.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Loading ALL {localAds.Count} ads from local database...");

                    // Load ALL ads from local database
                    var bind = BindingContext as ListAdsViewModel;
                    if (bind != null)
                    {
                        bind.AllAds.Clear();
                        bind.Adss.Clear();

                        foreach (var localAd in localAds)
                        {
                            var adsDetails = new AdsDetailsExtened(false)
                            {
                                AdsId = localAd.Id,
                                AdsTitle = localAd.AdsTitle,
                                AdsPrice = localAd.AdsPrice,
                                AdsFrom = localAd.AdsFrom,
                                AdsTo = localAd.AdsTo,
                                State = localAd.State.Espana(),
                                PostedTime = localAd.PostedTime,
                                UserId = localAd.UserId,
                                UserName = localAd.Username,
                                UserProfile = localAd.UserProfile
                            };

                            bind.AllAds.Add(adsDetails);
                            bind.Adss.Add(adsDetails);
                        }

                        System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Successfully loaded ALL {bind.Adss.Count} ads from local database");
                        
                        // Hide loading indicator since we have data to show
                        SmallLoading.IsVisible = false;
                    }
                }
                else
                {
                    // Show a user-friendly message about the server issue
                    await App.Current.MainPage.DisplayAlert(
                        "Server Temporarily Unavailable",
                        "We're experiencing temporary server issues. Please try refreshing in a moment or check your internet connection.",
                        "OK");
                }

                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] ShowAllAdsFromDatabase completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Error in ShowAllAdsFromDatabase: {ex.Message}");
                _ = ex.Handle();
            }
        }

        /// <summary>
        /// Check if ads exist in the local database and show them if API fails
        /// </summary>
        private async Task ShowRecentlyCreatedAdsIfApiFails()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] API failed - checking local database for ads...");

                // Check local database for any ads
                var localAds = await App.LocalDatabase.GetAllLocalAds();
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Found {localAds.Count} ads in local database");

                if (localAds.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Loading ads from local database...");

                    // Load ads from local database
                    var bind = BindingContext as ListAdsViewModel;
                    if (bind != null)
                    {
                        bind.AllAds.Clear();
                        bind.Adss.Clear();

                        foreach (var localAd in localAds)
                        {
                            var adsDetails = new AdsDetailsExtened(false)
                            {
                                AdsId = localAd.Id,
                                AdsTitle = localAd.AdsTitle,
                                AdsPrice = localAd.AdsPrice,
                                AdsFrom = localAd.AdsFrom,
                                AdsTo = localAd.AdsTo,
                                State = localAd.State.Espana(),
                                PostedTime = localAd.PostedTime,
                                UserId = localAd.UserId,
                                UserName = localAd.Username,
                                UserProfile = localAd.UserProfile
                            };

                            bind.AllAds.Add(adsDetails);
                            bind.Adss.Add(adsDetails);
                        }

                        System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Successfully loaded {bind.Adss.Count} ads from local database");
                    }
                }
                else
                {
                    // Show a user-friendly message about the server issue
                    await App.Current.MainPage.DisplayAlert(
                        "Server Temporarily Unavailable",
                        "We're experiencing temporary server issues. Your ads have been created successfully but may take a few minutes to appear. Please try refreshing in a moment.",
                        "OK");
                }

                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Fallback handling completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Error in fallback: {ex.Message}");
                _ = ex.Handle();
            }
        }


        /// <summary>
        /// Check what's actually stored in the local SQLite database and show ads immediately if available
        /// </summary>
        private async Task CheckLocalDatabase()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] === CHECKING LOCAL DATABASE ===");
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Current User ID: {App.CurrentUser?.Id ?? 0}");

                var localAds = await App.LocalDatabase.GetAllLocalAds();
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Local database contains {localAds.Count} ads");

                // No test data creation - rely on real ads from API

                // Filter for current user's ads - if CurrentUser is null, show all ads for testing
                List<PassingCar.LocalDatabase.LocalAd> userAds;
                if (App.CurrentUser != null)
                {
                    userAds = localAds.Where(ad => ad.UserId == App.CurrentUser.Id).ToList();
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Found {userAds.Count} ads for current user (ID: {App.CurrentUser.Id})");
                }
                else
                {
                    // FALLBACK: If user is not logged in, show all ads for testing purposes
                    userAds = localAds.ToList();
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] CurrentUser is null, showing all {userAds.Count} ads for testing");
                }

                foreach (var localAd in localAds)
                {
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] LocalAd: ID={localAd.Id}, Title='{localAd.AdsTitle}', UserId={localAd.UserId}, State={localAd.State}, PostedTime={localAd.PostedTime}");
                }

                // PERFORMANCE FIX: Show local ads immediately if available
                if (userAds.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Loading {userAds.Count} ads from local database immediately...");
                    await LoadAdsFromLocalDatabase(userAds);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] No ads found for current user. Database needs to be populated from successful API calls first.");

                    // Show better decorated popup message
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        bool createAd = await DisplayAlert(
                            "📦 No tienes anuncios",
                            "No se encontraron anuncios en la base de datos local. ¿Te gustaría crear tu primer anuncio ahora?",
                            "✨ Crear Anuncio",
                            "⏰ Más tarde"
                        );

                        if (createAd)
                        {
                            await Shell.Current.Navigation.PushAsync(new AddAds("MyAdsPage"));
                        }
                    });
                }

                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] === END LOCAL DATABASE CHECK ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Error checking local database: {ex.Message}");
            }
        }



        /// <summary>
        /// Load ads from local database and display them immediately
        /// </summary>
        private async Task LoadAdsFromLocalDatabase(List<LocalAd> localAds)
        {
            try
            {
                var bind = BindingContext as ListAdsViewModel;
                if (bind != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Clearing existing ads and loading {localAds.Count} from local database");

                    bind.AllAds.Clear();
                    bind.Adss.Clear();

                    foreach (var localAd in localAds)
                    {
                        var adsDetails = new AdsDetailsExtened(false)
                        {
                            AdsId = localAd.Id,
                            AdsTitle = localAd.AdsTitle,
                            AdsPrice = localAd.AdsPrice,
                            AdsFrom = localAd.AdsFrom,
                            AdsTo = localAd.AdsTo,
                            State = localAd.State.Espana(),
                            PostedTime = localAd.PostedTime,
                            UserId = localAd.UserId,
                            UserName = localAd.Username,
                            UserProfile = localAd.UserProfile
                        };

                        bind.AllAds.Add(adsDetails);
                        bind.Adss.Add(adsDetails);
                    }

                    System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Successfully loaded {bind.Adss.Count} ads from local database");

                    // Hide loading indicator since we have data to show
                    SmallLoading.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Error loading ads from local database: {ex.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[MyAdsPage] Disposing resources...");
                
                // Unsubscribe from user initialization event
                App.UserInitialized -= OnUserInitialized;
                
                // Dispose ViewModel if it implements IDisposable
                if (AdsLibrary is IDisposable disposableViewModel)
                {
                    disposableViewModel.Dispose();
                    System.Diagnostics.Debug.WriteLine("[MyAdsPage] AdsLibrary disposed");
                }
                
                System.Diagnostics.Debug.WriteLine("[MyAdsPage] Resources disposed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MyAdsPage] Error during disposal: {ex.Message}");
                _ = ex.Handle();
            }
        }

    }
}
