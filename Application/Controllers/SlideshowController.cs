using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FamilyBoard.Application.Controllers
{
    public class Slideshow : Controller
    {
        private readonly ILogger<Slideshow> _logger;

        public Slideshow(ILogger<Slideshow> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
    }
}