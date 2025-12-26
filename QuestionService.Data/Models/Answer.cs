using System.ComponentModel.DataAnnotations;

namespace QuestionService.Data.Models;

public class Answer
{
    [MaxLength(36)] public string Id { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(5000)] public required string Content { get; set; }

    [MaxLength(36)] public required string QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    [MaxLength(36)] public required string AuthorId { get; set; }
    [MaxLength(300)] public required string AuthorDisplayName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsAccepted { get; set; }
    public int Votes { get; set; }
}
