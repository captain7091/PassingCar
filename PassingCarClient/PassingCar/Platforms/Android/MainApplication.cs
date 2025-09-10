using Android.App;
using Android.Runtime;

namespace PassingCar;

[Application(UsesCleartextTraffic = true)]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        // Set up Android-specific exception handling
        AndroidEnvironment.UnhandledExceptionRaiser += OnAndroidUnhandledException;
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    
    private void OnAndroidUnhandledException(object sender, RaiseThrowableEventArgs e)
    {
        try
        {
            var exception = e.Exception;
            
            // Log Android-specific exception details
            System.Diagnostics.Debug.WriteLine($"[ANDROID EXCEPTION] Exception: {exception.Message}");
            System.Diagnostics.Debug.WriteLine($"[ANDROID EXCEPTION] Stack Trace: {exception.StackTrace}");
            
            // Log to Android log
            Android.Util.Log.Error("PassingCar", $"Unhandled Exception: {exception.Message}");
            Android.Util.Log.Error("PassingCar", $"Stack Trace: {exception.StackTrace}");
            
            // Mark as handled to prevent app termination if possible
            e.Handled = true;
            
            // Try to recover gracefully
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (Microsoft.Maui.Controls.Application.Current?.MainPage != null)
                    {
                        Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
                            "Error", 
                            "An unexpected error occurred. The app will attempt to recover.", 
                            "OK");
                    }
                }
                catch
                {
                    // If we can't show alert, just continue
                }
            });
        }
        catch
        {
            // If exception handling itself fails, just log
            Android.Util.Log.Error("PassingCar", "Critical error in Android exception handler");
        }
    }
}