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
    public async Task InvokeAsync(HttpContext context, ProfileDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated is true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var name = context.User.FindFirstValue(ClaimTypes.Name);

            if (userId is not null)
            {
                var exists = await db.UserProfiles.AnyAsync(profile => profile.Id == userId);

                if (!exists)
                {
                    db.UserProfiles.Add(new UserProfile
                    {
                        Id = userId,
                        // The claim is there in practice; the fallback only keeps the compiler happy.
                        DisplayName = name ?? "un-named"
                    });

                    await db.SaveChangesAsync();
                }
            }
        }

        await next(context);
    }
}
