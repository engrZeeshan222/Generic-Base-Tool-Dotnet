using System.Linq.Expressions;

namespace Patient.Domain.Specifications;

/// <summary>
/// Specification to get all active patients
/// Demonstrates the Specification pattern from Generic Toolkit
/// </summary>
public class ActivePatientsSpecification : BasePatientSpecification
{
    public ActivePatientsSpecification()
    {
        WhereExpression = p => p.IsActive == true;
        AddOrderBy(query => query.OrderBy(p => p.LastName).ThenBy(p => p.FirstName));
    }
}
