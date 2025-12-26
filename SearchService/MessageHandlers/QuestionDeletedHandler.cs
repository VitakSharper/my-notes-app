using Typesense;

namespace SearchService.MessageHandlers;

public class QuestionDeletedHandler(ITypesenseClient client)
{
    public async Task HandleAsync(Contracts.QuestionDeleted message)
    {
        await client.DeleteDocument<Models.SearchQuestion>("questions", message.QuestionId);

        Console.WriteLine($"Document deleted: {message.QuestionId}");
    }
}
