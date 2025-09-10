using Dapper.Contrib.Extensions;

namespace PassingCarApis.Models
{
    [Table("[Comment]")]
    public class Comment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public ProfileType UserProfile { get; set; }
        public int AdId { get; set; }
        public int ParentId { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
