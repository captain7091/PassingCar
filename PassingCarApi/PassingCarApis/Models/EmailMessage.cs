using Dapper.Contrib.Extensions;

namespace PassingCarApis.Models
{
    [Table("[EmailMessage]")]
    public class EmailMessage
    {
        public int Id { get; set; }
        public int UserdToSendId { get; set; }
        public ProfileType UserProfile { get; set; }
        public string? Content { get; set; }
        public EmailMessageState State { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public enum EmailMessageState
    {
        Pending,
        Sent,
        Error
    }
}
