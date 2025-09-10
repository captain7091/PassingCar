using PassingCar.Utils;
using System;

namespace PassingCar.Models.API.Ads
{
    public class AdFromAdsListModel
    {
        public byte[] FirstAdsImage { get; set; }
        public string AdsTitle { get; set; }
        public bool IsFavorite { get; set; }
        public DateTime PostedTime { get; set; }
        public int AdsId { get; set; }
        public AdsState State { get; set; }
        public string AdsFrom { get; set; }
        public string AdsTo { get; set; }
        public decimal AdsPrice { get; set; }
        public ProfileType UserProfile { get; set; }
        public byte[] UserProfilePhoto { get; set; }
        public string UserName { get; set; }
        public double UserRating { get; set; }
        public int UserId { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
