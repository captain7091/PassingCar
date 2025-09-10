namespace PassingCarApis.Models.API.Payment
{
    public class InsertPaymentInput
    {
        public ChargeData? ChargeRequest { get; set; }
        public int OfferId { get; set; }
    }
    public class ChargeData
    {
        public long? Amount { get; set; }
        public long? ApplicationFeeAmount { get; set; }
        public bool? Capture { get; set; }
        public string? Currency { get; set; }
        public string? Customer { get; set; }
        public string? Description { get; set; }
        public decimal? ExchangeRate { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        public string? ReceiptEmail { get; set; }
        public string? StatementDescriptor { get; set; }
        public string? StatementDescriptorSuffix { get; set; }
        public string? TransferGroup { get; set; }
        public IDictionary<string, object>? ExtraParams { get; set; }
    }
}
