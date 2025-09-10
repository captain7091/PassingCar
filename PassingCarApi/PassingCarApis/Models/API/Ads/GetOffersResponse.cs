namespace PassingCarApis.Models.API.Ads
{
    public class GetOffersResponse : ApiBaseResponse
    {
        public List<AdsParent>? Ads { get; set; }
    }
    public class AdsParent
    {
        public int AdsId { get; set; }
        public byte[]? FirstPhoto { get; set; }
        public string? Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<GroupOfOffers>? OffersGroups { get; set; }
        public int? ChatIdForViewer { get; set; }
        public List<OfferDetails>? SentOffers { get; set; }
    }
}
