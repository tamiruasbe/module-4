using MediatR;

namespace TmsApi.Application.Features.Courses.Commands.RemoveInstructor;

public record RemoveInstructorCommand(
    int CourseId
) : IRequest<bool>;