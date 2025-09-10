using PassingCarApis.Models;
using PassingCarApis.Models.API;

namespace PassingCarApis.Services
{
    public interface INotificationService
    {
        public Task<InsertResponse> AddNotification(IHubService hubService, string accessToken, Notification notification);
        public Task<bool> MarkSeen(int id);
    }
}
