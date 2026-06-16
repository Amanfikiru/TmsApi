using Microsoft.Extensions.Logging;

namespace TmsApi;

//--- The contract --
public interface IEnrollmentService
{
    Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
    Task<EnrollmentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id); 
}

//--- The updated, safely logged in-memory implementation --
public class EnrollmentService : IEnrollmentService
{
    private readonly Dictionary<string, EnrollmentRecord> _store = new();
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(ILogger<EnrollmentService> logger)
    {
        _logger = logger;
    }

    public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
    {
        // 1. Check for duplicate enrollment
        var existing = _store.Values
            .FirstOrDefault(e => e.StudentId == studentId && e.CourseCode == courseCode);

        if (existing is not null)
        {
            _logger.LogWarning(
                "Duplicate enrollment attempt {StudentId} already in {CourseCode} (record {EnrollmentId})",
                studentId, courseCode, existing.Id);
                
            return Task.FromResult(existing);
        }

        // 2. Proceed with new enrollment if no duplicate exists
        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new EnrollmentRecord(id, studentId, courseCode, DateTime.UtcNow);
        _store[id] = record;
        
        _logger.LogInformation(
            "Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
            studentId, courseCode, id);
            
        return Task.FromResult(record);
    }

    public Task<EnrollmentRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var record);
        
        if (record is null)
        {
            _logger.LogWarning("Enrollment {EnrollmentId} not found", id);
        }
        
        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
    {
        IReadOnlyList<EnrollmentRecord> all = _store.Values.ToList();
        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);
        
        if (removed)
        {
            _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
        }
        else
        {
            _logger.LogWarning("Delete failed: enrollment {EnrollmentId} not found", id);
        }
        
        return Task.FromResult(removed);
    }
}

//--- 💡 THE MISSING PIECE: The data shape record type must live here! --
public record EnrollmentRecord(
    string Id,
    string StudentId,
    string CourseCode,
    DateTime EnrolledAt);