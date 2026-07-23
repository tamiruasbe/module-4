using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Features.Courses.Commands.CreateCourse;

public class CreateCourseHandler(
    ICourseService service,
    ICachedCourseService cachedService)
    : IRequestHandler<CreateCourseCommand, CourseResponseDto>
{

    public async Task<CourseResponseDto> Handle(
        CreateCourseCommand command,
        CancellationToken ct)
    {

        var result =
            await service.CreateAsync(
                command.Request,
                ct);


        // Lab Step 6:
        // invalidate cache after write
        await cachedService
            .InvalidateCourseCacheAsync(ct);


        return result;
    }
}