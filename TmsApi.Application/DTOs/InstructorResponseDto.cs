namespace TmsApi.Application.DTOs;

public record InstructorResponseDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string? Department
);