using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Features.Courses.Commands.CreateCourse;

public record CreateCourseCommand(
    CreateCourseRequest Request
) : IRequest<CourseResponseDto>;