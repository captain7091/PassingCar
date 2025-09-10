namespace PassingCar.Models.API.Ads
{
    public class AdsFilter
    {
        public bool OnlyOwnAds { get; set; }
        public int FromLastXDays { get; set; }
        public string? ProvincieFrom { get; set; }
        public string? ProvincieTo { get; set; }
        public UserType? UserType { get; set; } // Added to match server model
        public bool OnlyFavorites { get; set; }
        public bool Active { get; set; }
        public bool OnlyNextOne { get; set; }
        public string? Relevance { get; set; }

        // Enhanced filtering for Anuncios module
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? PriceRange { get; set; } // "0-50", "50-100", "100-200", "200+"
        public bool ShowAllUsers { get; set; } = true; // For viewing all ads from all users
    }

    public enum UserType
    {
        Individuals,
        Comapanies
    }
}
