# Patient Microservice - Setup & Usage Guide

## Overview

This is a **comprehensive Patient microservice** built as a testbed to demonstrate **ALL features** of the Generic Toolkit. It follows Clean Architecture principles with 4 distinct layers.

## Architecture

```
Patient/
├── Patient.Domain/          # Domain entities, value objects, specifications
├── Patient.Application/     # Business logic, services, DTOs
├── Patient.Infra/          # Data access, DbContext, repositories, migrations
└── Patient.API/            # REST API controllers, DI configuration
```

## Prerequisites

- **.NET 6.0 SDK** or higher
- **SQL Server** (LocalDB, Express, or full version)
- **SQL Server Management Studio** (optional, for viewing data)

## Quick Start

### 1. Update Connection String (if needed)

Edit `Patient.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=PatientMicroserviceDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Connection String Options:**
- SQL Server Express: `Server=.\\SQLEXPRESS;...`
- LocalDB: `Server=(localdb)\\mssqllocaldb;...`
- Full SQL Server: `Server=localhost;...`
- With credentials: `Server=.\\SQLEXPRESS;Database=PatientMicroserviceDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;`

### 2. Create/Update Database

The API automatically applies migrations on startup (in Development mode). Alternatively, run manually:

```bash
cd Patient

# Apply migrations to create database
dotnet ef database update --project Patient.Infra/Patient.Infra.csproj --startup-project Patient.API/Patient.API.csproj

# Or to recreate database from scratch
dotnet ef database drop --project Patient.Infra/Patient.Infra.csproj --startup-project Patient.API/Patient.API.csproj --force
dotnet ef database update --project Patient.Infra/Patient.Infra.csproj --startup-project Patient.API/Patient.API.csproj
```

### 3. Run the API

```bash
cd Patient
dotnet run --project Patient.API/Patient.API.csproj
```

The API will start on:
- **HTTPS**: https://localhost:7001
- **HTTP**: http://localhost:5001
- **Swagger UI**: https://localhost:7001 (root URL)

### 4. Access Swagger UI

Open your browser to **https://localhost:7001**

The Swagger UI provides:
- Interactive API documentation
- Ability to test all endpoints
- Request/response examples

## Multi-Tenancy & User Context

The API uses HTTP headers for multi-tenancy and user tracking:

### Required Headers for API Calls:

```http
X-Tenant-Id: 1          # Facility/Hospital ID (required for multi-tenancy)
X-User-Id: 100          # Current user ID (for audit tracking)
X-Role-Id: 5            # User role ID (for authorization)
```

### Example cURL Request:

```bash
curl -X POST "https://localhost:7001/api/Patients" \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 1" \
  -H "X-User-Id: 100" \
  -H "X-Role-Id: 5" \
  -d '{
    "mrn": "MRN-001",
    "patientCode": "PAT-001",
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "1990-01-15",
    "gender": 1,
    "phone": "555-0100",
    "email": "john.doe@example.com",
    "address": {
      "street": "123 Main St",
      "city": "New York",
      "state": "NY",
      "zipCode": "10001",
      "country": "USA"
    },
    "emergencyContact": {
      "name": "Jane Doe",
      "relationship": "Spouse",
      "phone": "555-0101",
      "email": "jane.doe@example.com"
    },
    "bloodType": "O+",
    "allergies": "Penicillin"
  }'
