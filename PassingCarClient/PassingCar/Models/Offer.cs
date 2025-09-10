using Dapper.Contrib.Extensions;
using System;

namespace PassingCar.Models
{
    [Table("[Offer]")]
    public class Offer
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public OfferState State { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
    public enum OfferState
    {
        Pending,
        Sent,
        Seen,
        PaymentPending,
        Accepted,
        Rejected,
        Canceled
    }
}
