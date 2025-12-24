using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestionService.Data.Functional;
using QuestionService.Data.Models;
using QuestionService.Data.Repositories;
using QuestionService.DTOs;

namespace QuestionService.Controllers;

[ApiController]
[Route("[controller]")]
public class TagsController(ITagRepository repository) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<Tag>> GetTagById(string id, CancellationToken ct)
    {
        var result = await repository.GetByIdAsync(id, ct)
            .ToResultAsync(Error.NotFound("Tag", id));

        return result.Match(ToActionResult, ToErrorResult);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<Tag>> GetTagBySlug(string slug, CancellationToken ct)
    {
        var result = await repository.GetBySlugAsync(slug, ct)
            .ToResultAsync(Error.NotFound("Tag", slug));

        return result.Match(ToActionResult, ToErrorResult);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Tag>>> GetAllTags(CancellationToken ct) =>
        Ok(await repository.GetAllAsync(ct));

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Tag>> CreateTag(CreateTagDto dto, CancellationToken ct)
    {
        var validation = await ValidateSlugAsync(dto.Slug, ct);
        
        return await validation.MatchAsync(
            onSuccess: async _ =>
            {
                var tag = CreateTagFromDto(dto);
                var created = await repository.AddAsync(tag, ct);
                return (ActionResult<Tag>)CreatedAtAction(nameof(GetTagById), new { id = created.Id }, created);
            },
            onFailure: error => Task.FromResult<ActionResult<Tag>>(BadRequest(error.Message))
        );
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<Tag>> UpdateTag(string id, UpdateTagDto dto, CancellationToken ct)
    {
        var result = await repository
            .UpdateAsync(id, t => ApplyUpdate(t, dto), ct)
            .ToResultAsync(Error.NotFound("Tag", id));

        return result.Match(ToActionResult, ToErrorResult);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id, CancellationToken ct) =>
        await repository.DeleteAsync(id, ct) switch
        {
            true => NoContent(),
            false => NotFound()
        };

    private async Task<Result<bool>> ValidateSlugAsync(string slug, CancellationToken ct) =>
        await repository.SlugExistsAsync(slug, ct)
            ? Error.Conflict($"Tag with slug '{slug}' already exists")
            : true;

    private static Tag CreateTagFromDto(CreateTagDto dto) =>
        new()
        {
            Name = dto.Name,
            Slug = dto.Slug,
            Description = dto.Description
        };

    private static Tag ApplyUpdate(Tag tag, UpdateTagDto dto)
    {
        tag.Name = dto.Name ?? tag.Name;
        tag.Description = dto.Description ?? tag.Description;
        return tag;
    }

    private ActionResult<Tag> ToActionResult(Tag tag) => Ok(tag);

    private ActionResult<Tag> ToErrorResult(Error error) => error.Code switch
    {
        "NotFound" => NotFound(error.Message),
        "Conflict" => Conflict(error.Message),
        _ => BadRequest(error.Message)
    };
}
