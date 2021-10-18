using System;
using System.Linq;
using System.Threading.Tasks;
using FamilyBoard.Core.Graph;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FamilyBoard.Core.Image
{
    public class OnedriveService : IImageService
    {
        private readonly ILogger<OnedriveService> _logger;
        private readonly IGraphService _graphService;
        private readonly IConfiguration _configuration;

        public OnedriveService(ILogger<OnedriveService> logger,
                            IConfiguration configuration,
                            IGraphService graphService)
        {
            _logger = logger;
            _configuration = configuration;
            _graphService = graphService;
        }

        public async Task<ImageResponse> GetNextImage()
        {
            var graphServiceClient = _graphService.GetGraphServiceClient();

            string folderName = _configuration["Images:FolderName"] ?? "Pictures";

            var images = await graphServiceClient
                        .Me
                        .Drive
                        .Root
                        .ItemWithPath(folderName)
                        .Children
                        .Request()
                        .Top(999)
                        .GetAsync();

            ImageResponse result = new ImageResponse
            {
                Src = "https://dummyimage.com/200x600/000/fff.png&text=FOLDERNOTFOUND"
            };

            var filteredImages = images
                .CurrentPage
                .Where(child => child.Image != null && child.Photo != null);

            if (filteredImages.Count() > 0)
            {
                var rnd = new Random();
                var index = rnd.Next(filteredImages.Count());

                var imageItem = filteredImages.ElementAt(index);

                if (imageItem.AdditionalData.ContainsKey("@microsoft.graph.downloadUrl"))
                {
                    result.Src = imageItem.AdditionalData["@microsoft.graph.downloadUrl"].ToString();
                }

                if (imageItem.Photo.TakenDateTime.HasValue)
                {
                    result.Month = imageItem.Photo.TakenDateTime.Value.Month;
                    result.Year = imageItem.Photo.TakenDateTime.Value.Year;
                }
            }

            return result;
        }

    }
}