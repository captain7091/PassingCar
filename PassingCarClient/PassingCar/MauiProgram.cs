using Auth0.OidcClient;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Mopups.Interfaces;
using Mopups.Services;
using SkiaSharp.Views.Maui.Controls.Hosting;
using MauiEx;
using FFImageLoading.Maui;
#if IOS
using PassingCar.CustomServices;
#endif
using PassingCar.Services;
using PassingCar.ViewModels;

namespace PassingCar;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseFFImageLoading()
            .UseSkiaSharp()
            .UseMauiEx()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("Rajdhani-Bold.ttf", "RajBold");
                fonts.AddFont("Rajdhani-Light.ttf", "RajLight");
                fonts.AddFont("Rajdhani-Medium.ttf", "RajMedium");
                fonts.AddFont("Rajdhani-Regular.ttf", "RajRegular");
                fonts.AddFont("Rajdhani-SemiBold.ttf", "RajSemiBold");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Optimize: Configure for faster startup
        // Enable hardware acceleration and performance optimizations

#if DEBUG
        builder.Logging.AddDebug();
        builder.UseMauiApp<App>();
        builder.UseMauiCommunityToolkit();
        
        // Configure logging for better exception tracking
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
#else
        // In release mode, configure minimal logging
        builder.Logging.SetMinimumLevel(LogLevel.Error);
#endif
        
        // Configure exception handling
        builder.ConfigureMauiHandlers(handlers =>
        {
            // Add custom exception handling for handlers if needed
        });
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<IPopupNavigation>(MopupService.Instance);
        builder.Services.AddSingleton(new Auth0Client(new Auth0ClientOptions
        {
            Domain = "dev-563o20p6vsl70s1g.us.auth0.com",
            ClientId = "s51ALsQODGpouubmCgRcKUvCdwefeWhA",
            RedirectUri = "passingcar://callback/",
            PostLogoutRedirectUri = "passingcar://callback/",
            Scope = "openid profile email"
        }));
        // Register your services here
#if IOS
        builder.Services.AddSingleton<IAppleSignIn, AppleSignInService>();
              
#endif
        return builder.Build();
    }
}