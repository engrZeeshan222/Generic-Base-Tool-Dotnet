using GenericToolKit.Domain.Interfaces;

namespace Patient.Infra.Repositories;

/// <summary>
/// Custom Patient repository interface extending the generic repository
/// Demonstrates how to add domain-specific methods beyond the generic CRUD operations
/// </summary>
public interface IPatientRepository : IGenericRepository<Domain.Entities.Patient>
{
    /// <summary>
    /// Find patient by MRN (Medical Record Number)
    /// </summary>
    Task<Domain.Entities.Patient?> FindByMRNAsync(string mrn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find patient by email
    /// </summary>
    Task<Domain.Entities.Patient?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find patient by patient code
    /// </summary>
    Task<Domain.Entities.Patient?> FindByPatientCodeAsync(string patientCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get patients with upcoming appointments
    /// </summary>
    Task<List<Domain.Entities.Patient>> GetPatientsWithUpcomingAppointmentsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if MRN exists in the tenant
    /// </summary>
    Task<bool> IsMRNUniqueInTenantAsync(string mrn, int tenantId, int? excludePatientId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get patients by age range
    /// </summary>
    Task<List<Domain.Entities.Patient>> GetPatientsByAgeRangeAsync(int minAge, int maxAge, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search patients by name (first or last name)
    /// </summary>
    Task<List<Domain.Entities.Patient>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
}
