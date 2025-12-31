namespace GenericToolKit.Domain.Entities
{
    /// <summary>
    /// Base entity class that provides common audit properties and tenant isolation.
    /// All domain entities should inherit from this class to enable multi-tenancy and audit tracking.
    /// Following DDD principles, this represents the base aggregate root.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the identifier of the user who created this entity.
        /// </summary>
        public int? CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when this entity was created.
        /// </summary>
        public DateTime? CreatedOn { get; set; }
        
        /// <summary>
        /// Gets or sets the identifier of the user who last updated this entity.
        /// </summary>
        public int? UpdatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when this entity was last updated.
        /// </summary>
        public DateTime? UpdatedOn { get; set; }
        
        /// <summary>
        /// Gets or sets the tenant identifier for multi-tenant isolation.
        /// This property enables data isolation between different tenants in a multi-tenant application.
        /// </summary>
        public int? TenantId { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this entity has been soft deleted.
        /// </summary>
        public bool? IsDeleted { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when this entity was soft deleted.
        /// </summary>
        public DateTime? DeletedOn { get; set; }
        
        /// <summary>
        /// Gets or sets the identifier of the user who soft deleted this entity.
        /// </summary>
        public int? DeletedBy { get; set; }

        /// <summary>
        /// Marks the entity as soft deleted and sets the deletion audit properties.
        /// </summary>
        /// <param name="loginId">The identifier of the user performing the deletion.</param>
        public void SetDeletedProperties(int loginId)
        {
            IsDeleted = true;
            DeletedOn = DateTime.Now;
            DeletedBy = loginId;
        }
        
        /// <summary>
        /// Restores a soft-deleted entity by clearing the deletion properties.
        /// </summary>
        public void SetDeletedPropertiesToNull()
        {
            IsDeleted = false;
            DeletedOn = null;
            DeletedBy = null;
        }
    }
}
