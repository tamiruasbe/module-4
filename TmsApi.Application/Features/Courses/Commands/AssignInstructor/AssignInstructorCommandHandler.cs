using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Features.Courses.Commands.AssignInstructor;

public class AssignInstructorCommandHandler(
    IInstructorService instructorService)
    : IRequestHandler<AssignInstructorCommand, bool>
{
    public async Task<bool> Handle(
        AssignInstructorCommand command,
        CancellationToken ct)
    {
        return await instructorService.AssignInstructorAsync(
            command.CourseId,
            command.InstructorId,
            ct);
    }
}