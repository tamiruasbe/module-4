using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;

public interface IInstructorService
{
    Task<List<InstructorResponseDto>> GetInstructorsAsync(
        CancellationToken ct);

    Task<bool> AssignInstructorAsync(
        int courseId,
        string instructorId,
        CancellationToken ct);

    Task<bool> RemoveInstructorAsync(
        int courseId,
        CancellationToken ct);
}