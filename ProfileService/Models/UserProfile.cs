using System.ComponentModel.DataAnnotations;

namespace ProfileService.Models;

/// <summary>
/// What the application knows about a user, as opposed to what Keycloak knows: Keycloak owns the
/// credentials and the identity, this owns the parts a user can edit and the parts the app awards.
/// The id is the Keycloak subject, which is what ties the two together.
/// </summary>
public class UserProfile
{
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(200)]
    public required string DisplayName { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Awarded by the app - votes and accepted answers, once those services exist.</summary>
    public int Reputation { get; set; }
}
