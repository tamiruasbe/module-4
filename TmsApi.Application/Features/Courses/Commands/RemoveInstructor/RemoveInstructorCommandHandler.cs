using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Features.Courses.Commands.RemoveInstructor;

public class RemoveInstructorCommandHandler(
    IInstructorService instructorService)
    : IRequestHandler<RemoveInstructorCommand, bool>
{
    public async Task<bool> Handle(
        RemoveInstructorCommand command,
        CancellationToken ct)
    {
        return await instructorService.RemoveInstructorAsync(
            command.CourseId,
            ct);
    }
}