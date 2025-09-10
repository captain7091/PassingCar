namespace PassingCar.Models.API.User
{
    public class ReviewRequest
    {
        public int Rating { get; set; }
        public string Message { get; set; }
        public int ShippingId { get; set; }
    }
}
