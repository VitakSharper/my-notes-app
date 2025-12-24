namespace QuestionService.DTOs;

public record UpdateQuestionDto(string? Title = null, string? Content = null, List<string>? Tags = null);
