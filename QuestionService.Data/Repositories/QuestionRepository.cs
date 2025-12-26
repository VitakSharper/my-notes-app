using Microsoft.EntityFrameworkCore;
using QuestionService.Data.Models;

namespace QuestionService.Data.Repositories;

public sealed class QuestionRepository(QuestionDbContext context) : IQuestionRepository
{
    public Task<Question?> GetByIdAsync(string id, CancellationToken ct = default) =>
        context.Questions.FirstOrDefaultAsync(q => q.Id == id, ct);

    public Task<Question?> GetByIdWithAnswersAsync(string id, CancellationToken ct = default) =>
        context.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id, ct);

    public async Task<IReadOnlyList<Question>> GetAllAsync(CancellationToken ct = default) =>
        await context.Questions.ToListAsync(ct);

    public async Task<IReadOnlyList<Question>> GetByAskerIdAsync(string askerId, CancellationToken ct = default) =>
        await context.Questions
            .Where(q => q.AskerId == askerId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Question>> GetByTagSlugAsync(string tagSlug, CancellationToken ct = default) =>
        await context.Questions
            .Where(q => q.TagSlugs.Contains(tagSlug))
            .ToListAsync(ct);

    public async Task<Question> AddAsync(Question question, CancellationToken ct = default)
    {
        context.Questions.Add(question);
        await context.SaveChangesAsync(ct);
        return question;
    }

    public async Task<Question?> UpdateAsync(string id, Func<Question, Question> update, CancellationToken ct = default) =>
        await context.Questions.FirstOrDefaultAsync(q => q.Id == id, ct) switch
        {
            { } existing => await ApplyUpdateAsync(existing, update, ct),
            null => null
        };

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default) =>
        await context.Questions.FirstOrDefaultAsync(q => q.Id == id, ct) switch
        {
            { } existing => await RemoveAndSaveAsync(existing, ct),
            null => false
        };

    public async Task<IReadOnlyList<Question>> QueryAsync(
        Func<IQueryable<Question>, IQueryable<Question>> queryBuilder,
        CancellationToken ct = default) =>
        await queryBuilder(context.Questions).ToListAsync(ct);

    private async Task<Question> ApplyUpdateAsync(Question existing, Func<Question, Question> update, CancellationToken ct)
    {
        var updated = update(existing);
        updated.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return updated;
    }

    private async Task<bool> RemoveAndSaveAsync(Question existing, CancellationToken ct)
    {
        context.Questions.Remove(existing);
        await context.SaveChangesAsync(ct);
        return true;
    }
}
