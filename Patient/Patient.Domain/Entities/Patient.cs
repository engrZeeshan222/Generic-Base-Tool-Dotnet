using GenericToolKit.Domain.Entities;
using Patient.Domain.Enums;
using Patient.Domain.ValueObjects;

namespace Patient.Domain.Entities;

/// <summary>
/// Patient entity representing a hospital patient
/// Inherits from BaseEntity to get audit fields, soft delete, and multi-tenancy support
/// </summary>
public class Patient : BaseEntity
{
    /// <summary>
    /// Medical Record Number - Unique identifier for the patient
    /// </summary>
    public string MRN { get; set; } = string.Empty;

    /// <summary>
    /// Patient code - Alternative unique identifier
    /// </summary>
    public string PatientCode { get; set; } = string.Empty;

    /// <summary>
    /// Patient's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Patient's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Patient's gender
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// Contact phone number
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Physical address
    /// </summary>
    public Address Address { get; set; } = new Address();

    /// <summary>
    /// Emergency contact information
    /// </summary>
    public EmergencyContact EmergencyContact { get; set; } = new EmergencyContact();

    /// <summary>
    /// Indicates if the patient record is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Blood type
    /// </summary>
    public string? BloodType { get; set; }

    /// <summary>
    /// Known allergies
    /// </summary>
    public string? Allergies { get; set; }

    /// <summary>
    /// Medical notes
    /// </summary>
    public string? MedicalNotes { get; set; }

    // Navigation properties for related entities (for future expansion)
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    /// <summary>
    /// Computed property: Full name
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Computed property: Age
    /// </summary>
    public int Age
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    /// <summary>
    /// Domain method: Activate patient
    /// </summary>
    public void Activate()
    {
        if (IsDeleted == true)
        {
            throw new InvalidOperationException("Cannot activate a deleted patient. Please restore first.");
        }
        IsActive = true;
    }

    /// <summary>
    /// Domain method: Deactivate patient
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Domain validation: Validate MRN uniqueness (to be checked in application layer)
    /// </summary>
    public bool IsValidMRN()
    {
        return !string.IsNullOrWhiteSpace(MRN) && MRN.Length >= 5;
    }

    /// <summary>
    /// Domain validation: Validate patient age is valid
    /// </summary>
    public bool IsValidAge()
    {
        return DateOfBirth < DateTime.Today && Age >= 0 && Age < 150;
    }

    /// <summary>
    /// Domain validation: Validate basic patient information
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(FirstName) &&
               !string.IsNullOrWhiteSpace(LastName) &&
               IsValidMRN() &&
               IsValidAge();
    }
}
