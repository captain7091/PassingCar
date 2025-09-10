namespace PassingCarApis.Models.API.User
{
    public class GetUserRatingAndSoldResponse : ApiBaseResponse
    {
        public double Rating { get; set; }
        public decimal? Sold { get; set; }
    }
}
