using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBoard.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IGraphService _graphService;

        public HealthController(ILogger<HealthController> logger,
                            IConfiguration configuration,
                            IGraphService graphService)
        {
            _logger = logger;
            _configuration = configuration;
            _graphService = graphService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK, "application/json")]
        public async Task<ActionResult<object>> GetHealth()
        {
            _logger.LogTrace("REQUEST:" + nameof(GetHealth));

            var graphServiceClient = _graphService.GetGraphServiceClient();
            var token = await _graphService.GetAccessToken();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);
            client.BaseAddress = new Uri(graphServiceClient.BaseUrl);
            var response = await client.GetAsync("/me");

            var result = new
            {
                BaseUrl = graphServiceClient.BaseUrl,
                Response = response,
            };

            return Ok(result);
        }

        [HttpGet(nameof(ConnectionCheck))]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK, "application/json")]
        public async Task<ActionResult<object>> ConnectionCheck()
        {
            _logger.LogTrace("REQUEST:" + nameof(ConnectionCheck));

            var graphServiceClient = _graphService.GetGraphServiceClient();

            var client = new HttpClient();
            client.BaseAddress = new Uri(graphServiceClient.BaseUrl);
            var response = await client.GetAsync("");

            var result = new
            {
                BaseUrl = graphServiceClient.BaseUrl,
                Response = response,
            };

            return Ok(result);
        }
    }
}

