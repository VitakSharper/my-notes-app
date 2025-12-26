using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuestionService.Data;
using QuestionService.Data.Models;

namespace QuestionService.Services;

public class TagService(IMemoryCache cache, QuestionDbContext dbContext)
{
    private const string CacheKey = "TagService:Tags";

    public async Task<List<Tag>> GetTagsAsync(CancellationToken ct)
    {
        return await cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2);
            var tags = await dbContext.Tags.AsNoTracking().ToListAsync(ct);
            return tags;
        }) ?? [];
    }
    public void InvalidateCache() => cache.Remove(CacheKey);

    public async Task<bool> AreTagsValidAsync(List<string> slugs, CancellationToken ct)
    {
        var tags = await GetTagsAsync(ct);
        var tagSet = tags.Select(x => x.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return slugs.All(slug => tagSet.Contains(slug));
    }
}