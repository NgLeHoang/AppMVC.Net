using Microsoft.AspNetCore.Mvc;

namespace APPMVC.NET.Controllers {
    public class FirstController : Controller
    {
        private readonly ILogger<FirstController> _logger;
        public FirstController(ILogger<FirstController> logger)
        {
            _logger = logger;
        }

        public object TimeNow() => DateTime.Now;

        public IActionResult Privacy()
        {
            var url = Url.Action("Privacy", "Home");
            _logger.LogInformation("Chuyen huong den " + url);
            return LocalRedirect(url);
        }

        public IActionResult HelloView()
        {
            return View("/MyView/helloview.cshtml");
        }
    }
}