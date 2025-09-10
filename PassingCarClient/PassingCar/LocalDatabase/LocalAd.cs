using PassingCar.Extensions;
using PassingCar.Models;
using PassingCar.Models.API.Ads;
using PassingCar.Utils;
using SQLite;
using System;

namespace PassingCar.LocalDatabase
{
    public class LocalAd
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string FirstAdsImageBase64 { get; set; }
        public string AdsTitle { get; set; }
        public bool IsFavorite { get; set; }
        public string AdsFrom { get; set; }
        public string AdsTo { get; set; }
        public decimal AdsPrice { get; set; }
        public AdsState State { get; set; }
        public string UserProfilePhotoBase64 { get; set; }
        public string Username { get; set; }
        public double UserRating { get; set; }
        public int UserId { get; set; }
        public ProfileType UserProfile { get; set; }
        public DateTime PostedTime { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public LocalAd()
        {
        }
        public LocalAd(AdsDetailsExtened AdsDetails, AdsState adsState, int UserId, DateTime? modifiedAt)
        {
            try
            {
                Id = AdsDetails.AdsId;
                FirstAdsImageBase64 = AdsDetails.FirstAdsImage != null && AdsDetails.FirstAdsImage.Length > 0
                    ? Convert.ToBase64String(AdsDetails.FirstAdsImage)
                    : string.Empty;
                AdsTitle = AdsDetails.AdsTitle;
                IsFavorite = AdsDetails.IsFavorite;
                AdsFrom = AdsDetails.AdsFrom;
                AdsTo = AdsDetails.AdsTo;
                AdsPrice = AdsDetails.AdsPrice;
                State = adsState;
                UserProfilePhotoBase64 = AdsDetails.UserProfilePhoto != null && AdsDetails.UserProfilePhoto.Length > 0
                    ? Convert.ToBase64String(AdsDetails.UserProfilePhoto)
                    : string.Empty;
                Username = AdsDetails.UserName;
                UserProfile = AdsDetails.UserProfile;
                UserRating = AdsDetails.UserRating;
                PostedTime = AdsDetails.PostedTime;
                this.UserId = UserId;
                ModifiedAt = modifiedAt;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
            }
        }
        public byte[] GetFirstPhoto()
        {
            try
            {
                if (!string.IsNullOrEmpty(FirstAdsImageBase64))
                {
                    var imageBytes = Convert.FromBase64String(FirstAdsImageBase64);
                    System.Diagnostics.Debug.WriteLine($"[LocalAd] GetFirstPhoto for ad {Id}: Base64 length={FirstAdsImageBase64.Length}, bytes length={imageBytes?.Length ?? 0}");
                    return imageBytes;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[LocalAd] GetFirstPhoto for ad {Id}: No Base64 image data available");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LocalAd] GetFirstPhoto error for ad {Id}: {ex.Message}");
                _ = ex.Handle();
                return null;
            }
        }
        public byte[] GetUserProfilePhoto()
        {
            try
            {
                return !string.IsNullOrEmpty(UserProfilePhotoBase64) ? Convert.FromBase64String(UserProfilePhotoBase64) : null;
            }
            catch (Exception ex)
            {
                _ = ex.Handle();
                return null;
            }
        }
    }
}
