using System.Text.RegularExpressions;
using Typesense;

namespace SearchService.MessageHandlers;

public class QuestionUpdatedHandler(ITypesenseClient client)
{
    private static readonly Regex HtmlRgx = new("<.*?>", RegexOptions.Compiled);

    public async Task HandleAsync(Contracts.QuestionUpdated message)
    {
        await client.UpdateDocument("questions", message.QuestionId, new
        {
            message.Title,
            Content = StripHtml(message.Content),
            Tags = message.Tags.ToArray()
        });

        Console.WriteLine($"Document updated: {message.QuestionId}");
    }

    private static string StripHtml(string content) => HtmlRgx.Replace(content, string.Empty);
}