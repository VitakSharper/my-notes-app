using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ProfileService.Data;
using ProfileService.DTOs;

namespace ProfileService.Endpoints;

public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/me", async (ClaimsPrincipal user, ProfileDbContext db) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null) return Results.Unauthorized();

            var profile = await db.UserProfiles.FindAsync(userId);

            return profile is null ? Results.NotFound() : Results.Ok(profile);
        }).RequireAuthorization();

        // Anonymous on purpose: questions are readable without signing in, and enriching them with
        // display names has to work for those readers too.
        app.MapGet("/profiles/batch", async (string ids, ProfileDbContext db) =>
        {
            // Distinct because the same user can hold several answers on one question.
            var list = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();

            var rows = await db.UserProfiles
                .Where(profile => list.Contains(profile.Id))
                .Select(profile =>
                    new ProfileSummaryDto(profile.Id, profile.DisplayName, profile.Reputation))
                .ToListAsync();

            return Results.Ok(rows);
        });

        return app;
    }
}
