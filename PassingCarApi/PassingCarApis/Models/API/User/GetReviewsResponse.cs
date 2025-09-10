namespace PassingCarApis.Models.API.User
{
    public class GetReviewsResponse : ApiBaseResponse
    {
        public IEnumerable<ReviewM>? Reviews { get; set; }
    }
    public class ReviewM
    {
        public byte[]? ProfilePhoto { get; set; }
        public string? Name { get; set; }
        public double Rating { get; set; }
        public string? Message { get; set; }
    }
}
