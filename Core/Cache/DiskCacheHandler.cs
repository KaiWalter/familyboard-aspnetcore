using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FamilyBoard.Core.Cache
{
    public class DiskCacheHandler : IDistributedCache
    {
        private readonly string _cachePath;
        private Dictionary<string, byte[]> _cache;

        private readonly ILogger<DiskCacheHandler> _logger;

        public DiskCacheHandler(IOptions<DiskCacheOptions> options, ILogger<DiskCacheHandler> logger)
        {
            _cachePath = options.Value.CachePath;
            _logger = logger;

            this.ReadFromDisk(CancellationToken.None).Wait();
        }

        public byte[] Get(string key)
        {
            return this.GetAsync(key, CancellationToken.None).Result;
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token)
        {
            if (_cache.ContainsKey(key))
            {
                return _cache[key];
            }
            return null;
        }

        public void Refresh(string key)
        {
            this.RefreshAsync(key, CancellationToken.None).Wait();
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            // do nothing - cache items to not expire
        }

        public void Remove(string key)
        {
            this.RemoveAsync(key, CancellationToken.None).Wait();
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (_cache.ContainsKey(key))
            {
                _cache.Remove(key);
                await WriteToDisk(token);
            }
        }

        public void Set(string key, byte[] value)
        {
            this.SetAsync(key, value, CancellationToken.None).Wait();
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            this.SetAsync(key, value, options, CancellationToken.None).Wait();
        }

        public async Task SetAsync(string key, byte[] value, CancellationToken token = default)
        {
            await this.SetPrivate(key, value);
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            await this.SetPrivate(key, value);
        }

        private async Task SetPrivate(string key, byte[] value, CancellationToken token = default)
        {
            _cache[key] = value;
            await this.WriteToDisk(token);
        }

        private async Task WriteToDisk(CancellationToken token = default)
        {
            var json = JsonSerializer.Serialize(_cache);
            await File.WriteAllTextAsync(_cachePath, json, token);
            _logger.LogInformation($"token written to cache {_cachePath}");
        }

        private async Task ReadFromDisk(CancellationToken token = default)
        {
            if (File.Exists(_cachePath))
            {
                string json = await File.ReadAllTextAsync(_cachePath);
                _cache = JsonSerializer.Deserialize<Dictionary<string, byte[]>>(json);
                _logger.LogInformation($"token retrieved from cache {_cachePath}");
            }
            else
            {
                _cache = new Dictionary<string, byte[]>();
                _logger.LogWarning($"no token cache found in {_cachePath}");
                await this.WriteToDisk(token);
            }
        }
    }
}