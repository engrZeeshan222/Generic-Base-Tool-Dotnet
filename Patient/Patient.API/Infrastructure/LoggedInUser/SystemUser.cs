using GenericToolKit.Domain.Interfaces;

namespace Patient.API.Infrastructure.LoggedInUser;

/// <summary>
/// System user implementation for background jobs or system operations
/// Demonstrates how to handle operations not initiated by a user
/// </summary>
public class SystemUser : ILoggedInUser
{
    public int TenantId { get; set; } = 1; // System tenant
    public int LoginId { get; set; } = 0; // System user ID
    public int RoleId { get; set; } = 999; // System role
}
