using Microsoft.EntityFrameworkCore;
using QuestionService.Data.Converters;
using QuestionService.Data.Models;

namespace QuestionService.Data;

public class QuestionDbContext(DbContextOptions<QuestionDbContext> options) : DbContext(options)
{
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Every DateTime in this model is UTC; this keeps the kind through a round trip so the
        // JSON carries the Z the client needs. Same store type, so no migration is involved.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuestionDbContext).Assembly);
    }
}
