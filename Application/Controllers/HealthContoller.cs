using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FamilyBoard.Application.Controllers
{
    public class HealthResult
    {
        public string Result { get; set; }
    }

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
        public async Task<ActionResult<HealthResult>> HttpClientWithToken()
        {
            _logger.LogTrace("REQUEST:" + nameof(HttpClientWithToken));

            var token = await _graphService.GetAccessToken();

            var client = new HttpClient();
            client.Timeout = Timeout.InfiniteTimeSpan;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri("https://graph.microsoft.com");

            var response = await client.GetAsync("v1.0/me");
            var result = await response.Content.ReadAsStringAsync() + "|||||||" + response.ToString();

            _logger.LogTrace($"RESULT:{result}");
            return Ok(new HealthResult { Result = result });
        }

        [HttpGet(nameof(HttpClientWithoutToken))]
        [AllowAnonymous]
        public async Task<ActionResult<HealthResult>> HttpClientWithoutToken()
        {
            _logger.LogTrace("REQUEST:" + nameof(HttpClientWithoutToken));

            var client = new HttpClient();
            client.Timeout = Timeout.InfiniteTimeSpan;
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri("https://graph.microsoft.com");

            var response = await client.GetAsync("v1.0");
            var result = await response.Content.ReadAsStringAsync() + "|||||||" + response.ToString();

            _logger.LogTrace($"RESULT:{result}");
            return Ok(new HealthResult { Result = result });
        }

        // [HttpGet(nameof(HttpClient2))]
        // [AllowAnonymous]
        // public async Task<ActionResult<HealthResult>> HttpClient2()
        // {
        //     _logger.LogTrace("REQUEST:" + nameof(HttpClient2));

        //     var client = new HttpClient();
        //     client.Timeout = Timeout.InfiniteTimeSpan;
        //     client.BaseAddress = new Uri("https://ferien-api.de");

        //     var response = await client.GetAsync("api/v1/holidays/BW");
        //     var result = await response.Content.ReadAsStringAsync();

        //     _logger.LogTrace($"RESULT:{result}");
        //     return Ok(new HealthResult { Result = result });
        // }

        [HttpGet(nameof(CurlWithToken))]
        [AllowAnonymous]
        public async Task<ActionResult<HealthResult>> CurlWithToken()
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
            _logger.LogTrace($"RESULT:{consoleResult}");
            return Ok(new HealthResult { Result = consoleResult });
        }

        [HttpGet(nameof(CurlWithoutToken))]
        [AllowAnonymous]
        public ActionResult<HealthResult> CurlWithoutToken()
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
            _logger.LogTrace($"RESULT:{consoleResult}");
            return Ok(new HealthResult { Result = consoleResult });
        }
    }
}

