using System.Linq.Expressions;

namespace Patient.Domain.Specifications;

/// <summary>
/// Specification to find patients by age range
/// </summary>
public class PatientsByAgeRangeSpecification : BasePatientSpecification
{
    public PatientsByAgeRangeSpecification(int minAge, int maxAge)
    {
        var maxBirthDate = DateTime.Today.AddYears(-minAge);
        var minBirthDate = DateTime.Today.AddYears(-maxAge - 1);

        WhereExpression = p => p.DateOfBirth >= minBirthDate && p.DateOfBirth <= maxBirthDate;
        AddOrderBy(query => query.OrderBy(p => p.DateOfBirth));
    }
}
