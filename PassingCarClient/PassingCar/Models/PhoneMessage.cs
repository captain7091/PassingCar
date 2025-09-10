using Dapper.Contrib.Extensions;
using System;

namespace PassingCar.Models
{
    [Table("[PhoneMessage]")]
    public class PhoneMessage
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public PhoneMessageState State { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public enum PhoneMessageState
    {
        Pending,
        Sent,
        Error
    }
}
