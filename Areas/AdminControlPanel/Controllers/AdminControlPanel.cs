using APPMVC.NET.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APPMVC.NET.Areas.AdminControlPanel.Controllers
{
    [Area("AdminControlPanel")]
    [Authorize(Roles = RoleName.Administrator)]
    public class AdminControlPanel : Controller
    {
        [Route("/admin-control-panel/")]
        public IActionResult Index() => View();
    }
}