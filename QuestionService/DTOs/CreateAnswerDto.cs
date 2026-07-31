using System.ComponentModel.DataAnnotations;

namespace QuestionService.DTOs;

// The question id comes from the route (POST /questions/{questionId}/answers) and that is what the
// controller uses, so requiring it in the body as well only ever rejected otherwise valid requests.
public record CreateAnswerDto([Required] string Content);
