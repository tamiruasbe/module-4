
namespace TmsApi.Api.BackgroundServices;


using System;
using Microsoft.Extensions.DependencyInjection;
using TmsApi.Application.Interfaces;

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    // Injecting the Factory (which is a Singleton) instead of the Scoped service directly
    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ProcessBatch()
    {
        // Dynamically create a short-lived scope
        using (var scope = _scopeFactory.CreateScope())
        {
            // Resolve the Scoped service safely inside this block
            var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
            
            Console.WriteLine("Processing background batch using a safe scope...");
        } // The scope ends here, cleaning up resources properly
    }
}

