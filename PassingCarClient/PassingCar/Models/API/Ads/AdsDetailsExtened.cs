using System;
using Newtonsoft.Json;
using System.Windows.Input;
using PassingCar.ViewModels;
using PassingCar.Utils;
using PassingCar.LocalDatabase;
using PassingCar.Extensions;

namespace PassingCar.Models.API.Ads
{
    public class AdsDetailsExtened : BaseViewModel
    {
        private byte[]? _firstAdsImage;
        public byte[]? FirstAdsImage 
        { 
            get => _firstAdsImage;
            set
            {
                _firstAdsImage = value;
                OnPropertyChanged(nameof(FirstImage));
            }
        }
        private readonly LocalDB? _localDB;
        public string? AdsTitle { get; set; }
        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;

            set
            {
                _isFavorite = value;
                OnPropertyChanged(nameof(FavoriteImage));
            }
        }
        public DateTime PostedTime { get; set; }
        public int AdsId { get; set; }
        public int UserId { get; set; }
        public ProfileType UserProfile { get; set; }
        public DateTime? ModifiedAt { get; set; }
        private AdsState _state;
        public string State
        {
            get => _state.Espana();
            set
            {
                value = value.ReplaceEspana();
                _state = (AdsState)Enum.Parse(typeof(AdsState), value);
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(AdIsEnabled));
                OnPropertyChanged(nameof(FrameOpacity));
            }
        }
        public string? AdsFrom { get; set; }
        public string? AdsTo { get; set; }
        public decimal AdsPrice { get; set; }
        public byte[]? UserProfilePhoto { get; set; }
        public string? UserName { get; set; }
        public double UserRating { get; set; }
        private readonly bool onlyFavourites;
        public AdsDetailsExtened()
        {
        }
        public AdsDetailsExtened(bool onlyFavourites)
        {
            this.onlyFavourites = onlyFavourites;
        }
        public ICommand AddToFavorite => new Command(AddToFavoriteAction);
        public bool AdIsEnabled => _state <= AdsState.Posted;
        public bool AdIsNotEnabled => !AdIsEnabled;
        //public Color BackgroundColorFrame
        //{
        //    get
        //    {
        //        if (State <= AdsState.Posted)
        //        {
        //            return Color.FromHex("#f6f5f5");
        //        }
        //        else
        //        {
        //            return Color.LightGray;
        //        }
        //    }
        //}
        public double FrameOpacity => (onlyFavourites && _state <= AdsState.Posted) || (!onlyFavourites && _state <= AdsState.Delivered) ? 1 : 0.5;
        public string CityFrom
        {
            get
            {
                if (string.IsNullOrEmpty(AdsFrom))
                {
                    return string.Empty;
                }
                else
                {
                    try
                    {
                        AddressDetails addDet = JsonConvert.DeserializeObject<AddressDetails>(AdsFrom);
                        if (addDet != null)
                        {
                            string add = addDet.City;
                            if (!string.IsNullOrEmpty(addDet.Provincie))
                            {
                                add += $", {addDet.Provincie}";
                            }
                            return add;
                        }
                        else
                        {
                            return AdsFrom;
                        }
                    }
                    catch (Exception)
                    {
                        return AdsFrom;
                    }
                }
            }
        }
        public string CityTo
        {
            get
            {
                if (string.IsNullOrEmpty(AdsTo))
                {
                    return string.Empty;
                }
                else
                {
                    try
                    {
                        AddressDetails addDet = JsonConvert.DeserializeObject<AddressDetails>(AdsTo);
                        if (addDet != null)
                        {
                            string add = addDet.City;
                            if (!string.IsNullOrEmpty(addDet.Provincie))
                            {
                                add += $", {addDet.Provincie}";
                            }
                            return add;
                        }
                        else
                        {
                            return AdsTo;
                        }
                    }
                    catch (Exception)
                    {
                        return AdsTo;
                    }
                }
            }
        }
        public string RatingString => UserRating > 0 ? $"Rating: {UserRating:0.00}" : string.Empty;
        public ImageSource FavoriteImage => IsFavorite
                    ? "a_star_filled1.png"
                    : "a_star1.png";
        public ImageSource UserProfilePhotoImg => UserProfilePhoto != null && UserProfilePhoto.Length > 0
                    ? ImageSource.FromStream(() => new MemoryStream(UserProfilePhoto))
                    : "img_account.png";
        public ImageSource FirstImage
        {
            get
            {
                try
                {
                    if (FirstAdsImage != null && FirstAdsImage.Length > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AdsDetailsExtened] Loading image for ad {AdsId}, image size: {FirstAdsImage.Length} bytes");
                        return ImageSource.FromStream(() => new MemoryStream(FirstAdsImage));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[AdsDetailsExtened] No image data for ad {AdsId}, using fallback image");
                        return "empty.jpg";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdsDetailsExtened] Error loading image for ad {AdsId}: {ex.Message}");
                    return "empty.jpg";
                }
            }
        }
        public ImageSource UserStar1 => GetStarImage(1);
        public ImageSource UserStar2 => GetStarImage(2);
        public ImageSource UserStar3 => GetStarImage(3);
        public ImageSource UserStar4 => GetStarImage(4);
        public ImageSource UserStar5 => GetStarImage(5);
        private async void AddToFavoriteAction()
        {
            IsFavorite = !IsFavorite;
            _ = !IsFavorite ? await App.LocalDatabase.RemoveFromFavorite(AdsId) : await App.LocalDatabase.AddToFavorite(AdsId);
        }
        private ImageSource GetStarImage(int countStar)
        {
            return !(UserRating > 0)
                ? null
                : (UserRating - countStar) >= 0
                    ? ImageSource.FromResource($"PassingCar.Resources.Images.a_star_filled.png")
                    : (UserRating - countStar) >= -0.5
                                    ? "a_star_half.png"
                                    :"a_star.png";
        }
    }
}

