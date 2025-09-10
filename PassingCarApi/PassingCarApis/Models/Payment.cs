namespace PassingCarApis.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int OfferId { get; set; }
        public string? ChargeRequestJson { get; set; }
        public string? ChargeResponseJson { get; set; }
        public string? RefundRequestJson { get; set; }
        public string? RefundResponseJson { get; set; }
        public string? PaymentProofModelJson { get; set; }
        public string? PaymentProofFromTransporterModelJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
