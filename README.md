# GenericToolKit

A foundational .NET library implementing **Clean Architecture** and **Domain-Driven Design (DDD)** principles. Provides a production-ready generic service and repository pattern for Entity Framework Core applications with built-in multi-tenancy, audit tracking, and soft-delete capabilities.

[![.NET](https://img.shields.io/badge/.NET-6.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![EF Core](https://img.shields.io/badge/EF%20Core-7.0-512BD4?style=flat-square)](https://learn.microsoft.com/en-us/ef/core/)
[![License](https://img.shields.io/badge/License-MIT-green.svg?style=flat-square)](LICENSE)
[![C#](https://img.shields.io/badge/C%23-10.0-239120?style=flat-square&logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)

---

## 📋 Table of Contents

- [Features](#-features)
- [Quick Start](#-quick-start)
- [Installation](#-installation)
- [Architecture](#-architecture)
- [Usage](#-usage)
- [Multi-Tenancy](#-multi-tenancy)
- [API Reference](#-api-reference)
- [Advanced Features](#-advanced-features)
- [Contributing](#-contributing)
- [License](#-license)

---

## ✨ Features

- **Clean Architecture** - Proper separation of concerns across layers
- **Domain-Driven Design** - Repository pattern, specifications, and aggregate roots
- **Multi-Tenancy** - Built-in tenant isolation with automatic filtering
- **Audit Tracking** - Automatic CreatedBy, UpdatedBy, CreatedOn, UpdatedOn tracking
- **Soft Delete** - Mark records as deleted without physical removal
- **Change Tracking** - Detect and track entity changes
- **Transaction Management** - Built-in transaction support with commit/rollback
- **Interface Segregation** - SOLID principles with segregated interfaces
- **Pagination & Filtering** - Built-in support for common query patterns
- **Specification Pattern** - Encapsulate complex query logic
- **Generic CRUD Operations** - Reusable operations for any entity
- **Dependency Injection** - Easy DI registration with extension methods

---

## 🚀 Quick Start

### 1. Install NuGet Packages

```bash
dotnet add package GenericToolKit.Domain
dotnet add package GenericToolKit.Abstractions
dotnet add package GenericToolKit.Application
dotnet add package GenericToolKit.Infrastructure
```

### 2. Define Your Entity

```csharp
using GenericToolKit.Domain.Entities;

public class Patient : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string MedicalRecordNumber { get; set; }
}
```

### 3. Implement User Context

```csharp
using GenericToolKit.Abstractions.Interfaces;

public class LoggedInUser : ILoggedInUser
{
    public int TenantId { get; set; }  // Hospital/Facility ID
    public int LoginId { get; set; }   // Staff member ID
    public int RoleId { get; set; }    // Role ID
}
```

### 4. Register Services

```csharp
using GenericToolKit.Application.DependencyInjection;
using GenericToolKit.Infrastructure.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    // Register user context
    services.AddScoped<ILoggedInUser, LoggedInUser>();

    // Register repository and service for your entity
    services.AddGenericRepository<Patient>();
    services.AddGenericService<Patient>();
}
```

### 5. Use in Your Application

```csharp
using GenericToolKit.Application.Services;
using GenericToolKit.Domain.Models;

public class PatientsController
{
    private readonly IGenericService<Patient> _patientService;

    public PatientsController(IGenericService<Patient> patientService)
    {
        _patientService = patientService;
    }

    public async Task<List<Patient>> GetPatients(int hospitalId)
    {
        var filters = new BaseFilters
        {
            TenantId = hospitalId,
            IsAsNoTracking = true,  // Read-only query
            PageSize = 50,
            PageNumber = 1
        };

        return await _patientService.GetAll(filters);
    }

    public async Task<Patient> CreatePatient(Patient patient)
    {
        return await _patientService.Add(patient);
    }
}
```

---

## 📦 Installation

### NuGet Package Manager

```bash
Install-Package GenericToolKit.Domain
Install-Package GenericToolKit.Abstractions
Install-Package GenericToolKit.Application
Install-Package GenericToolKit.Infrastructure
```

### .NET CLI

```bash
dotnet add package GenericToolKit.Domain
dotnet add package GenericToolKit.Abstractions
dotnet add package GenericToolKit.Application
dotnet add package GenericToolKit.Infrastructure
```

### Package Reference

```xml
<ItemGroup>
  <PackageReference Include="GenericToolKit.Domain" Version="1.0.0" />
  <PackageReference Include="GenericToolKit.Abstractions" Version="1.0.0" />
  <PackageReference Include="GenericToolKit.Application" Version="1.0.0" />
  <PackageReference Include="GenericToolKit.Infrastructure" Version="1.0.0" />
</ItemGroup>
```

---

## 🏛️ Architecture

GenericToolKit follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                      │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  GenericRepository, BaseContext, QueryableExtensions   │ │
│  │  Depends on: Abstractions, Domain, EF Core 7.0         │ │
│  └────────────────────────────────────────────────────────┘ │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                     Application Layer                        │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  GenericService, IGenericService                        │ │
│  │  Depends on: Abstractions, Domain, EF Core 7.0         │ │
│  └────────────────────────────────────────────────────────┘ │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│                      Domain Layer                            │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  BaseEntity, BaseFilters, BaseInOutDTO                  │ │
│  │  Depends on: NOTHING (Pure Domain)                      │ │
│  └────────────────────────────────────────────────────────┘ │
└──────────────────────────▲──────────────────────────────────┘
                           │
┌──────────────────────────┴──────────────────────────────────┐
│                   Abstractions Layer                         │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  IGenericRepository, ILoggedInUser, IBaseSpecification │ │
│  │  Depends on: Domain, EF Core 7.0                        │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

| Layer | Responsibility | Dependencies |
|-------|---------------|-------------|
| **Domain** | Core business entities and models | None (Pure) |
| **Abstractions** | Shared interfaces and contracts | Domain, EF Core |
| **Application** | Business logic and use cases | Abstractions, Domain |
| **Infrastructure** | Data access and repositories | Abstractions, Domain |

---

## 💡 Usage

### Basic CRUD Operations

```csharp
// CREATE
var patient = new Patient { FirstName = "John", LastName = "Doe", DateOfBirth = new DateTime(1980, 1, 1), MedicalRecordNumber = "MRN001" };
var created = await _patientService.Add(patient);

// READ
var patient = await _patientService.GetById(1, filters);
var allPatients = await _patientService.GetAll(filters);

// UPDATE
patient.FirstName = "Jane";
var updated = await _patientService.Update(patient);

// DELETE (Soft Delete)
await _patientService.SoftDelete(patient.Id, filters);

// DELETE (Hard Delete - Permanent)
await _patientService.HardDelete(patient.Id, filters);
```

### Filtering and Pagination

```csharp
var filters = new BaseFilters
{
    TenantId = 1,
    PageNumber = 1,
    PageSize = 20,
    IsAsNoTracking = true,  // For read-only queries
    IncludeDeleted = false,  // Exclude soft-deleted records
    IgnoreTenantCheck = false  // Enforce tenant isolation
};

var patients = await _patientService.GetAll(filters);
```

### Query with Conditions

```csharp
// Find patients by last name
var patients = await _patientService.Find(
    predicate: p => p.LastName.Contains("Smith"),
    filters: filters
);

// Find single patient by MRN
var patient = await _patientService.FindOne(
    predicate: p => p.MedicalRecordNumber == "MRN001",
    filters: filters
);

// Check if any patient exists
bool hasPatients = await _patientService.Any(
    predicate: p => p.LastName == "Doe",
    filters: filters
);

// Count patients
int count = await _patientService.Count(
    predicate: p => p.IsDeleted == false,
    filters: filters
);
```

### Transaction Management

```csharp
// Begin transaction
await _patientService.BeginTransaction();

try
{
    var patient1 = await _patientService.Add(new Patient { FirstName = "John", LastName = "Doe", MedicalRecordNumber = "MRN001" });
    var patient2 = await _patientService.Add(new Patient { FirstName = "Jane", LastName = "Smith", MedicalRecordNumber = "MRN002" });

    // Commit transaction
    await _patientService.Commit();
}
catch
{
    // Rollback on error
    await _patientService.Rollback();
    throw;
}
```

### Change Tracking

```csharp
// Get changes for an entity
var changes = await _patientService.GetChanges(patientId, filters);

foreach (var change in changes)
{
    Console.WriteLine($"{change.PropertyName}: {change.OldValue} → {change.NewValue}");
}

// Check if entity has changes
bool hasChanges = await _patientService.IsEntityChanged(patientId, filters);
```

### Batch Operations

```csharp
// Add multiple entities
var patients = new List<Patient>
{
    new Patient { FirstName = "John", LastName = "Doe", MedicalRecordNumber = "MRN001" },
    new Patient { FirstName = "Jane", LastName = "Smith", MedicalRecordNumber = "MRN002" },
    new Patient { FirstName = "Bob", LastName = "Johnson", MedicalRecordNumber = "MRN003" }
};

await _patientService.AddMany(patients);

// Update multiple entities
await _patientService.UpdateMany(patients);

// Soft delete multiple entities
var ids = new List<int> { 1, 2, 3 };
await _patientService.SoftDeleteMany(ids, filters);
```

---

## 🏢 Multi-Tenancy

GenericToolKit includes built-in multi-tenancy support with automatic tenant isolation.

### Automatic Tenant Filtering

All entities inheriting from `BaseEntity` automatically include:

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }          // Multi-tenant isolation
    public int CreatedBy { get; set; }         // Audit: Creator
    public DateTime CreatedOn { get; set; }    // Audit: Creation time
    public int? UpdatedBy { get; set; }        // Audit: Last updater
    public DateTime? UpdatedOn { get; set; }   // Audit: Last update time
    public bool IsDeleted { get; set; }        // Soft delete flag
    public int? DeletedBy { get; set; }        // Audit: Who deleted
    public DateTime? DeletedOn { get; set; }   // Audit: When deleted
}
```

### Automatic Behaviors

1. **Tenant Isolation** - Queries automatically filtered by `TenantId`
2. **Soft Delete** - `IsDeleted = true` records excluded automatically
3. **Audit Tracking** - CreatedBy, UpdatedBy, etc. set automatically
4. **Query Filters** - Applied at DbContext level

### Cross-Tenant Queries (Admin Scenarios)

```csharp
var filters = new BaseFilters
{
    IgnoreTenantCheck = true,  // Bypass tenant filtering
    IncludeDeleted = true      // Include soft-deleted records
};

var allPatients = await _patientService.GetAll(filters);
```

### Implementing Multi-Tenancy

```csharp
public class LoggedInUser : ILoggedInUser
{
    private readonly IHttpContextAccessor _httpContext;

    public LoggedInUser(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;
    }

    public int TenantId => GetTenantFromClaims();
    public int LoginId => GetUserIdFromClaims();
    public int RoleId => GetRoleFromClaims();

    private int GetTenantFromClaims()
    {
        var claim = _httpContext.HttpContext?.User?.FindFirst("TenantId");
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}
```

---

## 📚 API Reference

### IGenericService<T> - Service Layer

The service layer provides high-level business operations:

#### CRUD Operations
- `Task<T> Add(T entity)` - Create new entity
- `Task<List<T>> AddMany(List<T> entities)` - Create multiple entities
- `Task<T> Update(T entity)` - Update existing entity
- `Task<List<T>> UpdateMany(List<T> entities)` - Update multiple entities

#### Query Operations
- `Task<List<T>> GetAll(BaseFilters filters)` - Get all entities with filters
- `Task<T> GetById(int id, BaseFilters filters)` - Get entity by ID
- `Task<List<T>> Find(Expression<Func<T, bool>> predicate, BaseFilters filters)` - Find with predicate
- `Task<T> FindOne(Expression<Func<T, bool>> predicate, BaseFilters filters)` - Find single entity
- `Task<bool> Any(Expression<Func<T, bool>> predicate, BaseFilters filters)` - Check if any exists
- `Task<int> Count(Expression<Func<T, bool>> predicate, BaseFilters filters)` - Count entities

#### Removal Operations
- `Task SoftDelete(int id, BaseFilters filters)` - Soft delete (mark as deleted)
- `Task SoftDeleteMany(List<int> ids, BaseFilters filters)` - Soft delete multiple
- `Task HardDelete(int id, BaseFilters filters)` - Hard delete (permanent)

#### Transaction Operations
- `Task BeginTransaction()` - Begin database transaction
- `Task Commit()` - Commit transaction
- `Task Rollback()` - Rollback transaction

#### Change Tracking
- `Task<List<TrackedEntityState>> GetChanges(int id, BaseFilters filters)` - Get entity changes
- `Task<bool> IsEntityChanged(int id, BaseFilters filters)` - Check if entity changed

#### Audit Operations
- `Task<Dictionary<string, object>> GetAuditProperties(T entity)` - Get audit properties
- `Task SetAuditProperties(T entity, Dictionary<string, object> auditData)` - Set audit properties

---

## 🔧 Advanced Features

### Specification Pattern

Use specifications to encapsulate complex query logic:

```csharp
using GenericToolKit.Abstractions.Interfaces;

public class ActivePatientsSpec : IBaseSpecification<Patient>
{
    public Expression<Func<Patient, bool>> Criteria =>
        p => !p.IsDeleted;

    public List<Expression<Func<Patient, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<Patient, object>> OrderBy { get; set; }
    public Expression<Func<Patient, object>> OrderByDescending { get; set; }
    public int Take { get; set; }
    public int Skip { get; set; }
    public bool IsPagingEnabled { get; set; }
}

// Usage
var spec = new ActivePatientsSpec();
var patients = await _repository.ListAsync(spec);
```

### Custom DbContext

```csharp
using GenericToolKit.Infrastructure.Data;

public class ApplicationDbContext : BaseContext
{
    public ApplicationDbContext(
        DbContextOptions options,
        ILoggedInUser loggedInUser
    ) : base(options, loggedInUser)
    {
    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Your custom configurations
        modelBuilder.Entity<Patient>()
            .HasIndex(p => p.MedicalRecordNumber)
            .IsUnique();
    }
}
```

### Projections and Custom Selectors

```csharp
var filters = new BaseFilters
{
    TenantId = 1,
    PageSize = 100
};

// Add custom selector for projection
filters.AddSelector<Patient, PatientDTO>(p => new PatientDTO
{
    Id = p.Id,
    FirstName = p.FirstName,
    LastName = p.LastName,
    MedicalRecordNumber = p.MedicalRecordNumber
});

var dtos = await _patientService.GetAll(filters);
```

### Recursive Entity Support

```csharp
public class Category : BaseEntity
{
    public string Name { get; set; }
    public int? ParentCategoryId { get; set; }
    public Category ParentCategory { get; set; }
    public List<Category> SubCategories { get; set; }
}

// Automatically handles nested entities
var category = new Category
{
    Name = "Electronics",
    SubCategories = new List<Category>
    {
        new Category { Name = "Laptops" },
        new Category { Name = "Phones" }
    }
};

await _categoryService.Add(category);  // Saves all nested categories
```

---

## 🛠️ Configuration

### Startup Configuration (ASP.NET Core)

```csharp
using GenericToolKit.Application.DependencyInjection;
using GenericToolKit.Infrastructure.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Configure DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
        );

        // Register user context
        services.AddHttpContextAccessor();
        services.AddScoped<ILoggedInUser, LoggedInUser>();

        // Register repositories and services for all entities
        services.AddGenericRepository<Patient>();
        services.AddGenericService<Patient>();

        services.AddGenericRepository<Appointment>();
        services.AddGenericService<Appointment>();

        services.AddGenericRepository<Doctor>();
        services.AddGenericService<Doctor>();
    }
}
```

### Connection String (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

---

## 🧪 Testing

GenericToolKit is designed to be testable. Use in-memory database for unit testing:

```csharp
using Microsoft.EntityFrameworkCore;
using GenericToolKit.Infrastructure.Repositories;

public class PatientServiceTests
{
    [Fact]
    public async Task Add_Patient_Should_Save_To_Database()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        var loggedInUser = new LoggedInUser { TenantId = 1, LoginId = 1 };
        var context = new ApplicationDbContext(options, loggedInUser);
        var repository = new GenericRepository<Patient>(context, loggedInUser);
        var service = new GenericService<Patient>(repository);

        var patient = new Patient { FirstName = "Test", LastName = "Patient", DateOfBirth = new DateTime(1990, 1, 1), MedicalRecordNumber = "TEST001" };

        // Act
        var result = await service.Add(patient);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Test", result.FirstName);
    }
}
```

---

## 📖 Documentation

- [Architecture Reference Guide](Generic-Tool-Code/ARCHITECTURE_REFERENCE_GUIDE.md) - Detailed architecture documentation
- [DDD Dependency Explanation](Generic-Tool-Code/DDD_DEPENDENCY_EXPLANATION.md) - Domain-Driven Design principles
- [Claude AI Guide](Generic-Tool-Code/CLAUDE.md) - Developer guidance for AI assistants

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Commit your changes** (`git commit -m 'Add amazing feature'`)
4. **Push to the branch** (`git push origin feature/amazing-feature`)
5. **Open a Pull Request**

### Development Setup

```bash
# Clone the repository
git clone https://github.com/yourusername/GenericToolKit.git
cd GenericToolKit

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run tests (if available)
dotnet test
```

### Coding Standards

- Follow C# coding conventions
- Maintain Clean Architecture principles
- Add XML documentation comments for public APIs
- Write unit tests for new features
- Keep backward compatibility

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2025 HMS Team

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

---

## 🙏 Acknowledgments

- Inspired by Clean Architecture principles by Robert C. Martin
- Domain-Driven Design patterns by Eric Evans
- Entity Framework Core team for excellent ORM framework
- SOLID principles and best practices from the .NET community

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/GenericToolKit/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/GenericToolKit/discussions)
- **Documentation**: [Wiki](https://github.com/yourusername/GenericToolKit/wiki)

---

## 🗺️ Roadmap

- [ ] Support for EF Core 8.0
- [ ] Additional specification implementations
- [ ] Performance optimization for large datasets
- [ ] GraphQL integration support
- [ ] Additional audit logging providers
- [ ] Real-time change notifications
- [ ] Advanced caching strategies
- [ ] MongoDB support

---

## ⭐ Star History

If you find this project useful, please consider giving it a star on GitHub!

[![Star History Chart](https://api.star-history.com/svg?repos=yourusername/GenericToolKit&type=Date)](https://star-history.com/#yourusername/GenericToolKit&Date)

---

**Made with ❤️ by HMS Team**
