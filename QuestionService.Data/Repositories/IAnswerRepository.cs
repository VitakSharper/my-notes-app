using QuestionService.Data.Models;

namespace QuestionService.Data.Repositories;

public interface IAnswerRepository
{
    Task<Answer?> GetByIdAsync(string id, CancellationToken ct = default);
    
    Task<IReadOnlyList<Answer>> GetByQuestionIdAsync(string questionId, CancellationToken ct = default);
    
    Task<Answer> AddAsync(Answer answer, CancellationToken ct = default);
    
    Task<Answer?> UpdateAsync(string id, Func<Answer, Answer> update, CancellationToken ct = default);
    
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
