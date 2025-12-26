using SearchService.Models;
using System.Text.RegularExpressions;
using Typesense;

namespace SearchService.Endpoints;

public static class SearchEndpoints
{
    private static readonly Regex SomeRgx = new(@"\[(.*?)\]", RegexOptions.Compiled);
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/search", async (string query, ITypesenseClient client) =>
        {
            string? tag = null;
            var tagMatch = SomeRgx.Match(query);
            if (tagMatch.Success)
            {
                tag = tagMatch.Groups[1].Value;
                query = SomeRgx.Replace(tagMatch.Value, "").Trim();
            }

            var searchParams = new SearchParameters(query, "title,content");

            if (!string.IsNullOrEmpty(tag))
            {
                searchParams.FilterBy = $"tags:=[{tag}]";
            }

            try
            {
                var result = await client.Search<SearchQuestion>("questions", searchParams);
                return Results.Ok(result.Hits.Select(hit => hit.Document));
            }
            catch (Exception e)
            {
                return Results.Problem("Typesense search failed.", e.Message);
            }
        });

        return app;
    }
}
