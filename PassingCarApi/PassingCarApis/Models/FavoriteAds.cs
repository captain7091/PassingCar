using Dapper.Contrib.Extensions;

namespace PassingCarApis.Models
{
    [Table("[FavoriteAds]")]
    public class FavoriteAds
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public int UserId { get; set; }
        public ProfileType UserProfile { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
