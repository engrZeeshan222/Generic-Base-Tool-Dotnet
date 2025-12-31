using System.Linq.Expressions;

namespace GenericToolKit.Domain.Models
{
    /// <summary>
    /// Base filter class for query operations with pagination, sorting, and tenant isolation support.
    /// This class follows the Single Responsibility Principle by focusing solely on query filtering.
    /// </summary>
    public class BaseFilters
    {
        /// <summary>
        /// Gets or sets the entity identifier filter.
        /// </summary>
        public int Id { get; set; } = 0;
        
        /// <summary>
        /// Gets or sets the tenant identifier for multi-tenant filtering.
        /// When set, queries will be automatically filtered to the specified tenant.
        /// </summary>
        public int TenantId { get; set; }
        
        /// <summary>
        /// Gets or sets the creator identifier filter.
        /// </summary>
        public int CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the updater identifier filter.
        /// </summary>
        public int UpdatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the deleter identifier filter.
        /// </summary>
        public int DeleteBy { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to use AsNoTracking for the query.
        /// When true, entities returned will not be tracked by the DbContext.
        /// </summary>
        public bool IsAsNoTracking { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore the active (non-deleted) check.
        /// When true, soft-deleted entities will be included in the results.
        /// </summary>
        public bool IgnoreActiveCheck { get; set; } = false;
        
        /// <summary>
        /// Gets or sets a value indicating whether to ignore tenant isolation checks.
        /// When true, queries will return data across all tenants (use with caution).
        /// </summary>
        public bool IgnoreTenantCheck { get; set; } = false;

        // Pagination options
        /// <summary>
        /// Gets or sets a value indicating whether to apply pagination.
        /// </summary>
        public bool ApplyPagination { get; set; } = false;
        
        private int skip;
        private int take;

        /// <summary>
        /// Gets or sets the number of records to take (page size).
        /// Defaults to 20 if not set.
        /// </summary>
        public int? Take
        {
            get
            {
                return take == 0 ? 20 : take;
            }
            set
            {
                take = value ?? 0;
            }
        }

        /// <summary>
        /// Gets or sets the number of records to skip (for pagination).
        /// Defaults to 0 if not set.
        /// </summary>
        public int? Skip
        {
            get
            {
                return skip == 0 ? 0 : skip;
            }
            set
            {
                skip = value ?? 0;
            }
        }

        // Sorting options
        /// <summary>
        /// Gets or sets the property name to sort by.
        /// </summary>
        public string ApplySorting { get; set; }
        
        /// <summary>
        /// Gets or sets the list of order expressions for complex sorting.
        /// </summary>
        public List<OrderExpression> OrderExpressions { get; set; } = new List<OrderExpression>();
        
        /// <summary>
        /// Gets or sets a value indicating whether to ignore auto-includes.
        /// </summary>
        public bool IsIgnoreAutoIncludes { get; set; } = false;
        
        /// <summary>
        /// Gets or sets a value indicating whether to include soft-deleted entities.
        /// </summary>
        public bool IncludeSoftDeletedEntitiesAlso { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the start date filter for CreatedOn property.
        /// </summary>
        public DateTime? StartDate { get; set; } = null;
        
        /// <summary>
        /// Gets or sets the end date filter for CreatedOn property.
        /// </summary>
        public DateTime? EndDate { get; set; } = null;
    }

    /// <summary>
    /// Represents an order expression for query sorting.
    /// </summary>
    public class OrderExpression
    {
        /// <summary>
        /// Gets or sets the type of ordering to apply.
        /// </summary>
        public OrderTypeEnum OrderType { get; set; }
        
        /// <summary>
        /// Gets or sets the expression selector for the property to order by.
        /// </summary>
        public Expression<Func<IQueryable, object>> Selector { get; set; }
    }
    
    /// <summary>
    /// Enumeration of order types for query sorting.
    /// </summary>
    public enum OrderTypeEnum
    {
        OrderBy = 1,
        OrderByDescending = 2,
        ThenBy = 3,
        ThenByDescending = 4
    }
}
