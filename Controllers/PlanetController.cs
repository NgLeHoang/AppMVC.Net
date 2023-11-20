using APPMVC.NET.Services;
using Microsoft.AspNetCore.Mvc;

namespace APPMVC.NET.Controllers
{
    [Route("he-mat-troi")]
    public class PlanetController : Controller
    {
        private readonly PlanetServices _planetService;
        private readonly ILogger<PlanetController> _logger;
        public PlanetController(PlanetServices planetService, ILogger<PlanetController> logger)
        {
            _planetService = planetService;
            _logger = logger;
        }

        [Route("danh-sach-cac-hanh-tinh.html")]
        public IActionResult Index()
        {
            return View();
        }

        [BindProperty(SupportsGet = true, Name = "action")]
        public string Name { get; set; }

        [Route("[controller]-[action].html")]
        public IActionResult Mercury()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }
        public IActionResult Mars()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }
        public IActionResult Earth()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }
        public IActionResult Jupiter()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }
        public IActionResult Neptune()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }
        public IActionResult Saturn()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }
        public IActionResult Uranus()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }
        public IActionResult Venus()
        {
            var planet = _planetService.Where(p => p.Name == Name).FirstOrDefault();
            return View("Detail", planet);
        }

        [Route("hanhtinh/{id:int}")]
        public IActionResult PlanetInfo(int id)
        {
            var planet = _planetService.Where(p => p.Id == id).FirstOrDefault();
            return View("Detail", planet);
        }
    }
}