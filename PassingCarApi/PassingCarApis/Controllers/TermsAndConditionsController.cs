using Microsoft.AspNetCore.Mvc;

namespace PassingCarApis.Controllers
{
    public class TermsAndConditionsController : Controller
    {
        public IActionResult Register()
        {
            return View();
        }
    }
}
