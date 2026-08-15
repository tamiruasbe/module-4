// using Asp.Versioning;
// using MediatR;
// using Microsoft.AspNetCore.Mvc;
// using TmsApi.Application.Enrollments.Commands;
// using TmsApi.Application.Enrollments.Queries;

// namespace TmsApi.Api.Controllers;

// [ApiController]
// [Route("api/v{version:apiVersion}/enrollments")]
// [ApiVersion("2.0")]
// public class EnrollmentsController(IMediator mediator) : ControllerBase
// {

// [HttpGet]
// public async Task<IActionResult> GetAll(
//     CancellationToken ct)
// {
//     var result = await mediator.Send(
//         new GetEnrollmentsQuery(),
//         ct);

//     return Ok(result);
// }
//     [HttpPost]
//     public async Task<IActionResult> Enroll(
//         EnrollStudentCommand command,
//         CancellationToken ct)
//     {
//         var result = await mediator.Send(command, ct);

//         return result.Match<IActionResult>(
//             onSuccess: created => CreatedAtAction(
//                 nameof(GetSchedule),
//                 new { studentId = created.StudentId },
//                 created),

//             onFailure: error =>
//             {
//                 var status = error.Code switch
//                 {
//                     "course_not_found" =>
//                         StatusCodes.Status404NotFound,

//                     "course_full" or "already_enrolled" =>
//                         StatusCodes.Status409Conflict,

//                     _ =>
//                         StatusCodes.Status400BadRequest
//                 };

//                 return Problem(
//                     statusCode: status,
//                     title: "Enrollment rejected",
//                     detail: error.Message,
//                     type: $"https://tms.local/errors/{error.Code}");
//             });
//     }


//     [HttpGet("{studentId}/schedule")]
//     public async Task<IActionResult> GetSchedule(
//         int studentId,
//         CancellationToken ct)
//     {
//         var schedule = await mediator.Send(
//             new GetStudentScheduleQuery(studentId),
//             ct);

//         return Ok(schedule);
//     }
// }
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TmsApi.Api.Hubs;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;
using TmsApi.Application.Hubs;
using TmsApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
public class EnrollmentsController(
    IMediator mediator,
    IHubContext<TmsHub, ITmsHubClient> hubContext,
    TmsDbContext context
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetEnrollmentsQuery(),
            ct);

        return Ok(result);
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

//     [HttpPost("{id}/approve")]
// public async Task<IActionResult> Approve(
//     int id,
//     CancellationToken ct)
// {
//     var enrollment = await context.Enrollments
//         .FirstOrDefaultAsync(e => e.Id == id, ct);

//     if (enrollment is null)
//     {
//         return NotFound(new
//         {
//             message = $"Enrollment {id} was not found."
//         });
//     }

//     if (enrollment.Status == "Approved")
//     {
//         return Ok(new
//         {
//             id = enrollment.Id,
//             status = enrollment.Status,
//             message = "Enrollment is already approved."
//         });
//     }

//     enrollment.Status = "Approved";

//     await context.SaveChangesAsync(ct);

//     return Ok(new
//     {
//         id = enrollment.Id,
//         studentId = enrollment.StudentId,
//         courseId = enrollment.CourseId,
//         status = enrollment.Status
//     });
// }
[HttpPost("{id}/approve")]
public async Task<IActionResult> Approve(
    int id,
    CancellationToken ct)
{
    var enrollment = await context.Enrollments
        .FirstOrDefaultAsync(e => e.Id == id, ct);

    if (enrollment is null)
    {
        return NotFound(new
        {
            message = $"Enrollment {id} was not found."
        });
    }

    if (enrollment.Status == "Approved")
    {
        return Ok(new
        {
            id = enrollment.Id,
            status = enrollment.Status,
            message = "Enrollment is already approved."
        });
    }

    // 1. Update database
    enrollment.Status = "Approved";

    await context.SaveChangesAsync(ct);

    // 2. Send update to ALL connected Angular clients
    await hubContext.Clients.All.ReceiveEnrollmentStatusUpdated(
        enrollment.Id.ToString(),
        enrollment.Status
    );

    // 3. Return HTTP response
    return Ok(new
    {
        id = enrollment.Id,
        studentId = enrollment.StudentId,
        courseId = enrollment.CourseId,
        status = enrollment.Status
    });
}
}