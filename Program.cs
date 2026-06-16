using TmsApi;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Framework Services (Authentication/Authorization) ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "CustomPlaceholder";
    options.DefaultChallengeScheme = "CustomPlaceholder";
})
.AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, MinimalAuthHandler>("CustomPlaceholder", null);

builder.Services.AddAuthorization();


// --- 2. Your Application Services (MUST BE BEFORE builder.Build()) ---
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Strict Host Validation
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// --- 3. Configuration & Options Validation ---
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration(PaymentOptions.SectionName)
    .ValidateDataAnnotations() 
    .ValidateOnStart();

var app = builder.Build(); 


// --- 3. Middleware Pipeline (app.Use...) ---
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler("/error");
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// --- 4. Endpoints (app.Map...) ---
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
}))
.RequireAuthorization();

app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});

app.MapGet("/api/enrollments/test-logs", async (IEnrollmentService enrollmentService) =>
{
    // Trigger successful creation
    var enrollment = await enrollmentService.EnrollAsync("S-001", "CS-101");
    
    // Trigger the warning path by attempting exact same mutation again
    await enrollmentService.EnrollAsync("S-001", "CS-101");
    
    // Trigger missing resource warning lookup
    await enrollmentService.GetByIdAsync("nonexistent-id");
    
    // Trigger deletion metrics
    await enrollmentService.DeleteAsync(enrollment.Id);
    await enrollmentService.DeleteAsync("nonexistent-id");

    return Results.Ok("Log audit sequence executed.");
});

app.MapGet("/error", () => Results.Problem("An unexpected error occurred."));

app.Run();


public class MinimalAuthHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>
{
    public MinimalAuthHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
    {
        // Force an unauthenticated state to trigger a 401 Challenge cleanly!
        return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.NoResult());
    }
}