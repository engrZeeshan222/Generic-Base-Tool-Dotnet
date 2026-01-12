using GenericToolKit.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Patient.Infra.Data;

/// <summary>
/// Design-time factory for creating PatientDbContext during migrations
/// This is used by EF Core tools when creating migrations
/// </summary>
public class PatientDbContextFactory : IDesignTimeDbContextFactory<PatientDbContext>
{
    public PatientDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PatientDbContext>();

        // Use a connection string for design-time (migrations)
        optionsBuilder.UseSqlServer(
            @"Server=.\SQLEXPRESS;Database=PatientMicroserviceDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");

        // Create a design-time ILoggedInUser implementation
        var designTimeUser = new DesignTimeLoggedInUser();

        return new PatientDbContext(optionsBuilder.Options, designTimeUser);
    }

    /// <summary>
    /// Design-time implementation of ILoggedInUser for migrations
    /// </summary>
    private class DesignTimeLoggedInUser : ILoggedInUser
    {
        public int TenantId { get; set; } = 1;
        public int LoginId { get; set; } = 0;
        public int RoleId { get; set; } = 0;
    }
}
