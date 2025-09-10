using Dapper.Contrib.Extensions;

namespace PassingCarApis.Models
{
    [Table("[ValidationCode]")]
    public class ValidationCode
    {
        public int Id { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Code { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
