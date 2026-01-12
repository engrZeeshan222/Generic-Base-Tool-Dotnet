using GenericToolKit.Application.Services;
using GenericToolKit.Domain.Interfaces;
using GenericToolKit.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Patient.Application.DTOs;
using Patient.Application.Mapping;

namespace Patient.Application.Services;

/// <summary>
/// Patient service implementation
/// Inherits from GenericService to get all standard operations
/// Demonstrates business logic and all toolkit features
/// </summary>
public class PatientService : GenericService<Domain.Entities.Patient>, IPatientService
{
    private readonly IGenericRepository<Domain.Entities.Patient> _repository;
    private readonly ILoggedInUser _loggedInUser;

    public PatientService(
        IGenericRepository<Domain.Entities.Patient> repository,
        ILoggedInUser loggedInUser)
        : base(repository, loggedInUser)
    {
        _repository = repository;
        _loggedInUser = loggedInUser;
    }

    public async Task<PatientDto> CreatePatientAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        // Validate MRN uniqueness using toolkit's Any method
        var mrnExists = await Any(p => p.MRN == request.MRN && p.TenantId == _loggedInUser.TenantId, cancellationToken);

        if (mrnExists)
        {
            throw new InvalidOperationException($"MRN '{request.MRN}' already exists in this facility.");
        }

        // Map request to entity
        var patient = PatientMapper.MapToEntity(request);

        // Validate domain rules
        if (!patient.IsValid())
        {
            throw new InvalidOperationException("Patient data is invalid.");
        }

        // Add using toolkit's Add method (will set audit properties automatically)
        var createdPatient = await Add(patient);

