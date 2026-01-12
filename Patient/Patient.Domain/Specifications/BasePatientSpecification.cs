using System.Linq.Expressions;
using GenericToolKit.Domain.Interfaces;

namespace Patient.Domain.Specifications;

/// <summary>
/// Base specification for Patient queries
/// Implements IBaseSpecification from Generic Toolkit
/// </summary>
public abstract class BasePatientSpecification : IBaseSpecification<Entities.Patient>
{
    protected BasePatientSpecification()
    {
        Includes = new List<Expression<Func<Entities.Patient, object>>>();
        IncludeStrings = new List<string>();
    }

    public Expression<Func<Entities.Patient, bool>> WhereExpression { get; protected set; } = null!;

    public List<Expression<Func<Entities.Patient, object>>> Includes { get; protected set; }

    public List<string> IncludeStrings { get; protected set; }

    public Func<IQueryable<Entities.Patient>, IOrderedQueryable<Entities.Patient>> OrderByDelegate { get; protected set; } = null!;

    public bool IsAsNoTracking { get; protected set; } = true;

    /// <summary>
    /// Add include for related entities
    /// </summary>
    protected void AddInclude(Expression<Func<Entities.Patient, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Add include by string
    /// </summary>
    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Set ordering
    /// </summary>
    protected void AddOrderBy(Func<IQueryable<Entities.Patient>, IOrderedQueryable<Entities.Patient>> orderByDelegate)
    {
        OrderByDelegate = orderByDelegate;
    }

    /// <summary>
    /// Set tracking behavior
    /// </summary>
    protected void SetTracking(bool isTracking)
    {
        IsAsNoTracking = !isTracking;
    }
}