```

## Generic Toolkit Features Demonstrated

### 1. **Multi-Tenancy**
- Automatic tenant isolation via `X-Tenant-Id` header
- All queries filtered by `TenantId`
- Prevents cross-tenant data leakage

**Example:**
```http
GET /api/Patients
X-Tenant-Id: 1    # Only returns patients for tenant 1
```

### 2. **Audit Tracking**
- `CreatedBy`, `CreatedOn` - Set automatically on creation
- `UpdatedBy`, `UpdatedOn` - Set automatically on updates
- `DeletedBy`, `DeletedOn` - Set automatically on soft delete

**View audit fields:**
```http
GET /api/Patients/1
```

Response includes:
```json
{
  "id": 1,
  "firstName": "John",
  "createdBy": 100,
  "createdOn": "2024-01-15T10:30:00Z",
  "updatedBy": 105,
  "updatedOn": "2024-01-16T14:20:00Z",
  "tenantId": 1
}
```

### 3. **Soft Delete**
- Deleted records are marked with `IsDeleted = true`
- Automatically excluded from queries
- Can be restored if needed

**Soft delete a patient:**
```http
DELETE /api/Patients/1
X-User-Id: 100
```

**View soft-deleted patients (admin only):**
```http
GET /api/Patients?includeSoftDeleted=true
```

### 4. **Repository Pattern**
- Generic CRUD operations
- Custom patient-specific queries
- Testable data access layer

### 5. **Service Pattern**
- Business logic separation
- Transaction management
- Validation and domain rules

### 6. **Specification Pattern**
- Reusable query logic
- Complex filtering

**Example - Get active patients:**
```http
GET /api/Patients/by-specification/active
```

**Example - Get patients by age range:**
```http
GET /api/Patients/by-age-range?minAge=25&maxAge=45
```

### 7. **Transaction Management**
- ACID compliance
- Rollback on error

**Create multiple patients in a transaction:**
```http
POST /api/TransactionDemo/create-batch
Content-Type: application/json

[
  { "mrn": "MRN-100", ... },
  { "mrn": "MRN-101", ... },
  { "mrn": "MRN-102", ... }
]
```

If any patient fails, ALL are rolled back.

### 8. **Change Tracking**
- Detect what changed
- Get old vs new values
- Audit trail

**Get change history:**
```http
GET /api/Patients/1/change-history
```

Returns JSON showing what changed.

### 9. **Pagination & Filtering**
```http
GET /api/Patients?skip=0&take=10                    # First 10 patients
GET /api/Patients?skip=10&take=10                   # Next 10 patients
GET /api/Patients?includeInactive=true              # Include inactive
GET /api/Patients?includeSoftDeleted=true           # Include deleted
```

### 10. **Search**
```http
GET /api/Patients/search?searchTerm=john           # Search by name
```

## API Endpoints

### Patient Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Patients` | Create new patient |
| PUT | `/api/Patients/{id}` | Update patient |
| GET | `/api/Patients/{id}` | Get patient by ID |
| GET | `/api/Patients` | Get all patients (with pagination) |
| DELETE | `/api/Patients/{id}` | Soft delete patient |
| DELETE | `/api/Patients/{id}/hard` | Hard delete patient (permanent) |

### Special Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Patients/active` | Get active patients only |
| GET | `/api/Patients/search?searchTerm=...` | Search by name |
| POST | `/api/Patients/{id}/activate` | Activate patient |
| POST | `/api/Patients/{id}/deactivate` | Deactivate patient |
| GET | `/api/Patients/{id}/change-history` | Get change tracking |
| GET | `/api/Patients/exists?mrn=...` | Check if MRN exists |

### Specification Pattern

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Patients/by-specification/active` | Get using ActivePatientsSpecification |
| GET | `/api/Patients/by-age-range?minAge=25&maxAge=45` | Get by age range |

### Transactions

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/TransactionDemo/create-batch` | Create multiple (with rollback on error) |
| POST | `/api/TransactionDemo/demo-rollback` | Intentional rollback demo |

## Database Schema

The migration creates the following tables:

### Patients Table
- `Id` (PK)
- `MRN` (Unique per tenant)
- `PatientCode`
- `FirstName`, `LastName`
- `DateOfBirth`
- `Gender`
- `Phone`, `Email`
- `Address_*` (Owned entity)
- `EmergencyContact_*` (Owned entity)
- `IsActive`
- `BloodType`, `Allergies`, `MedicalNotes`
- **Audit Fields**: `CreatedBy`, `CreatedOn`, `UpdatedBy`, `UpdatedOn`, `TenantId`
- **Soft Delete**: `IsDeleted`, `DeletedBy`, `DeletedOn`

### Appointments Table
- `Id` (PK)
- `PatientId` (FK)
- `AppointmentDateTime`
- `Reason`, `DoctorName`, `Status`, `Notes`
- **Audit Fields** (same as Patients)

