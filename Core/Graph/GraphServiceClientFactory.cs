using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Graph;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FamilyBoard.Core.Graph
{
    // Factory to instantiate the GraphServiceClient to be used to call Graph
    public class GraphServiceClientFactory
    {
        public static GraphServiceClient GetAuthenticatedGraphClient(Func<Task<string>> acquireAccessToken,
                                                                                 string baseUrl)
        {
            var httpClient = GraphClientFactory.Create(new CustomAuthenticationProvider(acquireAccessToken));
            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.Timeout = Timeout.InfiniteTimeSpan;
            return new GraphServiceClient(httpClient);
        }
    }

    internal class CustomAuthenticationProvider : IAuthenticationProvider
    {
        public CustomAuthenticationProvider(Func<Task<string>> acquireTokenCallback)
        {
            acquireAccessToken = acquireTokenCallback;
        }

        private readonly Func<Task<string>> acquireAccessToken;

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            string accessToken = await acquireAccessToken.Invoke();

            // Append the access token to the request.
            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
        }
    }
}