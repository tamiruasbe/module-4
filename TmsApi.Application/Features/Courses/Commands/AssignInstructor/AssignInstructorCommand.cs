using MediatR;

namespace TmsApi.Application.Features.Courses.Commands.AssignInstructor;

public record AssignInstructorCommand(
    int CourseId,
    string InstructorId
) : IRequest<bool>;