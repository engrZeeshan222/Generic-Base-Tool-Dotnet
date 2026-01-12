using System.Linq.Expressions;
using GenericToolKit.Domain.Interfaces;
using GenericToolKit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Patient.Domain.Entities;

namespace Patient.Infra.Data;

/// <summary>
/// Patient microservice database context
/// Inherits from BaseContext to get:
/// - Automatic tenant filtering (TenantId)
/// - Automatic soft-delete filtering (IsDeleted)
/// - Automatic audit property setting (CreatedBy, UpdatedBy, etc.)
/// </summary>
public class PatientDbContext : BaseContext
{
    private readonly ILoggedInUser _loggedInUser;

    public PatientDbContext(
        DbContextOptions<PatientDbContext> options,
        ILoggedInUser loggedInUser)
        : base(options, loggedInUser)
    {
        _loggedInUser = loggedInUser;
    }

    // DbSets for all entities
    public DbSet<Domain.Entities.Patient> Patients { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // NOTE: Not calling base.OnModelCreating due to bug in BaseContext with nullable bool comparison
        // Instead, we'll manually apply the query filters with proper nullable handling

        // Apply entity configurations first
        modelBuilder.ApplyConfiguration(new PatientEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AppointmentEntityConfiguration());

        // Manually apply global query filters for all BaseEntity types
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Check if entity inherits from BaseEntity
            if (typeof(GenericToolKit.Domain.Entities.BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");

                // Soft Delete Filter: IsDeleted != true (handles null as not deleted)
                var isDeletedProperty = Expression.Property(parameter, "IsDeleted");
                var isDeletedFilter = Expression.NotEqual(
                    isDeletedProperty,
                    Expression.Constant(true, typeof(bool?))
                );

                // Tenant Filter: TenantId == currentUser.TenantId (only if TenantId > 0)
                var tenantIdProperty = Expression.Property(parameter, "TenantId");
                var currentTenantId = Expression.Constant(_loggedInUser.TenantId, typeof(int?));

                Expression combinedFilter;

                // Only apply tenant filter if TenantId > 0
                if (_loggedInUser.TenantId > 0)
                {
                    var tenantFilter = Expression.Equal(tenantIdProperty, currentTenantId);
                    combinedFilter = Expression.AndAlso(isDeletedFilter, tenantFilter);
                }
                else
                {
                    combinedFilter = isDeletedFilter;
                }

                var lambda = Expression.Lambda(combinedFilter, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
