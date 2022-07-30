using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace FamilyBoard.Core.Graph
{
    public interface IGraphService
    {
        GraphServiceClient GetGraphServiceClient();
        Task<AuthenticationResult> GetAccessToken();
    }
}