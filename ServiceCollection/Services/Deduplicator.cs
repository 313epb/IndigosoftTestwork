using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace App.Services;

public class Deduplicator
{
    private readonly MemoryCache _cache;

    public bool IsDuplicate(Tick tick)
    {
        if (_cache.TryGetValue(tick.UniqueKey, out _))
        {
            return true;
        }
        _cache.Set(tick.UniqueKey, true, TimeSpan.FromSeconds(5));
        return false;
    }
}