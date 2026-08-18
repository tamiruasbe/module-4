using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Controllers.V1;


[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("1.0")]
public class CoursesController(TmsDbContext context) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);

        pageSize = Math.Clamp(pageSize, 1, 50);


        var baseQuery = context.Courses
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
            (int)Math.Ceiling(totalCount / (double)pageSize);


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

    [HttpDelete("{id:int}")]
public async Task<IActionResult> DeleteCourse(
    int id,
    CancellationToken ct)
{
    var course = await context.Courses
        .Include(c => c.Enrollments)
        .FirstOrDefaultAsync(c => c.Id == id, ct);

    // Course does not exist
    if (course is null)
    {
        return NotFound(new ProblemDetails
        {
            Title = "Course not found",
            Detail = $"Course with ID {id} was not found.",
            Status = StatusCodes.Status404NotFound
        });
    }

    // Course has active enrollments
    if (course.Enrollments.Any())
    {
        return Conflict(new ProblemDetails
        {
            Title = "Cannot delete course",
            Detail = "Cannot delete course because active student enrollments exist.",
            Status = StatusCodes.Status409Conflict
        });
    }

    // No enrollments, so delete is allowed
    context.Courses.Remove(course);

    await context.SaveChangesAsync(ct);

    return NoContent();
}
}