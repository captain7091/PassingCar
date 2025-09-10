namespace PassingCarApis.Models.API.Ads
{
    public class AdsDetails
    {
        public Models.Ads? Ads { get; set; }
        public UserExtended? User { get; set; }
        public bool IsFavorite { get; set; }
    }
}
