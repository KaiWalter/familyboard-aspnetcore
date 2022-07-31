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

        [HttpGet(nameof(HttpClientWithToken))]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK, "application/json")]
        public async Task<ActionResult<object>> HttpClientWithToken()
        {
            _logger.LogTrace("REQUEST:" + nameof(HttpClientWithToken));

            var token = await _graphService.GetAccessToken();

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);
            client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0");
            var response = await client.GetAsync("/me");

            var result = new
            {
                Response = response,
            };

            return Ok(result);
        }

        [HttpGet(nameof(HttpClientWithoutToken))]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK, "application/json")]
        public async Task<ActionResult<object>> HttpClientWithoutToken()
        {
            _logger.LogTrace("REQUEST:" + nameof(HttpClientWithoutToken));

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0");
            var response = await client.GetAsync("");

            var result = new
            {
                Response = response,
            };

            return Ok(result);
        }

        [HttpGet(nameof(CurlWithToken))]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK, "application/json")]
        public async Task<ActionResult<object>> CurlWithToken()
        {
            _logger.LogTrace("REQUEST:" + nameof(CurlWithToken));

            var token = await _graphService.GetAccessToken();

            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "curl";
            process.StartInfo.Arguments = $"-H \"Authorization: Bearer {token.AccessToken}\" https://graph.microsoft.com/v1.0/me";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string consoleResult = process.StandardOutput.ReadToEnd();

            var result = new
            {
                consoleResult,
            };

            return Ok(result);
        }

        [HttpGet(nameof(CurlWithoutToken))]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK, "application/json")]
        public async Task<ActionResult<object>> CurlWithoutToken()
        {
            _logger.LogTrace("REQUEST:" + nameof(CurlWithoutToken));

            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "curl";
            process.StartInfo.Arguments = "https://graph.microsoft.com/v1.0";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string consoleResult = process.StandardOutput.ReadToEnd();

            var result = new
            {
                consoleResult,
            };

            return Ok(result);
        }
    }
}

