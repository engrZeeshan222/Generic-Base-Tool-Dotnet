using GenericToolKit.Domain.Entities;

namespace Patient.Domain.Entities;

/// <summary>
/// Appointment entity - Demonstrates relationship with Patient entity
/// Also inherits from BaseEntity to show multi-entity repository usage
/// </summary>
public class Appointment : BaseEntity
{
    /// <summary>
    /// Foreign key to Patient
    /// </summary>
    public int PatientId { get; set; }

    /// <summary>
    /// Navigation property to Patient
    /// </summary>
    public virtual Patient Patient { get; set; } = null!;

    /// <summary>
    /// Appointment date and time
    /// </summary>
    public DateTime AppointmentDateTime { get; set; }

    /// <summary>
    /// Appointment reason
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Doctor name
    /// </summary>
    public string DoctorName { get; set; } = string.Empty;

    /// <summary>
    /// Appointment status
    /// </summary>
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    /// <summary>
    /// Notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Domain method: Cancel appointment
    /// </summary>
    public void Cancel()
    {
        if (Status == AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel a completed appointment.");
        }
        Status = AppointmentStatus.Cancelled;
    }

    /// <summary>
    /// Domain method: Complete appointment
    /// </summary>
    public void Complete()
    {
        if (Status == AppointmentStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot complete a cancelled appointment.");
        }
        Status = AppointmentStatus.Completed;
    }

    /// <summary>
    /// Domain validation
    /// </summary>
    public bool IsValid()
    {
        return PatientId > 0 &&
               AppointmentDateTime > DateTime.Now &&
               !string.IsNullOrWhiteSpace(Reason) &&
               !string.IsNullOrWhiteSpace(DoctorName);
    }
}

public enum AppointmentStatus
{
    Scheduled = 1,
    Completed = 2,
    Cancelled = 3,
    NoShow = 4
}
