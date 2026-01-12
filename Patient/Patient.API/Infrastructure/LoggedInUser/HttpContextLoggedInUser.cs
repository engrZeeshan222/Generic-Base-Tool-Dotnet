using GenericToolKit.Domain.Interfaces;

namespace Patient.API.Infrastructure.LoggedInUser;

/// <summary>
/// Implementation of ILoggedInUser that retrieves user context from HTTP headers
/// Demonstrates multi-tenancy and user tracking from API requests
///
/// Usage:
/// - Client sends headers: X-Tenant-Id, X-User-Id, X-Role-Id
/// - Or use claims from JWT token in production
/// </summary>
public class HttpContextLoggedInUser : ILoggedInUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextLoggedInUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int TenantId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return 1; // Default tenant for system operations

            // Try to get from header
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
            {
                if (int.TryParse(tenantIdHeader, out var tenantId))
                {
                    return tenantId;
                }
            }

            // Try to get from claims (if using JWT authentication)
            var tenantClaim = context.User?.FindFirst("TenantId");
            if (tenantClaim != null && int.TryParse(tenantClaim.Value, out var tenantIdFromClaim))
            {
                return tenantIdFromClaim;
            }

            // Default to tenant 1 if not found
            return 1;
        }
        set { }
    }

    public int LoginId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return 0; // System user

            // Try to get from header
            if (context.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
            {
                if (int.TryParse(userIdHeader, out var userId))
                {
                    return userId;
                }
            }

            // Try to get from claims (if using JWT authentication)
            var userClaim = context.User?.FindFirst("UserId") ?? context.User?.FindFirst("sub");
            if (userClaim != null && int.TryParse(userClaim.Value, out var userIdFromClaim))
            {
                return userIdFromClaim;
            }

            // Default to user 0 (system) if not found
            return 0;
        }
        set { }
    }

    public int RoleId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return 0;

            // Try to get from header
            if (context.Request.Headers.TryGetValue("X-Role-Id", out var roleIdHeader))
            {
                if (int.TryParse(roleIdHeader, out var roleId))
                {
                    return roleId;
                }
            }

            // Try to get from claims (if using JWT authentication)
            var roleClaim = context.User?.FindFirst("RoleId") ?? context.User?.FindFirst("role");
            if (roleClaim != null && int.TryParse(roleClaim.Value, out var roleIdFromClaim))
            {
                return roleIdFromClaim;
            }

            return 0;
        }
        set { }
    }
}
