

using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;

using TmsApi.Application.Interfaces;
namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(
    ICourseService courseService,
    IEnrollmentService enrollmentService) : ControllerBase
{
    // ════════════════════════════════════════
    // GET /api/courses/5/enrollments
    // ════════════════════════════════════════
    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List enrolments for a course")]
    [EndpointDescription(
        "Returns all enrollments for the specified course. " +
        "Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetEnrollments(
        int courseId,
        CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);

        if (course is null)
            return NotFound();

        var enrollments = await enrollmentService
            .GetByCourseAsync(courseId, ct);

        return Ok(enrollments);
    }

    // ════════════════════════════════════════
    // GET /api/courses/5/enrollments/10
    // ════════════════════════════════════════
    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one enrolment for a course")]
    [EndpointDescription(
        "Returns a single enrollment by its ID within the specified course. " +
        "Returns 404 if either the course or the enrollment does not exist.")]
    public async Task<IActionResult> GetEnrollment(
        int courseId,
        int id,
        CancellationToken ct)
    {
        var enrollment = await enrollmentService
            .GetByIdAsync(courseId, id, ct);

        return enrollment is not null
            ? Ok(enrollment)
            : NotFound();
    }

    // ════════════════════════════════════════
    // POST /api/courses/5/enrollments
    // ════════════════════════════════════════
    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Enrol a student in a course")]
    [EndpointDescription(
        "Returns 404 if the course does not exist. " +
        "Returns 409 if the course has reached MaxCapacity. " +
        "The enroll HATEOAS link on GET /api/courses/{id} is only " +
        "present when enrollment is still possible.")]
    public async Task<IActionResult> EnrollStudent(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        // Gate 1: Course must exist → 404
        var course = await courseService.GetByIdAsync(courseId, ct);

        if (course == null)
            return NotFound();

        // Gate 2: Course must not be full → 409
        if (course.EnrollmentCount >= course.MaxCapacity)
            return Conflict(new ProblemDetails
            {
                Title  = "Course is full",
                Detail = $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
                Status = StatusCodes.Status409Conflict
            });

        // All gates passed → create enrollment
        var enrollment = await enrollmentService
            .CreateAsync(courseId, request, ct);

        return CreatedAtAction(
            nameof(GetEnrollment),
            new { courseId, id = enrollment.Id },
            enrollment);
    }
}