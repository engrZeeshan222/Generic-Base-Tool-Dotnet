using System.Linq.Expressions;

namespace Patient.Domain.Specifications;

/// <summary>
/// Specification to find patient by MRN
/// </summary>
public class PatientByMRNSpecification : BasePatientSpecification
{
    public PatientByMRNSpecification(string mrn, bool includeAppointments = false)
    {
        WhereExpression = p => p.MRN == mrn;

        if (includeAppointments)
        {
            AddInclude(p => p.Appointments);
        }
    }
}
