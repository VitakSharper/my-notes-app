using System.Text.RegularExpressions;
using Typesense;

namespace SearchService.MessageHandlers;

public class QuestionCreatedHandler(ITypesenseClient client)
{
    private static readonly Regex HtmlRgx = new("<.*?>", RegexOptions.Compiled);
    public async Task HandleAsync(Contracts.QuestionCreated message)
    {
        var created = new DateTimeOffset(message.Created).ToUnixTimeSeconds();

        var searchQuestion = new Models.SearchQuestion
        {
            Id = message.QuestionId,
            Title = message.Title,
            Content = StripHtml(message.Content),
            Tags = message.Tags.ToArray(),
            CreatedAt = created
        };

        await client.CreateDocument("questions", searchQuestion);

        Console.WriteLine($"Document created: {searchQuestion.Id}");
    }

    private static string StripHtml(string content) => HtmlRgx.Replace(content, string.Empty);
}