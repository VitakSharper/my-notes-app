using System.Text.RegularExpressions;
using Typesense;

namespace SearchService.MessageHandlers;

public class QuestionUpdatedHandler(ITypesenseClient client)
{
    private static readonly Regex HtmlRgx = new("<.*?>", RegexOptions.Compiled);

    public async Task HandleAsync(Contracts.QuestionUpdated message)
    {

        var searchQuestion = new Models.SearchQuestion
        {
            Id = message.QuestionId,
            Title = message.Title,
            Content = StripHtml(message.Content),
            Tags = message.Tags.ToArray(),
        };

        await client.UpdateDocument("questions", searchQuestion.Id, searchQuestion);

        Console.WriteLine($"Document updated: {searchQuestion.Id}");
    }

    private static string StripHtml(string content) => HtmlRgx.Replace(content, string.Empty);
}
