using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/grades")]
public class GradesController : ControllerBase
{
    private readonly TmsDbContext _context;

    public GradesController(TmsDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> PostGrade(
        [FromBody] GradeRequest request,
        CancellationToken ct)
    {
        // 1. Validate score
        if (request.Score < 0 || request.Score > 100)
        {
            return BadRequest(new
            {
                message = "Score must be between 0 and 100."
            });
        }

        // 2. Find the enrollment
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(
                e =>
                    e.StudentId == request.StudentId &&
                    e.CourseId == request.CourseId &&
                    !e.IsArchived,
                ct);

        // 3. Enrollment does not exist
        if (enrollment == null)
        {
            return NotFound(new
            {
                message = "The student is not enrolled in this course."
            });
        }

        // 4. Save the grade
        enrollment.Grade = request.Score;

        // 5. Save changes to PostgreSQL
        await _context.SaveChangesAsync(ct);

        // 6. Return success
        return Ok(new
        {
            id = enrollment.Id.ToString(),
            success = true
        });
    }
}

public record GradeRequest(
    int StudentId,
    int CourseId,
    decimal Score);