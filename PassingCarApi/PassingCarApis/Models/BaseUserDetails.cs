using Newtonsoft.Json;

namespace PassingCarApis.Models
{
    public class BaseUserDetails
    {
        public byte[]? UserProfilePhoto { get; set; }
        public string? UserName { get; set; }
        public double UserRating { get; set; }
        public string? JuridicDetails { get; set; }
        public string JuridicName
        {
            get
            {
                if (string.IsNullOrEmpty(JuridicDetails))
                {
                    return string.Empty;
                }
                return JsonConvert.DeserializeObject<JuridicDetails>(JuridicDetails)?.CompanyName ?? string.Empty;
            }
        }
    }
}
