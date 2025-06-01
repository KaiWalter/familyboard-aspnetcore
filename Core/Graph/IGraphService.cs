using Microsoft.Graph;

namespace FamilyBoard.Core.Graph
{
    public interface IGraphService
    {
        public GraphServiceClient GetGraphServiceClient();
    }
}
