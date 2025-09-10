using Dapper.Contrib.Extensions;

namespace PassingCarApis.Models
{
    [Table("[Notification]")]
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public ProfileType UserProfile { get; set; }
        public bool Seen { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? ShellRoute { get; set; }
        public string? Parameters { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}