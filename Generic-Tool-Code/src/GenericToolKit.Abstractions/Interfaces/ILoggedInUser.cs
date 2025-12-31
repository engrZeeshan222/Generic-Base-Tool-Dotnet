namespace GenericToolKit.Domain.Interfaces
{
    /// <summary>
    /// Represents the current system user context for multi-tenant applications.
    /// This interface should be implemented to provide tenant isolation and audit information.
    /// Following the Dependency Inversion Principle (SOLID), this interface defines the contract
    /// for user context without depending on concrete implementations.
    /// </summary>
    public interface ILoggedInUser
    {
        /// <summary>
        /// Gets or sets the tenant identifier for multi-tenant isolation.
        /// This property is used to ensure data isolation between different tenants.
        /// </summary>
        int TenantId { get; set; }
        
        /// <summary>
        /// Gets or sets the current user's login identifier.
        /// Used for audit tracking (CreatedBy, UpdatedBy, DeletedBy).
        /// </summary>
        int LoginId { get; set; }
        
        /// <summary>
        /// Gets or sets the current user's role identifier.
        /// Used for authorization and permission checks.
        /// </summary>
        int RoleId { get; set; }
    }
}


