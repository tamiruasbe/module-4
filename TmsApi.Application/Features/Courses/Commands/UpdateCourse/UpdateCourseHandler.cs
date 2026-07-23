using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Features.Courses.Commands.UpdateCourse;

public class UpdateCourseHandler(
    ICourseService service,
    ICachedCourseService cachedService)
    : IRequestHandler<UpdateCourseCommand, bool>
{

    public async Task<bool> Handle(
        UpdateCourseCommand command,
        CancellationToken ct)
    {

        await service.UpdateAsync(
            command,
            ct);


        // Lab Step 6:
        // remove stale cache
        await cachedService
            .InvalidateCourseCacheAsync(ct);


        return true;
    }
}