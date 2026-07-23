using MediatR;

namespace TmsApi.Application.Features.Courses.Commands.DeleteCourse;

public record DeleteCourseCommand(
    int Id
) : IRequest<bool>;