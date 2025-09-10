using Dapper.Contrib.Extensions;

namespace PassingCarApis.Models
{
    [Table("[Review]")]
    public class Review
    {
        public int Id { get; set; }
        public int ReviewedUserId { get; set; }
        public int ReviewerUserId { get; set; }
        public int Rating { get; set; }
        public string? Message { get; set; }
        public string? ReviewedUserProfile { get; set; }
        public string? ReviewerUserProfile { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
