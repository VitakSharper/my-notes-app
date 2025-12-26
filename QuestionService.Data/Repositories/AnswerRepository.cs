using Microsoft.EntityFrameworkCore;
using QuestionService.Data.Models;

namespace QuestionService.Data.Repositories;

public sealed class AnswerRepository(QuestionDbContext context) : IAnswerRepository
{
    public Task<Answer?> GetByIdAsync(string id, CancellationToken ct = default) =>
        context.Answers.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Answer>> GetByQuestionIdAsync(string questionId, CancellationToken ct = default) =>
        await context.Answers
            .Where(a => a.QuestionId == questionId)
            .ToListAsync(ct);

    public async Task<Answer> AddAsync(Answer answer, CancellationToken ct = default)
    {
        context.Answers.Add(answer);
        await context.SaveChangesAsync(ct);
        return answer;
    }

    public async Task<Answer?> UpdateAsync(string id, Func<Answer, Answer> update, CancellationToken ct = default) =>
        await context.Answers.FirstOrDefaultAsync(a => a.Id == id, ct) switch
        {
            { } existing => await ApplyUpdateAsync(existing, update, ct),
            null => null
        };

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default) =>
        await context.Answers.FirstOrDefaultAsync(a => a.Id == id, ct) switch
        {
            { } existing => await RemoveAndSaveAsync(existing, ct),
            null => false
        };

    private async Task<Answer> ApplyUpdateAsync(Answer existing, Func<Answer, Answer> update, CancellationToken ct)
    {
        var updated = update(existing);
        updated.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return updated;
    }

    private async Task<bool> RemoveAndSaveAsync(Answer existing, CancellationToken ct)
    {
        context.Answers.Remove(existing);
        await context.SaveChangesAsync(ct);
        return true;
    }
}
