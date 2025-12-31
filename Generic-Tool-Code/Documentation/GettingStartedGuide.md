# GenericToolKit - Getting Started Guide

A complete step-by-step manual for using GenericToolKit in your .NET applications. This guide will walk you through everything from installation to advanced features using a Hospital Management System as an example.

---

## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [Installation](#2-installation)
3. [Understanding the Architecture](#3-understanding-the-architecture)
4. [Your First Entity](#4-your-first-entity)
5. [Setting Up User Context](#5-setting-up-user-context)
6. [Configuring Dependency Injection](#6-configuring-dependency-injection)
7. [Basic CRUD Operations](#7-basic-crud-operations)
8. [Query Operations](#8-query-operations)
9. [Multi-Tenancy](#9-multi-tenancy)
10. [Audit Tracking](#10-audit-tracking)
11. [Soft Delete](#11-soft-delete)
12. [Transaction Management](#12-transaction-management)
13. [Change Tracking](#13-change-tracking)
14. [Advanced Filtering](#14-advanced-filtering)
15. [Batch Operations](#15-batch-operations)
16. [Specification Pattern](#16-specification-pattern)
17. [Custom DbContext](#17-custom-dbcontext)
18. [Best Practices](#18-best-practices)
19. [Troubleshooting](#19-troubleshooting)
20. [Complete Example](#20-complete-example)

---

## 1. Prerequisites

Before you begin, ensure you have:

- **.NET 6.0 SDK** or higher installed
- **Visual Studio 2022** or **VS Code** with C# extension
- **SQL Server** or any EF Core supported database
- Basic understanding of:
  - C# and .NET
  - Entity Framework Core
  - Dependency Injection
  - ASP.NET Core (if building web apps)

---

## 2. Installation

### Step 2.1: Install NuGet Packages

Open your project and install the following packages:

```bash
# Using .NET CLI
dotnet add package GenericToolKit.Domain
dotnet add package GenericToolKit.Abstractions
dotnet add package GenericToolKit.Application
dotnet add package GenericToolKit.Infrastructure

# You'll also need Entity Framework Core
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### Step 2.2: Verify Installation

Check your `.csproj` file to ensure packages are installed:

```xml
<ItemGroup>
  <PackageReference Include="GenericToolKit.Domain" Version="1.0.0" />
  <PackageReference Include="GenericToolKit.Abstractions" Version="1.0.0" />
  <PackageReference Include="GenericToolKit.Application" Version="1.0.0" />
  <PackageReference Include="GenericToolKit.Infrastructure" Version="1.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="7.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="7.0.0" />
</ItemGroup>
```

---

## 3. Understanding the Architecture

GenericToolKit follows **Clean Architecture** with three main layers:

```
Your Application
├── Domain Layer (Entities, Models)
│   └── GenericToolKit.Domain
│       ├── BaseEntity (Your entities inherit from this)
│       ├── BaseFilters (Query filtering)
│       └── Validation Extensions
│
├── Application Layer (Business Logic)
│   └── GenericToolKit.Application
│       ├── IGenericService<T> (Service interface)
│       └── GenericService<T> (Service implementation)
│
└── Infrastructure Layer (Data Access)
    └── GenericToolKit.Infrastructure
        ├── GenericRepository<T> (Repository implementation)
        ├── BaseContext (DbContext base class)
        └── Query Extensions
```

**Key Concepts:**
- **BaseEntity**: All your entities inherit from this (provides Id, TenantId, audit fields)
- **IGenericService**: Provides CRUD, queries, transactions, change tracking
- **BaseFilters**: Controls pagination, tenant filtering, soft delete filtering
- **ILoggedInUser**: Provides current user context for multi-tenancy and auditing

---

## 4. Your First Entity

### Step 4.1: Create an Entity

Create a new class that inherits from `BaseEntity`:

```csharp
using GenericToolKit.Domain.Entities;

namespace HMS.Domain.Entities
{
    public class Patient : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string MedicalRecordNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string BloodGroup { get; set; }
    }
}
```

### Step 4.2: What You Get Automatically

By inheriting from `BaseEntity`, your `Patient` entity automatically has:

```csharp
// Inherited properties (you don't need to add these):
public int Id { get; set; }                    // Primary key
public int TenantId { get; set; }              // Multi-tenant isolation (Hospital/Facility)
public int CreatedBy { get; set; }             // Which staff member created this record
public DateTime CreatedOn { get; set; }        // When it was created
public int? UpdatedBy { get; set; }            // Which staff member last updated
public DateTime? UpdatedOn { get; set; }       // When it was updated
public bool IsDeleted { get; set; }            // Soft delete flag
public int? DeletedBy { get; set; }            // Who deleted it
public DateTime? DeletedOn { get; set; }       // When it was deleted
```

### Step 4.3: Create More Entities (Optional)

```csharp
public class Doctor : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Specialization { get; set; }
    public string LicenseNumber { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
}

public class Appointment : BaseEntity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; }
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; }
    public string Notes { get; set; }
}

public class MedicalRecord : BaseEntity
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; }
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; }
    public DateTime VisitDate { get; set; }
    public string Diagnosis { get; set; }
    public string Treatment { get; set; }
    public string Prescription { get; set; }
}
```

---

## 5. Setting Up User Context

### Step 5.1: Create LoggedInUser Implementation

Create a class that implements `ILoggedInUser`:

```csharp
using GenericToolKit.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HMS.Infrastructure
{
    public class LoggedInUser : ILoggedInUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoggedInUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int TenantId => GetTenantIdFromClaims(); // Hospital/Facility ID
        public int LoginId => GetUserIdFromClaims();    // Staff member ID
        public int RoleId => GetRoleIdFromClaims();     // Role (Doctor, Nurse, Admin, etc.)

        private int GetTenantIdFromClaims()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("TenantId"); // Hospital/Facility ID
            return claim != null ? int.Parse(claim.Value) : 1; // Default to 1
        }

        private int GetUserIdFromClaims()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("UserId"); // Staff ID
            return claim != null ? int.Parse(claim.Value) : 1;
        }

        private int GetRoleIdFromClaims()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var claim = user?.FindFirst("RoleId");
            return claim != null ? int.Parse(claim.Value) : 1;
        }
    }
}
```

### Step 5.2: Simple Implementation (For Testing)

If you don't need user claims, use a simple implementation:

```csharp
public class LoggedInUser : ILoggedInUser
{
    public int TenantId { get; set; } = 1; // Default hospital
    public int LoginId { get; set; } = 1;  // Default staff member
    public int RoleId { get; set; } = 1;   // Default role
}
```

---

## 6. Configuring Dependency Injection

### Step 6.1: Create Your DbContext

```csharp
using GenericToolKit.Infrastructure.Data;
using GenericToolKit.Abstractions.Interfaces;
using Microsoft.EntityFrameworkCore;
using HMS.Domain.Entities;

namespace HMS.Infrastructure.Data
{
    public class HospitalDbContext : BaseContext
    {
        public HospitalDbContext(
            DbContextOptions<HospitalDbContext> options,
            ILoggedInUser loggedInUser
        ) : base(options, loggedInUser)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Your custom configurations
            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.MedicalRecordNumber)
                .IsUnique();

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany()
                .HasForeignKey(a => a.DoctorId);
        }
    }
}
```

### Step 6.2: Configure Services in Startup.cs or Program.cs

**For ASP.NET Core 6+ (Program.cs):**

```csharp
using GenericToolKit.Application.DependencyInjection;
using GenericToolKit.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using HMS.Infrastructure;
using HMS.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<HospitalDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Add HttpContextAccessor (for getting user from claims)
builder.Services.AddHttpContextAccessor();

// Register LoggedInUser
builder.Services.AddScoped<ILoggedInUser, LoggedInUser>();

// Register GenericRepository and GenericService for each entity
builder.Services.AddGenericRepository<Patient>();
builder.Services.AddGenericService<Patient>();

builder.Services.AddGenericRepository<Doctor>();
builder.Services.AddGenericService<Doctor>();

builder.Services.AddGenericRepository<Appointment>();
builder.Services.AddGenericService<Appointment>();

builder.Services.AddGenericRepository<MedicalRecord>();
builder.Services.AddGenericService<MedicalRecord>();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Step 6.3: Configure Connection String (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HospitalManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Step 6.4: Create Database Migration

```bash
# Add initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

---

## 7. Basic CRUD Operations

### Step 7.1: CREATE - Adding New Records

```csharp
using GenericToolKit.Application.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IGenericService<Patient> _patientService;

    public PatientsController(IGenericService<Patient> patientService)
    {
        _patientService = patientService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePatient([FromBody] Patient patient)
    {
        // GenericToolKit automatically sets:
        // - CreatedBy (from ILoggedInUser.LoginId - Staff member who registered)
        // - CreatedOn (current timestamp)
        // - TenantId (from ILoggedInUser.TenantId - Hospital/Facility ID)

        var created = await _patientService.Add(patient);

        return CreatedAtAction(nameof(GetPatient), new { id = created.Id }, created);
    }
}
```

### Step 7.2: READ - Getting Records

```csharp
using GenericToolKit.Domain.Models;

[HttpGet("{id}")]
public async Task<IActionResult> GetPatient(int id)
{
    var filters = new BaseFilters
    {
        TenantId = 1, // Or get from ILoggedInUser (Hospital ID)
        IsAsNoTracking = true // Read-only query (better performance)
    };

    var patient = await _patientService.GetById(id, filters);

    if (patient == null)
        return NotFound();

    return Ok(patient);
}

[HttpGet]
public async Task<IActionResult> GetAllPatients([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
{
    var filters = new BaseFilters
    {
        TenantId = 1,
        PageNumber = page,
        PageSize = pageSize,
        IsAsNoTracking = true
    };

    var patients = await _patientService.GetAll(filters);

    return Ok(patients);
}
```

### Step 7.3: UPDATE - Updating Records

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdatePatient(int id, [FromBody] Patient patient)
{
    if (id != patient.Id)
        return BadRequest();

    var filters = new BaseFilters { TenantId = 1 };

    // Check if exists
    var existing = await _patientService.GetById(id, filters);
    if (existing == null)
        return NotFound();

    // Update properties
    existing.FirstName = patient.FirstName;
    existing.LastName = patient.LastName;
    existing.PhoneNumber = patient.PhoneNumber;
    existing.Email = patient.Email;
    existing.Address = patient.Address;

    // GenericToolKit automatically sets:
    // - UpdatedBy (from ILoggedInUser.LoginId - Staff member who updated)
    // - UpdatedOn (current timestamp)

    var updated = await _patientService.Update(existing);

    return Ok(updated);
}
```

### Step 7.4: DELETE - Removing Records

```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> DeletePatient(int id)
{
    var filters = new BaseFilters { TenantId = 1 };

    // Soft delete (marks IsDeleted = true, doesn't remove from database)
    // Important for medical records - never permanently delete patient data!
    await _patientService.SoftDelete(id, filters);

    return NoContent();
}

[HttpDelete("{id}/permanent")]
[Authorize(Roles = "Admin")] // Only admins can permanently delete
public async Task<IActionResult> PermanentDeletePatient(int id)
{
    var filters = new BaseFilters { TenantId = 1 };

    // Hard delete (permanently removes from database)
    // Use with extreme caution for medical data!
    await _patientService.HardDelete(id, filters);

    return NoContent();
}
```

---

## 8. Query Operations

### Step 8.1: Find with Predicate

```csharp
[HttpGet("search")]
public async Task<IActionResult> SearchPatients([FromQuery] string keyword)
{
    var filters = new BaseFilters
    {
        TenantId = 1,
        IsAsNoTracking = true
    };

    // Find all patients matching criteria
    var patients = await _patientService.Find(
        predicate: p => p.FirstName.Contains(keyword) ||
                       p.LastName.Contains(keyword) ||
                       p.MedicalRecordNumber.Contains(keyword),
        filters: filters
    );

    return Ok(patients);
}
```

### Step 8.2: Find One (Single Result)

```csharp
[HttpGet("by-mrn/{mrn}")]
public async Task<IActionResult> GetPatientByMRN(string mrn)
{
    var filters = new BaseFilters
    {
        TenantId = 1,
        IsAsNoTracking = true
    };

    var patient = await _patientService.FindOne(
        predicate: p => p.MedicalRecordNumber == mrn,
        filters: filters
    );

    if (patient == null)
        return NotFound();

    return Ok(patient);
}
```

### Step 8.3: Check Existence

```csharp
[HttpGet("exists/{mrn}")]
public async Task<IActionResult> PatientExists(string mrn)
{
    var filters = new BaseFilters { TenantId = 1 };

    bool exists = await _patientService.Any(
        predicate: p => p.MedicalRecordNumber == mrn,
        filters: filters
    );

    return Ok(new { exists });
}
```

### Step 8.4: Count Records

```csharp
[HttpGet("count")]
public async Task<IActionResult> GetPatientCount([FromQuery] string gender = null)
{
    var filters = new BaseFilters { TenantId = 1 };

    int count = await _patientService.Count(
        predicate: p => gender == null || p.Gender == gender,
        filters: filters
    );

    return Ok(new { count });
}
```

---

## 9. Multi-Tenancy

### Step 9.1: Understanding Tenant Isolation

GenericToolKit automatically filters all queries by `TenantId`. In a hospital context:

- Hospital 1 can only see their own patients
- Hospital 2 can only see their own patients
- Patient data is completely isolated between facilities

### Step 9.2: Using Tenant Filtering

```csharp
[HttpGet]
public async Task<IActionResult> GetMyPatients()
{
    var filters = new BaseFilters
    {
        TenantId = 1, // Current hospital from ILoggedInUser
        IsAsNoTracking = true
    };

    // Only returns patients for Hospital 1
    var patients = await _patientService.GetAll(filters);

    return Ok(patients);
}
```

### Step 9.3: Cross-Tenant Queries (Admin Only)

```csharp
[HttpGet("admin/all-hospitals")]
[Authorize(Roles = "SystemAdmin")] // Make sure only system admins can access
public async Task<IActionResult> GetAllPatientsAllHospitals()
{
    var filters = new BaseFilters
    {
        IgnoreTenantCheck = true, // Bypasses hospital filtering
        IsAsNoTracking = true
    };

    // Returns patients for ALL hospitals
    var patients = await _patientService.GetAll(filters);

    return Ok(patients);
}
```

### Step 9.4: Getting Tenant from Claims

```csharp
[HttpGet]
public async Task<IActionResult> GetMyPatients()
{
    // Get hospital ID from logged in user
    var hospitalId = User.FindFirst("TenantId")?.Value;

    var filters = new BaseFilters
    {
        TenantId = int.Parse(hospitalId),
        IsAsNoTracking = true
    };

    var patients = await _patientService.GetAll(filters);

    return Ok(patients);
}
```

---

## 10. Audit Tracking

### Step 10.1: Automatic Audit Fields

Every entity automatically tracks:

```csharp
public class Patient : BaseEntity
{
    // Your properties
    public string FirstName { get; set; }

    // Automatically tracked (inherited from BaseEntity):
    // - CreatedBy: Staff member ID who registered this patient
    // - CreatedOn: Timestamp when patient was registered
    // - UpdatedBy: Staff member ID who last updated patient info
    // - UpdatedOn: Timestamp when last updated
    // - TenantId: Which hospital owns this patient record
}
```

### Step 10.2: Viewing Audit Information

```csharp
[HttpGet("{id}/audit")]
public async Task<IActionResult> GetPatientAuditInfo(int id)
{
    var filters = new BaseFilters
    {
        TenantId = 1,
        IsAsNoTracking = true
    };

    var patient = await _patientService.GetById(id, filters);

    if (patient == null)
        return NotFound();

    var auditInfo = new
    {
        patient.Id,
        PatientName = $"{patient.FirstName} {patient.LastName}",
        RegisteredBy = patient.CreatedBy,
        RegisteredOn = patient.CreatedOn,
        LastUpdatedBy = patient.UpdatedBy,
        LastUpdatedOn = patient.UpdatedOn,
        HospitalId = patient.TenantId
    };

    return Ok(auditInfo);
}
```

### Step 10.3: Getting Audit Properties Programmatically

```csharp
[HttpGet("{id}/audit-properties")]
public async Task<IActionResult> GetAuditProperties(int id)
{
    var filters = new BaseFilters { TenantId = 1 };

    var patient = await _patientService.GetById(id, filters);

    if (patient == null)
        return NotFound();

    // Get audit properties as dictionary
    var auditProps = await _patientService.GetAuditProperties(patient);

    return Ok(auditProps);
}
```

---

## 11. Soft Delete

### Step 11.1: Understanding Soft Delete

Soft delete marks records as deleted (`IsDeleted = true`) without removing them from the database.

**Critical for Healthcare:**
- Medical records must be preserved for legal and compliance reasons
- Patient data recovery possible
- Maintains referential integrity
- Audit trail preserved
- Supports HIPAA and other healthcare regulations

### Step 11.2: Soft Delete Single Record

```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> SoftDeletePatient(int id)
{
    var filters = new BaseFilters { TenantId = 1 };

    // Marks IsDeleted = true
    // Sets DeletedBy and DeletedOn automatically
    // Patient record preserved in database
    await _patientService.SoftDelete(id, filters);

    return NoContent();
}
```

### Step 11.3: Soft Delete Multiple Records

```csharp
[HttpDelete("batch")]
public async Task<IActionResult> SoftDeletePatients([FromBody] List<int> ids)
{
    var filters = new BaseFilters { TenantId = 1 };

    await _patientService.SoftDeleteMany(ids, filters);

    return NoContent();
}
```

### Step 11.4: Including Soft-Deleted Records in Queries

```csharp
[HttpGet("include-deleted")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> GetAllIncludingDeleted()
{
    var filters = new BaseFilters
    {
        TenantId = 1,
        IncludeDeleted = true, // Include soft-deleted patient records
        IsAsNoTracking = true
    };

    var patients = await _patientService.GetAll(filters);

    return Ok(patients);
}
```

### Step 11.5: Restoring Soft-Deleted Records

```csharp
[HttpPost("{id}/restore")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> RestorePatient(int id)
{
    var filters = new BaseFilters
    {
        TenantId = 1,
        IncludeDeleted = true // Need this to find deleted records
    };

    var patient = await _patientService.GetById(id, filters);

    if (patient == null)
        return NotFound();

    // Restore by setting IsDeleted to false
    patient.IsDeleted = false;
    patient.DeletedBy = null;
    patient.DeletedOn = null;

    await _patientService.Update(patient);

    return Ok(patient);
}
```

---

## 12. Transaction Management

### Step 12.1: Using Transactions

```csharp
[HttpPost("create-appointment")]
public async Task<IActionResult> CreateAppointmentWithRecords([FromBody] CreateAppointmentDto dto)
{
    // Begin transaction
    await _patientService.BeginTransaction();

    try
    {
        // Verify patient exists
        var filters = new BaseFilters { TenantId = 1 };
        var patient = await _patientService.GetById(dto.PatientId, filters);

        if (patient == null)
            throw new Exception("Patient not found");

        // Verify doctor exists
        var doctor = await _doctorService.GetById(dto.DoctorId, filters);

        if (doctor == null)
            throw new Exception("Doctor not found");

        // Create appointment
        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            AppointmentDate = dto.AppointmentDate,
            Status = "Scheduled",
            Notes = dto.Notes
        };

        var createdAppointment = await _appointmentService.Add(appointment);

        // Create medical record if provided
        if (!string.IsNullOrEmpty(dto.InitialDiagnosis))
        {
            var medicalRecord = new MedicalRecord
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                VisitDate = DateTime.UtcNow,
                Diagnosis = dto.InitialDiagnosis,
                Treatment = dto.InitialTreatment
            };

            await _medicalRecordService.Add(medicalRecord);
        }

        // Commit transaction
        await _patientService.Commit();

        return Ok(createdAppointment);
    }
    catch (Exception ex)
    {
        // Rollback on error
        await _patientService.Rollback();
        return BadRequest(new { error = ex.Message });
    }
}
```

---

## 13. Change Tracking

### Step 13.1: Get Changes for an Entity

```csharp
[HttpGet("{id}/changes")]
public async Task<IActionResult> GetPatientChanges(int id)
{
    var filters = new BaseFilters { TenantId = 1 };

    var changes = await _patientService.GetChanges(id, filters);

    return Ok(changes);
}
```

### Step 13.2: Check if Entity Changed

```csharp
[HttpGet("{id}/has-changes")]
public async Task<IActionResult> HasPatientChanged(int id)
{
    var filters = new BaseFilters { TenantId = 1 };

    bool hasChanges = await _patientService.IsEntityChanged(id, filters);

    return Ok(new { hasChanges });
}
```

---

## 14. Advanced Filtering

### Step 14.1: Pagination

```csharp
[HttpGet]
public async Task<IActionResult> GetPatientsPaginated(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    var filters = new BaseFilters
    {
        TenantId = 1,
        PageNumber = page,
        PageSize = pageSize,
        IsAsNoTracking = true
    };

    var patients = await _patientService.GetAll(filters);

    return Ok(new
    {
        page,
        pageSize,
        data = patients
    });
}
```

### Step 14.2: Sorting

```csharp
[HttpGet("sorted")]
public async Task<IActionResult> GetPatientsSorted([FromQuery] string sortBy = "LastName")
{
    var filters = new BaseFilters
    {
        TenantId = 1,
        IsAsNoTracking = true
    };

    // Add sorting via ApplySorting
    filters.ApplySorting = query =>
    {
        return sortBy.ToLower() switch
        {
            "firstname" => query.OrderBy(p => p.FirstName),
            "lastname" => query.OrderBy(p => p.LastName),
            "dob" => query.OrderBy(p => p.DateOfBirth),
            "mrn" => query.OrderBy(p => p.MedicalRecordNumber),
            _ => query.OrderBy(p => p.Id)
        };
    };

    var patients = await _patientService.GetAll(filters);

    return Ok(patients);
}
```

### Step 14.3: NoTracking for Read-Only Queries

```csharp
[HttpGet("readonly")]
public async Task<IActionResult> GetPatientsReadOnly()
{
    var filters = new BaseFilters
    {
        TenantId = 1,
        IsAsNoTracking = true, // Better performance for read-only
        PageSize = 100
    };

    var patients = await _patientService.GetAll(filters);

    return Ok(patients);
}
```

---

## 15. Batch Operations

### Step 15.1: Add Multiple Records

```csharp
[HttpPost("batch")]
public async Task<IActionResult> CreateMultiplePatients([FromBody] List<Patient> patients)
{
    var created = await _patientService.AddMany(patients);

    return Ok(created);
}
```

### Step 15.2: Update Multiple Records

```csharp
[HttpPut("batch")]
public async Task<IActionResult> UpdateMultiplePatients([FromBody] List<Patient> patients)
{
    var updated = await _patientService.UpdateMany(patients);

    return Ok(updated);
}
```

### Step 15.3: Delete Multiple Records

```csharp
[HttpDelete("batch")]
public async Task<IActionResult> DeleteMultiplePatients([FromBody] List<int> ids)
{
    var filters = new BaseFilters { TenantId = 1 };

    await _patientService.SoftDeleteMany(ids, filters);

    return NoContent();
}
```

---

## 16. Specification Pattern

### Step 16.1: Create a Specification

```csharp
using GenericToolKit.Abstractions.Interfaces;
using System.Linq.Expressions;

public class CriticalPatientsSpec : IBaseSpecification<Patient>
{
    private readonly int _ageThreshold;

    public CriticalPatientsSpec(int ageThreshold = 65)
    {
        _ageThreshold = ageThreshold;
    }

    public Expression<Func<Patient, bool>> Criteria =>
        p => DateTime.UtcNow.Year - p.DateOfBirth.Year >= _ageThreshold && !p.IsDeleted;

    public List<Expression<Func<Patient, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();

    public Expression<Func<Patient, object>> OrderBy =>
        p => p.DateOfBirth; // Order by date of birth ascending (oldest first)

    public Expression<Func<Patient, object>> OrderByDescending { get; set; }

    public int Take { get; set; }
    public int Skip { get; set; }
    public bool IsPagingEnabled { get; set; }
}
```

### Step 16.2: Use the Specification

```csharp
[HttpGet("critical")]
public async Task<IActionResult> GetCriticalPatients([FromQuery] int ageThreshold = 65)
{
    var spec = new CriticalPatientsSpec(ageThreshold);
    var filters = new BaseFilters
    {
        TenantId = 1,
        IsAsNoTracking = true
    };

    // Use specification with repository
    var patients = await _patientRepository.ListAsync(spec);

    return Ok(patients);
}
```

---

## 17. Custom DbContext

### Step 17.1: Extend BaseContext

```csharp
using GenericToolKit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class HospitalDbContext : BaseContext
{
    public HospitalDbContext(
        DbContextOptions<HospitalDbContext> options,
        ILoggedInUser loggedInUser
    ) : base(options, loggedInUser)
    {
    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // IMPORTANT: Call base first to get GenericToolKit configurations
        base.OnModelCreating(modelBuilder);

        // Add your custom configurations
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasIndex(e => e.MedicalRecordNumber).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
            entity.Property(e => e.LicenseNumber).HasMaxLength(50).IsRequired();
        });
    }
}
```

---

## 18. Best Practices

### 1. Always Use BaseFilters

```csharp
// ✅ GOOD
var filters = new BaseFilters
{
    TenantId = hospitalId,
    IsAsNoTracking = true
};
var patients = await _patientService.GetAll(filters);

// ❌ BAD - Don't query without filters
var patients = await _patientService.GetAll(null); // Will fail
```

### 2. Use AsNoTracking for Read-Only Queries

```csharp
// ✅ GOOD - Better performance
var filters = new BaseFilters
{
    TenantId = 1,
    IsAsNoTracking = true // Use for read-only
};

// ❌ BAD - Tracking when not needed
var filters = new BaseFilters
{
    TenantId = 1,
    IsAsNoTracking = false // Slower for read-only queries
};
```

### 3. Use Transactions for Multi-Step Operations

```csharp
// ✅ GOOD
await _service.BeginTransaction();
try
{
    await _service.Add(appointment);
    await _service.Add(medicalRecord);
    await _service.Commit();
}
catch
{
    await _service.Rollback();
    throw;
}

// ❌ BAD - No transaction for related operations
await _service.Add(appointment);
await _service.Add(medicalRecord); // If this fails, appointment is still added
```

### 4. Don't Expose Entities Directly in APIs

```csharp
// ✅ GOOD - Use DTOs
public class PatientDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MedicalRecordNumber { get; set; }
}

[HttpGet]
public async Task<IActionResult> GetPatients()
{
    var patients = await _patientService.GetAll(filters);
    var dtos = patients.Select(p => new PatientDto
    {
        Id = p.Id,
        FirstName = p.FirstName,
        LastName = p.LastName,
        MedicalRecordNumber = p.MedicalRecordNumber
    });
    return Ok(dtos);
}

// ❌ BAD - Exposing entities directly
[HttpGet]
public async Task<IActionResult> GetPatients()
{
    var patients = await _patientService.GetAll(filters);
    return Ok(patients); // Exposes all fields including audit fields
}
```

### 5. Handle Tenant Isolation Properly

```csharp
// ✅ GOOD - Get hospital ID from authenticated user
var hospitalId = User.FindFirst("TenantId")?.Value;
var filters = new BaseFilters { TenantId = int.Parse(hospitalId) };

// ❌ BAD - Hardcoded or from request body (security risk)
var filters = new BaseFilters { TenantId = dto.TenantId }; // User can fake hospital ID!
```

### 6. Never Permanently Delete Medical Records

```csharp
// ✅ GOOD - Use soft delete for patient records
await _patientService.SoftDelete(id, filters);

// ❌ BAD - Permanent deletion violates healthcare compliance
await _patientService.HardDelete(id, filters); // Don't do this for medical data!
```

---

## 19. Troubleshooting

### Issue 1: "BaseFilters is null"

**Problem:**
```csharp
var patients = await _patientService.GetAll(null); // Error!
```

**Solution:**
```csharp
var filters = new BaseFilters { TenantId = 1 };
var patients = await _patientService.GetAll(filters);
```

### Issue 2: "No records returned even though they exist"

**Problem:** Soft-deleted records are excluded by default

**Solution:**
```csharp
var filters = new BaseFilters
{
    TenantId = 1,
    IncludeDeleted = true // Include soft-deleted patient records
};
```

### Issue 3: "Can't see records from other hospitals"

**Problem:** Tenant filtering is enabled by default

**Solution:**
```csharp
var filters = new BaseFilters
{
    IgnoreTenantCheck = true // Only for system admin operations!
};
```

### Issue 4: "Entity not tracked for updates"

**Problem:** Used AsNoTracking for query, then tried to update

**Solution:**
```csharp
// For updates, use tracking
var filters = new BaseFilters
{
    TenantId = 1,
    IsAsNoTracking = false // Enable tracking for updates
};
var patient = await _patientService.GetById(id, filters);
patient.PhoneNumber = "Updated";
await _patientService.Update(patient);
```

### Issue 5: "Migration fails"

**Problem:** DbContext not configured properly

**Solution:**
1. Ensure your DbContext inherits from `BaseContext`
2. Call `base.OnModelCreating(modelBuilder)` first
3. Pass `ILoggedInUser` to base constructor

---

## 20. Complete Example

Here's a complete working example of a Patient API for Hospital Management:

```csharp
using GenericToolKit.Application.Services;
using GenericToolKit.Domain.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IGenericService<Patient> _patientService;
    private readonly ILoggedInUser _loggedInUser;

    public PatientsController(
        IGenericService<Patient> patientService,
        ILoggedInUser loggedInUser)
    {
        _patientService = patientService;
        _loggedInUser = loggedInUser;
    }

    // GET: api/patients
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string search = null)
    {
        var filters = new BaseFilters
        {
            TenantId = _loggedInUser.TenantId, // Current hospital
            PageNumber = page,
            PageSize = pageSize,
            IsAsNoTracking = true
        };

        List<Patient> patients;

        if (!string.IsNullOrEmpty(search))
        {
            patients = await _patientService.Find(
                p => p.FirstName.Contains(search) ||
                     p.LastName.Contains(search) ||
                     p.MedicalRecordNumber.Contains(search),
                filters
            );
        }
        else
        {
            patients = await _patientService.GetAll(filters);
        }

        return Ok(new
        {
            page,
            pageSize,
            total = patients.Count,
            data = patients
        });
    }

    // GET: api/patients/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var filters = new BaseFilters
        {
            TenantId = _loggedInUser.TenantId,
            IsAsNoTracking = true
        };

        var patient = await _patientService.GetById(id, filters);

        if (patient == null)
            return NotFound();

        return Ok(patient);
    }

    // GET: api/patients/mrn/{mrn}
    [HttpGet("mrn/{mrn}")]
    public async Task<IActionResult> GetByMRN(string mrn)
    {
        var filters = new BaseFilters
        {
            TenantId = _loggedInUser.TenantId,
            IsAsNoTracking = true
        };

        var patient = await _patientService.FindOne(
            p => p.MedicalRecordNumber == mrn,
            filters
        );

        if (patient == null)
            return NotFound();

        return Ok(patient);
    }

    // POST: api/patients
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PatientCreateDto dto)
    {
        var patient = new Patient
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            MedicalRecordNumber = dto.MedicalRecordNumber,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            Address = dto.Address,
            BloodGroup = dto.BloodGroup
        };

        var created = await _patientService.Add(patient);

        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    // PUT: api/patients/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PatientUpdateDto dto)
    {
        var filters = new BaseFilters
        {
            TenantId = _loggedInUser.TenantId,
            IsAsNoTracking = false // Need tracking for update
        };

        var patient = await _patientService.GetById(id, filters);

        if (patient == null)
            return NotFound();

        patient.FirstName = dto.FirstName;
        patient.LastName = dto.LastName;
        patient.PhoneNumber = dto.PhoneNumber;
        patient.Email = dto.Email;
        patient.Address = dto.Address;

        var updated = await _patientService.Update(patient);

        return Ok(updated);
    }

    // DELETE: api/patients/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var filters = new BaseFilters
        {
            TenantId = _loggedInUser.TenantId
        };

        // Soft delete only - medical records must be preserved
        await _patientService.SoftDelete(id, filters);

        return NoContent();
    }

    // POST: api/patients/batch
    [HttpPost("batch")]
    public async Task<IActionResult> CreateBatch([FromBody] List<PatientCreateDto> dtos)
    {
        var patients = dtos.Select(dto => new Patient
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            MedicalRecordNumber = dto.MedicalRecordNumber,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            Address = dto.Address,
            BloodGroup = dto.BloodGroup
        }).ToList();

        var created = await _patientService.AddMany(patients);

        return Ok(created);
    }

    // GET: api/patients/critical
    [HttpGet("critical")]
    public async Task<IActionResult> GetCriticalPatients([FromQuery] int ageThreshold = 65)
    {
        var filters = new BaseFilters
        {
            TenantId = _loggedInUser.TenantId,
            IsAsNoTracking = true
        };

        var patients = await _patientService.Find(
            p => DateTime.UtcNow.Year - p.DateOfBirth.Year >= ageThreshold,
            filters
        );

        return Ok(patients);
    }
}

// DTOs
public class PatientCreateDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; }
    public string MedicalRecordNumber { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string BloodGroup { get; set; }
}

public class PatientUpdateDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
}
```

---

## Conclusion

You now have a complete understanding of how to use GenericToolKit for Hospital Management!

**Key Takeaways:**
1. All entities inherit from `BaseEntity`
2. Use `IGenericService<T>` for all operations
3. Always provide `BaseFilters` with proper `TenantId` (Hospital ID)
4. Use `IsAsNoTracking = true` for read-only queries
5. Use transactions for multi-step operations
6. **Always use soft delete for medical records** - never permanently delete patient data
7. Audit tracking is automatic and critical for healthcare compliance
8. Multi-tenancy ensures hospital data isolation

**Healthcare-Specific Best Practices:**
- Never permanently delete patient records
- Always maintain audit trails
- Use tenant isolation to separate hospital data
- Implement proper role-based access control
- Follow HIPAA and healthcare compliance requirements

**Next Steps:**
- Review the main [README.md](../../README.md) for API reference
- Check [ARCHITECTURE_REFERENCE_GUIDE.md](../ARCHITECTURE_REFERENCE_GUIDE.md) for architecture details
- See [DDD_DEPENDENCY_EXPLANATION.md](../DDD_DEPENDENCY_EXPLANATION.md) for DDD principles

Happy coding with GenericToolKit for Hospital Management Systems! 🏥
