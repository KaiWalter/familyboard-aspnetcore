using Microsoft.Extensions.Options;

namespace FamilyBoard.Core.Cache
{
    public class DiskCacheOptions : IOptions<DiskCacheOptions>
    {
        public string CachePath { get; set; }

        /// <inheritdoc cref="IOptions{TOptions}.Value"/>
        DiskCacheOptions IOptions<DiskCacheOptions>.Value => this;
    }
}