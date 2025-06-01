using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FamilyBoard.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ImageController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;

        private readonly IConfiguration _configuration;

        private readonly IImageService _imageService;

        public ImageController(
            ILogger<ImageController> logger,
            IConfiguration configuration,
            IImageService imageService
        )
        {
            _logger = logger;
            _configuration = configuration;
            _imageService = imageService;
        }

        /// <summary>
        /// Retrieves the URL of the next image to disply.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ImageResponse), StatusCodes.Status200OK, "application/json")]
        public async Task<ActionResult<ImageResponse>> GetNextImage()
        {
            _logger.LogTrace("REQUEST:" + nameof(GetNextImage));

            var result = await _imageService.GetNextImage();
            return Ok(result);
        }
    }
}
