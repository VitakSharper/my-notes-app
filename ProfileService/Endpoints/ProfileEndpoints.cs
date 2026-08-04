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

        // The caller can only edit their own profile, so there is no id in the route: the token
        // decides whose row is written. The updated profile comes back so the client can refresh the
        // session it built at sign-in.
        app.MapPut("/profiles/me", async (
            UpdateProfileDto dto, ClaimsPrincipal user, ProfileDbContext db) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null) return Results.Unauthorized();

            var profile = await db.UserProfiles.FindAsync(userId);

            // The middleware creates the row on the way in, so this only fires if it was deleted
            // between that and here.
            if (profile is null) return Results.NotFound();

            profile.DisplayName = dto.DisplayName;
            profile.Description = dto.Description;

            await db.SaveChangesAsync();

            return Results.Ok(profile);
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

        // One profile, for the page a question card links to. Anonymous for the same reason as the
        // batch above, and declared last on purpose: routing prefers a literal segment over a
        // parameter, so "me" and "batch" still reach their own handlers rather than being read as an
        // id - but reading in that order makes the precedence obvious.
        app.MapGet("/profiles/{id}", async (string id, ProfileDbContext db) =>
        {
            var profile = await db.UserProfiles.FindAsync(id);

            return profile is null ? Results.NotFound() : Results.Ok(profile);
        });

        return app;
    }
}
