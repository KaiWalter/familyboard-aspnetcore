using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FamilyBoard.Core.Image
{
    public class OnedriveService : IImageService
    {
        private readonly ILogger<OnedriveService> _logger;
        private readonly IGraphService _graphService;
        private readonly IConfiguration _configuration;
        private readonly string _imagesPlayedPath;

        private Dictionary<string, ImagePlayed> _imagesPlayed = new Dictionary<string, ImagePlayed>();

        public OnedriveService(ILogger<OnedriveService> logger,
                            IConfiguration configuration,
                            IGraphService graphService)
        {
            _logger = logger;
            _configuration = configuration;
            _graphService = graphService;
            _imagesPlayedPath = System.Environment.GetEnvironmentVariable("IMAGESPLAYEDPATH") ?? ".imagesplayed.json";
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
                await MergeImagesPlayed(filteredImages);

                // choose random image - only choose from first half based on sort order
                var imageList = _imagesPlayed.ToList().OrderBy(x => x.Value.Counter).ThenBy(x => x.Value.LastPlayed).ToArray();
                var rnd = new Random();
                var index = rnd.Next(imageList.Count() / 2);
                var selectedImage = imageList[index];

                // find choosen image in OneDrive filtered list
                var imageItem = filteredImages.Where(x => x.Name == selectedImage.Key).FirstOrDefault();

                if (imageItem != null)
                {
                    if (imageItem.AdditionalData.ContainsKey("@microsoft.graph.downloadUrl"))
                    {
                        result.Src = imageItem.AdditionalData["@microsoft.graph.downloadUrl"].ToString();
                    }

                    if (imageItem.Photo.TakenDateTime.HasValue)
                    {
                        result.Month = imageItem.Photo.TakenDateTime.Value.Month;
                        result.Year = imageItem.Photo.TakenDateTime.Value.Year;
                    }

                    // update image played counter
                    if (_imagesPlayed.ContainsKey(imageItem.Name))
                    {
                        _imagesPlayed[imageItem.Name].Counter++;
                        _imagesPlayed[imageItem.Name].LastPlayed = DateTime.UtcNow;
                    }

                }

                await PersistImagesPlayed();
            }

            return result;
        }

        private async Task PersistImagesPlayed()
        {
            var imagesPlayedJson = JsonSerializer.Serialize(_imagesPlayed);
            await File.WriteAllTextAsync(_imagesPlayedPath, imagesPlayedJson);
        }

        private async Task MergeImagesPlayed(IEnumerable<Microsoft.Graph.DriveItem> filteredImages)
        {
            if (File.Exists(_imagesPlayedPath))
            {
                string json = await File.ReadAllTextAsync(_imagesPlayedPath);
                _imagesPlayed = JsonSerializer.Deserialize<Dictionary<string, ImagePlayed>>(json);
            }

            foreach (var kv in _imagesPlayed)
            {
                kv.Value.Exists = false;
            }

            foreach (var imageMergeItem in filteredImages)
            {
                if (_imagesPlayed.ContainsKey(imageMergeItem.Name))
                {
                    _imagesPlayed[imageMergeItem.Name].Exists = true;
                }
                else
                {
                    _imagesPlayed.Add(imageMergeItem.Name, new ImagePlayed
                    {
                        Counter = 0,
                        LastPlayed = DateTime.MinValue,
                        Exists = true,
                    });
                }
            }

            foreach (var kv in _imagesPlayed.ToList().Where(x => !x.Value.Exists))
            {
                _imagesPlayed.Remove(kv.Key);
            }
        }
    }
}