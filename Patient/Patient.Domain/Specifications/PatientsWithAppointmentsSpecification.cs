using System.Linq.Expressions;

namespace Patient.Domain.Specifications;

/// <summary>
/// Specification to get patients with their appointments
/// Demonstrates eager loading via specifications
/// </summary>
public class PatientsWithAppointmentsSpecification : BasePatientSpecification
{
    public PatientsWithAppointmentsSpecification(DateTime? fromDate = null)
    {
        if (fromDate.HasValue)
        {
            WhereExpression = p => p.Appointments.Any(a => a.AppointmentDateTime >= fromDate.Value);
        }
        else
        {
            WhereExpression = p => p.Appointments.Any();
        }

        AddInclude(p => p.Appointments);
        AddOrderBy(query => query.OrderByDescending(p => p.CreatedOn));
    }
}
