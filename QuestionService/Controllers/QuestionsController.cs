using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionService.Data.Functional;
using QuestionService.Data.Models;
using QuestionService.Data.Repositories;
using QuestionService.DTOs;
using System.Security.Claims;

namespace QuestionService.Controllers;

[ApiController]
[Route("[controller]")]
public class QuestionsController(IQuestionRepository repository, ITagRepository tagRepository) : ControllerBase
{
    // Keycloak JWT claim names
    private const string KeycloakSubjectClaim = "sub";
    private const string KeycloakPreferredUsernameClaim = "preferred_username";
    private const string KeycloakNameClaim = "name";

    [HttpGet("{id}")]
    public async Task<ActionResult<Question>> GetQuestionById(string id, CancellationToken ct)
    {
        var result = await repository
            .UpdateAsync(id, q => { q.ViewCount++; return q; }, ct)
            .ToResultAsync(Error.NotFound("Question", id));

        return result.Match(ToActionResult, ToErrorResult);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Question>>> GetAllQuestions([FromQuery] string? tag, CancellationToken ct) =>
        tag is not null
            ? Ok(await repository.GetByTagSlugAsync(tag, ct))
            : Ok(await repository.GetAllAsync(ct));

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IReadOnlyList<Question>>> GetQuestionsByUser(string userId, CancellationToken ct) =>
        Ok(await repository.GetByAskerIdAsync(userId, ct));

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Question>> CreateQuestion(CreateQuestionDto dto, CancellationToken ct) =>
        await ExtractUserInfo()
            .MatchAsync(
                onSuccess: async user =>
                {
                    var tagValidation = await ValidateTagsAsync(dto.Tags, ct);
                    return await tagValidation.MatchAsync(
                        onSuccess: async _ =>
                        {
                            var question = CreateQuestionFromDto(dto, user);
                            var created = await repository.AddAsync(question, ct);
                            return (ActionResult<Question>)CreatedAtAction(nameof(GetQuestionById), new { id = created.Id }, created);
                        },
                        onFailure: error => Task.FromResult<ActionResult<Question>>(BadRequest(error.Message))
                    );
                },
                onFailure: error => Task.FromResult<ActionResult<Question>>(Unauthorized(error.Message))
            );

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<Question>> UpdateQuestion(string id, UpdateQuestionDto dto, CancellationToken ct) =>
        await ExtractUserInfo()
            .MatchAsync(
                onSuccess: async user =>
                {
                    if (dto.Tags is not null)
                    {
                        var tagValidation = await ValidateTagsAsync(dto.Tags, ct);
                        if (tagValidation.IsFailure)
                            return tagValidation.Match(_ => default!, ToErrorResult);
                    }

                    var result = await repository
                        .UpdateAsync(id, q => ApplyUpdate(q, dto, user.UserId), ct)
                        .ToResultAsync(Error.NotFound("Question", id));
                    return result.Match(ToActionResult, ToErrorResult);
                },
                onFailure: error => Task.FromResult<ActionResult<Question>>(Unauthorized(error.Message))
            );

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteQuestion(string id, CancellationToken ct) =>
        await repository.DeleteAsync(id, ct) switch
        {
            true => NoContent(),
            false => NotFound()
        };

    private async Task<Result<bool>> ValidateTagsAsync(List<string> tagSlugs, CancellationToken ct)
    {
        if (tagSlugs.Count == 0)
            return true;

        var invalidSlugs = await tagRepository.GetInvalidSlugsAsync(tagSlugs, ct);

        return invalidSlugs.Count == 0
            ? true
            : Error.Validation($"Invalid tags: {string.Join(", ", invalidSlugs)}");
    }

    private Result<UserInfo> ExtractUserInfo()
    {
        var userId = User.FindFirstValue(KeycloakSubjectClaim)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        var userName = User.FindFirstValue(KeycloakPreferredUsernameClaim)
                    ?? User.FindFirstValue(KeycloakNameClaim)
                    ?? User.FindFirstValue(ClaimTypes.Name);

        return (userId, userName) switch
        {
            ({ } id, { } name) => new UserInfo(id, name),
            _ => Error.Unauthorized("User identity could not be determined")
        };

    }

    private static Question CreateQuestionFromDto(CreateQuestionDto dto, UserInfo user) =>
        new()
        {
            Title = dto.Title,
            Content = dto.Content,
            TagSlugs = dto.Tags,
            AskerId = user.UserId,
            AskerDisplayName = user.DisplayName
        };

    private static Question ApplyUpdate(Question question, UpdateQuestionDto dto, string userId)
    {
        if (question.AskerId != userId) return question;

        question.Title = dto.Title ?? question.Title;
        question.Content = dto.Content ?? question.Content;
        question.TagSlugs = dto.Tags ?? question.TagSlugs;
        question.UpdatedAt = DateTime.UtcNow;
        return question;
    }

    private ActionResult<Question> ToActionResult(Question question) => Ok(question);

    private ActionResult<Question> ToErrorResult(Error error) => error.Code switch
    {
        "NotFound" => NotFound(error.Message),
        "Unauthorized" => Unauthorized(error.Message),
        "Validation" => BadRequest(error.Message),
        _ => BadRequest(error.Message)
    };

    private sealed record UserInfo(string UserId, string DisplayName);
}