using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Features.Courses.Queries.GetCourses;

public record GetCoursesQuery
    : IRequest<List<CourseResponseDto>>;