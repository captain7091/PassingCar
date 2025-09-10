using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace PassingCar;

class Program : MauiApplication
{
    public Program()
    {
        // Set up Tizen-specific exception handling
        AppDomain.CurrentDomain.UnhandledException += OnTizenUnhandledException;
    }
    
    private static void OnTizenUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            Exception exception = (Exception)e.ExceptionObject;
            
            // Log Tizen-specific exception details
            System.Diagnostics.Debug.WriteLine($"[TIZEN EXCEPTION] Unhandled Exception: {exception.Message}");
            System.Diagnostics.Debug.WriteLine($"[TIZEN EXCEPTION] Stack Trace: {exception.StackTrace}");
            
            // Log to Tizen system log if available
            try
            {
                Tizen.Log.Error("PassingCar", $"Unhandled Exception: {exception.Message}");
                Tizen.Log.Error("PassingCar", $"Stack Trace: {exception.StackTrace}");
            }
            catch
            {
                // If Tizen logging fails, just continue
            }
            
            // Try to recover gracefully if not terminating
            if (!e.IsTerminating)
            {
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
            }
        }
        catch
        {
            // If exception handling itself fails, just log
            System.Diagnostics.Debug.WriteLine("Critical error in Tizen exception handler");
        }
    }
    
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    static void Main(string[] args)
    {
        var app = new Program();
        app.Run(args);
    }
}