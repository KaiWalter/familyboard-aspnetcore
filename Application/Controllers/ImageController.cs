using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using FamilyBoard.Core.Image;
using Microsoft.AspNetCore.Authorization;

namespace FamilyBoard.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;

        private readonly IConfiguration _configuration;

        private readonly IImageService _imageService;

        public ImageController(ILogger<ImageController> logger,
                            IConfiguration configuration,
                            IImageService imageService)
        {
            _logger = logger;
            _configuration = configuration;
            _imageService = imageService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ImageResponse>> GetNextImage()
        {
            _logger.LogTrace("REQUEST:" + nameof(GetNextImage));

            var result = await _imageService.GetNextImage();
            return Ok(result);
        }
    }
}

