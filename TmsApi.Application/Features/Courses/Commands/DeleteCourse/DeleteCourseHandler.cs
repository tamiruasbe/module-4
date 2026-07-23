using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Features.Courses.Commands.DeleteCourse;

public class DeleteCourseHandler(
    ICourseService service,
    ICachedCourseService cachedService)
    : IRequestHandler<DeleteCourseCommand, bool>
{

    public async Task<bool> Handle(
        DeleteCourseCommand command,
        CancellationToken ct)
    {

        await service.DeleteAsync(
            command.Id,
            ct);


        // Lab Step 6:
        // invalidate cache after delete
        await cachedService
            .InvalidateCourseCacheAsync(ct);


        return true;
    }
}