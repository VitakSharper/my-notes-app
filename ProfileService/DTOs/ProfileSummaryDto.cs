namespace ProfileService.DTOs;

/// <summary>
/// The slice of a profile a question or an answer needs: enough to render a card, nothing more.
/// </summary>
public record ProfileSummaryDto(string UserId, string DisplayName, int Reputation);
