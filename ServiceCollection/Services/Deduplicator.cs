using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace App.Services;

public class Deduplicator(IMemoryCache cache, ILogger<Deduplicator> logger)
{
    public bool IsDuplicate(Tick tick)
    {
        if (cache.TryGetValue(tick.UniqueKey, out _))
        {
            return true;
        }
        cache.Set(tick.UniqueKey, true, TimeSpan.FromSeconds(5));
        return false;
    }
}