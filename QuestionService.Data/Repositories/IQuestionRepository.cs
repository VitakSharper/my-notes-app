using QuestionService.Data.Models;

namespace QuestionService.Data.Repositories;

public interface IQuestionRepository
{
    Task<Question?> GetByIdAsync(string id, CancellationToken ct = default);
    
    Task<Question?> GetByIdWithAnswersAsync(string id, CancellationToken ct = default);
    
    Task<IReadOnlyList<Question>> GetAllAsync(CancellationToken ct = default);
    
    Task<IReadOnlyList<Question>> GetByAskerIdAsync(string askerId, CancellationToken ct = default);
    
    Task<IReadOnlyList<Question>> GetByTagSlugAsync(string tagSlug, CancellationToken ct = default);
    
    Task<Question> AddAsync(Question question, CancellationToken ct = default);
    
    Task<Question?> UpdateAsync(string id, Func<Question, Question> update, CancellationToken ct = default);
    
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    
    Task<IReadOnlyList<Question>> QueryAsync(
        Func<IQueryable<Question>, IQueryable<Question>> queryBuilder,
        CancellationToken ct = default);
}
