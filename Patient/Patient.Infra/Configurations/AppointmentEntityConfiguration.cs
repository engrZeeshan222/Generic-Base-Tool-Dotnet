using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Patient.Domain.Entities;

namespace Patient.Infra.Data;

/// <summary>
/// EF Core configuration for Appointment entity
/// </summary>
public class AppointmentEntityConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        // Table name
        builder.ToTable("Appointments");

        // Primary key (inherited from BaseEntity)
        builder.HasKey(a => a.Id);

        // Properties
        builder.Property(a => a.PatientId)
            .IsRequired();

        builder.Property(a => a.AppointmentDateTime)
            .IsRequired();

        builder.Property(a => a.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.DoctorName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Status)
            .IsRequired();

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        // Relationship (other side configured in PatientEntityConfiguration)
        builder.HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId);

        // Indexes
        builder.HasIndex(a => a.PatientId)
            .HasDatabaseName("IX_Appointments_PatientId");

        builder.HasIndex(a => a.AppointmentDateTime)
            .HasDatabaseName("IX_Appointments_AppointmentDateTime");

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("IX_Appointments_Status");

        // Composite index for common queries
        builder.HasIndex(a => new { a.PatientId, a.AppointmentDateTime })
            .HasDatabaseName("IX_Appointments_PatientId_DateTime");
    }
}
