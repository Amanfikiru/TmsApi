var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "CustomPlaceholder";
    options.DefaultChallengeScheme = "CustomPlaceholder";
})
.AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, MinimalAuthHandler>("CustomPlaceholder", null);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<TmsApi.RequestLoggingMiddleware>();
app.UseExceptionHandler("/error"); 

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
}))
.RequireAuthorization();

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