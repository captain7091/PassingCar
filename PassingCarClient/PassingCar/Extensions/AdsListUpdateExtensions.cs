using PassingCar.LocalDatabase;
using PassingCar.Models.API.Ads;


namespace PassingCar.Extensions
{
    public static class AdsListUpdateExtensions
    {
        public static async Task<List<LocalAd>> Update(this List<LocalAd> localAds, GetNextAdsResponse updatedAdsResponse, bool onlyFavorites = false)
        {
            if (updatedAdsResponse != null && updatedAdsResponse.Success)
            {
                List<LocalAd> udpatedAds = new List<LocalAd>();
                if (updatedAdsResponse.AdsItem != null && updatedAdsResponse.AdsItem.Any())
                {
                    foreach (AdFromAdsListModel item in updatedAdsResponse.AdsItem)
                    {
                        AdsDetailsExtened adsDetails = new AdsDetailsExtened(onlyFavorites)
                        {
                            FirstAdsImage = item.FirstAdsImage,
                            AdsTitle = item.AdsTitle,
                            IsFavorite = item.IsFavorite,
                            AdsId = item.AdsId,
                            AdsFrom = item.AdsFrom,
                            UserProfile = item.UserProfile,
                            AdsTo = item.AdsTo,
                            AdsPrice = item.AdsPrice,
                            State = item.State.Espana(),
                            UserProfilePhoto = item.UserProfilePhoto,
                            UserName = item.UserName,
                            UserRating = item.UserRating,
                            PostedTime = item.PostedTime,
                            UserId = item.UserId,
                            ModifiedAt = item.ModifiedAt,
                        };
                        LocalAd asdsad = new LocalAd(adsDetails, item.State, item.UserId, item.ModifiedAt);
                        udpatedAds.Add(asdsad);
                    }
                }
                if (udpatedAds != null && udpatedAds.Any())
                {
                    foreach (LocalAd item in udpatedAds)
                    {
                        _ = await App.LocalDatabase.UpdateAdd(item);
                        _ = localAds.RemoveAll(a => a.Id == item.Id);
                        localAds.Add(item);
                    }
                    localAds = localAds.OrderBy(a => a.Id).ToList();
                }
            }
            else
            {
                if (updatedAdsResponse != null && !string.IsNullOrEmpty(updatedAdsResponse.ErrorMessage))
                {
                    Console.WriteLine(updatedAdsResponse.ErrorMessage);
                }
            }
            return localAds;
        }
    }
}
