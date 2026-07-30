using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
public class EnrollmentsController(IMediator mediator) : ControllerBase
{
     [HttpGet]
    public IActionResult GetAll()
    {
        var enrollments = new List<object>
        {
            new
            {
                id = "1",
                studentId = 1001,
                studentName = "Liya Kebede",
                courseId = 1,
                courseName = "Advanced Java Services",
                status = "Pending",
                enrolledAt = DateTime.UtcNow
            },
            new
            {
                id = "2",
                studentId = 1002,
                studentName = "Abel Bekele",
                courseId = 2,
                courseName = "Cloud Computing",
                status = "Approved",
                enrolledAt = DateTime.UtcNow
            }
        };

        return Ok(enrollments);
    }

    [HttpPost("{id}/approve")]
    public IActionResult Approve(string id)
    {
        Console.WriteLine($"Enrollment {id} approved.");
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Enroll(
        EnrollStudentCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        return result.Match<IActionResult>(
            onSuccess: created => CreatedAtAction(
                nameof(GetSchedule),
                new { studentId = created.StudentId },
                created),

            onFailure: error =>
            {
                var status = error.Code switch
                {
                    "course_not_found" =>
                        StatusCodes.Status404NotFound,

                    "course_full" or "already_enrolled" =>
                        StatusCodes.Status409Conflict,

                    _ =>
                        StatusCodes.Status400BadRequest
                };

                return Problem(
                    statusCode: status,
                    title: "Enrollment rejected",
                    detail: error.Message,
                    type: $"https://tms.local/errors/{error.Code}");
            });
    }


    [HttpGet("{studentId}/schedule")]
    public async Task<IActionResult> GetSchedule(
        int studentId,
        CancellationToken ct)
    {
        var schedule = await mediator.Send(
            new GetStudentScheduleQuery(studentId),
            ct);

        return Ok(schedule);
    }
}