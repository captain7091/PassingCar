using System.Collections.Generic;

namespace PassingCar.Models.API.User
{
    public class GetUsersResponse : ApiBaseResponse
    {
        public IEnumerable<Models.User> Result { get; set; }
    }
}
