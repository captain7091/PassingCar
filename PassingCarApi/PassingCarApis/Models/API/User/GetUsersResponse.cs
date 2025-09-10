namespace PassingCarApis.Models.API.User
{
    public class GetUsersResponse : ApiBaseResponse
    {
        public IEnumerable<Models.User>? Result { get; set; }
    }
}
