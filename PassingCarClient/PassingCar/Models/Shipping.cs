using Dapper.Contrib.Extensions;
using System;

namespace PassingCar.Models
{
    [Table("[Shipping]")]
    public class Shipping
    {
        public int Id { get; set; }
        public int OfferId { get; set; }
        public ShippingState State { get; set; }
        public string CurrentLocation { get; set; }
        public string ConfirmationCode { get; set; }
        public string History { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
    public enum ShippingState
    {
        PendingForPickup,
        UberPickupConfirmed,
        ClientPickupConfirmed,
        Delivered,
        Finalizado,
        Canceled
    }
}
