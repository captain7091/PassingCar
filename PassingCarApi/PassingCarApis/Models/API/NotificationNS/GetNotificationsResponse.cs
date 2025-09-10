namespace PassingCarApis.Models.API.NotificationNS
{
    public class GetNotificationsResponse : ApiBaseResponse
    {
        public IEnumerable<Notification>? Notifications { get; set; }
    }
}
