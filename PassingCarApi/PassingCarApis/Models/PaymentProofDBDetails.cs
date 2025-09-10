namespace PassingCarApis.Models
{
    public class PaymentProofDBDetails
    {
        public string? ClientName { get; set; }
        public int OfferId { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PersonalInfo { get; set; }
        public string? JuridicDetails { get; set; }
        public string? Title { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public decimal Price { get; set; }
    }
}
