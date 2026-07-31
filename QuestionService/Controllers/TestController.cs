using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace QuestionService.Controllers;

/// <summary>
/// Endpoints that exist purely so the client app can exercise its error handling and its
/// authentication flow. Nothing here belongs to the question domain.
/// </summary>
[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [Authorize]
    [HttpGet("auth")]
    public ActionResult TestAuth()
    {
        var name = User.FindFirstValue("name")
                   ?? User.FindFirstValue(ClaimTypes.Name)
                   ?? User.FindFirstValue("preferred_username");

        return Ok($"{name} has been authorized");
    }

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
}
