
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Application.DTOs;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController(
    ICachedCourseService cachedCourseService,
    ICourseService courseService,
    LinkGenerator linkGenerator) : ControllerBase
{
    // ════════════════════════════════════════
    // GET /api/courses
    // ════════════════════════════════════════
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription(
        "Returns a paginated, optionally filtered list of TMS courses. " +
        "PageSize is capped at 50. " +
        "Use search to filter by title or code. " +
        "Use orderBy (Title, Code, MaxCapacity) and descending to sort.")]
    public async Task<IActionResult> GetCourses(
    [FromQuery] PagedRequest request,
    CancellationToken ct)
{
    var result = await cachedCourseService.GetAllCoursesAsync(ct);

    return Ok(result);
}

    // ════════════════════════════════════════
    // GET /api/courses/{id}
    // ════════════════════════════════════════
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription(
        "Returns course details with HATEOAS links. " +
        "Returns 404 if the course does not exist. " +
        "The enroll link is only present when the course has remaining capacity.")]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);

        if (course is null)
            return NotFound();

        // Build hrefs using LinkGenerator
        // Never use string interpolation — routes can change
        var selfHref = linkGenerator.GetPathByName(
            HttpContext,
            nameof(GetCourseById),
            new { id });

        var enrollmentsHref = linkGenerator.GetPathByName(
            HttpContext,
            "ListCourseEnrollments",
            new { courseId = id });

        // Always present links
        var links = new List<LinkDto>
        {
            new(selfHref!,        "self",        "GET"),
            new(selfHref!,        "update",      "PUT"),
            new(selfHref!,        "delete",      "DELETE"),
            new(enrollmentsHref!, "enrollments", "GET"),
        };

        // Conditional link — only when course has capacity
        // Angular uses presence/absence to show/hide Enrol button
        if (course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(new(enrollmentsHref!, "enroll", "POST"));
        }

        var detailDto = new CourseDetailDto
        {
            Id              = course.Id,
            Code            = course.Code,
            Title           = course.Title,
            MaxCapacity     = course.MaxCapacity,
            EnrollmentCount = course.EnrollmentCount,
            Links           = links
        };

        return Ok(detailDto);
    }

    // ════════════════════════════════════════
    // POST /api/courses
    // ════════════════════════════════════════
    [HttpPost]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription(
        "Creates a course with a unique code. " +
        "Code must follow the pattern XXX-000 (e.g. CSE-101). " +
        "Returns 409 if the course code already exists. " +
        "Returns 400 if validation fails.")]
    public async Task<IActionResult> CreateCourse(
        
        CreateCourseRequest request,
        CancellationToken ct)
        
    {
        if (await courseService.CodeExistsAsync(request.Code, ct))
            return Conflict(new ProblemDetails
            {
                Title  = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });

        var result = await courseService.CreateAsync(request, ct);
        await cachedCourseService.InvalidateCourseCacheAsync(ct); 
        return CreatedAtAction(
            nameof(GetCourseById),
            new { id = result.Id },
            result);
    }

    // ════════════════════════════════════════
// GET /api/courses/search
// ════════════════════════════════════════
[HttpGet("search")]
[EnableRateLimiting("search")]
[EndpointSummary("Search courses")]
[EndpointDescription(
    "Searches courses by title or code using the search-only rate limit policy.")]
public async Task<IActionResult> SearchCourses(
    [FromQuery] string? term,
    CancellationToken ct)
{
    var results = await courseService.SearchAsync(term, ct);

    return Ok(results);
}
// ════════════════════════════════════════
// DELETE /api/courses/{id}
// ════════════════════════════════════════
[HttpDelete("{id:int}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public async Task<IActionResult> DeleteCourse(
    int id,
    CancellationToken ct)
{
    // Check whether the course exists
    var course = await courseService.GetByIdAsync(id, ct);

    if (course is null)
    {
        return NotFound(new ProblemDetails
        {
            Title = "Course not found",
            Detail = $"Course with ID {id} does not exist.",
            Status = StatusCodes.Status404NotFound
        });
    }

    // IMPORTANT:
    // If students are enrolled, reject deletion.
    if (course.EnrollmentCount > 0)
    {
        return Conflict(new ProblemDetails
        {
            Title = "Cannot delete course",
            Detail = "Cannot delete course because active student enrollments exist.",
            Status = StatusCodes.Status409Conflict
        });
    }

    var deleted = await courseService.DeleteAsync(id, ct);

    if (!deleted)
    {
        return NotFound(new ProblemDetails
        {
            Title = "Course not found",
            Detail = $"Course with ID {id} does not exist.",
            Status = StatusCodes.Status404NotFound
        });
    }

    await cachedCourseService.InvalidateCourseCacheAsync(ct);

    return NoContent();
}

}