namespace PassingCarApis.Models.API.Payment
{
    public class UpdateRefundRequest
    {
        public int PaymentId { get; set; }
        public RefundData? Refund { get; set; }
    }
    public class RefundData
    {
        public long? Amount { get; set; }
        public string? Charge { get; set; }
        public string? Currency { get; set; }
        public string? Customer { get; set; }
        public string? InstructionsEmail { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        public string? Origin { get; set; }
        public string? PaymentIntent { get; set; }
        public string? Reason { get; set; }
        public bool? RefundApplicationFee { get; set; }
        public bool? ReverseTransfer { get; set; }
        public IDictionary<string, object>? ExtraParams { get; set; }
    }
}
