# GenericToolKit - Architecture Reference Guide

## Clean Architecture Dependency Rules

### Dependency Flow Principle
**Dependencies should point INWARD** - Outer layers can depend on inner layers, but inner layers should NEVER depend on outer layers.

```
┌─────────────────────────────────────────────────────────────┐
│                    Your Project Layers                        │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐         ┌──────────────────┐         │
│  │   Application    │─────────▶│    Domain        │         │
│  │   (Business      │         │  (Entities,       │         │
│  │    Logic)        │         │   Interfaces)    │         │
│  └──────────────────┘         └──────────────────┘         │
│         │                              ▲                     │
│         │                              │                     │
│         ▼                              │                     │
│  ┌──────────────────┐                 │                     │
│  │ Infrastructure   │─────────────────┘                     │
│  │ (Data Access,    │                                         │
│  │  Repositories)   │                                         │
│  └──────────────────┘                                         │
└─────────────────────────────────────────────────────────────┘
```

## Reference Rules for Medication Project

### 1. **Medication.Application** (Application Layer)
**Can Reference:**
- ✅ `GenericToolKit.Application` - For service interfaces and implementations
- ✅ `GenericToolKit.Domain` - For entities, interfaces, models (if needed)

**Should NOT Reference:**
- ❌ `GenericToolKit.Infrastructure` - Application should not depend on Infrastructure

**Example:**
```csharp
// Medication.Application can use:
using GenericToolKit.Application.Services;  // ✅ OK
using GenericToolKit.Domain.Entities;        // ✅ OK (if needed)
using GenericToolKit.Domain.Interfaces;     // ✅ OK (if needed)
```

### 2. **Medication.Infrastructure** (Infrastructure Layer)
**Can Reference:**
- ✅ `GenericToolKit.Infrastructure` - For repository implementations
- ✅ `GenericToolKit.Domain` - For entities, interfaces, models

**Should NOT Reference:**
- ❌ `GenericToolKit.Application` - Infrastructure should not depend on Application

**Example:**
```csharp
// Medication.Infrastructure can use:
using GenericToolKit.Infrastructure.Repositories;  // ✅ OK
using GenericToolKit.Domain.Entities;              // ✅ OK
using GenericToolKit.Domain.Interfaces;           // ✅ OK
```

### 3. **Medication.Domain** (Domain Layer)
**Can Reference:**
- ✅ `GenericToolKit.Domain` - For base entities, interfaces, models

**Should NOT Reference:**
- ❌ `GenericToolKit.Application` - Domain should never depend on Application
- ❌ `GenericToolKit.Infrastructure` - Domain should never depend on Infrastructure

**Example:**
```csharp
// Medication.Domain can use:
using GenericToolKit.Domain.Entities;      // ✅ OK
using GenericToolKit.Domain.Interfaces;   // ✅ OK
using GenericToolKit.Domain.Models;      // ✅ OK
```

## Visual Dependency Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    GenericToolKit                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐       │
│  │  Application  │───▶│   Domain    │◀───│Infrastructure│       │
│  │   (Services)  │    │ (Entities,  │    │(Repositories)│       │
│  │               │    │ Interfaces) │    │              │       │
│  └──────────────┘    └──────────────┘    └──────────────┘       │
│       ▲                    ▲                    ▲                │
│       │                    │                    │                │
│       │                    │                    │                │
└───────┼────────────────────┼────────────────────┼────────────────┘
        │                    │                    │
        │                    │                    │
┌───────┼────────────────────┼────────────────────┼────────────────┐
│       │                    │                    │                │
│  ┌────┴────┐          ┌────┴────┐          ┌────┴────┐          │
│  │Medication│         │Medication│         │Medication│         │
│  │App      │          │Domain   │          │Infra    │          │
│  └─────────┘          └─────────┘          └─────────┘          │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘
```

## Summary Table

| Your Layer | Can Reference GenericToolKit | Cannot Reference GenericToolKit |
|------------|------------------------------|--------------------------------|
| **Application** | ✅ Application<br>✅ Domain | ❌ Infrastructure |
| **Infrastructure** | ✅ Infrastructure<br>✅ Domain | ❌ Application |
| **Domain** | ✅ Domain | ❌ Application<br>❌ Infrastructure |

## Why These Rules?

### 1. **Separation of Concerns**
Each layer has a specific responsibility:
- **Domain**: Core business entities and contracts
- **Application**: Business logic and orchestration
- **Infrastructure**: Technical implementation details

### 2. **Dependency Inversion Principle (SOLID)**
- High-level modules (Application) should not depend on low-level modules (Infrastructure)
- Both should depend on abstractions (Domain)

### 3. **Testability**
- Domain can be tested independently
- Application can be tested with mock repositories
- Infrastructure can be tested with mock contexts

### 4. **Maintainability**
- Changes in Infrastructure don't affect Domain
- Changes in Application don't affect Domain
- Domain remains stable and reusable

## Real-World Example

### ✅ CORRECT Usage:

**Medication.Domain/Entities/MedicationRecord.cs:**
```csharp
using GenericToolKit.Domain.Entities;  // ✅ OK - Domain → Domain

public class MedicationRecord : BaseEntity  // Inherits from GenericToolKit
{
    public string MedicationName { get; set; }
}
```

**Medication.Application/Services/MedicationService.cs:**
```csharp
using GenericToolKit.Application.Services;  // ✅ OK - Application → Application
using GenericToolKit.Domain.Entities;        // ✅ OK - Application → Domain

public class MedicationService : IGenericService<MedicationRecord>
{
    private readonly IGenericService<MedicationRecord> _service;
    // Use GenericToolKit.Application services
}
```

**Medication.Infrastructure/Repositories/MedicationRepository.cs:**
```csharp
using GenericToolKit.Infrastructure.Repositories;  // ✅ OK - Infrastructure → Infrastructure
using GenericToolKit.Domain.Interfaces;             // ✅ OK - Infrastructure → Domain

public class MedicationRepository : GenericRepository<MedicationRecord>
{
    // Use GenericToolKit.Infrastructure repositories
}
```

### ❌ INCORRECT Usage:

**Medication.Domain/Entities/MedicationRecord.cs:**
```csharp
using GenericToolKit.Application.Services;  // ❌ WRONG - Domain should not depend on Application
```

**Medication.Application/Services/MedicationService.cs:**
```csharp
using GenericToolKit.Infrastructure.Repositories;  // ❌ WRONG - Application should not depend on Infrastructure
```

**Medication.Infrastructure/Repositories/MedicationRepository.cs:**
```csharp
using GenericToolKit.Application.Services;  // ❌ WRONG - Infrastructure should not depend on Application
```

## Key Takeaways

1. **Domain → Domain Only**: Your domain layer should only reference GenericToolKit.Domain
2. **Application → Application + Domain**: Your application layer can reference GenericToolKit.Application and Domain
3. **Infrastructure → Infrastructure + Domain**: Your infrastructure layer can reference GenericToolKit.Infrastructure and Domain
4. **Never Cross Boundaries**: Application should never reference Infrastructure, and vice versa
5. **Domain is King**: Domain is the center - everything depends on it, but it depends on nothing

## Dependency Injection Setup

When setting up DI, you'll typically do this in your startup/program:

```csharp
// Register GenericToolKit services
services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));

// Register your Medication services
services.AddScoped<IMedicationService, MedicationService>();
services.AddScoped<IMedicationRepository, MedicationRepository>();
```

This way, your Application layer uses the interfaces from GenericToolKit, and the Infrastructure layer provides the implementations.

