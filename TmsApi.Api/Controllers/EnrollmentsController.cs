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

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
public class EnrollmentsController(
    IMediator mediator,
    IHubContext<TmsHub, ITmsHubClient> hubContext
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
}