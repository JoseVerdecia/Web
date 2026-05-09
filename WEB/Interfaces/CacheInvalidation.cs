namespace WEB.Interfaces;


/*public interface ICacheInvalidator
{
    Task EvictByTagAsync(string tag, CancellationToken cancellationToken = default);
}


public class MemoryCacheInvalidator : ICacheInvalidator
{
    private readonly IMemoryCache _cache;
    public MemoryCacheInvalidator(IMemoryCache cache) => _cache = cache;

    public Task EvictByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        (_cache as MemoryCache)?.Compact(1.0);
        return Task.CompletedTask;
    }
}*/