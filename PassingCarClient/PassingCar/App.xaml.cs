using System.Globalization;
using PassingCar.Extensions;
using PassingCar.LocalDatabase;
using PassingCar.Views; // Ensure the correct using statement for your views
using PassingCar.Hubs;

namespace PassingCar;

public partial class App : Application
{
    private static LocalDB localDatabase;
    public static LocalDB LocalDatabase
    {
        get
        {
            if (localDatabase == null)
            {
                localDatabase = new LocalDB(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "passingcar.db3"));
            }
            return localDatabase;
        }
    }
    public static PassingCar.ViewModels.MainPageViewModel HeaderContext { get; set; }
    
    private static PassingCar.Models.User _currentUser;
    public static PassingCar.Models.User CurrentUser 
    {
        get => _currentUser;
        set
        {
            _currentUser = value;
            // Notify when user is initialized
            if (value != null)
            {
                System.Diagnostics.Debug.WriteLine($"[App] CurrentUser set: ID={value.Id}, Name='{value.Name}'");
                UserInitialized?.Invoke(value);
            }
        }
    }
    
    // Event to notify when user is initialized
    public static event Action<PassingCar.Models.User> UserInitialized;

    public App()
    {
        InitializeComponent();

        // Set the culture as before
        var culture = new CultureInfo("en-US");
        string separator = ".";
        culture.NumberFormat.NumberDecimalSeparator = separator;
        culture.NumberFormat.CurrencyDecimalSeparator = separator;
        culture.NumberFormat.PercentDecimalSeparator = separator;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        // Ensure we subscribe to unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        
        // Handle unobserved task exceptions
        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        // Initialize HeaderContext
        HeaderContext = new PassingCar.ViewModels.MainPageViewModel();

        // Set the MainPage
      //  MainPage = new Initial(); // Replace this with the actual MainPage if necessary
    }

    // Handle unhandled exceptions globally
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        try
        {
            Exception e = (Exception)args.ExceptionObject;
            
            // Log the exception details
            System.Diagnostics.Debug.WriteLine($"[GLOBAL EXCEPTION] Unhandled Exception: {e.Message}");
            System.Diagnostics.Debug.WriteLine($"[GLOBAL EXCEPTION] Stack Trace: {e.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"[GLOBAL EXCEPTION] Source: {e.Source}");
            
            // Log to console as well
            Console.WriteLine($"Unhandled Exception: {e.Message}");
            Console.WriteLine($"Stack Trace: {e.StackTrace}");
            
            // Try to show user-friendly error message if possible
            if (Current?.MainPage != null)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await Current.MainPage.DisplayAlert(
                            "Application Error", 
                            "The application encountered an unexpected error. The app will attempt to recover.", 
                            "OK");
                    }
                    catch
                    {
                        // If we can't show the alert, just continue
                    }
                });
            }
            
            // Attempt recovery by navigating to initial page
            if (!args.IsTerminating)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        if (Current != null)
                        {
                            Current.MainPage = new Views.Initial();
                        }
                    }
                    catch
                    {
                        // If recovery fails, let the app terminate
                    }
                });
            }
        }
        catch
         {
             // If exception handling itself fails, just log to console
             Console.WriteLine("Critical error in exception handler");
         }
     }
     
     // Handle unobserved task exceptions
     private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
     {
         try
         {
             var ex = args.Exception;
             
             // Log the exception details
             System.Diagnostics.Debug.WriteLine($"[UNOBSERVED TASK EXCEPTION] Exception: {ex.Message}");
             System.Diagnostics.Debug.WriteLine($"[UNOBSERVED TASK EXCEPTION] Stack Trace: {ex.StackTrace}");
             
             // Log to console as well
             Console.WriteLine($"Unobserved Task Exception: {ex.Message}");
             Console.WriteLine($"Stack Trace: {ex.StackTrace}");
             
             // Mark as observed to prevent app termination
             args.SetObserved();
             
             // Handle each inner exception
             foreach (var innerEx in ex.InnerExceptions)
             {
                 System.Diagnostics.Debug.WriteLine($"[UNOBSERVED TASK EXCEPTION] Inner Exception: {innerEx.Message}");
                 
                 // Use our extension method to handle the exception
                 _ = innerEx.Handle();
             }
         }
         catch
         {
             // If exception handling itself fails, just log to console
             Console.WriteLine("Critical error in unobserved task exception handler");
         }
     }

    // Override CreateWindow to ensure MainPage is properly initialized
    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Ensure the MainPage is set
        if (MainPage == null)
        {
            MainPage = new Initial(); // Replace `Initial` with your main page class if different
        }

        // Return the new window with the MainPage set
        return new Window(MainPage);
    }
    protected override async void OnStart()
    {
        try
        {
            if (Current.MainPage == null)
            {
                Current.MainPage = new Initial();
            }

            // Clean up all test ads from database on app startup
            _ = Task.Run(async () =>
            {
                try
                {
                    int removedCount = await LocalDatabase.RemoveTestAds();
                    System.Diagnostics.Debug.WriteLine($"[App] Removed {removedCount} test ads from database on startup");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[App] Error removing test ads: {ex.Message}");
                    _ = ex.Handle();
                }
            });

            // Optimize: Apply device-specific optimizations
            StartupExtensions.ApplyDeviceOptimizations();

            // Optimize: Preload essential services in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await StartupExtensions.PreloadEssentialServicesAsync();
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                }
            });
        }
        catch (Exception ex)
        {
            _ = ex.Handle();
        }
    }
    
    protected override async void OnSleep()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[App] OnSleep - Disposing SignalR hubs to prevent memory leaks");
            await PassingCarHubs.DisposeAllHubs();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Error disposing hubs on sleep: {ex.Message}");
            _ = ex.Handle();
        }
        
        base.OnSleep();
    }
    
    protected override async void OnResume()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[App] OnResume - Reconnecting SignalR hubs");
            // Don't automatically reconnect - let the app reconnect when needed
            base.OnResume();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Error on resume: {ex.Message}");
            _ = ex.Handle();
        }
    }
    protected override void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);
        if (!string.IsNullOrEmpty(uri.PathAndQuery))
        {
            string[] splits = uri.PathAndQuery.Split('=');
            if (splits.Length == 2 && splits[0].Contains("Open/Ads?adID"))
            {
                if (int.TryParse(splits[1], out int adId) && adId > 0)
                {
                    // Handle the adId and navigate accordingly
                    MainPage = new Initial(async () =>
                    {
                        await this.GoToAds(adId);
                        await Task.Delay(300);
                        this.ClosePopUp();
                    });
                }
            }
        }
    }
}