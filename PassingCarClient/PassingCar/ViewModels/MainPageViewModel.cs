using PassingCar.Extensions;
using PassingCar.IntegrationsWithApi;
using PassingCar.Models;
using PassingCar.Models.API.Ads;
using PassingCar.Utils;
using PassingCar.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;


namespace PassingCar.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private ImageSource profilePhoto;
        public ImageSource ProfileImageSource
        {
            get => profilePhoto;
            set
            {
                try
                {
                    //handle exception
                    profilePhoto = value;
                    OnPropertyChanged(nameof(ProfileImageSource));
                }
                catch (Exception ex)
                {
                    _ = ex.Handle();
                }
            }
        }
        private string _userName;
        public string UserName 
        { 
            get => _userName; 
            set 
            { 
                _userName = value; 
                OnPropertyChanged(nameof(UserName)); 
            } 
        }
        
        private string _profileType;
        public string ProfileType 
        { 
            get => _profileType; 
            set 
            { 
                _profileType = value; 
                OnPropertyChanged(nameof(ProfileType)); 
            } 
        }
        public ObservableCollection<AdsDetailsExtened> MyAdss { get; set; }
        public ObservableCollection<NotificationExtended> Notifications { get; set; }
        public ListShippingsViewModel ListShippingsViewModel { get; set; }
        public async Task LoadProfileName(User userdata)
        {
            try
            {
                var profile = await Api.GetProfile();
                string userName = profile == PassingCar.Utils.ProfileType.Fisica || userdata.JuridicDetails is null ? $"{userdata.Name} {userdata.Surname}" : $"{userdata.JuridicDetails.CompanyName}";
                string profileType = $"Persona {profile.CustomToString()}";
                
                // CRITICAL FIX: Update UI on main thread to ensure proper binding
                Device.BeginInvokeOnMainThread(() =>
                {
                    App.HeaderContext.UserName = userName;
                    App.HeaderContext.ProfileType = profileType;
                    System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Profile loaded: UserName='{userName}', ProfileType='{profileType}'");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Error loading profile: {ex.Message}");
                _ = ex.Handle();
            }
        }
        public MainPageViewModel()
        {
            MyAdss = new ObservableCollection<AdsDetailsExtened>();
            Notifications = new ObservableCollection<NotificationExtended>();
            ProfileImageSource = "img_account.png";
        }
        public async Task AsyncLoad(AdsFilter filter, LoadingSmall smallLoading, INavigation navigation)
        {

            try
            {
                smallLoading.IsVisible = true;
                // Removed artificial delay for instant response
                //handle exception
                filter.Active = true;
                filter.OnlyOwnAds = true;
                filter.OnlyFavorites = false;
                DateTime reqTime = DateTime.Now;
                List<LocalDatabase.LocalAd> localAds = await App.LocalDatabase.GetAdsForCheck(filter);
                GetNextAdsResponse updatedAds = await Api.CheckAdsUpdates(localAds);
                localAds = await localAds.Update(updatedAds);
                if (localAds != null && localAds.Any())
                {
                    foreach (LocalDatabase.LocalAd item in localAds)
                    {
                        IEnumerable<AdsDetailsExtened> where = MyAdss.Where(a => a.AdsId == item.Id);
                        if (where != null && where.Any())
                        {
                            if (item.State >= Models.AdsState.PaymentPending)
                            {
                                _ = MyAdss.Remove(where.First());
                            }
                        }
                        else if (item.State < Models.AdsState.PaymentPending)
                        {
                            MyAdss.Insert(0, new AdsDetailsExtened(filter.OnlyFavorites)
                            {
                                FirstAdsImage = item.GetFirstPhoto(),
                                AdsTitle = item.AdsTitle,
                                IsFavorite = item.IsFavorite,
                                AdsId = item.Id,
                                AdsFrom = item.AdsFrom,
                                AdsTo = item.AdsTo,
                                AdsPrice = item.AdsPrice,
                                State = item.State.ToString(),
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
                await Task.Delay(1000);
                smallLoading.IsVisible = false;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Disposing resources...");
                    
                    // Clear collections to release references
                    MyAdss?.Clear();
                    Notifications?.Clear();
                    ListShippingsViewModel?.Dispose();
                    
                    System.Diagnostics.Debug.WriteLine("[MainPageViewModel] Resources disposed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MainPageViewModel] Error during disposal: {ex.Message}");
                    _ = ex.Handle();
                }
            }
            base.Dispose(disposing);
        }
    }
}
    
