using Dapper.Contrib.Extensions;

namespace PassingCarApis.Models
{
    [Table("[Chat]")]
    public class Chat
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public int CustomerUserId { get; set; }
        public int UberUserId { get; set; }
        public ProfileType CustomerUserProfile { get; set; }
        public ProfileType UberUserProfile { get; set; }
        public ChateState State { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
    public enum ChateState
    {
        Censored,
        Open,
        Closed
    }
}
