using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Enrollments.Queries;

public class GetEnrollmentsHandler(
    IEnrollmentService repo)
    : IRequestHandler<GetEnrollmentsQuery, List<EnrollmentDto>>
{
    public async Task<List<EnrollmentDto>> Handle(
        GetEnrollmentsQuery request,
        CancellationToken ct)
    {
        var enrollments = await repo.GetAllAsync(ct);

        return enrollments
            .Select(e => new EnrollmentDto(
                e.Id.ToString(),
                e.StudentId,
                e.Student.Name,
                e.CourseId,
                e.Course.Title,
                e.Status.ToString(),
                e.EnrolledAt))
            .ToList();
    }
}