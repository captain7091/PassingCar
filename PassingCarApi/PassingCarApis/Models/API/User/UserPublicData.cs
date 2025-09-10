namespace PassingCarApis.Models.API.User
{
    public class UserPublicData : ApiBaseResponse
    {
        public string? Name { get; set; }
        public double Rating { get; set; }
        public int PublishedAds { get; set; }
        public int SentPackages { get; set; }
        public int TransportedPackages { get; set; }
        public int CanceledDeliveries { get; set; }
        public int OffersSent { get; set; }
        public decimal SpentMoney { get; set; }
        public decimal EarnedMoney { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
