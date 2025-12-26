using Contracts;
using Typesense;

namespace SearchService.MessageHandlers;

public class AnswerCountHandler(ITypesenseClient client)
{
    public async Task HandleAsync(UpdatedAnswerCount message)
    {
        await client.UpdateDocument("questions", message.QuestionId, new
        {
            AnswerCount = message.AnswerCount
        });

        Console.WriteLine($"Document answer count updated: {message.QuestionId} -> {message.AnswerCount}");
    }
}
