

using System;
using System.Linq;
using System.Threading.Tasks;
using IntegratedCacheUtils.Stores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web.TokenCacheProviders;

namespace FamilyBoard.Core.Image
{
    public class OnedriveService : IImageService
    {
        private readonly ILogger<OnedriveService> _logger;
        private readonly IMsalAccountActivityStore _msalAccountActivityStore;
        private readonly IMsalTokenCacheProvider _msalTokenCacheProvider;
        private readonly IConfiguration _configuration;

        public OnedriveService(ILogger<OnedriveService> logger,
                            IConfiguration configuration,
                            IMsalAccountActivityStore msalAccountActivityStore,
                            IMsalTokenCacheProvider msalTokenCacheProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _msalAccountActivityStore = msalAccountActivityStore;
            _msalTokenCacheProvider = msalTokenCacheProvider;
        }

        public async Task<ImageResponse> GetNextImage()
        {
            string[] scopes = _configuration.GetValue<string>("Graph:Scopes")?.Split(' ');
            var graphServiceClient = GetGraphServiceClient(scopes);

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

        private GraphServiceClient GetGraphServiceClient(string[] scopes)
        {
            return GraphServiceClientFactory.GetAuthenticatedGraphClient(async () =>
            {
                IConfidentialClientApplication app = GetConfidentialClientApplication();
                var account = await _msalAccountActivityStore.GetMsalAccountLastActivity();
                var token = await app.AcquireTokenSilent(scopes, new MsalAccount
                {
                    HomeAccountId = new AccountId(
                                            account.AccountIdentifier,
                                            account.AccountObjectId,
                                            account.AccountTenantId)
                })
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                return token.AccessToken;
            }, _configuration.GetValue<string>("Graph:BaseUrl"));
        }

        private IConfidentialClientApplication GetConfidentialClientApplication()
        {
            var config = new AuthenticationConfig();
            _configuration.GetSection("AzureAd").Bind(config);
            var app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority(new Uri(config.Authority))
                .Build();

            _msalTokenCacheProvider.Initialize(app.UserTokenCache);

            return app;
        }
    }
}