        // Map to DTO
        return PatientMapper.MapToDto(createdPatient);
    }

    public async Task<PatientDto> UpdatePatientAsync(UpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        // Get existing patient
        var existingPatient = await GetByIdQuery(request.Id, detached: false).SingleOrDefaultAsync(cancellationToken);

        if (existingPatient == null)
        {
            throw new InvalidOperationException($"Patient with ID {request.Id} not found.");
        }

        // Update properties
        existingPatient.FirstName = request.FirstName;
        existingPatient.LastName = request.LastName;
        existingPatient.DateOfBirth = request.DateOfBirth;
        existingPatient.Gender = request.Gender;
        existingPatient.Phone = request.Phone;
        existingPatient.Email = request.Email;
        existingPatient.BloodType = request.BloodType;
        existingPatient.Allergies = request.Allergies;
        existingPatient.MedicalNotes = request.MedicalNotes;

        // Update address (owned entity)
        existingPatient.Address = new Domain.ValueObjects.Address
        {
            Street = request.Address.Street,
            City = request.Address.City,
            State = request.Address.State,
            ZipCode = request.Address.ZipCode,
            Country = request.Address.Country
        };

        // Update emergency contact (owned entity)
        existingPatient.EmergencyContact = new Domain.ValueObjects.EmergencyContact
        {
            Name = request.EmergencyContact.Name,
            Relationship = request.EmergencyContact.Relationship,
            Phone = request.EmergencyContact.Phone,
            Email = request.EmergencyContact.Email
        };

        // Validate domain rules
        if (!existingPatient.IsValid())
        {
            throw new InvalidOperationException("Updated patient data is invalid.");
        }

        // Update using toolkit's UpdateOne method (will set UpdatedBy and UpdatedOn automatically)
        await UpdateOne(existingPatient, cancellationToken);

        // Map to DTO
        return PatientMapper.MapToDto(existingPatient);
    }

    public async Task<PatientDto?> GetPatientByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var patient = await GetByIdQuery(id, detached: true).SingleOrDefaultAsync(cancellationToken);

        return patient == null ? null : PatientMapper.MapToDto(patient);
    }

    public async Task<List<PatientDto>> GetActivePatientsAsync(CancellationToken cancellationToken = default)
    {
        // Using BaseFilters to demonstrate filtering
        var filters = new BaseFilters
        {
            IsAsNoTracking = true,
            TenantId = _loggedInUser.TenantId
        };

        var patients = await GetAll(filters);

        // Filter active patients in memory (could also be done in query)
        return patients
            .Where(p => p.IsActive)
            .Select(PatientMapper.MapToDto)
            .ToList();
    }

    public async Task<List<PatientDto>> SearchPatientsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        // Case-insensitive search using toolkit's Find method
        var lowerSearchTerm = searchTerm.ToLower();
        var query = Find(p =>
            p.FirstName.ToLower().Contains(lowerSearchTerm) ||
            p.LastName.ToLower().Contains(lowerSearchTerm),
            findOptions: null);

        var patients = await query.ToListAsync(cancellationToken);

        return patients.Select(PatientMapper.MapToDto).ToList();
    }

    public async Task<bool> ActivatePatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var patient = await GetByIdQuery(patientId, detached: false).SingleOrDefaultAsync(cancellationToken);

        if (patient == null)
        {
            throw new InvalidOperationException($"Patient with ID {patientId} not found.");
        }

        // Use domain method
        patient.Activate();

        // Update using toolkit (will set audit properties)
        await UpdateOne(patient, cancellationToken);

        return true;
    }

    public async Task<bool> DeactivatePatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var patient = await GetByIdQuery(patientId, detached: false).SingleOrDefaultAsync(cancellationToken);

        if (patient == null)
        {
            throw new InvalidOperationException($"Patient with ID {patientId} not found.");
        }

        // Use domain method
        patient.Deactivate();

        // Update using toolkit (will set audit properties)
        await UpdateOne(patient, cancellationToken);

        return true;
    }

    public async Task<string> GetPatientChangeHistoryAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var patient = await GetByIdQuery(patientId, detached: false).SingleOrDefaultAsync(cancellationToken);

        if (patient == null)
        {
            throw new InvalidOperationException($"Patient with ID {patientId} not found.");
        }

        // Demonstrate change tracking feature
        // Modify patient to show change detection
        patient.Phone = "000-000-0000"; // Temporary change for demonstration

        // Use toolkit's DetectChange method to get JSON of changes
        var changeJson = await DetectChange(patient);

        // Restore original values using toolkit's RestoreOriginalValuesAsync
        await RestoreOriginalValuesAsync(patient, new List<string> { "Phone" });

        return changeJson;
    }

    // ============================================================================
    // COMPREHENSIVE METHODS TO TEST ALL GENERIC SERVICE FEATURES
    // ============================================================================

    public async Task<List<PatientDto>> CreatePatientsBulkAsync(List<CreatePatientRequest> requests, CancellationToken cancellationToken = default)
    {
        // Tests AddMany method
        var patients = requests.Select(PatientMapper.MapToEntity).ToList();

        // Validate all patients
        foreach (var patient in patients)
        {
            if (!patient.IsValid())
            {
                throw new InvalidOperationException($"Invalid patient data: {patient.MRN}");
            }
        }

        // Use AddMany to insert all at once
        var success = await AddMany(patients);

        if (!success)
        {
            throw new InvalidOperationException("Failed to create patients in bulk");
        }

        // Map to DTOs
        return patients.Select(PatientMapper.MapToDto).ToList();
    }

    public async Task<PatientDto> SaveOrUpdatePatientAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        // Tests SaveOrUpdate method (upsert operation)
        var patient = PatientMapper.MapToEntity(request);

        // Check if patient with same MRN exists
        var existingPatient = await FindOne(p => p.MRN == request.MRN && p.TenantId == _loggedInUser.TenantId, findOptions: null);

        if (existingPatient != null)
        {
            // Update existing patient
            patient.Id = existingPatient.Id;
            patient.FirstName = request.FirstName;
            patient.LastName = request.LastName;
            patient.DateOfBirth = request.DateOfBirth;
            patient.Gender = request.Gender;
            patient.Phone = request.Phone;
            patient.Email = request.Email;
            patient.BloodType = request.BloodType;
            patient.Allergies = request.Allergies;
            patient.MedicalNotes = request.MedicalNotes;
            patient.Address = new Domain.ValueObjects.Address
            {
                Street = request.Address.Street,
                City = request.Address.City,
                State = request.Address.State,
                ZipCode = request.Address.ZipCode,
                Country = request.Address.Country
            };
            patient.EmergencyContact = new Domain.ValueObjects.EmergencyContact
            {
                Name = request.EmergencyContact.Name,
                Relationship = request.EmergencyContact.Relationship,
                Phone = request.EmergencyContact.Phone,
                Email = request.EmergencyContact.Email
            };
        }

        // Use SaveOrUpdate (will insert if new, update if exists)
        var result = await SaveOrUpdate(patient, setAuditProperties: true, shouldSave: true);

        return PatientMapper.MapToDto(result);
    }

    public async Task<List<PatientDto>> GetPatientsByIdsAsync(List<int> ids, CancellationToken cancellationToken = default)
    {
        // Tests ListAsync method
        var patients = await ListAsync(ids, cancellationToken);
        return patients.Select(PatientMapper.MapToDto).ToList();
    }

    public async Task<int> CountPatientsAsync(System.Linq.Expressions.Expression<Func<Domain.Entities.Patient, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        // Tests Count method
        // Note: Tenant filtering is handled automatically by BaseContext query filters
        if (predicate == null)
        {
            // Count all patients (tenant filtering applied automatically)
            predicate = p => true;
        }

        return await Count(predicate, cancellationToken);
    }

    public async Task<bool> SoftDeletePatientsAsync(List<int> patientIds, CancellationToken cancellationToken = default)
    {
        // Tests SoftDeleteMany method
        var patients = await ListAsync(patientIds, cancellationToken);

        if (patients.Count == 0)
        {
            return false;
        }

        return await SoftDeleteMany(patients, cancellationToken);
    }

    public async Task<int> HardDeletePatientsByConditionAsync(System.Linq.Expressions.Expression<Func<Domain.Entities.Patient, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // Tests HardDeleteMany method
        return await HardDeleteMany(predicate);
    }

    public async Task<int> HardDeletePatientEntityAsync(int patientId, CancellationToken cancellationToken = default)
    {
        // Tests HardDeleteOne method
        var patient = await GetByIdQuery(patientId, detached: false).SingleOrDefaultAsync(cancellationToken);

        if (patient == null)
        {
            return 0;
        }

        return await HardDeleteOne(patient);
    }

    public async Task<bool> RemovePatientsListAsync(List<int> patientIds, CancellationToken cancellationToken = default)
    {
        // Tests RemoveListOfEntities method
        var patients = await ListAsync(patientIds, cancellationToken);

        if (patients.Count == 0)
        {
            return false;
        }

        return await RemoveListOfEntities(patients);
    }

    public async Task<string> GetPatientFullJsonComparisonAsync(int patientId, CancellationToken cancellationToken = default)
    {
        // Tests LogFullJsonComparison method
        var patient = await GetByIdQuery(patientId, detached: false).SingleOrDefaultAsync(cancellationToken);

        if (patient == null)
        {
            throw new InvalidOperationException($"Patient with ID {patientId} not found.");
        }

        // Make some changes to demonstrate comparison
        var originalPhone = patient.Phone;
        var originalEmail = patient.Email;
        patient.Phone = "999-999-9999";
        patient.Email = "changed@example.com";

        // Get full JSON comparison (OldData vs NewData vs ChangedProperties)
        var comparisonJson = await LogFullJsonComparison(patient);

        // Restore original values
        patient.Phone = originalPhone;
        patient.Email = originalEmail;
        await RestoreOriginalValuesAsync(patient, new List<string> { "Phone", "Email" });

        return comparisonJson;
    }

    public async Task<PatientDto> SetPatientAuditPropertiesAsync(int patientId, CancellationToken cancellationToken = default)
    {
        // Tests SetAuditPropertiesAsync method
        var patient = await GetByIdQuery(patientId, detached: false).SingleOrDefaultAsync(cancellationToken);

        if (patient == null)
        {
            throw new InvalidOperationException($"Patient with ID {patientId} not found.");
        }

        // Manually set audit properties
        var updatedPatient = await SetAuditPropertiesAsync(patient);

        return PatientMapper.MapToDto(updatedPatient);
    }

    public async Task<List<PatientDto>> GetPatientsWithAdvancedFiltersAsync(
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
        CancellationToken cancellationToken = default)
    {
        // Tests ALL BaseFilters properties comprehensively
        var filters = new BaseFilters
        {
            // Basic properties
            IsAsNoTracking = true,
            IncludeSoftDeletedEntitiesAlso = includeSoftDeleted,
            IgnoreTenantCheck = ignoreTenantCheck,

            // Audit filtering
            CreatedBy = createdBy ?? 0,
            UpdatedBy = updatedBy ?? 0,
            DeleteBy = deleteBy ?? 0,

            // Date range filtering
            StartDate = startDate,
            EndDate = endDate,

            // Pagination
            ApplyPagination = skip.HasValue || take.HasValue,
            Skip = skip ?? 0,
            Take = take ?? 20,

            // Sorting
            ApplySorting = sortBy
        };

        // Set tenant ID if not ignoring tenant check
        if (!ignoreTenantCheck)
        {
            filters.TenantId = _loggedInUser.TenantId;
        }

        var patients = await GetAll(filters);

        return patients.Select(PatientMapper.MapToDto).ToList();
    }
}
