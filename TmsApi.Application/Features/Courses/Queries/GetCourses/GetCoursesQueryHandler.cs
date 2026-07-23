using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Features.Courses.Queries.GetCourses;


public class GetCoursesQueryHandler
    : IRequestHandler<GetCoursesQuery, List<CourseResponseDto>>
{

    private readonly ICachedCourseService _cachedCourseService;


    public GetCoursesQueryHandler(
        ICachedCourseService cachedCourseService)
    {
        _cachedCourseService = cachedCourseService;
    }



    public async Task<List<CourseResponseDto>> Handle(
        GetCoursesQuery request,
        CancellationToken ct)
    {

        return await _cachedCourseService
            .GetAllCoursesAsync(ct);

    }
}