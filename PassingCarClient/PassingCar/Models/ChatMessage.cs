using Dapper.Contrib.Extensions;
using System;

namespace PassingCar.Models
{
    [Table("[ChatMessage]")]
    public class ChatMessage
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public ChatMessageState State { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
    public class ChatMessageWithOtherUserId : ChatMessage
    {
        public int OtherUserId { get; set; }
    }
    public enum ChatMessageState
    {
        Pending,
        Sent,
        Seen,
        Error
    }
}
