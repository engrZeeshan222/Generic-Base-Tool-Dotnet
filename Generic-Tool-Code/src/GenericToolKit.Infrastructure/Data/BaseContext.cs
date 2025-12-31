using GenericToolKit.Domain.Entities;
using GenericToolKit.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace GenericToolKit.Infrastructure.Data
{
    /// <summary>
    /// Base DbContext class that provides common functionality for consumer microservices.
    /// Consumer microservices should inherit from this class to get automatic tenant filtering,
    /// soft delete filtering, and audit property management.
    /// Following DDD principles and Clean Architecture, this provides a foundation for data access.
    /// </summary>
    public abstract class BaseContext : DbContext
    {
        private ILoggedInUser? _loggedInUser;

        /// <summary>
        /// Initializes a new instance of the BaseContext class.
        /// </summary>
        /// <param name="options">The options to be used by a DbContext.</param>
        /// <param name="loggedInUser">Optional system user context for tenant isolation and audit tracking.</param>
        protected BaseContext(DbContextOptions options, ILoggedInUser? loggedInUser = null) 
            : base(options)
        {
            _loggedInUser = loggedInUser;
        }

        /// <summary>
        /// Gets or sets the system user context for tenant isolation and audit tracking.
        /// This can be set after construction if needed (e.g., from DI container).
        /// </summary>
        public ILoggedInUser? loggedInUser
        {
            get => _loggedInUser;
            set => _loggedInUser = value;
        }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types
        /// exposed in DbSet properties on your derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure all entities that inherit from BaseEntity
            ConfigureBaseEntityProperties(modelBuilder);
        }

        /// <summary>
        /// Configures common properties for all entities that inherit from BaseEntity.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        private void ConfigureBaseEntityProperties(ModelBuilder modelBuilder)
        {
            // Get all entity types that inherit from BaseEntity
            var entityTypes = modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType));

            foreach (var entityType in entityTypes)
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                Expression? filterExpression = null;

                // Configure soft delete query filter
                var isDeletedProperty = entityType.FindProperty(nameof(BaseEntity.IsDeleted));
                if (isDeletedProperty != null)
                {
                    var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                    var condition = Expression.Equal(property, Expression.Constant(false));
                    filterExpression = condition;
                }

                // Configure tenant isolation query filter if system user is available
                if (_loggedInUser != null)
                {
                    var tenantIdProperty = entityType.FindProperty(nameof(BaseEntity.TenantId));
                    if (tenantIdProperty != null)
                    {
                        var property = Expression.Property(parameter, nameof(BaseEntity.TenantId));
                        var condition = Expression.Equal(property, Expression.Constant(_loggedInUser.TenantId));
                        
                        // Combine with soft delete filter if it exists
                        if (filterExpression != null)
                        {
                            filterExpression = Expression.AndAlso(filterExpression, condition);
                        }
                        else
                        {
                            filterExpression = condition;
                        }
                    }
                }

                // Apply the combined filter if we have one
                if (filterExpression != null)
                {
                    var lambda = Expression.Lambda(filterExpression, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        /// <summary>
        /// Overrides SaveChangesAsync to automatically set audit properties before saving.
        /// </summary>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditProperties();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Overrides SaveChanges to automatically set audit properties before saving.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        public override int SaveChanges()
        {
            SetAuditProperties();
            return base.SaveChanges();
        }

        /// <summary>
        /// Sets audit properties (CreatedBy, CreatedOn, UpdatedBy, UpdatedOn, TenantId) 
        /// for all entities that inherit from BaseEntity.
        /// </summary>
        private void SetAuditProperties()
        {
            if (_loggedInUser == null)
                return;

            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && 
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            var timestamp = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedBy = _loggedInUser.LoginId;
                    entity.CreatedOn = timestamp;
                }

                entity.UpdatedBy = _loggedInUser.LoginId;
                entity.UpdatedOn = timestamp;
                entity.TenantId = _loggedInUser.TenantId;
                entity.IsDeleted = entity.IsDeleted ?? false;
            }
        }
    }
}

