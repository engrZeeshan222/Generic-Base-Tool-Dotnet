namespace Patient.Domain.ValueObjects;

/// <summary>
/// Emergency contact value object (owned entity in EF Core)
/// </summary>
public class EmergencyContact
{
    public string Name { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Check if emergency contact is valid
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(Phone);
    }
}
