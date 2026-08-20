using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using TmsApi.Infrastructure.Identity;


namespace TmsApi.Infrastructure.Persistence;public class TmsDbContext : IdentityDbContext<TmsUser>
{
    public TmsDbContext(
        DbContextOptions<TmsDbContext> options)
        : base(options)
    {
    }
public DbSet<Student> Students => Set<Student>();
public DbSet<Course> Courses => Set<Course>();
public DbSet<Enrollment> Enrollments => Set<Enrollment>();
public DbSet<Assessment> Assessments => Set<Assessment>();
public DbSet<Certificate> Certificates => Set<Certificate>();
public DbSet<RefreshToken> RefreshTokens { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // IMPORTANT: Configure ASP.NET Core Identity tables first
    base.OnModelCreating(modelBuilder);

    // Configure your existing entity configurations
    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(TmsDbContext).Assembly
    );

    // Existing global query filter
    modelBuilder.Entity<Enrollment>()
        .HasQueryFilter(e => !e.IsArchived);
}

public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
{
    foreach(var entry in ChangeTracker.Entries<Student>())
    {
        if(entry.State == EntityState.Modified ||
           entry.State == EntityState.Added)
        {
             entry.Property("LastUpdated")
            //      .CurrentValue = DateTime.UtcNow;
            
  
  .CurrentValue = DateTime.Now;
        }
    }


    return await base.SaveChangesAsync(cancellationToken);
}
}