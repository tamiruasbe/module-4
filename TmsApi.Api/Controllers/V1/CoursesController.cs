using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Features.Courses.Commands.UpdateCourse;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("1.0")]
public class CoursesController : ControllerBase
{
    private readonly TmsDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediator _mediator;

    public CoursesController(
        TmsDbContext context,
        IAuthorizationService authorizationService,
        IMediator mediator)
    {
        _context = context;
        _authorizationService = authorizationService;
        _mediator = mediator;
    }

    // ============================================================
    // GET /api/v1/courses
    // Existing endpoint
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);

        pageSize = Math.Clamp(pageSize, 1, 50);

        var baseQuery = _context.Courses
            .AsNoTracking();

        var totalCount = await baseQuery
            .CountAsync(ct);

        var items = await baseQuery
            .OrderBy(c => c.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                EnrollmentCount = c.Enrollments.Count
            })
            .ToListAsync(ct);

        var totalPages =
            (int)Math.Ceiling(
                totalCount / (double)pageSize);

        return Ok(new
        {
            items,
            totalCount,
            page,
            pageSize,
            totalPages,
            hasNext = page < totalPages,
            hasPrevious = page > 1
        });
    }


    // ============================================================
    // PUT /api/v1/courses/{id}
    //
    // Module 11 Session 3
    // Exercise 5 - Resource-Based Authorization
    // ============================================================

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCourse(
        int id,
        [FromBody] UpdateCourseCommand command,
        CancellationToken ct)
    {
        // --------------------------------------------------------
        // Step 1: Find the existing course
        // --------------------------------------------------------

        var course = await _context.Courses
            .FirstOrDefaultAsync(
                c => c.Id == id,
                ct);

        if (course is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Course not found",
                Detail =
                    $"Course with ID {id} was not found.",
                Status =
                    StatusCodes.Status404NotFound
            });
        }


        // --------------------------------------------------------
        // Step 2: Resource-based authorization
        //
        // CourseInstructorHandler receives:
        //
        // User  -> currently logged-in user
        // course -> course being modified
        //
        // Admin:
        //     allowed
        //
        // Instructor:
        //     allowed only when
        //     course.InstructorId == User.Id
        // --------------------------------------------------------

        var authResult =
            await _authorizationService.AuthorizeAsync(
                User,
                course,
                "CanEditCourse");

        if (!authResult.Succeeded)
        {
            return Forbid();
        }


        // --------------------------------------------------------
        // Step 3: Make sure URL ID and command ID match
        // --------------------------------------------------------

        if (command.Id != id)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Course ID mismatch",
                Detail =
                    "The course ID in the URL must match " +
                    "the course ID in the request body.",
                Status =
                    StatusCodes.Status400BadRequest
            });
        }


        // --------------------------------------------------------
        // Step 4: Send command through MediatR
        //
        // UpdateCourseHandler will:
        //
        // 1. Call ICourseService.UpdateAsync()
        // 2. Invalidate course cache
        // --------------------------------------------------------

        var result =
            await _mediator.Send(
                command,
                ct);

        if (!result)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Course update failed",
                Detail = "The course could not be updated.",
                Status =
                    StatusCodes.Status400BadRequest
            });
        }


        // --------------------------------------------------------
        // Step 5: Successful update
        // --------------------------------------------------------

        return NoContent();
    }


    // ============================================================
    // DELETE /api/v1/courses/{id}
    //
    // Existing endpoint
    // ============================================================

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCourse(
        int id,
        CancellationToken ct)
    {
        var course = await _context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(
                c => c.Id == id,
                ct);

        // Course does not exist
        if (course is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Course not found",
                Detail =
                    $"Course with ID {id} was not found.",
                Status =
                    StatusCodes.Status404NotFound
            });
        }

        // Course has active enrollments
        if (course.Enrollments.Any())
        {
            return Conflict(new ProblemDetails
            {
                Title = "Cannot delete course",
                Detail =
                    "Cannot delete course because active " +
                    "student enrollments exist.",
                Status =
                    StatusCodes.Status409Conflict
            });
        }

        // No enrollments, so delete is allowed
        _context.Courses.Remove(course);

        await _context.SaveChangesAsync(ct);

        return NoContent();
    }
}