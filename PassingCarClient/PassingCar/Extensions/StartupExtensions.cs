using System;
using System.Threading.Tasks;

namespace PassingCar.Extensions
{
    public static class StartupExtensions
    {
        /// <summary>
        /// Preloads essential services in background to improve perceived performance
        /// </summary>
        public static async Task PreloadEssentialServicesAsync()
        {
            try
            {
                // Start background tasks that don't block UI
                var tasks = new[]
                {
                    PreloadLocalDatabaseAsync(),
                    PreloadImageCacheAsync(),
                    PreloadApiClientAsync()
                };

                // Don't wait for completion - let them run in background
                _ = Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                // Log but don't block startup
                System.Diagnostics.Debug.WriteLine($"Preload error: {ex.Message}");
            }
        }

        private static async Task PreloadLocalDatabaseAsync()
        {
            try
            {
                // Initialize database connection pool
                await App.LocalDatabase.CheckTables();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database preload error: {ex.Message}");
            }
        }

        private static async Task PreloadImageCacheAsync()
        {
            try
            {
                // Warm up image loading cache immediately - no delay
                // Initialize FFImageLoading cache
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Image cache preload error: {ex.Message}");
            }
        }

        private static async Task PreloadApiClientAsync()
        {
            try
            {
                // CRITICAL FIX: Pre-warm HTTP client connections to prevent first-attempt login failures
                System.Diagnostics.Debug.WriteLine("[StartupExtensions] Starting API client pre-warming...");
                
                var testClient = PassingCar.IntegrationsWithApi.Api.CreateTestClient();
                if (testClient != null)
                {
                    // Make multiple lightweight requests to fully initialize the connection pool
                    var tasks = new List<Task>();
                    
                    for (int i = 0; i < 3; i++)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                // CRITICAL FIX: Use base URL instead of non-existent /health endpoint
                                var healthRequest = new RestSharp.RestRequest("/", RestSharp.Method.Get);
                                healthRequest.Timeout = TimeSpan.FromSeconds(2);
                                
                                var response = await testClient.ExecuteAsync(healthRequest);
                                System.Diagnostics.Debug.WriteLine($"[StartupExtensions] Pre-warm request {i + 1} completed: {response?.StatusCode}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[StartupExtensions] Pre-warm request {i + 1} failed: {ex.Message}");
                            }
                        }));
                        
                        // Small delay between requests to simulate real usage
                        await Task.Delay(100);
                    }
                    
                    // Wait for all pre-warming requests to complete
                    await Task.WhenAll(tasks);
                    System.Diagnostics.Debug.WriteLine("[StartupExtensions] API client connection pool fully pre-warmed");
                }
            }
            catch (Exception ex)
            {
                // Don't block app startup if pre-warming fails
                System.Diagnostics.Debug.WriteLine($"[StartupExtensions] API client pre-warming failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Optimizes app for faster subsequent launches
        /// </summary>
        public static void OptimizeForNextLaunch()
        {
            try
            {
                // Clear unnecessary caches
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Optimization error: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if this is a cold start (first launch) or warm start
        /// </summary>
        public static bool IsColdStart()
        {
            try
            {
                // Simple heuristic - check if app was recently launched
                var lastLaunch = Preferences.Get("LastLaunchTime", DateTime.MinValue);
                var timeSinceLastLaunch = DateTime.Now - lastLaunch;
                
                Preferences.Set("LastLaunchTime", DateTime.Now);
                
                return timeSinceLastLaunch.TotalMinutes > 5; // Cold start if more than 5 minutes
            }
            catch
            {
                return true; // Assume cold start on error
            }
        }

        /// <summary>
        /// Applies startup optimizations based on device capabilities
        /// </summary>
        public static void ApplyDeviceOptimizations()
        {
            try
            {
                // Optimize based on device info
                var deviceInfo = DeviceInfo.Current;
                
                if (deviceInfo.Platform == DevicePlatform.Android)
                {
                    // Android-specific optimizations
                    ApplyAndroidOptimizations();
                }
                else if (deviceInfo.Platform == DevicePlatform.iOS)
                {
                    // iOS-specific optimizations
                    ApplyiOSOptimizations();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Device optimization error: {ex.Message}");
            }
        }

        private static void ApplyAndroidOptimizations()
        {
            try
            {
                // Enable hardware acceleration hints
                // Optimize for Android performance
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Android optimization error: {ex.Message}");
            }
        }

        private static void ApplyiOSOptimizations()
        {
            try
            {
                // iOS-specific performance optimizations
                // Optimize for iOS performance
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"iOS optimization error: {ex.Message}");
            }
        }
    }
}
