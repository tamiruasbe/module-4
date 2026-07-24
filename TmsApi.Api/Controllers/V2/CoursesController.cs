using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.DTOs;
namespace TmsApi.Controllers.V2;


[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(
    ICachedCourseService cachedCourseService,
    TmsDbContext context) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {

        page = Math.Max(1, page);

        pageSize = Math.Clamp(pageSize, 1, 50);



        var rows = await cachedCourseService
            .GetAllCoursesAsync(ct);



        var totalCount = rows.Count;



        var data = rows
            .OrderBy(c => c.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();



        var totalPages =
            (int)Math.Ceiling(
                totalCount / (double)pageSize);



        var hasNext = page < totalPages;

        var hasPrevious = page > 1;



        return Ok(new
        {
            data,

            meta = new
            {
                totalCount,
                page,
                pageSize,
                totalPages,
                hasNext,
                hasPrevious
            },


            links = new
            {
                self = $"/api/v2/courses?page={page}&pageSize={pageSize}",


                next = hasNext
                    ? $"/api/v2/courses?page={page + 1}&pageSize={pageSize}"
                    : (string?)null,


                prev = hasPrevious
                    ? $"/api/v2/courses?page={page - 1}&pageSize={pageSize}"
                    : (string?)null,


                enroll = "/api/v2/enrollments"
            }
        });
    }

    [HttpPut("{id:int}")]
public async Task<IActionResult> UpdateCourse(
    int id,
    [FromBody] UpdateCourseRequest request,
    CancellationToken ct)
{
    var course = await context.Courses
        .FirstOrDefaultAsync(c => c.Id == id, ct);

    if (course is null)
        return NotFound();

    course.Title = request.Title;

    await context.SaveChangesAsync(ct);

    // IMPORTANT: invalidate the cache after the write succeeds
    await cachedCourseService.InvalidateCourseCacheAsync(ct);

    return Ok(new
    {
        message = "Course updated",
        id = course.Id,
        title = course.Title
    });
}
}