namespace PassingCarApis.Models.API.Ads
{
    public class GetAdsDetailsResponse : ApiBaseResponse
    {
        public AdsMoreDetails? AdsMoreDetails { get; set; }
    }
    public class AdsMoreDetails
    {
        public List<byte[]>? NextPhotos { get; set; }
        public AdsState State { get; set; }
        public string? Description { get; set; }
        public string? JuridicDetails { get; set; }
        public bool IsMyAds { get; set; }
        public List<GroupOfOffers>? OffersGroups { get; set; }
        public List<OfferDetails>? SentOffers { get; set; }
        public int? ChatIdForViewer { get; set; }
        public int ViewCounter { get; set; }
    }
    public class GroupOfOffers
    {
        public byte[]? UserProfilePhoto { get; set; }
        public double UserRating { get; set; }
        public string? UserName { get; set; }
        public string? UserType { get; set; }
        public int? ChatId { get; set; }
        public int UserId { get; set; }
        public ProfileType UserProfile { get; set; }
        public string? UserProfiledString { get; set; }
        public List<OfferDetails>? Offers { get; set; }
    }
    public class OfferDetails
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public OfferState State { get; set; }
        public ProfileType UserProfile { get; set; }
    }
    public class AdsPhotosModel
    {
        public byte[]? Photo1 { get; set; }
        public byte[]? Photo2 { get; set; }
        public byte[]? Photo3 { get; set; }
        public byte[]? Photo4 { get; set; }
    }
}
