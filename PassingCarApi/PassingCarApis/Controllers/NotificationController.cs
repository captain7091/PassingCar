using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PassingCarApis.Models;
using PassingCarApis.Models.API;
using PassingCarApis.Services;

namespace PassingCarApis.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    [ApiController]
    public class NotificationController : PassingCarBaseController
    {

        public NotificationController(INotificationService notificationService, IHubService hubService, ILoginService loginService) : base(notificationService, hubService, loginService)
        {

        }
        [HttpPost]
        public async Task<InsertResponse> AddNotification(Notification notification)
        {
            string? token = await HttpContext.GetTokenAsync("access_token");
            return await notificationService.AddNotification(hubService, token!, notification);
        }
        [HttpPost("[controller]/[action]/{id}")]
        public async Task<bool> MarkSeen(int id)
        {
            return await notificationService.MarkSeen(id);
        }

    }
}