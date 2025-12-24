using Microsoft.EntityFrameworkCore;
using QuestionService.Data.Models;

namespace QuestionService.Data.Repositories;

public sealed class TagRepository(QuestionDbContext context) : ITagRepository
{
    public Task<Tag?> GetByIdAsync(string id, CancellationToken ct = default) =>
        context.Tags.FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Tag?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        context.Tags.FirstOrDefaultAsync(t => t.Slug == slug, ct);

    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default) =>
        await context.Tags.ToListAsync(ct);

    public async Task<IReadOnlyList<Tag>> GetBySlugsAsync(IEnumerable<string> slugs, CancellationToken ct = default) =>
        await context.Tags
            .Where(t => slugs.Contains(t.Slug))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetInvalidSlugsAsync(IEnumerable<string> slugs, CancellationToken ct = default)
    {
        var slugList = slugs.ToList();
        var existingSlugs = await context.Tags
            .Where(t => slugList.Contains(t.Slug))
            .Select(t => t.Slug)
            .ToListAsync(ct);

        return slugList.Except(existingSlugs).ToList();
    }

    public async Task<bool> AllExistAsync(IEnumerable<string> slugs, CancellationToken ct = default)
    {
        var slugList = slugs.ToList();
        var count = await context.Tags.CountAsync(t => slugList.Contains(t.Slug), ct);
        return count == slugList.Count;
    }

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) =>
        context.Tags.AnyAsync(t => t.Slug == slug, ct);

    public async Task<Tag> AddAsync(Tag tag, CancellationToken ct = default)
    {
        context.Tags.Add(tag);
        await context.SaveChangesAsync(ct);
        return tag;
    }

    public async Task<Tag?> UpdateAsync(string id, Func<Tag, Tag> update, CancellationToken ct = default) =>
        await context.Tags.FirstOrDefaultAsync(t => t.Id == id, ct) switch
        {
            { } existing => await ApplyUpdateAsync(existing, update, ct),
            null => null
        };

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default) =>
        await context.Tags.FirstOrDefaultAsync(t => t.Id == id, ct) switch
        {
            { } existing => await RemoveAndSaveAsync(existing, ct),
            null => false
        };

    private async Task<Tag> ApplyUpdateAsync(Tag existing, Func<Tag, Tag> update, CancellationToken ct)
    {
        update(existing);
        await context.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<bool> RemoveAndSaveAsync(Tag existing, CancellationToken ct)
    {
        context.Tags.Remove(existing);
        await context.SaveChangesAsync(ct);
        return true;
    }
}
