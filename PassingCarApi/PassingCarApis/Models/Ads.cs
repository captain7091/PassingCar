using Dapper.Contrib.Extensions;

namespace PassingCarApis.Models
{
    [Table("[Ads]")]
    public class Ads
    {
        public int ID { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public byte[]? Photo1 { get; set; }
        public byte[]? Photo2 { get; set; }
        public byte[]? Photo3 { get; set; }
        public byte[]? Photo4 { get; set; }
        public string? Description { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public decimal Price { get; set; }
        public int ViewsCounter { get; set; }
        public string? Size { get; set; }
        public string? Weight { get; set; }
        public string? Fragile { get; set; }
        public ProfileType UserProfile { get; set; }
        public AdsState State { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
    public enum AdsState
    {
        Pending,
        Posted,
        PaymentPending,
        AcceptedForTransit,
        InTransit,
        Delivered,
        Closed,
        Disabled,
        Blocked,
    }
}
