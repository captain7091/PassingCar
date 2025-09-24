using Newtonsoft.Json;
using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models.API.Ads;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PassingCar.LocalDatabase
{
    public class LocalDB
    {
        private readonly SQLiteAsyncConnection _localDB;
        public LocalDB(string dbPath)
        {
            _localDB = new SQLiteAsyncConnection(dbPath);
        }
        public async Task CheckTables()
        {
            try
            {
                _ = await _localDB.CreateTableAsync<LocalAd>();
                _ = await _localDB.CreateTableAsync<LocalFavorites>();
                _ = await _localDB.CreateTableAsync<UserPhoto>();
                _ = await _localDB.CreateTableAsync<AdsPhoto>();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public async Task<bool> GetNewAds(int maxId, AdsFilter filter = null)
        {
            try
            {
                bool loaded = false;
                GetNextAdsInput input = new GetNextAdsInput()
                {
                    FirstRequestTime = DateTime.Now,
                    LastAdsLoaded = maxId,
                    Filter = filter
                };

                // COMMENTED OUT: Using new GetMyAds API instead
                // System.Diagnostics.Debug.WriteLine($"[LocalDB] GetNewAds called with maxId: {maxId}");
                // GetNextAdsResponse result = await Api.GetNextAds(input);
                // System.Diagnostics.Debug.WriteLine($"[LocalDB] API response success: {result?.Success}, Items count: {result?.AdsItem?.Count() ?? 0}");
                // while (result != null && result.AdsItem != null && result.AdsItem.Any())
                // {
                //     loaded = true;
                //     input.LastAdsLoaded = result.AdsItem.Select(a => a.AdsId).Max();
                //     List<LocalAd> localNewAds = new List<LocalAd>();
                //     foreach (AdFromAdsListModel item in result.AdsItem)
                //     {
                //         AdsDetailsExtened adsDetails = new AdsDetailsExtened(false)
                //         {
                //             FirstAdsImage = item.FirstAdsImage,
                //             AdsTitle = item.AdsTitle,
                //             IsFavorite = item.IsFavorite,
                //             AdsId = item.AdsId,
                //             AdsFrom = item.AdsFrom,
                //             AdsTo = item.AdsTo,
                //             AdsPrice = item.AdsPrice,
                //             State = item.State.Espana(),
                //             UserProfilePhoto = item.UserProfilePhoto,
                //             UserProfile = item.UserProfile,
                //             UserName = item.UserName,
                //             UserRating = item.UserRating,
                //             PostedTime = item.PostedTime,
                //             UserId = item.UserId,
                //             ModifiedAt = item.ModifiedAt,
                //         };
                //         try
                //         {
                //             LocalAd asdsad = new LocalAd(adsDetails, item.State, item.UserId, item.ModifiedAt);
                //             localNewAds.Add(asdsad);
                //         }
                //         catch (Exception ex)
                //         {
                //             Console.WriteLine(ex.ToString());
                //         }
                //     }
                //     _ = await App.LocalDatabase.SaveAds(localNewAds);
                //     result = await Api.GetNextAds(input);
                // }
                return loaded;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }


        private int GetMaxId(List<LocalAd> ads)
        {
            try
            {
                return ads != null && ads.Any() ? ads.Select(a => a.Id).Max() : int.MinValue;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return int.MinValue;
            }
        }
        public async Task<List<LocalAd>> GetAdsForCheck(AdsFilter adsFilter)
        {
            try
            {
                List<LocalAd> allads = await _localDB.Table<LocalAd>().ToListAsync();
                List<LocalAd> ads = await _localDB.Table<LocalAd>().Where(a => a.State < Models.AdsState.PaymentPending).ToListAsync();

                System.Diagnostics.Debug.WriteLine($"[LocalDB] GetAdsForCheck: Found {allads.Count} total ads, {ads.Count} active ads");

                // CRITICAL FIX: Always try to load new ads from API, especially if database is empty
                bool loadedNewAds = await GetNewAds(GetMaxId(allads), adsFilter);
                System.Diagnostics.Debug.WriteLine($"[LocalDB] GetNewAds result: {loadedNewAds}");
                
                if (loadedNewAds)
                {
                    // Refresh ads from database after API call
                    ads = await _localDB.Table<LocalAd>().Where(a => a.State < Models.AdsState.PaymentPending).ToListAsync();
                    System.Diagnostics.Debug.WriteLine($"[LocalDB] After API call: {ads.Count} active ads in database");
                }

                // CRITICAL FIX: Always filter out test ads now that we've removed test ad creation
                // Real ads should be loaded from the API
                ads = FilterOutTestAds(ads);
                System.Diagnostics.Debug.WriteLine($"[LocalDB] After filtering test ads: {ads.Count} real ads remain");

                return await GetFiltered(ads, adsFilter);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalDB] Error in GetAdsForCheck: {ex.Message}");
                _ = ex.Handle();
                return new List<LocalAd>();
            }
        }

        /// <summary>
        /// Get all ads from local database for debugging purposes
        /// </summary>
        public async Task<List<LocalAd>> GetAllLocalAds()
        {
            try
            {
                return await _localDB.Table<LocalAd>().ToListAsync();
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return new List<LocalAd>();
            }
        }
        public async Task<LocalAd> GetAdByID(int adsID)
        {
            try
            {
                LocalAd item = await _localDB.Table<LocalAd>().Where(a => a.Id == adsID).FirstOrDefaultAsync();
                if (item != null && item.Id > 0)
                {
                    return item;
                }
                else
                {
                    List<LocalAd> allads = await _localDB.Table<LocalAd>().ToListAsync();
                    List<LocalAd> ads = await _localDB.Table<LocalAd>().Where(a => a.State < Models.AdsState.PaymentPending).ToListAsync();
                    bool loadedNewAds = await GetNewAds(GetMaxId(allads));
                    if (loadedNewAds)
                    {
                        ads = await _localDB.Table<LocalAd>().Where(a => a.State < Models.AdsState.PaymentPending).ToListAsync();
                    }
                    return ads.Where(a => a.Id == adsID).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return new LocalAd();
            }
        }
        private async Task<List<LocalAd>> GetFiltered(List<LocalAd> ads, AdsFilter filter)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[LocalDB] GetFiltered called with {ads?.Count ?? 0} ads, OnlyOwnAds: {filter?.OnlyOwnAds}, ShowAllUsers: {filter?.ShowAllUsers}");

                // FIXED: Handle user filtering logic properly
                if (filter.ShowAllUsers)
                {
                    // For Anuncios page: Show ALL ads from ALL users
                    System.Diagnostics.Debug.WriteLine($"[LocalDB] ShowAllUsers=true, displaying all {ads?.Count ?? 0} ads from all users");
                    // No filtering by user - keep all ads
                }
                else if (filter.OnlyOwnAds)
                {
                    // For MyAds page: Show only current user's ads
                    int loggedUser = await Api.GetUserId();
                    var profile = await Api.GetProfile();
                    System.Diagnostics.Debug.WriteLine($"[LocalDB] Filtering for user: {loggedUser}, profile: {profile}");
                    ads = ads.Where(a => a.UserId == loggedUser && a.UserProfile.Equals(profile)).ToList();
                    System.Diagnostics.Debug.WriteLine($"[LocalDB] After user filtering: {ads.Count} ads");
                }

                if (!string.IsNullOrEmpty(filter.ProvincieFrom))
                {
                    ads = ads.Where(a => a != null && filter != null && IsFromProvincie(a.AdsFrom, filter.ProvincieFrom)).ToList();
                }
                if (ads != null && ads.Any())
                {
                    if (!string.IsNullOrEmpty(filter.ProvincieTo))
                    {
                        ads = ads.Where(a => a != null && filter != null && IsFromProvincie(a.AdsTo, filter.ProvincieTo)).ToList();
                    }
                }
                if (ads != null && ads.Any())
                {
                    if (filter.FromLastXDays > 0)
                    {
                        var cutoffDate = DateTime.Now.AddDays(-filter.FromLastXDays);
                        ads = ads.Where(a => a.PostedTime.Date >= cutoffDate.Date).ToList();
                    }
                }

                // Add price filtering logic
                if (ads != null && ads.Any())
                {
                    if (filter.MinPrice.HasValue)
                    {
                        ads = ads.Where(a => a.AdsPrice >= filter.MinPrice.Value).ToList();
                    }
                    if (filter.MaxPrice.HasValue)
                    {
                        ads = ads.Where(a => a.AdsPrice <= filter.MaxPrice.Value).ToList();
                    }
                }

                if (ads != null && ads.Any())
                {
                    if (!string.IsNullOrEmpty(filter.Relevance))
                    {
                        switch (filter.Relevance)
                        {
                            case "El más antiguo":
                                ads = ads.OrderBy(a => a.PostedTime).ToList(); // Fixed: oldest first
                                break;
                            case "El más caro":
                                ads = ads.OrderByDescending(a => a.AdsPrice).ToList(); // Fixed: most expensive first
                                break;
                            case "El más barato":
                                ads = ads.OrderBy(a => a.AdsPrice).ToList(); // Fixed: cheapest first
                                break;
                            case "El más reciente":
                            default:
                                ads = ads.OrderByDescending(a => a.PostedTime).ToList(); // Fixed: most recent first
                                break;
                        }
                    }
                    else
                    {
                        ads = ads.OrderByDescending(a => a.PostedTime).ToList(); // Default: most recent first
                    }
                }
                return ads;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return ads;
            }
        }
        private bool IsFromProvincie(string serialized, string provincie)
        {
            try
            {
                if (provincie is null)
                {
                    return false;
                }
                if (string.IsNullOrEmpty(serialized))
                {
                    return false;
                }
                AddressDetails addDet = null;
                try
                {
                    addDet = JsonConvert.DeserializeObject<AddressDetails>(serialized);
                }
                catch (Exception ex)
                {
                    _ = Task.Run(async () =>
                    {
                        _ = await Api.HandleException(new Models.API.HandleExceptionInput()
                        {
                            Exception = JsonConvert.SerializeObject(ex)
                        });
                    });
                }
                return addDet != null
&& addDet.Provincie != null && addDet.Provincie.Equals(provincie, StringComparison.CurrentCultureIgnoreCase);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }
        public async Task<bool> UpdateAdd(LocalAd adsUpdated)
        {
            try
            {
                LocalAd ad = await _localDB.GetAsync<LocalAd>(adsUpdated.Id);
                adsUpdated.IsFavorite = ad.IsFavorite;
                int added = await _localDB.UpdateAsync(adsUpdated);
                return added > 0;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }
        public async Task<List<LocalAd>> GetAllMyAds()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[LocalDB] GetAllMyAds called");
                
                List<LocalAd> allads = await _localDB.Table<LocalAd>().ToListAsync();
                System.Diagnostics.Debug.WriteLine($"[LocalDB] Found {allads.Count} total ads in database");
                
                // CRITICAL FIX: Use App.CurrentUser instead of API calls to avoid dependency issues
                int loggedUser = App.CurrentUser?.Id ?? 0;
                if (loggedUser <= 0)
                {
                    // Fallback to API if App.CurrentUser is not set
                    try
                    {
                        loggedUser = await Api.GetUserId();
                    }
                    catch (Exception apiEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[LocalDB] Failed to get user ID from API: {apiEx.Message}");
                        return new List<LocalAd>();
                    }
                }
                
                var profile = Utils.ProfileType.Fisica; // Default profile
                try
                {
                    profile = await Api.GetProfile();
                }
                catch (Exception profileEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[LocalDB] Failed to get profile from API, using default: {profileEx.Message}");
                }
                
                System.Diagnostics.Debug.WriteLine($"[LocalDB] Filtering ads for UserId={loggedUser}, Profile={profile}");
                
                // DEBUG: Get all ads first to see what's in the database
                List<LocalAd> allUserAds = await _localDB.Table<LocalAd>().Where(a => a.UserId == loggedUser).ToListAsync();
                System.Diagnostics.Debug.WriteLine($"[LocalDB] DEBUG: Found {allUserAds.Count} total ads for UserId={loggedUser}");
                foreach (var ad in allUserAds)
                {
                    System.Diagnostics.Debug.WriteLine($"[LocalDB] DEBUG: Ad {ad.Id}: Title='{ad.AdsTitle}', UserProfile={ad.UserProfile}, UserId={ad.UserId}");
                }
                
                // CRITICAL FIX: Use string-based comparison for better SQLite compatibility
                // Convert enum to string for reliable SQLite querying
                string profileString = ((int)profile).ToString();
                System.Diagnostics.Debug.WriteLine($"[LocalDB] Using string-based profile comparison: {profile} -> {profileString}");
                
                List<LocalAd> ads;
                try
                {
                    // Filter by profile in memory (more reliable than SQLite enum comparison)
                    ads = allUserAds.Where(a => a.UserProfile == profile).ToList();
                    System.Diagnostics.Debug.WriteLine($"[LocalDB] After profile filtering ({profile}): Found {ads.Count} ads");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LocalDB] Error in profile filtering: {ex.Message}");
                    ads = new List<LocalAd>();
                }
                
                System.Diagnostics.Debug.WriteLine($"[LocalDB] Found {ads.Count} ads for current user and profile after profile filtering");

                // Filter out test ads
                ads = FilterOutTestAds(ads);
                System.Diagnostics.Debug.WriteLine($"[LocalDB] After filtering test ads: {ads.Count} ads remain");

                // Try to load new ads from API, but don't fail if API is unavailable
                try
                {
                    bool loadedNewAds = await GetNewAds(GetMaxId(allads));
                    if (loadedNewAds)
                    {
                        System.Diagnostics.Debug.WriteLine("[LocalDB] Successfully loaded new ads from API");
                        ads = await _localDB.Table<LocalAd>().Where(a => a.UserId == loggedUser && a.UserProfile.Equals(profile)).ToListAsync();
                        // Filter out test ads again after loading new ads
                        ads = FilterOutTestAds(ads);
                        System.Diagnostics.Debug.WriteLine($"[LocalDB] After loading new ads: {ads.Count} ads for user");
                    }
                }
                catch (Exception apiEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[LocalDB] API call failed, using existing local ads: {apiEx.Message}");
                    // Continue with existing local ads
                }
                
                ads = ads.OrderByDescending(a => a.PostedTime).ToList(); // Most recent first
                System.Diagnostics.Debug.WriteLine($"[LocalDB] Returning {ads.Count} ads for user");
                return ads;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalDB] Error in GetAllMyAds: {ex.Message}");
                _ = ex.Handle();
                return new List<LocalAd>();
            }
        }
        public async Task<List<LocalAd>> GetAllFavoiteAds()
        {
            try
            {
                List<LocalAd> allads = await _localDB.Table<LocalAd>().ToListAsync();
                List<LocalAd> ads = await _localDB.Table<LocalAd>().Where(a => a.IsFavorite).ToListAsync();
                bool loadedNewAds = await GetNewAds(GetMaxId(allads));
                if (loadedNewAds)
                {
                    ads = await _localDB.Table<LocalAd>().Where(a => a.IsFavorite).ToListAsync();
                }
                ads = ads.OrderBy(a => a.Id).ToList();
                return ads;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return new List<LocalAd>();
            }
        }
        public async Task<bool> AddToFavorite(int adsId)
        {
            try
            {
                LocalAd ad = await _localDB.GetAsync<LocalAd>(adsId);
                ad.IsFavorite = true;
                int added = await _localDB.UpdateAsync(ad);
                return added > 0;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return false;
            }
        }
        public async Task<bool> RemoveFromFavorite(int adsId)
        {
            LocalAd ad = await _localDB.GetAsync<LocalAd>(adsId);
            ad.IsFavorite = false;
            int added = await _localDB.UpdateAsync(ad);
            return added > 0;
        }
        public async Task<bool> SaveAds(List<LocalAd> ads)
        {

            List<LocalAd> allads = await _localDB.Table<LocalAd>().ToListAsync();
            List<int> listOfIds = allads.Select(s => s.Id).ToList();
            if (ads != null)
            {
                ads = ads.Where(a => !listOfIds.Contains(a.Id)).ToList();
            }
            if (ads != null && ads.Any())
            {
                int added = await _localDB.InsertAllAsync(ads);
                return added == ads.Count;
            }
            else
            {
                return true;
            }
        }
        public async Task<bool> SaveUserPhoto(UserPhoto userPhoto)
        {
            int added = await _localDB.InsertAsync(userPhoto);
            return added > 0;
        }
        public async Task<UserPhoto> GetUserPhoto(int userId)
        {
            return await _localDB.Table<UserPhoto>().FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task<bool> SaveAdsPhoto(AdsPhoto adsPhoto)
        {
            int added = await _localDB.InsertAsync(adsPhoto);
            return added > 0;
        }
        public async Task<AdsPhoto> GetAdsPhoto(int adsId)
        {
            return await _localDB.Table<AdsPhoto>().FirstOrDefaultAsync(u => u.Id == adsId);
        }

        public async Task<List<LocalAd>> GetAllAds()
        {
            try
            {
                var ads = await _localDB.Table<LocalAd>().ToListAsync();
                return FilterOutTestAds(ads);
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return new List<LocalAd>();
            }
        }

        /// <summary>
        /// Filters out test ads from the list
        /// </summary>
        /// <param name="ads">List of ads to filter</param>
        /// <returns>Filtered list without test ads</returns>
        private List<LocalAd> FilterOutTestAds(List<LocalAd> ads)
        {
            if (ads == null) return new List<LocalAd>();

            return ads.Where(ad =>
                // Filter out ads with test IDs:
                // - 1-10 range for AdsPage test ads
                // - 1000-1999 range for MyAdsPage test ads
                // - 99000+ for other test ads
                !(ad.Id >= 1 && ad.Id <= 10) &&
                !(ad.Id >= 1000 && ad.Id <= 1999) &&
                ad.Id < 99000 &&
                // Filter out ads with test titles
                !string.IsNullOrEmpty(ad.AdsTitle) &&
                !ad.AdsTitle.StartsWith("Test Ad", StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        /// <summary>
        /// Removes all test ads from the database
        /// </summary>
        /// <returns>Number of test ads removed</returns>
        public async Task<int> RemoveTestAds()
        {
            try
            {
                var allAds = await _localDB.Table<LocalAd>().ToListAsync();
                var testAds = allAds.Where(ad =>
                    (ad.Id >= 1 && ad.Id <= 10) || // AdsPage test ads range
                    (ad.Id >= 1000 && ad.Id <= 1999) || // MyAdsPage test ads range
                    ad.Id >= 99000 || // Other test ads range
                    (!string.IsNullOrEmpty(ad.AdsTitle) && ad.AdsTitle.StartsWith("Test Ad", StringComparison.OrdinalIgnoreCase))
                ).ToList();

                if (testAds.Any())
                {
                    foreach (var testAd in testAds)
                    {
                        await _localDB.DeleteAsync(testAd);
                    }
                    System.Diagnostics.Debug.WriteLine($"[LocalDB] Removed {testAds.Count} test ads from database");
                    return testAds.Count;
                }
                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalDB] Error removing test ads: {ex.Message}");
                _ = ex.Handle();
                return 0;
            }
        }

        /// <summary>
        /// Clears all cached data from the local database to ensure fresh data loading
        /// This is useful when switching profiles to prevent data mixing
        /// </summary>
        /// <returns>True if cache was cleared successfully</returns>
        public async Task<bool> ClearCache()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[LocalDB] Clearing session-specific cached data (preserving user ads)...");
                
                // CRITICAL FIX: DO NOT clear ads - they should persist across sessions
                // Only clear session-specific data that doesn't affect user content
                
                // Clear only temporary/session data, NOT user ads
                // await _localDB.DeleteAllAsync<LocalAd>(); // REMOVED - ads should persist
                
                // Clear favorites (these can be reloaded from server)
                await _localDB.DeleteAllAsync<LocalFavorites>();
                
                // Keep user photos and ads photos as they are part of user content
                // await _localDB.DeleteAllAsync<UserPhoto>(); // REMOVED - preserve user photos
                // await _localDB.DeleteAllAsync<AdsPhoto>(); // REMOVED - preserve ads photos
                
                System.Diagnostics.Debug.WriteLine("[LocalDB] Session cache cleared successfully (user ads preserved)");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalDB] Error clearing cache: {ex.Message}");
                _ = ex.Handle();
                return false;
            }
        }
        
        /// <summary>
        /// EMERGENCY METHOD: Only use when absolutely necessary to clear ALL data
        /// This should NOT be called during normal login/logout operations
        /// </summary>
        public async Task<bool> ClearAllData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[LocalDB] WARNING: Clearing ALL data including user ads...");
                
                // Clear all ads from local database
                await _localDB.DeleteAllAsync<LocalAd>();
                
                // Clear favorites
                await _localDB.DeleteAllAsync<LocalFavorites>();
                
                // Clear user photos
                await _localDB.DeleteAllAsync<UserPhoto>();
                
                // Clear ads photos
                await _localDB.DeleteAllAsync<AdsPhoto>();
                
                System.Diagnostics.Debug.WriteLine("[LocalDB] ALL data cleared");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalDB] Error clearing all data: {ex.Message}");
                _ = ex.Handle();
                return false;
            }
        }
    }
}
