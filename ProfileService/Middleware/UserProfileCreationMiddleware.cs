using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ProfileService.Data;
using ProfileService.Models;

namespace ProfileService.Middleware;

/// <summary>
/// Creates the profile row the first time an authenticated user reaches this service. Registration
/// stays with Keycloak, so there is no sign-up hook to attach to: the token itself is the trigger,
/// and its subject becomes the profile id.
/// </summary>
public class UserProfileCreationMiddleware(RequestDelegate next)
{
    // Keycloak JWT claim names, same order of preference as QuestionsController: the realm puts the
    // username in preferred_username, and ClaimTypes.Name alone resolved to nothing - every profile
    // created before this was named "un-named".
    private const string KeycloakPreferredUsernameClaim = "preferred_username";
    private const string KeycloakNameClaim = "name";

    public async Task InvokeAsync(HttpContext context, ProfileDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated is true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var name = context.User.FindFirstValue(KeycloakPreferredUsernameClaim)
                       ?? context.User.FindFirstValue(KeycloakNameClaim)
                       ?? context.User.FindFirstValue(ClaimTypes.Name);

            if (userId is not null)
            {
                var exists = await db.UserProfiles.AnyAsync(profile => profile.Id == userId);

                if (!exists)
                {
                    db.UserProfiles.Add(new UserProfile
                    {
                        Id = userId,
                        // Keycloak requires a username, so the chain above yields one in practice;
                        // the literal is only here to keep DisplayName non-null.
                        DisplayName = name ?? "un-named"
                    });

                    await db.SaveChangesAsync();
                }
            }
        }

        await next(context);
    }
}
