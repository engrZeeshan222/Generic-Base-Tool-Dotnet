# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**GenericToolKit** is a foundational .NET library implementing Clean Architecture and Domain-Driven Design (DDD) principles. It provides a reusable generic service and repository pattern for Entity Framework Core applications with built-in multi-tenancy, audit tracking, and soft-delete capabilities.

**Tech Stack**: .NET 6.0 / Entity Framework Core 7.0 / C# 10+

**Purpose**: This toolkit serves as the base infrastructure layer for the HMS.api solution, providing 120+ domain modules with consistent CRUD operations, multi-tenant isolation, and audit capabilities.

## Build Commands

```bash
# Build entire solution
dotnet build GenericToolKit.sln

# Build specific project
dotnet build src/GenericToolKit.Domain/GenericToolKit.Domain.csproj
dotnet build src/GenericToolKit.Application/GenericToolKit.Application.csproj
dotnet build src/GenericToolKit.Infrastructure/GenericToolKit.Infrastructure.csproj
dotnet build src/GenericToolKit.Abstractions/GenericToolKit.Abstractions.csproj

# Restore NuGet packages
dotnet restore GenericToolKit.sln

# Clean build artifacts
dotnet clean GenericToolKit.sln
```

**Note**: No test projects exist in this solution. Tests should be created in consuming applications.

## Solution Architecture

### Project Structure

```
GenericToolKit/
├── src/
│   ├── GenericToolKit.Abstractions/   # Shared interfaces and contracts
│   ├── GenericToolKit.Domain/         # Core domain entities and models
│   ├── GenericToolKit.Application/    # Application services (use cases)
│   └── GenericToolKit.Infrastructure/ # Repository implementations
├── GenericToolKit.sln
├── README.md
├── ARCHITECTURE_REFERENCE_GUIDE.md
└── DDD_DEPENDENCY_EXPLANATION.md
```

### Clean Architecture Layers

**Dependency Flow**: Infrastructure → Application → Domain ← Abstractions

```
┌────────────────────────┐
│   Infrastructure       │  Implements data access
│   (Repositories)       │  Depends on: Abstractions, Domain
└───────────┬────────────┘
            │
            ▼
┌────────────────────────┐
│   Application          │  Implements business logic
│   (Services)           │  Depends on: Abstractions, Domain
└───────────┬────────────┘
            │
            ▼
┌────────────────────────┐
│   Domain               │  Core business entities
│   (Entities, Models)   │  Depends on: Nothing (pure)
└────────────────────────┘
            ▲
            │
┌───────────┴────────────┐
│   Abstractions         │  Shared contracts
│   (Interfaces)         │  Depends on: Domain
└────────────────────────┘
```

**Critical Rule**: Domain has ZERO dependencies. Application and Infrastructure both depend on Domain and Abstractions, but never on each other.

### Layer Responsibilities

**GenericToolKit.Domain** (Pure Domain):
- `BaseEntity` - Abstract base for all entities with audit properties
- `BaseFilters` - Query filtering and pagination model
- `TypesValidator` - Validation extension methods
- **Dependencies**: None (Newtonsoft.Json only)

**GenericToolKit.Abstractions** (Shared Contracts):
- `IGenericRepository<T>` - Repository interface with segregated sub-interfaces
- `ILoggedInUser` - User context for multi-tenancy
- `IBaseSpecification<T>` - Specification pattern interface
- `BaseEntry<T>` - Change tracking model
- `ExceptionHandler` - Exception handling utility
- **Dependencies**: Domain, EF Core 7.0

**GenericToolKit.Application** (Business Logic):
- `IGenericService<T>` - Service interface (segregated: CRUD, Query, Audit, Transaction, ChangeTracking)
- `GenericService<T>` - Service implementation wrapping repositories
- DI registration extensions
- **Dependencies**: Abstractions, Domain, EF Core 7.0

**GenericToolKit.Infrastructure** (Data Access):
- `GenericRepository<T>` - Repository implementation
- `BaseContext` - DbContext with automatic tenant filtering and audit tracking
- `QueryableExtensions` - IQueryable filtering helpers
- **Dependencies**: Abstractions, Domain, EF Core 7.0

## Multi-Tenancy Architecture

All entities inheriting from `BaseEntity` automatically include:

```csharp
public int TenantId { get; set; }         // Tenant isolation
public int CreatedBy { get; set; }        // Audit: who created
public DateTime CreatedOn { get; set; }   // Audit: when created
public int? UpdatedBy { get; set; }       // Audit: who updated
public DateTime? UpdatedOn { get; set; }  // Audit: when updated
public bool IsDeleted { get; set; }       // Soft delete flag
public int? DeletedBy { get; set; }       // Audit: who deleted
public DateTime? DeletedOn { get; set; }  // Audit: when deleted
```

**Automatic Behaviors**:
- Queries are automatically filtered by `TenantId` via `BaseContext` query filters
- Soft-deleted records (`IsDeleted = true`) are excluded from queries
- Audit properties are automatically set on `SaveChangesAsync()`
- Use `BaseFilters.IgnoreTenantCheck = true` for cross-tenant queries (admin scenarios)

## Interface Segregation Pattern (ISP)

Both services and repositories follow Interface Segregation Principle:

**Repository Interfaces** (`IGenericRepository<T>` composed of):
- `ITransactionRepository<T>` - BeginTransaction, Commit, Rollback
- `IEntityCrudRepository<T>` - Add, AddMany, Update, UpdateMany
- `IEntityQueryRepository<T>` - GetAll, GetById, Find, FindOne, Any, Count
- `IEntityChangeTrackingRepository<T>` - GetChanges, IsEntityChanged
- `IAuditRepository<T>` - GetAuditProperties, SetAuditProperties
- `IEntityRemovalRepository<T>` - SoftDelete, HardDelete, SoftDeleteMany

