namespace Patient.Domain.ValueObjects;

/// <summary>
/// Address value object (owned entity in EF Core)
/// </summary>
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Full address as a single string
    /// </summary>
    public string FullAddress => $"{Street}, {City}, {State} {ZipCode}, {Country}";

    /// <summary>
    /// Check if address is valid
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Street) &&
               !string.IsNullOrWhiteSpace(City) &&
               !string.IsNullOrWhiteSpace(Country);
    }
}
