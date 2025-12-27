using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionService.Data.Functional;
using QuestionService.Data.Models;
using QuestionService.Data.Repositories;
using QuestionService.DTOs;
using QuestionService.Services;
using System.Security.Claims;
using Wolverine;

namespace QuestionService.Controllers;

[ApiController]
[Route("[controller]")]
public class QuestionsController(
    IQuestionRepository repository,
    IAnswerRepository answerRepository,
    TagService tagService,
    IMessageBus bus) : ControllerBase
{
    // Keycloak JWT claim names
    private const string KeycloakSubjectClaim = "sub";
    private const string KeycloakPreferredUsernameClaim = "preferred_username";
    private const string KeycloakNameClaim = "name";

    [HttpGet("{id}")]
    public async Task<ActionResult<Question>> GetQuestionById(string id, CancellationToken ct)
    {
        var question = await repository.GetByIdWithAnswersAsync(id, ct);

        if (question is null)
            return NotFound($"Question with id '{id}' was not found.");

        question.ViewCount++;
        await repository.UpdateAsync(id, q =>
        {
            q.ViewCount = question.ViewCount;
            return q;
        }, ct);

        return Ok(question);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Question>>> GetAllQuestions([FromQuery] string? tag,
        CancellationToken ct) =>
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
                    if (dto.Tags.Count > 0 && !await tagService.AreTagsValidAsync(dto.Tags, ct))
                        return (ActionResult<Question>)BadRequest("One or more tags are invalid.");

                    var question = CreateQuestionFromDto(dto, user);
                    var created = await repository.AddAsync(question, ct);

                    await bus.PublishAsync(new QuestionCreated(
                        created.Id!,
                        created.Title,
                        created.Content,
                        created.CreatedAt,
                        created.TagSlugs));

                    return (ActionResult<Question>)CreatedAtAction(nameof(GetQuestionById), new { id = created.Id },
                        created);
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
                    if (dto.Tags is not null && dto.Tags.Count > 0 && !await tagService.AreTagsValidAsync(dto.Tags, ct))
                        return (ActionResult<Question>)BadRequest("One or more tags are invalid.");

                    var result = await repository
                        .UpdateAsync(id, q => ApplyUpdate(q, dto, user.UserId), ct)
                        .ToResultAsync(Error.NotFound("Question", id));

                    return result.Match(
                        onSuccess: updated =>
                        {
                            bus.PublishAsync(new QuestionUpdated(
                                updated.Id!,
                                updated.Title,
                                updated.Content,
                                updated.TagSlugs));
                            return ToActionResult(updated);
                        },
                        onFailure: ToErrorResult);
                },
                onFailure: error => Task.FromResult<ActionResult<Question>>(Unauthorized(error.Message))
            );

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteQuestion(string id, CancellationToken ct)
    {
        var deleted = await repository.DeleteAsync(id, ct);

        if (!deleted) return NotFound();
        await bus.PublishAsync(new QuestionDeleted(id));
        return NoContent();
    }

    // Answer endpoints

    [Authorize]
    [HttpPost("{questionId}/answers")]
    public async Task<ActionResult<Answer>>
        CreateAnswer(string questionId, CreateAnswerDto dto, CancellationToken ct) =>
        await ExtractUserInfo()
            .MatchAsync(
                onSuccess: async user =>
                {
                    var question = await repository.GetByIdAsync(questionId, ct);
                    if (question is null)
                        return (ActionResult<Answer>)NotFound($"Question with id '{questionId}' was not found.");

                    var answer = new Answer
                    {
                        QuestionId = questionId,
                        Content = dto.Content,
                        AuthorId = user.UserId,
                        AuthorDisplayName = user.DisplayName
                    };

                    var created = await answerRepository.AddAsync(answer, ct);

                    var updatedQuestion = await repository.UpdateAsync(questionId, q =>
                    {
                        q.AnswerCount++;
                        return q;
                    }, ct);

                    await bus.PublishAsync(new UpdatedAnswerCount(questionId, updatedQuestion!.AnswerCount));

                    return (ActionResult<Answer>)CreatedAtAction(nameof(GetQuestionById), new { id = questionId },
                        created);
                },
                onFailure: error => Task.FromResult<ActionResult<Answer>>(Unauthorized(error.Message))
            );

    [Authorize]
    [HttpPut("{questionId}/answers/{answerId}")]
    public async Task<ActionResult> UpdateAnswer(string questionId, string answerId, UpdateAnswerDto dto,
        CancellationToken ct) =>
        await ExtractUserInfo()
            .MatchAsync(
                onSuccess: async user =>
                {
                    var answer = await answerRepository.GetByIdAsync(answerId, ct);
                    if (answer is null || answer.QuestionId != questionId)
                        return (ActionResult)NotFound($"Answer with id '{answerId}' was not found.");

                    if (answer.AuthorId != user.UserId)
                        return Unauthorized("You can only update your own answers.");

                    await answerRepository.UpdateAsync(answerId, a =>
                    {
                        a.Content = dto.Content ?? a.Content;
                        return a;
                    }, ct);

                    return NoContent();
                },
                onFailure: error => Task.FromResult<ActionResult>(Unauthorized(error.Message))
            );

    [Authorize]
    [HttpDelete("{questionId}/answers/{answerId}")]
    public async Task<ActionResult> DeleteAnswer(string questionId, string answerId, CancellationToken ct) =>
        await ExtractUserInfo()
            .MatchAsync(
                onSuccess: async user =>
                {
                    var answer = await answerRepository.GetByIdAsync(answerId, ct);
                    if (answer is null || answer.QuestionId != questionId)
                        return (ActionResult)NotFound($"Answer with id '{answerId}' was not found.");

                    if (answer.AuthorId != user.UserId)
                        return Unauthorized("You can only delete your own answers.");

                    if (answer.IsAccepted)
                        return BadRequest("Cannot delete an accepted answer.");

                    await answerRepository.DeleteAsync(answerId, ct);

                    var updatedQuestion = await repository.UpdateAsync(questionId, q =>
                    {
                        q.AnswerCount = Math.Max(0, q.AnswerCount - 1);
                        return q;
                    }, ct);

                    await bus.PublishAsync(new UpdatedAnswerCount(questionId, updatedQuestion!.AnswerCount));

                    return NoContent();
                },
                onFailure: error => Task.FromResult<ActionResult>(Unauthorized(error.Message))
            );

    [Authorize]
    [HttpPost("{questionId}/answers/{answerId}/accept")]
    public async Task<ActionResult> AcceptAnswer(string questionId, string answerId, CancellationToken ct) =>
        await ExtractUserInfo()
            .MatchAsync(
                onSuccess: async user =>
                {
                    var question = await repository.GetByIdAsync(questionId, ct);
                    if (question is null)
                        return NotFound($"Question with id '{questionId}' was not found.");

                    if (question.AskerId != user.UserId)
                        return Unauthorized("Only the question author can accept an answer.");

                    if (question.HasAcceptedAnswer)
                        return BadRequest("This question already has an accepted answer.");

                    var answer = await answerRepository.GetByIdAsync(answerId, ct);
                    if (answer is null || answer.QuestionId != questionId)
                        return NotFound($"Answer with id '{answerId}' was not found.");

                    await answerRepository.UpdateAsync(answerId, a =>
                    {
                        a.IsAccepted = true;
                        return a;
                    }, ct);

                    await repository.UpdateAsync(questionId, q =>
                    {
                        q.HasAcceptedAnswer = true;
                        return q;
                    }, ct);

                    await bus.PublishAsync(new AnswerAccepted(questionId, answerId));

                    return NoContent();
                },
                onFailure: error => Task.FromResult<ActionResult>(Unauthorized(error.Message))
            );

    //Error endpoint

    [HttpGet("errors")]
    public ActionResult GetErrorResponses(int code)
    {
        ModelState.AddModelError("Problem one", "Validation problem one");
        ModelState.AddModelError("Problem two", "Validation problem two");

        return code switch
        {
            400 => BadRequest("Opposite of good request."),
            401 => Unauthorized(),
            403 => Forbid(),
            404 => NotFound(),
            500 => throw new Exception("This is a server error."),
            _ => ValidationProblem(ModelState)
        };
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