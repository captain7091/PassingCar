using Microsoft.AspNetCore.Mvc;
using PassingCarApis.Services;

namespace PassingCarApis.Controllers
{
    public class PassingCarBaseController : ControllerBase
    {
        public INotificationService notificationService;
        public IHubService hubService;
        public ILoginService loginService;
        public PassingCarBaseController(INotificationService notificationService, IHubService hubService, ILoginService loginService)
        {
            this.notificationService = notificationService;
            this.hubService = hubService;
            this.loginService = loginService;
        }
    }
}
