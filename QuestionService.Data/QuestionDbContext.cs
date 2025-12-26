using Microsoft.EntityFrameworkCore;
using QuestionService.Data.Models;

namespace QuestionService.Data;

public class QuestionDbContext(DbContextOptions<QuestionDbContext> options) : DbContext(options)
{
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuestionDbContext).Assembly);
    }
}
