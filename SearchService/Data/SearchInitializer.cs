using Typesense;

namespace SearchService.Data;

public static class SearchInitializer
{
    public static async Task EnsureIndexExists(ITypesenseClient client)
    {
        const string schemaName = "questions";

        try
        {
            await client.RetrieveCollection(schemaName);
            Console.WriteLine($"Collection {schemaName} has been created already.");
            return;
        }
        catch (TypesenseApiNotFoundException)
        {
            Console.WriteLine($"Collection {schemaName} has not been created yet.");
        }

        var schema = new Schema(schemaName, new List<Field>
        {
            new("id", FieldType.String) { Index = true },
            new("title", FieldType.String) { Index = true },
            new("content", FieldType.String) { Index = true },
            new("tags", FieldType.StringArray) { Index = true },
            new("createdAt", FieldType.Int64) { Index = true },
            new("hasAcceptedAnswer", FieldType.Bool) { Index = true },
            new("answerCount", FieldType.Int32) { Index = true }
        })
        { DefaultSortingField = "createdAt" };

        await client.CreateCollection(schema);
        Console.WriteLine($"Collection {schemaName} has been created.");
    }
}