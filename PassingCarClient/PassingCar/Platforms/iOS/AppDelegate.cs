using Foundation;
using ObjCRuntime;
using UIKit;

namespace PassingCar;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // Set up iOS-specific exception handling
        Runtime.MarshalObjectiveCException += OnMarshalObjectiveCException;
        
        return base.FinishedLaunching(application, launchOptions);
    }
    
    private void OnMarshalObjectiveCException(object sender, MarshalObjectiveCExceptionEventArgs args)
    {
        try
        {
            var exception = args.Exception;
            
            // Log iOS-specific exception details
            System.Diagnostics.Debug.WriteLine($"[iOS EXCEPTION] Objective-C Exception: {exception.Message}");
            System.Diagnostics.Debug.WriteLine($"[iOS EXCEPTION] Stack Trace: {exception.StackTrace}");
            
            // Log to iOS console
            Foundation.NSLog($"PassingCar iOS Exception: {exception.Message}");
            Foundation.NSLog($"Stack Trace: {exception.StackTrace}");
            
            // Try to recover gracefully
            Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
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
            
            // Mark as handled to prevent app termination if possible
            args.ExceptionMode = MarshalObjectiveCExceptionMode.Default;
        }
        catch
        {
            // If exception handling itself fails, just log
            Foundation.NSLog("Critical error in iOS exception handler");
        }
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}