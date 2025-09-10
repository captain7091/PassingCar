using Microsoft.UI.Xaml;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PassingCar.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
        
        // Set up Windows-specific exception handling
        this.UnhandledException += OnWindowsUnhandledException;
    }
    
    private void OnWindowsUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        try
        {
            var exception = e.Exception;
            
            // Log Windows-specific exception details
            System.Diagnostics.Debug.WriteLine($"[WINDOWS EXCEPTION] Unhandled Exception: {exception.Message}");
            System.Diagnostics.Debug.WriteLine($"[WINDOWS EXCEPTION] Stack Trace: {exception.StackTrace}");
            
            // Log to Windows Event Log if possible
            try
            {
                System.Diagnostics.EventLog.WriteEntry("PassingCar", $"Unhandled Exception: {exception.Message}\nStack Trace: {exception.StackTrace}", System.Diagnostics.EventLogEntryType.Error);
            }
            catch
            {
                // If we can't write to event log, just continue
            }
            
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
            e.Handled = true;
        }
        catch
        {
            // If exception handling itself fails, just log
            System.Diagnostics.Debug.WriteLine("Critical error in Windows exception handler");
        }
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}