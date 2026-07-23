using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Features.Courses.Commands.UpdateCourse;

// namespace TmsApi.Infrastructure.Services;
namespace TmsApi.Infrastructure.Persistence;



public class CourseService(
    TmsDbContext context,
    ILogger<CourseService> logger) : ICourseService
{
    public Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);

    public async Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        context.Courses.Add(course);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created course {CourseId} ({Code})",
            course.Id,
            course.Code);

        return (await GetByIdAsync(course.Id, ct))!;
    }

    public Task<bool> CodeExistsAsync(
        string code,
        CancellationToken ct) =>
        context.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Code == code, ct);

    // ===========================
    // NEW METHOD FOR EXERCISE 4
    // ===========================

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct)
    {
        IQueryable<Course> query = context.Courses.AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                EF.Functions.ILike(c.Title, $"%{request.Search}%") ||
                EF.Functions.ILike(c.Code, $"%{request.Search}%"));
        }

        // Count BEFORE paging
        var totalCount = await query.CountAsync(ct);

        // Sorting
        query = request.OrderBy switch
        {
            "Code" => request.Descending
                ? query.OrderByDescending(c => c.Code)
                : query.OrderBy(c => c.Code),

            "MaxCapacity" => request.Descending
                ? query.OrderByDescending(c => c.MaxCapacity)
                : query.OrderBy(c => c.MaxCapacity),

            _ => request.Descending
                ? query.OrderByDescending(c => c.Title)
                : query.OrderBy(c => c.Title)
        };

        // Paging + Projection
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto(
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                c.Enrollments.Count))
            .ToListAsync(ct);

        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    // Task ICourseService.GetCoursesAsync(PagedRequest request, CancellationToken ct)
    // {
    //     return GetCoursesAsync(request, ct);
    // }
public async Task<List<Course>> GetAllAsync(CancellationToken ct)
{
    return await context.Courses
        .Include(c => c.Enrollments)
        .ToListAsync(ct);
}

    public async Task<Course?> GetByCodeAsync(
    string courseCode,
    CancellationToken ct)
{
    return await context.Courses
        .Include(c => c.Enrollments)
        .FirstOrDefaultAsync(
            c => c.Code == courseCode,
            ct);
}

public async Task<bool> UpdateAsync(
    UpdateCourseCommand command,
    CancellationToken ct)
{
    var course = await context.Courses
        .FirstOrDefaultAsync(
            c => c.Id == command.Id,
            ct);

    if (course == null)
        return false;


    course.Code = command.Code;
    course.Title = command.Title;
    course.MaxCapacity = command.MaxCapacity;


    await context.SaveChangesAsync(ct);

    logger.LogInformation(
        "Updated course {CourseId}",
        course.Id);

    return true;
}

public async Task<bool> DeleteAsync(
    int id,
    CancellationToken ct)
{
    var course = await context.Courses
        .FirstOrDefaultAsync(
            c => c.Id == id,
            ct);


    if (course == null)
        return false;


    context.Courses.Remove(course);


    await context.SaveChangesAsync(ct);


    logger.LogInformation(
        "Deleted course {CourseId}",
        id);


    return true;
}

    public Task<IEnumerable<CourseResponseDto>> SearchAsync(string? term, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}