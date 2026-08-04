using System.ComponentModel.DataAnnotations;

namespace ProfileService.DTOs;

/// <summary>
/// The editable half of a profile: Keycloak owns the identity, the app owns this. The lengths mirror
/// <see cref="Models.UserProfile"/> so the framework rejects what the database would truncate -
/// AddValidation in Program.cs is what turns these attributes into a 400.
/// </summary>
public record UpdateProfileDto(
    [Required] [MaxLength(200)] string DisplayName,
    [MaxLength(1000)] string? Description);
