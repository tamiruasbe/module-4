
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Routing;
// using TmsApi.Dtos;
// using TmsApi.Services;

// namespace TmsApi.Controllers;


// [ApiController]
// [Route("api/courses")]
// public class CoursesController(
//     ICourseService courseService,
//     LinkGenerator linkGenerator
// ) : ControllerBase
// {
// [HttpGet("{id:int}", Name = nameof(GetCourseById))]
// public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
// {
//     // Get the course from service
//     var course = await courseService.GetByIdAsync(id, ct);

//     if (course is null)
//         return NotFound();

//     // ─────────────────────────────────────────────
//     // TODO 1: Build hrefs using LinkGenerator
//     // GetPathByName → uses route Name = nameof(...)
//     // Never use string interpolation here
//     // ─────────────────────────────────────────────
//     var selfHref = linkGenerator.GetPathByName(
//         HttpContext,
//         nameof(GetCourseById),
//         new { id });                    // matches {id:int} in route template

//     var enrollmentsHref = linkGenerator.GetPathByName(
//         HttpContext,
//         "ListCourseEnrollments",        // Name we set in Step 6
//         new { courseId = id });         // matches {courseId:int} in route

//     // ─────────────────────────────────────────────
//     // TODO 2: Build the Links list
//     // Always present: self, update, delete, enrollments
//     // Conditional: enroll — only if course has capacity
//     // ─────────────────────────────────────────────
//     var links = new List<LinkDto>
//     {
//         new(selfHref!,        "self",        "GET"),
//         new(selfHref!,        "update",      "PUT"),
//         new(selfHref!,        "delete",      "DELETE"),
//         new(enrollmentsHref!, "enrollments", "GET"),
//     };

//     // Conditional link — the key HATEOAS benefit
//     // Angular renders "Enrol" button ONLY when this link exists
//     // No capacity logic needed in Angular TypeScript
//     if (course.EnrollmentCount < course.MaxCapacity)
//     {
//         links.Add(new(enrollmentsHref!, "enroll", "POST"));
//     }

//     // ─────────────────────────────────────────────
//     // TODO 3: Build CourseDetailDto and return Ok
//     // ─────────────────────────────────────────────
//     var detailDto = new CourseDetailDto
//     {
//         Id              = course.Id,
//         Code            = course.Code,
//         Title           = course.Title,
//         MaxCapacity     = course.MaxCapacity,
//         EnrollmentCount = course.EnrollmentCount,
//         Links           = links
//     };

//     return Ok(detailDto);
// }

// [HttpGet]
// public async Task<IActionResult> GetCourses(
//     [FromQuery] PagedRequest request,
//     CancellationToken ct)
// {
//     var result = await courseService.GetCoursesAsync(request, ct);

//     return Ok(result);
// }
// // [HttpPost]
// // public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
// // {
// // var result = await courseService.CreateAsync(request, ct);
// // return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
// // }



// [HttpPost]
//     public async Task<IActionResult> CreateCourse(
//         CreateCourseRequest request,
//         CancellationToken ct)
//     {
//         // Check BEFORE hitting the database
//         // Prevents 500 crash from unique index violation
//         if (await courseService.CodeExistsAsync(request.Code, ct))
//             return Conflict(new ProblemDetails
//             {
//                 Title  = "Course code already exists",
//                 Detail = $"A course with code '{request.Code}' is already registered.",
//                 Status = StatusCodes.Status409Conflict
//             });

//         var result = await courseService.CreateAsync(request, ct);

//         return CreatedAtAction(
//             nameof(GetCourseById),
//             new { id = result.Id },
//             result);
//     }

    
// }



using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController(
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
        var result = await courseService.GetCoursesAsync(request, ct);
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

        return CreatedAtAction(
            nameof(GetCourseById),
            new { id = result.Id },
            result);
    }
}