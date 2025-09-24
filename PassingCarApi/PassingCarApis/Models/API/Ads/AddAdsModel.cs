namespace PassingCarApis.Models.API.Ads
{
    public class AddAdsModel
    {
        public string? Title { get; set; }
        public string? Photo1 { get; set; }
        public string? Photo2 { get; set; }
        public string? Photo3 { get; set; }
        public string? Photo4 { get; set; }
        public string? Description { get; set; }
        public AddressDetails? From { get; set; }
        public AddressDetails? To { get; set; }
        public decimal Price { get; set; }
        public SizeDetails? Size { get; set; }
        public string? Weigth { get; set; }
        public bool IsFragile { get; set; }
        public ProfileType ProfileType { get; set; }
    }
    public class AddressDetails
    {
        public string? City { get; set; }
        public string? ZIPCode { get; set; }
        public string? Address { get; set; }
        public string? Provincie { get; set; }
        public string? PhoneNumber { get; set; }
    }
    public class SizeDetails
    {
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
