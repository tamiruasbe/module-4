using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Tms.Api.Authorization;

public class CourseInstructorHandler
    : AuthorizationHandler<CourseInstructorRequirement, Course>
{

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CourseInstructorRequirement requirement,
        Course resource)
    {
        // Get logged-in user's ID
        var userId = context.User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        // Check roles
        var isAdmin = context.User.IsInRole("Admin");
        var isInstructor = context.User.IsInRole("Instructor");
        // Admin can edit ANY course.
        if (isAdmin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        // Instructor can edit only their own course.
        if (isInstructor &&
            resource.InstructorId == userId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}