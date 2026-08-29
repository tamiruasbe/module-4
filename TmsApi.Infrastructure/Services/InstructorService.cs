using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Identity;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class InstructorService(
    UserManager<TmsUser> userManager,
    TmsDbContext context)
    : IInstructorService
{
    public async Task<List<InstructorResponseDto>> GetInstructorsAsync(
        CancellationToken ct)
    {
        var users = await userManager.Users
            .ToListAsync(ct);

        var instructors = new List<InstructorResponseDto>();

        foreach (var user in users)
        {
            if (await userManager.IsInRoleAsync(user, "Instructor"))
            {
                instructors.Add(
                    new InstructorResponseDto(
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        user.Email ?? string.Empty,
                        user.Department
                    )
                );
            }
        }

        return instructors;
    }

    public async Task<bool> AssignInstructorAsync(
        int courseId,
        string instructorId,
        CancellationToken ct)
    {
        var course = await context.Courses
            .FirstOrDefaultAsync(
                c => c.Id == courseId,
                ct);

        if (course == null)
            return false;

        var instructor = await userManager.FindByIdAsync(instructorId);

        if (instructor == null)
            return false;

        var isInstructor =
            await userManager.IsInRoleAsync(
                instructor,
                "Instructor");

        if (!isInstructor)
            return false;

        course.InstructorId = instructorId;

        await context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> RemoveInstructorAsync(
        int courseId,
        CancellationToken ct)
    {
        var course = await context.Courses
            .FirstOrDefaultAsync(
                c => c.Id == courseId,
                ct);

        if (course == null)
            return false;

        course.InstructorId = null;

        await context.SaveChangesAsync(ct);

        return true;
    }
}