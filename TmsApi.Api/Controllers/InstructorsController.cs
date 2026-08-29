using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Features.Courses.Commands.AssignInstructor;
using TmsApi.Application.Features.Courses.Commands.RemoveInstructor;
using TmsApi.Application.Features.Instructors.Queries.GetInstructors;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/instructors")]
[ApiVersion("1.0")]
[Authorize(Roles = "Admin")]
public class InstructorsController(
    IMediator mediator)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetInstructors(
        CancellationToken ct)
    {
        var instructors = await mediator.Send(
            new GetInstructorsQuery(),
            ct);

        return Ok(instructors);
    }

    [HttpPut("courses/{courseId:int}")]
    public async Task<IActionResult> AssignInstructor(
        int courseId,
        [FromBody] AssignInstructorRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new AssignInstructorCommand(
                courseId,
                request.InstructorId),
            ct);

        if (!result)
        {
            return BadRequest(new
            {
                detail =
                    "Course or instructor not found, or selected user is not an Instructor."
            });
        }

        return Ok(new
        {
            message = "Instructor assigned successfully."
        });
    }

    [HttpDelete("courses/{courseId:int}")]
    public async Task<IActionResult> RemoveInstructor(
        int courseId,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new RemoveInstructorCommand(courseId),
            ct);

        if (!result)
        {
            return NotFound(new
            {
                detail = "Course not found."
            });
        }

        return Ok(new
        {
            message = "Instructor removed successfully."
        });
    }

    public record AssignInstructorRequest(
        string InstructorId);
}