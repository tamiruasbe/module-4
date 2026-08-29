using MediatR;
using TmsApi.Application.DTOs;

namespace TmsApi.Application.Features.Instructors.Queries.GetInstructors;

public record GetInstructorsQuery
    : IRequest<List<InstructorResponseDto>>;