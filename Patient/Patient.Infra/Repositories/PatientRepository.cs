using GenericToolKit.Domain.Interfaces;
using GenericToolKit.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Patient.Infra.Data;

namespace Patient.Infra.Repositories;

/// <summary>
/// Custom Patient repository implementation
/// Inherits from GenericRepository to get all standard CRUD operations
/// Adds domain-specific query methods
/// </summary>
public class PatientRepository : GenericRepository<Domain.Entities.Patient>, IPatientRepository
{
    private readonly PatientDbContext _context;

    public PatientRepository(PatientDbContext context, ILoggedInUser loggedInUser)
        : base(context, loggedInUser)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Patient?> FindByMRNAsync(string mrn, CancellationToken cancellationToken = default)
    {
        // Using the toolkit's FindOne method with expression
        return await FindOne(p => p.MRN == mrn, null);
    }

    public async Task<Domain.Entities.Patient?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await FindOne(p => p.Email == email, null);
    }

    public async Task<Domain.Entities.Patient?> FindByPatientCodeAsync(string patientCode, CancellationToken cancellationToken = default)
    {
        return await FindOne(p => p.PatientCode == patientCode, null);
    }

    public async Task<List<Domain.Entities.Patient>> GetPatientsWithUpcomingAppointmentsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        // Demonstrates eager loading with Include
        return await _context.Patients
            .Include(p => p.Appointments.Where(a => a.AppointmentDateTime >= fromDate && a.AppointmentDateTime <= toDate))
            .Where(p => p.Appointments.Any(a => a.AppointmentDateTime >= fromDate && a.AppointmentDateTime <= toDate))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsMRNUniqueInTenantAsync(string mrn, int tenantId, int? excludePatientId = null, CancellationToken cancellationToken = default)
    {
        // Using the toolkit's Any method
        if (excludePatientId.HasValue)
        {
            return !await Any(p => p.MRN == mrn && p.TenantId == tenantId && p.Id != excludePatientId.Value, cancellationToken);
        }

        return !await Any(p => p.MRN == mrn && p.TenantId == tenantId, cancellationToken);
    }

    public async Task<List<Domain.Entities.Patient>> GetPatientsByAgeRangeAsync(int minAge, int maxAge, CancellationToken cancellationToken = default)
    {
        var maxBirthDate = DateTime.Today.AddYears(-minAge);
        var minBirthDate = DateTime.Today.AddYears(-maxAge - 1);

        // Using the toolkit's Find method which returns IQueryable
        var query = Find(p => p.DateOfBirth >= minBirthDate && p.DateOfBirth <= maxBirthDate);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<Domain.Entities.Patient>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        // Case-insensitive search in first name or last name
        var lowerSearchTerm = searchTerm.ToLower();

        var query = Find(p =>
            p.FirstName.ToLower().Contains(lowerSearchTerm) ||
            p.LastName.ToLower().Contains(lowerSearchTerm));

        return await query
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync(cancellationToken);
    }
}
