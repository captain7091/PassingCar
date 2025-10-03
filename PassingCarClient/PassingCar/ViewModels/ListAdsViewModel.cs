using PassingCar.Extensions;
using PassingCar.Hubs;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models.API.Ads;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace PassingCar.ViewModels
{
    public class ListAdsViewModel : BaseViewModel
    {
        public ObservableCollection<AdsDetailsExtened> Adss { get; set; }
        public ObservableCollection<AdsDetailsExtened> AllAds { get; set; }
        public ObservableCollection<AdsParentExtended> AdsOffer { get; set; }

        

        public bool KeepLoading { get; set; }
        public AdsFilter AdsFilter { get; set; }
        private readonly ContentView smallLoading;
        public bool onlyFavourites;
        private readonly bool onlyMyAds;

        private readonly INavigation navigation;
        private readonly AdFromAdsListModel lastLoaded;

        public ListAdsViewModel(ContentView smallLoading, bool onlyFavourites = false, INavigation navigation = null, bool onlyMyAds = false)
        {
            try
            {
                //handle exception
                this.onlyFavourites = onlyFavourites;
                this.onlyMyAds = onlyMyAds;
                Adss = new ObservableCollection<AdsDetailsExtened>();
                AllAds = new ObservableCollection<AdsDetailsExtened>();

                this.smallLoading = smallLoading;
                this.navigation = navigation;

                PassingCarHubs.UpdateEvent(PassingCarHubs.Ads.AdsStateChanged, (input) =>
                {
                    ChangeAdsStateInput item = input as ChangeAdsStateInput;
                    IEnumerable<AdsDetailsExtened> thisAdss = Adss.Where(a => a.AdsId == item.AdId);
                    if (this.onlyMyAds || this.onlyFavourites)
                    {
                        if (thisAdss != null && thisAdss.Count() > 0)
                        {
                            foreach (AdsDetailsExtened ad in thisAdss)
                            {
                                Adss[Adss.IndexOf(ad)].State = item.State.ToString();
                            }
                        }
                    }
                    else
                    {
                        // FIXED: Remove ads with state >= 3 (AcceptedForTransit and above) from Anuncios screen - these are ads where payment has been completed
                        if (item.State >= Models.AdsState.AcceptedForTransit)
                        {
                            if (thisAdss != null && thisAdss.Count() > 0)
                            {
                                foreach (AdsDetailsExtened ad in thisAdss)
                                {
                                    Adss.RemoveAt(Adss.IndexOf(ad));
                                    OnPropertyChanged(nameof(Adss));
                                }
                            }
                        }
                    }
                });
                lastLoaded = new AdFromAdsListModel()
                {
                    AdsId = int.MinValue
                };
                PassingCarHubs.UpdateEvent(PassingCarHubs.Ads.NewAds, async (input) =>
                {
                    if (input is AdFromAdsListModel item)
                    {
                        int loggedUser = await Api.GetUserId();
                        if (!onlyFavourites && (!onlyMyAds || loggedUser == item.UserId))
                        {
                            Adss.Insert(0, new AdsDetailsExtened(onlyFavourites)
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
                                UserName = item.UserName,
                                UserRating = item.UserRating,
                                PostedTime = item.PostedTime,
                                UserId = item.UserId,
                                UserProfile = item.UserProfile,
                                ModifiedAt = item.ModifiedAt,
                            });
                        }
                    }
                });
                AdsFilter = new AdsFilter();
                KeepLoading = true;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }  
        public async Task CustomLoad(AdsFilter filter)
        {
            try
            {
                //handle exception
                // Copy all filter properties to ensure they are properly passed through
                AdsFilter.Active = filter.Active;
                AdsFilter.OnlyOwnAds = filter.OnlyOwnAds;
                AdsFilter.OnlyFavorites = filter.OnlyFavorites;
                AdsFilter.FromLastXDays = filter.FromLastXDays;
                AdsFilter.ProvincieFrom = filter.ProvincieFrom;
                AdsFilter.ProvincieTo = filter.ProvincieTo;
                AdsFilter.UserType = filter.UserType;
                AdsFilter.OnlyNextOne = filter.OnlyNextOne;
                AdsFilter.Relevance = filter.Relevance;
                AdsFilter.MinPrice = filter.MinPrice;
                AdsFilter.MaxPrice = filter.MaxPrice;
                AdsFilter.PriceRange = filter.PriceRange;
                AdsFilter.ShowAllUsers = filter.ShowAllUsers;
                DateTime reqTime = DateTime.Now;
                List<LocalDatabase.LocalAd> localAds = await App.LocalDatabase.GetAdsForCheck(filter);
                // COMMENTED OUT: Using new GetMyAds API instead
                 GetNextAdsResponse updatedAds = await Api.CheckAdsUpdates(localAds);
                localAds = await localAds.Update(updatedAds);
                Adss.Clear();
                AllAds.Clear();
                if (localAds != null && localAds.Any())
                {
                    foreach (LocalDatabase.LocalAd item in localAds)
                    {
                        // Create the ad details object
                        var adDetails = new AdsDetailsExtened(filter.OnlyFavorites)
                        {
                            FirstAdsImage = item.GetFirstPhoto(),
                            AdsTitle = item.AdsTitle,
                            IsFavorite = item.IsFavorite,
                            AdsId = item.Id,
                            AdsFrom = item.AdsFrom,
                            AdsTo = item.AdsTo,
                            AdsPrice = item.AdsPrice,
                            State = item.State.Espana(),
                            UserProfilePhoto = item.GetUserProfilePhoto(),
                            UserName = item.Username,
                            UserRating = item.UserRating,
                            PostedTime = item.PostedTime,
                            UserId = item.UserId,
                            UserProfile = item.UserProfile,
                            ModifiedAt = item.ModifiedAt,
                        };

                        // Always add to AllAds for filtering purposes
                        AllAds.Add(adDetails);

                        // Only add to Adss if it meets the display criteria
                        // FIXED: Exclude ads with state >= 3 (AcceptedForTransit and above) from Anuncios screen - these are ads where payment has been completed
                        IEnumerable<AdsDetailsExtened> where = Adss.Where(a => a.AdsId == item.Id);
                        if (where != null && where.Any())
                        {
                            if (item.State >= Models.AdsState.AcceptedForTransit)
                            {
                                _ = Adss.Remove(where.First());
                            }
                        }
                        else if (item.State < Models.AdsState.AcceptedForTransit)
                        {
                            Adss.Insert(0, adDetails);
                        }
                    }
                }

                smallLoading.IsVisible = false;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public async Task LoadFavorites()
        {
            try
            {
                //handle exception
                Adss.Clear();
                List<LocalDatabase.LocalAd> localAds = await App.LocalDatabase.GetAllFavoiteAds();
                // COMMENTED OUT: Using new GetMyAds API instead
                 GetNextAdsResponse updatedAds = await Api.CheckAdsUpdates(localAds);
                localAds = await localAds.Update(updatedAds);
                if (localAds != null && localAds.Any())
                {
                    foreach (LocalDatabase.LocalAd item in localAds)
                    {
                        IEnumerable<AdsDetailsExtened> where = Adss.Where(a => a.AdsId == item.Id);
                        if (where != null && where.Any())
                        {
                            if (!item.State.Espana().Equals(where.First().State))
                            {
                                int index = Adss.IndexOf(where.First());
                                Adss.RemoveAt(index);
                                Adss.Insert(index, new AdsDetailsExtened(false)
                                {
                                    FirstAdsImage = item.GetFirstPhoto(),
                                    AdsTitle = item.AdsTitle,
                                    IsFavorite = item.IsFavorite,
                                    AdsId = item.Id,
                                    AdsFrom = item.AdsFrom,
                                    AdsTo = item.AdsTo,
                                    AdsPrice = item.AdsPrice,
                                    State = item.State.Espana(),
                                    UserProfilePhoto = item.GetUserProfilePhoto(),
                                    UserName = item.Username,
                                    UserRating = item.UserRating,
                                    PostedTime = item.PostedTime,
                                    UserId = item.UserId,
                                    UserProfile = item.UserProfile,
                                    ModifiedAt = item.ModifiedAt,
                                });
                            }
                        }
                        else
                        {
                            Adss.Insert(0, new AdsDetailsExtened(true)
                            {
                                FirstAdsImage = item.GetFirstPhoto(),
                                AdsTitle = item.AdsTitle,
                                IsFavorite = item.IsFavorite,
                                AdsId = item.Id,
                                AdsFrom = item.AdsFrom,
                                AdsTo = item.AdsTo,
                                AdsPrice = item.AdsPrice,
                                State = item.State.Espana(),
                                UserProfilePhoto = item.GetUserProfilePhoto(),
                                UserName = item.Username,
                                UserRating = item.UserRating,
                                PostedTime = item.PostedTime,
                                UserId = item.UserId,
                                UserProfile = item.UserProfile,
                                ModifiedAt = item.ModifiedAt,
                            });
                        }

                    }
                }
                AllAds = Adss;

                smallLoading.IsVisible = false;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public async Task LoadMyAds()
        {
            try
            {
                //handle exception
                List<LocalDatabase.LocalAd> localAds = await App.LocalDatabase.GetAllMyAds();
                // COMMENTED OUT: Using new GetMyAds API instead
                 GetNextAdsResponse updatedAds = await Api.CheckAdsUpdates(localAds);
                localAds = await localAds.Update(updatedAds);
                AllAds.Clear();
                if (localAds != null && localAds.Any())
                {
                    foreach (LocalDatabase.LocalAd item in localAds)
                    {
                        // Create the ad details object
                        var adDetails = new AdsDetailsExtened(false)
                        {
                            FirstAdsImage = item.GetFirstPhoto(),
                            AdsTitle = item.AdsTitle,
                            IsFavorite = item.IsFavorite,
                            AdsId = item.Id,
                            AdsFrom = item.AdsFrom,
                            AdsTo = item.AdsTo,
                            AdsPrice = item.AdsPrice,
                            State = item.State.Espana(),
                            UserProfilePhoto = item.GetUserProfilePhoto(),
                            UserName = item.Username,
                            UserRating = item.UserRating,
                            PostedTime = item.PostedTime,
                            UserId = item.UserId,
                            UserProfile = item.UserProfile,
                            ModifiedAt = item.ModifiedAt,
                        };

                        // Always add to AllAds for filtering purposes
                        AllAds.Add(adDetails);

                        // Update or add to Adss for display
                        IEnumerable<AdsDetailsExtened> where = Adss.Where(a => a.AdsId == item.Id);
                        if (where != null && where.Any())
                        {
                            if (!item.State.Espana().Equals(where.First().State))
                            {
                                int index = Adss.IndexOf(where.First());
                                Adss.RemoveAt(index);
                                Adss.Insert(index, adDetails);
                            }
                        }
                        else
                        {
                            Adss.Insert(0, adDetails);
                        }
                    }
                }

                smallLoading.IsVisible = false;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public void KeepSync()
        {
            //if (App.AdsHub != null)
            //{
            //    App.AdsHub.On<AdsDetails>("NewAds", (item) =>
            //    {
            //        if (!Adss.Any(a => a.Equals(item)))
            //        {
            //            Adss.Add(new AdsDetailsExtened(Adss)
            //            {
            //                User = item.User,
            //                Ads = item.Ads,
            //                IsFavorite = item.IsFavorite,
            //            });
            //        }
            //    });
            //}
        }

        /// <summary>
        /// Clears all collections to prevent data mixing between profile switches
        /// </summary>
        public void ClearCollections()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[ListAdsViewModel] Clearing collections for profile switch...");
                Adss?.Clear();
                AllAds?.Clear();
                AdsOffer?.Clear();
                OnPropertyChanged(nameof(Adss));
                OnPropertyChanged(nameof(AllAds));
                OnPropertyChanged(nameof(AdsOffer));
                System.Diagnostics.Debug.WriteLine("[ListAdsViewModel] Collections cleared successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ListAdsViewModel] Error clearing collections: {ex.Message}");
                _ = ex.Handle();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[ListAdsViewModel] Disposing resources...");
                    
                    // Clear collections to release references
                    ClearCollections();
                    
                    // Note: Hub event subscriptions are managed globally by PassingCarHubs
                    // Individual ViewModels don't need to unsubscribe as the hub manages the lifecycle
                    
                    System.Diagnostics.Debug.WriteLine("[ListAdsViewModel] Resources disposed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ListAdsViewModel] Error during disposal: {ex.Message}");
                    _ = ex.Handle();
                }
            }
            base.Dispose(disposing);
        }
    }
}
