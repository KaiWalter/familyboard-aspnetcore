using System;
using FamilyBoard.Core.Cache.Stores;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web.TokenCacheProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FamilyBoard.Core.Graph
{
    public class GraphService : IGraphService
    {
        private readonly ILogger<GraphService> _logger;
        private readonly IMsalAccountActivityStore _msalAccountActivityStore;
        private readonly IMsalTokenCacheProvider _msalTokenCacheProvider;
        private readonly IConfiguration _configuration;

        public GraphService(ILogger<GraphService> logger,
                            IConfiguration configuration,
                            IMsalAccountActivityStore msalAccountActivityStore,
                            IMsalTokenCacheProvider msalTokenCacheProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _msalAccountActivityStore = msalAccountActivityStore;
            _msalTokenCacheProvider = msalTokenCacheProvider;
        }

        public GraphServiceClient GetGraphServiceClient()
        {
            return GraphServiceClientFactory.GetAuthenticatedGraphClient(async () =>
            {
                string[] scopes = _configuration.GetValue<string>(Constants.GraphScope)?.Split(' ');
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
            _configuration.GetSection(Constants.AzureAdConfigSectionName).Bind(config);
            var app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                                                          .WithClientSecret(config.ClientSecret)
                                                          .WithAuthority(new Uri(config.Authority))
                                                          .Build();

            _msalTokenCacheProvider.Initialize(app.UserTokenCache);

            return app;
        }
    }
}
