using GenericToolKit.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Patient.Application.DTOs;
using Patient.Application.Services;
using Patient.Domain.Specifications;

namespace Patient.API.Controllers;

/// <summary>
/// Patient API Controller
/// Demonstrates all Generic Toolkit features through RESTful endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(
        IPatientService patientService,
        ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new patient
    /// Demonstrates: Add, Audit tracking, Multi-tenancy, Validation
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _patientService.CreatePatientAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetPatientById), new { id = patient.Id }, patient);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing patient
    /// Demonstrates: Update, Audit tracking (UpdatedBy, UpdatedOn)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePatient(int id, [FromBody] UpdatePatientRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.Id = id;
            var patient = await _patientService.UpdatePatientAsync(request, cancellationToken);
            return Ok(patient);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get patient by ID
    /// Demonstrates: GetById, AsNoTracking
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientById(int id, CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetPatientByIdAsync(id, cancellationToken);

        if (patient == null)
        {
            return NotFound(new { error = $"Patient with ID {id} not found" });
        }

        return Ok(patient);
    }

    /// <summary>
    /// Get all patients with filtering and pagination
    /// Demonstrates: GetAll, BaseFilters, Pagination, Sorting, Tenant filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPatients(
        [FromQuery] int? skip = null,
        [FromQuery] int? take = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] bool includeSoftDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var filters = new BaseFilters
        {
            IsAsNoTracking = true,
            ApplyPagination = skip.HasValue || take.HasValue,
            Skip = skip,
            Take = take,
            IncludeSoftDeletedEntitiesAlso = includeSoftDeleted
        };

        var patients = await _patientService.GetAll(filters);

        // Filter active if needed
        if (!includeInactive)
        {
            patients = patients.Where(p => p.IsActive).ToList();
        }

        var dtos = patients.Select(p => new PatientDto
        {
            Id = p.Id,
            MRN = p.MRN,
            PatientCode = p.PatientCode,
            FirstName = p.FirstName,
            LastName = p.LastName,
            FullName = p.FullName,
            Age = p.Age,
            DateOfBirth = p.DateOfBirth,
            Gender = p.Gender,
            Phone = p.Phone,
            Email = p.Email,
            IsActive = p.IsActive,
            CreatedOn = p.CreatedOn,
            UpdatedOn = p.UpdatedOn
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Get active patients only
    /// Demonstrates: Custom business logic, Filtering
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivePatients(CancellationToken cancellationToken)
    {
        var patients = await _patientService.GetActivePatientsAsync(cancellationToken);
        return Ok(patients);
    }

    /// <summary>
    /// Search patients by name
    /// Demonstrates: Custom queries, LINQ expressions
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchPatients([FromQuery] string searchTerm, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest(new { error = "Search term is required" });
        }

        var patients = await _patientService.SearchPatientsAsync(searchTerm, cancellationToken);
        return Ok(patients);
    }

    /// <summary>
    /// Get patients using specification pattern
    /// Demonstrates: Specification pattern, ListBySpecs
    /// </summary>
    [HttpGet("by-specification/active")]
    [ProducesResponseType(typeof(List<Domain.Entities.Patient>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientsBySpecification(CancellationToken cancellationToken)
    {
        var spec = new ActivePatientsSpecification();
        var patients = await _patientService.ListBySpecs(spec, cancellationToken);
        return Ok(patients);
    }

    /// <summary>
    /// Get patients by age range using specification
    /// Demonstrates: Specification with parameters
    /// </summary>
    [HttpGet("by-age-range")]
    [ProducesResponseType(typeof(List<Domain.Entities.Patient>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientsByAgeRange(
        [FromQuery] int minAge,
        [FromQuery] int maxAge,
        CancellationToken cancellationToken)
    {
        var spec = new PatientsByAgeRangeSpecification(minAge, maxAge);
        var patients = await _patientService.ListBySpecs(spec, cancellationToken);
        return Ok(patients);
    }

    /// <summary>
    /// Activate a patient
    /// Demonstrates: Domain methods, Update operations
    /// </summary>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivatePatient(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _patientService.ActivatePatientAsync(id, cancellationToken);
            return Ok(new { message = "Patient activated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate a patient
    /// Demonstrates: Domain methods, Update operations
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivatePatient(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _patientService.DeactivatePatientAsync(id, cancellationToken);
            return Ok(new { message = "Patient deactivated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Soft delete a patient
    /// Demonstrates: Soft delete, IsDeleted flag, DeletedBy/DeletedOn audit
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDeletePatient(int id, CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetByIdQuery(id, detached: false)
            .SingleOrDefaultAsync(cancellationToken);

        if (patient == null)
        {
            return NotFound(new { error = $"Patient with ID {id} not found" });
        }

        var result = await _patientService.SoftDeleteOne(patient, cancellationToken);

        if (result)
        {
            return Ok(new { message = "Patient soft deleted successfully" });
        }

        return BadRequest(new { error = "Failed to delete patient" });
    }

    /// <summary>
    /// Hard delete a patient (permanent)
    /// Demonstrates: Hard delete operation
    /// </summary>
    [HttpDelete("{id}/hard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> HardDeletePatient(int id, CancellationToken cancellationToken)
    {
        var result = await _patientService.HardDeleteById(id);

        if (result)
        {
            return Ok(new { message = "Patient permanently deleted" });
        }

        return NotFound(new { error = $"Patient with ID {id} not found" });
    }

    /// <summary>
    /// Get change history for a patient
    /// Demonstrates: Change tracking, DetectChange method
    /// </summary>
    [HttpGet("{id}/change-history")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChangeHistory(int id, CancellationToken cancellationToken)
    {
        try
        {
            var changeJson = await _patientService.GetPatientChangeHistoryAsync(id, cancellationToken);
            return Ok(new { changes = changeJson });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Check if patient exists
    /// Demonstrates: Any operation
    /// </summary>
    [HttpGet("exists")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> PatientExists([FromQuery] string mrn, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(mrn))
        {
            return BadRequest(new { error = "MRN is required" });
        }

        var exists = await _patientService.Any(p => p.MRN == mrn);
        return Ok(new { exists, mrn });
    }

    // ============================================================================
    // COMPREHENSIVE ENDPOINTS TO TEST ALL GENERIC SERVICE FEATURES
    // ============================================================================

    /// <summary>
    /// Create multiple patients in bulk
    /// Demonstrates: AddMany method
    /// </summary>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(List<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePatientsBulk([FromBody] List<CreatePatientRequest> requests, CancellationToken cancellationToken)
    {
        try
        {
            var patients = await _patientService.CreatePatientsBulkAsync(requests, cancellationToken);
            return Ok(new { message = $"Successfully created {patients.Count} patients", patients });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Save or update patient (upsert operation)
    /// Demonstrates: SaveOrUpdate method
    /// </summary>
    [HttpPost("save-or-update")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveOrUpdatePatient([FromBody] CreatePatientRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _patientService.SaveOrUpdatePatientAsync(request, cancellationToken);
            return Ok(patient);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get patients by list of IDs
    /// Demonstrates: ListAsync method
    /// </summary>
    [HttpPost("by-ids")]
    [ProducesResponseType(typeof(List<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientsByIds([FromBody] List<int> ids, CancellationToken cancellationToken)
    {
        if (ids == null || ids.Count == 0)
        {
            return BadRequest(new { error = "At least one ID is required" });
        }

        var patients = await _patientService.GetPatientsByIdsAsync(ids, cancellationToken);
        return Ok(patients);
    }

    /// <summary>
    /// Count patients matching criteria
    /// Demonstrates: Count method
    /// </summary>
    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> CountPatients(
        [FromQuery] string? mrn = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        System.Linq.Expressions.Expression<Func<Domain.Entities.Patient, bool>>? predicate = null;

        if (!string.IsNullOrWhiteSpace(mrn))
        {
            predicate = p => p.MRN == mrn;
        }
        else if (isActive.HasValue)
        {
            predicate = p => p.IsActive == isActive.Value;
        }

        var count = await _patientService.CountPatientsAsync(predicate, cancellationToken);
        return Ok(new { count });
    }

    /// <summary>
    /// Soft delete multiple patients
    /// Demonstrates: SoftDeleteMany method
    /// </summary>
    [HttpDelete("bulk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SoftDeletePatientsBulk([FromBody] List<int> patientIds, CancellationToken cancellationToken)
    {
        if (patientIds == null || patientIds.Count == 0)
        {
            return BadRequest(new { error = "At least one patient ID is required" });
        }

        var result = await _patientService.SoftDeletePatientsAsync(patientIds, cancellationToken);

        if (result)
        {
            return Ok(new { message = $"Successfully soft deleted {patientIds.Count} patients" });
        }

        return BadRequest(new { error = "Failed to soft delete patients" });
    }

    /// <summary>
    /// Hard delete patients by condition
    /// Demonstrates: HardDeleteMany method
    /// </summary>
    [HttpDelete("by-condition")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> HardDeletePatientsByCondition(
        [FromQuery] string? mrnPattern = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        System.Linq.Expressions.Expression<Func<Domain.Entities.Patient, bool>>? predicate = null;

        if (!string.IsNullOrWhiteSpace(mrnPattern))
        {
            predicate = p => p.MRN.Contains(mrnPattern);
        }
        else if (isActive.HasValue)
        {
            predicate = p => p.IsActive == isActive.Value;
        }
        else
        {
            return BadRequest(new { error = "At least one condition is required" });
        }

        var deletedCount = await _patientService.HardDeletePatientsByConditionAsync(predicate, cancellationToken);
        return Ok(new { message = $"Permanently deleted {deletedCount} patients" });
    }

    /// <summary>
    /// Hard delete single patient entity
    /// Demonstrates: HardDeleteOne method
    /// </summary>
    [HttpDelete("{id}/hard-entity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> HardDeletePatientEntity(int id, CancellationToken cancellationToken)
    {
        var result = await _patientService.HardDeletePatientEntityAsync(id, cancellationToken);

        if (result > 0)
        {
            return Ok(new { message = "Patient permanently deleted", deletedCount = result });
        }

        return NotFound(new { error = $"Patient with ID {id} not found" });
    }

    /// <summary>
    /// Remove list of patient entities (hard delete)
    /// Demonstrates: RemoveListOfEntities method
    /// </summary>
    [HttpDelete("remove-list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemovePatientsList([FromBody] List<int> patientIds, CancellationToken cancellationToken)
    {
        if (patientIds == null || patientIds.Count == 0)
        {
            return BadRequest(new { error = "At least one patient ID is required" });
        }

        var result = await _patientService.RemovePatientsListAsync(patientIds, cancellationToken);

        if (result)
        {
            return Ok(new { message = $"Successfully removed {patientIds.Count} patients" });
        }

        return BadRequest(new { error = "Failed to remove patients" });
    }

    /// <summary>
    /// Get full JSON comparison for patient (old vs new data)
    /// Demonstrates: LogFullJsonComparison method
    /// </summary>
    [HttpGet("{id}/full-json-comparison")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientFullJsonComparison(int id, CancellationToken cancellationToken)
    {
        try
        {
            var comparisonJson = await _patientService.GetPatientFullJsonComparisonAsync(id, cancellationToken);
            return Ok(new { comparison = comparisonJson });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Manually set audit properties for a patient
    /// Demonstrates: SetAuditPropertiesAsync method
    /// </summary>
    [HttpPost("{id}/set-audit-properties")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPatientAuditProperties(int id, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _patientService.SetPatientAuditPropertiesAsync(id, cancellationToken);
            return Ok(patient);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get patients with advanced BaseFilters (tests all BaseFilters properties)
    /// Demonstrates: All BaseFilters properties (CreatedBy, UpdatedBy, DeleteBy, IgnoreTenantCheck, Sorting, DateRange, etc.)
    /// </summary>
    [HttpGet("advanced-filters")]
    [ProducesResponseType(typeof(List<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientsWithAdvancedFilters(
        [FromQuery] int? createdBy = null,
        [FromQuery] int? updatedBy = null,
        [FromQuery] int? deleteBy = null,
        [FromQuery] bool ignoreTenantCheck = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] bool includeSoftDeleted = false,
        [FromQuery] int? skip = null,
        [FromQuery] int? take = null,
        CancellationToken cancellationToken = default)
    {
        var patients = await _patientService.GetPatientsWithAdvancedFiltersAsync(
            createdBy: createdBy,
            updatedBy: updatedBy,
            deleteBy: deleteBy,
            ignoreTenantCheck: ignoreTenantCheck,
            sortBy: sortBy,
            startDate: startDate,
            endDate: endDate,
            includeSoftDeleted: includeSoftDeleted,
            skip: skip,
            take: take,
            cancellationToken: cancellationToken);

        return Ok(new
        {
            count = patients.Count,
            filters = new
            {
                createdBy,
                updatedBy,
                deleteBy,
                ignoreTenantCheck,
                sortBy,
                startDate,
                endDate,
                includeSoftDeleted,
                skip,
                take
            },
            patients
        });
    }

    /// <summary>
    /// Get patient by MRN using specification
    /// Demonstrates: PatientByMRNSpecification
    /// </summary>
    [HttpGet("by-mrn/{mrn}")]
    [ProducesResponseType(typeof(Domain.Entities.Patient), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientByMRN(string mrn, CancellationToken cancellationToken)
    {
        var spec = new PatientByMRNSpecification(mrn);
        var patients = await _patientService.ListBySpecs(spec, cancellationToken);

        if (patients.Count == 0)
        {
            return NotFound(new { error = $"Patient with MRN '{mrn}' not found" });
        }

        return Ok(patients.First());
    }

    /// <summary>
    /// Get patients with appointments using specification
    /// Demonstrates: PatientsWithAppointmentsSpecification
    /// </summary>
    [HttpGet("with-appointments")]
    [ProducesResponseType(typeof(List<Domain.Entities.Patient>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientsWithAppointments(CancellationToken cancellationToken)
    {
        var spec = new PatientsWithAppointmentsSpecification();
        var patients = await _patientService.ListBySpecs(spec, cancellationToken);
        return Ok(patients);
    }
}
