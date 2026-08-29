using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Features.Instructors.Queries.GetInstructors;

public class GetInstructorsQueryHandler(
    IInstructorService instructorService)
    : IRequestHandler<GetInstructorsQuery, List<InstructorResponseDto>>
{
    public async Task<List<InstructorResponseDto>> Handle(
        GetInstructorsQuery request,
        CancellationToken ct)
    {
        return await instructorService.GetInstructorsAsync(ct);
    }
}