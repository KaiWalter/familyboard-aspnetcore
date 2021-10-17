using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Diagnostics;
using System.Threading.Tasks;
using FamilyBoard.Application.Models;
using FamilyBoard.Core;

namespace FamilyBoard.Application.Controllers
{
    [Authorize]
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;

        private readonly ITokenAcquisition _tokenAcquisition;

        private string[] _graphScopes = new[] { "offline_access", "User.Read", "Calendars.Read", "Files.Read.All" };

        public LoginController(ILogger<LoginController> logger,
                            ITokenAcquisition tokenAcquisition,
                            IConfiguration configuration)
        {
            _logger = logger;
            _tokenAcquisition = tokenAcquisition;
        }

        [AuthorizeForScopes(ScopeKeySection = "Graph:Scopes")]
        public async Task<IActionResult> Index()
        {
            _logger.LogTrace("LOGIN REQUEST:" + nameof(Index));
            GraphServiceClient graphClient = GetGraphServiceClient(_graphScopes);
            var me = await graphClient.Me.Request().GetAsync();
            ViewData["Me"] = me;
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private GraphServiceClient GetGraphServiceClient(string[] scopes)
        {
            return GraphServiceClientFactory.GetAuthenticatedGraphClient(async () =>
            {
                string result = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                return result;
            }, "https://graph.microsoft.com/beta");
        }
    }
}