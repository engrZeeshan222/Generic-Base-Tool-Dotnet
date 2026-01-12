using GenericToolKit.Application.Services;
using Patient.Application.DTOs;
using System.Linq.Expressions;

namespace Patient.Application.Services;

/// <summary>
/// Custom Patient service interface extending generic service
/// Adds domain-specific business operations
/// </summary>
public interface IPatientService : IGenericService<Domain.Entities.Patient>
{
    /// <summary>
    /// Create a new patient with validation and business logic
    /// </summary>
    Task<PatientDto> CreatePatientAsync(CreatePatientRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing patient
    /// </summary>
    Task<PatientDto> UpdatePatientAsync(UpdatePatientRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get patient by ID with mapping to DTO
    /// </summary>
    Task<PatientDto?> GetPatientByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active patients
    /// </summary>
    Task<List<PatientDto>> GetActivePatientsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Search patients by name
    /// </summary>
    Task<List<PatientDto>> SearchPatientsAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate patient
    /// </summary>
    Task<bool> ActivatePatientAsync(int patientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate patient
    /// </summary>
    Task<bool> DeactivatePatientAsync(int patientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get patient change history (demonstrates change tracking)
    /// </summary>
    Task<string> GetPatientChangeHistoryAsync(int patientId, CancellationToken cancellationToken = default);

    // ============================================================================
    // COMPREHENSIVE METHODS TO TEST ALL GENERIC SERVICE FEATURES
    // ============================================================================

    /// <summary>
    /// Create multiple patients in bulk (tests AddMany)
    /// </summary>
    Task<List<PatientDto>> CreatePatientsBulkAsync(List<CreatePatientRequest> requests, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save or update patient (upsert operation - tests SaveOrUpdate)
    /// </summary>
    Task<PatientDto> SaveOrUpdatePatientAsync(CreatePatientRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get patients by list of IDs (tests ListAsync)
    /// </summary>
    Task<List<PatientDto>> GetPatientsByIdsAsync(List<int> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Count patients matching criteria (tests Count)
    /// </summary>
    Task<int> CountPatientsAsync(Expression<Func<Domain.Entities.Patient, bool>>? predicate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete multiple patients (tests SoftDeleteMany)
    /// </summary>
    Task<bool> SoftDeletePatientsAsync(List<int> patientIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard delete multiple patients by predicate (tests HardDeleteMany)
    /// </summary>
    Task<int> HardDeletePatientsByConditionAsync(Expression<Func<Domain.Entities.Patient, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard delete single patient entity (tests HardDeleteOne)
    /// </summary>
    Task<int> HardDeletePatientEntityAsync(int patientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove list of patient entities (tests RemoveListOfEntities)
    /// </summary>
    Task<bool> RemovePatientsListAsync(List<int> patientIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get full JSON comparison for patient (tests LogFullJsonComparison)
    /// </summary>
    Task<string> GetPatientFullJsonComparisonAsync(int patientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually set audit properties (tests SetAuditPropertiesAsync)
    /// </summary>
    Task<PatientDto> SetPatientAuditPropertiesAsync(int patientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get patients with advanced BaseFilters (tests all BaseFilters properties)
    /// </summary>
    Task<List<PatientDto>> GetPatientsWithAdvancedFiltersAsync(
        int? createdBy = null,
        int? updatedBy = null,
        int? deleteBy = null,
        bool ignoreTenantCheck = false,
        string? sortBy = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        bool includeSoftDeleted = false,
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default);
}
