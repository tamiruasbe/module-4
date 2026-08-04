using MediatR;

namespace TmsApi.Application.Enrollments.Queries;

public record GetEnrollmentsQuery()
    : IRequest<List<EnrollmentDto>>;

public record EnrollmentDto(
    string Id,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseName,
    string Status,
    DateTime EnrolledAt);