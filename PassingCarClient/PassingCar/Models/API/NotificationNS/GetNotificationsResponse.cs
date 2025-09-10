using System.Collections.Generic;

namespace PassingCar.Models.API.NotificationNS
{
    public class GetNotificationsResponse : ApiBaseResponse
    {
        public IEnumerable<Notification> Notifications { get; set; }
    }
}
