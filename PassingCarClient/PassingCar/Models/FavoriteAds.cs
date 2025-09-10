using Dapper.Contrib.Extensions;
using System;

namespace PassingCar.Models
{
    [Table("[FavoriteAds]")]
    public class FavoriteAds
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
