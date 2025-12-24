using QuestionService.Data.Models;

namespace QuestionService.Data.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(string id, CancellationToken ct = default);
    
    Task<Tag?> GetBySlugAsync(string slug, CancellationToken ct = default);
    
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default);
    
    Task<IReadOnlyList<Tag>> GetBySlugsAsync(IEnumerable<string> slugs, CancellationToken ct = default);
    
    Task<IReadOnlyList<string>> GetInvalidSlugsAsync(IEnumerable<string> slugs, CancellationToken ct = default);
    
    Task<bool> AllExistAsync(IEnumerable<string> slugs, CancellationToken ct = default);
    
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    
    Task<Tag> AddAsync(Tag tag, CancellationToken ct = default);
    
    Task<Tag?> UpdateAsync(string id, Func<Tag, Tag> update, CancellationToken ct = default);
    
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
