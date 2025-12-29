using QuestionService.Data.Repositories;
using QuestionService.Services;

namespace QuestionService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuestionServices(this IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IAnswerRepository, AnswerRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        // Register services
        services.AddMemoryCache();
        services.AddScoped<TagService>();

        return services;
    }
}
