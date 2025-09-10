using Microsoft.AspNetCore.Mvc;

namespace PassingCarApis.Controllers
{
    public class OpenController : Controller
    {
        public IActionResult Ads(int adID)
        {
            return Redirect("https://www.passingcar.com/#[object%20Object]");
        }
    }
}