**Service Interfaces** (`IGenericService<T>` composed of):
- `ITransactionService` - Transaction management
- `ICrudService<T>` - CRUD operations
- `IQueryService<T>` - Query operations
- `IChangeTrackingService<T>` - Change tracking
- `IAuditService<T>` - Audit operations
- `IRemovalService<T>` - Removal operations
- `IAdditionalService<T>` - Additional operations

## Typical Usage Pattern

### 1. Define Entity (in consuming application)
```csharp
public class Patient : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string MedicalRecordNumber { get; set; }
}
```

### 2. Implement ILoggedInUser (in consuming application)
```csharp
public class LoggedInUser : ILoggedInUser
{
    public int TenantId { get; set; }  // Hospital/Facility ID
    public int LoginId { get; set; }   // Staff member ID
    public int RoleId { get; set; }    // Role ID
}
```

### 3. Register in DI (in consuming application)
```csharp
services.AddScoped<ILoggedInUser, LoggedInUser>();
services.AddGenericRepository<Patient>();
services.AddGenericService<Patient>();
```

### 4. Use in Application Code
```csharp
public class PatientService
{
    private readonly IGenericService<Patient> _service;

    public async Task<List<Patient>> GetPatients(int hospitalId)
    {
        var filters = new BaseFilters
        {
            TenantId = hospitalId,
            IsAsNoTracking = true,  // Read-only query
            PageSize = 50,
            PageNumber = 1
        };
        return await _service.GetAll(filters);
    }
}
```

## Recent Architectural Changes (Branch: fix_rf_1997)

**Major Refactoring in Progress**: Introduction of `GenericToolKit.Abstractions` layer to eliminate circular dependencies.

**Files Moved to Abstractions**:
- `IGenericRepository.cs` (from Domain)
- `ILoggedInUser.cs` (from Domain)
- `IBaseSpecification.cs` (from Domain)
- `IProjectableSpecifications.cs` (from Domain)
- `BaseEntry.cs` (from Domain)
- `ExceptionHandler.cs` (from Domain)

**Files Moved to Infrastructure**:
- `QueryableExtensions.cs` (from Domain to Infrastructure/Extensions)

**Rationale**:
- Domain should contain only pure business logic (entities, value objects)
- EF Core-specific interfaces belong in Abstractions
- EF Core extension methods belong in Infrastructure
- Eliminates circular dependency between layers

**Status**: Changes staged but not committed. Project references updated.

## NuGet Package Generation

All projects are configured with `GeneratePackageOnBuild: true`:
- `GenericToolKit.Domain` v1.0.0
- `GenericToolKit.Application` v1.0.0
- `GenericToolKit.Infrastructure` v1.0.0
- `GenericToolKit.Abstractions` v1.0.0

Packages are MIT licensed, authored by HMS Team.

## Dependency Injection Extensions

**Application Layer** (`ServiceCollectionExtensions.cs`):
```csharp
services.AddGenericService<TEntity>();  // Registers IGenericService<T> and GenericService<T>
```

**Infrastructure Layer** (`ServiceCollectionExtensions.cs`):
```csharp
services.AddGenericRepository<TEntity>();  // Registers IGenericRepository<T> and GenericRepository<T>
services.AddBaseContext<TContext>(options);  // Registers BaseContext-derived DbContext
```

## Important Development Notes

- **No Tests**: This library has no test projects. Tests should be implemented in consuming applications using in-memory EF Core providers.
- **Entity Framework Version**: Uses EF Core 7.0, not EF6. Consuming applications (HMS.api) may use EF6 for legacy compatibility.
- **Migration History**: Previously named `GenericTool`, renamed to `GenericToolKit`. Legacy references to `FacilityId` have been renamed to `TenantId`.
- **C# Features**: Nullable reference types enabled, implicit usings enabled.
- **Thread Safety**: Repository and service instances should be scoped (not singleton) to avoid DbContext threading issues.

## Specification Pattern

Use Ardalis.Specification pattern for complex queries:

```csharp
public class ActivePatientsSpec : Specification<Patient>
{
    public ActivePatientsSpec()
    {
        Query.Where(p => !p.IsDeleted)
             .OrderByDescending(p => p.CreatedOn);
    }
}

// Usage
var patients = await _repository.ListAsync(new ActivePatientsSpec());
```

## BaseContext Query Filters

`BaseContext` automatically applies:
1. **Tenant Filter**: `WHERE TenantId = @currentTenantId`
2. **Soft Delete Filter**: `WHERE IsDeleted = false`

Override with `BaseFilters.IgnoreTenantCheck = true` or `BaseFilters.IncludeDeleted = true`.

## Common Pitfalls

1. **Do NOT** reference `GenericToolKit.Infrastructure` from Application layer
2. **Do NOT** reference `GenericToolKit.Application` from Domain layer
3. **Do NOT** reference `GenericToolKit.Infrastructure` from Domain layer
4. **Always** use `IGenericService<T>` instead of `IGenericRepository<T>` in application code
5. **Remember** that `BaseEntity` is abstract - do not attempt to instantiate directly
6. **Ensure** `ILoggedInUser` is properly configured for audit tracking to work

## Integration with Parent HMS.api Solution

This library is consumed by 120+ domain modules in `HMS.Data/Modules/`:
- Each module's entities inherit from `BaseEntity`
- Each module's services use `IGenericService<T>`
- Each module's repositories extend `GenericRepository<T>`
- All modules automatically inherit multi-tenancy and audit capabilities

See parent `CLAUDE.md` at repository root for HMS-specific integration patterns.