## Testing Scenarios

### Scenario 1: Multi-Tenant Isolation

1. Create patient for Tenant 1:
```http
POST /api/Patients
X-Tenant-Id: 1
X-User-Id: 100
```

2. Try to retrieve from Tenant 2:
```http
GET /api/Patients/1
X-Tenant-Id: 2
```

**Result**: Returns 404 (tenant isolation working)

### Scenario 2: Audit Tracking

1. Create patient as User 100
2. Update patient as User 105
3. View patient - check `CreatedBy=100`, `UpdatedBy=105`

### Scenario 3: Soft Delete & Restore

1. Create patient
2. Soft delete: `DELETE /api/Patients/1`
3. Try to get: `GET /api/Patients/1` → Returns 404
4. Get with deleted: `GET /api/Patients?includeSoftDeleted=true` → Patient visible with `IsDeleted=true`

### Scenario 4: Transaction Rollback

1. POST to `/api/TransactionDemo/create-batch` with:
   - Patient 1: Valid
   - Patient 2: Duplicate MRN (will fail)
   - Patient 3: Valid

**Result**: None are created (all rolled back)

## Troubleshooting

### Database Connection Issues

**Error**: "Cannot open database"

**Solution**:
1. Verify SQL Server is running
2. Check connection string in `appsettings.json`
3. Ensure database exists: `dotnet ef database update`

### Migration Issues

**Error**: "No migrations found"

**Solution**:
```bash
cd Patient
dotnet ef migrations add InitialCreate --project Patient.Infra --startup-project Patient.API
dotnet ef database update --project Patient.Infra --startup-project Patient.API
```

### Port Already in Use

**Error**: "Address already in use"

**Solution**: Change ports in `Patient.API/Properties/launchSettings.json`

## Project Structure Details

### Patient.Domain
- **Entities**: `Patient.cs`, `Appointment.cs`
- **Value Objects**: `Address.cs`, `EmergencyContact.cs`
- **Specifications**: `ActivePatientsSpecification.cs`, `PatientByMRNSpecification.cs`, etc.
- **Enums**: `Gender.cs`, `AppointmentStatus.cs`

### Patient.Application
- **Services**: `IPatientService.cs`, `PatientService.cs`
- **DTOs**: `PatientDto.cs`, `CreatePatientRequest.cs`, `UpdatePatientRequest.cs`
- **Mapping**: `PatientMapper.cs`

### Patient.Infra
- **Data**: `PatientDbContext.cs`, `PatientDbContextFactory.cs`
- **Configurations**: `PatientEntityConfiguration.cs`, `AppointmentEntityConfiguration.cs`
- **Repositories**: `IPatientRepository.cs`, `PatientRepository.cs`
- **Migrations**: Auto-generated migration files

### Patient.API
- **Controllers**: `PatientsController.cs`, `TransactionDemoController.cs`
- **Infrastructure**: `HttpContextLoggedInUser.cs`, `SystemUser.cs`
- **Configuration**: `Program.cs`, `appsettings.json`

## Development

### Adding New Features

1. **Add entity** in `Patient.Domain/Entities`
2. **Add DbSet** in `PatientDbContext`
3. **Create migration**: `dotnet ef migrations add <MigrationName>`
4. **Add service** in `Patient.Application/Services`
5. **Add controller** in `Patient.API/Controllers`
6. **Update database**: `dotnet ef database update`

### Running Tests

Currently, no unit tests are included. To add tests:

```bash
dotnet new xunit -n Patient.Tests
dotnet add Patient.Tests reference Patient.Application
dotnet add Patient.Tests reference Patient.Domain
```

## Additional Resources

- **Swagger UI**: https://localhost:7001
- **Generic Toolkit Documentation**: See `Generic-Tool-Code/README.md`
- **EF Core Documentation**: https://docs.microsoft.com/ef/core

## Support

For issues or questions:
1. Check this SETUP.md guide
2. Review Swagger UI documentation
3. Check Generic Toolkit documentation
4. Review application logs in console output

---

**Built with Generic Toolkit** - A comprehensive testbed demonstrating all toolkit features in a real-world microservice architecture.
