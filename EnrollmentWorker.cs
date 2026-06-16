using Microsoft.Extensions.DependencyInjection;

namespace TmsApi;

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    // We inject the factory, NOT the scoped service directly
    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ProcessBatch()
    {
        // TODO2: Create a short-lived scope using the injected factory
        using var scope = _scopeFactory.CreateScope();

        // TODO3: Resolve the scoped service from the new scope's ServiceProvider
        var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

        // TODO4: Use the service safely inside this thread-safe, isolated scope
        // (Simulating work here; for example retrieving data to run scholarship calculations)
        var allEnrollments = enrollmentService.GetAllAsync().GetAwaiter().GetResult();
    }
}