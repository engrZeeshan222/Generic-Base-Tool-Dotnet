using GenericToolKit.Domain.Entities;
using System.Linq.Expressions;

namespace GenericToolKit.Domain.Interfaces
{
    /// <summary>
    /// Base specification interface following the Specification Pattern (DDD).
    /// This interface enables building complex queries using a fluent, composable API.
    /// Following SOLID principles, this interface segregates query concerns.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface IBaseSpecification<T> where T : BaseEntity
    {
        /// <summary>
        /// Gets the where expression for filtering entities.
        /// </summary>
        Expression<Func<T, bool>> WhereExpression { get; }
        
        /// <summary>
        /// Gets the list of include expressions for eager loading related entities.
        /// </summary>
        List<Expression<Func<T, object>>> Includes { get; }
        
        /// <summary>
        /// Gets the list of include strings for eager loading related entities by string path.
        /// </summary>
        List<string> IncludeStrings { get; }

        /// <summary>
        /// Gets the order by delegate for sorting the query results.
        /// </summary>
        Func<IQueryable<T>, IOrderedQueryable<T>> OrderByDelegate { get; }
        
        /// <summary>
        /// Gets a value indicating whether to use AsNoTracking for the query.
        /// </summary>
        bool IsAsNoTracking { get; }
    }
}


