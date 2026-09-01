using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;
namespace TmsApi.Infrastructure.Persistence;
public static class DataSeeder
{
    private static readonly (string Code, string Title, int MaxCapacity)[] Courses =
    [
        ("CSE-101", "Web Development Fundamentals",       30),
        ("CSE-102", "TypeScript Essentials",              30),
        ("CSE-103", "Git and Collaborative Workflows",    25),
        ("CSE-201", "ASP.NET Core Fundamentals",          28),
        ("CSE-202", "Entity Framework Core and PostgreSQL", 28),
        ("CSE-203", "Building RESTful Web APIs",          28),
        ("CSE-301", "Advanced Web API Patterns",          24),
        ("CSE-302", "Angular Fundamentals",               26),
        ("CSE-303", "Angular Advanced",                   24),
        ("CSE-304", "Full-Stack Integration",             22),
        ("CSE-305", "Testing and Quality Assurance",      22),
        ("CSE-306", "Security and Authentication",        20),
        ("DAT-101", "Database Design Foundations",        30),
        ("DAT-201", "Advanced SQL and Indexing",          26),
        ("DAT-202", "Data Modelling for the Web",         26),
        ("ARC-101", "Software Architecture Patterns",     22),
        ("ARC-201", "Cloud-Native Architecture",          22),
        ("DEV-101", "DevOps Foundations",                 24),
        ("DEV-201", "Continuous Delivery Pipelines",      22),
        ("MOB-101", "Mobile App Foundations",             24),
        ("MOB-201", "Cross-Platform Mobile",              22),
        ("AI-101",  "Applied Machine Learning",           20),
        ("AI-201",  "Generative AI for Developers",       18),
        ("UX-101",  "UX Research and Wireframing",        24),
        ("UX-201",  "Design Systems and Tokens",          22),
    ];

    public static async Task SeedAsync(
        TmsDbContext context,
        CancellationToken ct = default)
    {
        // Habit 1: MigrateAsync first
        // await context.Database.MigrateAsync(ct);
        if (context.Database.IsRelational())
    {
        await context.Database.MigrateAsync(ct);
    }

        // Habit 2: Idempotency guard
        if (await context.Courses.AnyAsync(ct))
            return;

        // Habit 3: Deterministic data
        foreach (var (code, title, maxCapacity) in Courses)
        {
            context.Courses.Add(new Course
            {
                Code        = code,
                Title       = title,
                MaxCapacity = maxCapacity
            });
        }

        await context.SaveChangesAsync(ct);
        if (!await context.Enrollments.AnyAsync(ct))
{
    var courses = await context.Courses
        .OrderBy(c => c.Id)
        .ToListAsync(ct);

    context.Enrollments.AddRange(
        new Enrollment { StudentId = 1,  CourseId = courses[0].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 2,  CourseId = courses[1].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 3,  CourseId = courses[2].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 4,  CourseId = courses[3].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 5,  CourseId = courses[4].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 6,  CourseId = courses[5].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 7,  CourseId = courses[6].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 8,  CourseId = courses[7].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 9,  CourseId = courses[8].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 10, CourseId = courses[9].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },

        new Enrollment { StudentId = 11, CourseId = courses[10].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 12, CourseId = courses[11].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 13, CourseId = courses[12].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 14, CourseId = courses[13].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 15, CourseId = courses[14].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 16, CourseId = courses[15].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 17, CourseId = courses[16].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 18, CourseId = courses[17].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 19, CourseId = courses[18].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 20, CourseId = courses[19].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },

        new Enrollment { StudentId = 21, CourseId = courses[20].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 22, CourseId = courses[21].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 23, CourseId = courses[22].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 24, CourseId = courses[23].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 25, CourseId = courses[24].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },

        new Enrollment { StudentId = 26, CourseId = courses[0].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 27, CourseId = courses[1].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 28, CourseId = courses[2].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 29, CourseId = courses[3].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 30, CourseId = courses[4].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },

        new Enrollment { StudentId = 31, CourseId = courses[5].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 32, CourseId = courses[6].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 33, CourseId = courses[7].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 34, CourseId = courses[8].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 35, CourseId = courses[9].Id,  Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 36, CourseId = courses[10].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 37, CourseId = courses[11].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 38, CourseId = courses[12].Id, Year = 2026, EnrolledAt = DateTime.UtcNow },
        new Enrollment { StudentId = 39, CourseId = courses[12].Id, Year = 2026, EnrolledAt = DateTime.UtcNow }

    );

    await context.SaveChangesAsync(ct);
}
    }
    
}
