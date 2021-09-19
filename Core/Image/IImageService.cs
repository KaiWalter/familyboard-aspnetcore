using System.Threading.Tasks;

namespace FamilyBoard.Core.Image
{
    public interface IImageService
    {
        Task<ImageResponse> GetNextImage();
    }
}