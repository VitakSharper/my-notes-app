using System.ComponentModel.DataAnnotations;

namespace QuestionService.DTOs;

public record UpdateTagDto(
    [MaxLength(50)] string? Name = null,
    [MaxLength(1000)] string? Description = null);
