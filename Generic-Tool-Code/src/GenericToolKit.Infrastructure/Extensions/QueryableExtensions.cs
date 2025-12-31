using GenericToolKit.Domain.Entities;
using GenericToolKit.Domain.Extensions;
using GenericToolKit.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace GenericToolKit.Domain.Extensions
{
    /// <summary>
    /// Extension methods for IQueryable to apply filtering, pagination, and sorting.
    /// EF Core-specific behaviors (e.g., AsNoTracking / EF.Property) live in Infrastructure.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Applies query filters including tenant isolation, pagination, sorting, and soft-delete filtering.
        /// </summary>
        /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
        /// <param name="sourceQuery">The source queryable to apply filters to.</param>
        /// <param name="filters">The filter options to apply.</param>
        /// <returns>The filtered queryable.</returns>
        public static IQueryable<T> ApplyQueryFilters<T>(this IQueryable<T> sourceQuery, BaseFilters filters) where T : BaseEntity
        {
            try
            {
                if (!filters.IsValidObject())
                    return sourceQuery;

                sourceQuery = ApplyDefaultFilters(sourceQuery);
                
                // Apply tenant isolation filter
                if (!filters.IgnoreTenantCheck && filters.TenantId > 0)
                {
                    sourceQuery = sourceQuery.Where(x => x.TenantId == filters.TenantId);
                }
                
                // Apply creator filter
                if (filters.CreatedBy > 0)
                {
                    sourceQuery = sourceQuery.Where(x => x.CreatedBy == filters.CreatedBy);
                }

                // Apply updater filter
                if (filters.UpdatedBy > 0)
                {
                    sourceQuery = sourceQuery.Where(x => x.UpdatedBy == filters.UpdatedBy);
                }
                
                // Apply deleter filter
                if (filters.DeleteBy > 0)
                {
                    sourceQuery = sourceQuery.Where(x => x.DeletedBy == filters.DeleteBy);
                }

                // Apply no-tracking if requested
                if (filters.IsAsNoTracking)
                {
                    sourceQuery = sourceQuery.AsNoTracking();
                }
                
                // Apply active check filter (exclude soft-deleted)
                if (filters.IgnoreActiveCheck)
                {
                    sourceQuery = sourceQuery.Where(x => (x.IsDeleted == true));
                }
                
                // Apply pagination
                if(filters.ApplyPagination)
                    sourceQuery = sourceQuery.Skip(filters.Skip.GetValueOrDefault()).Take(filters.Take.GetValueOrDefault());
                
                // Apply sorting
                if (!string.IsNullOrWhiteSpace(filters.ApplySorting) || filters.OrderExpressions.Count != 0)
                {
                    // Complex sorting
                    if (filters.OrderExpressions.Count != 0)
                    {
                        IOrderedQueryable<T> orderedQuery = null;
                        foreach (var expression in filters.OrderExpressions)
                        {
                            if(expression.OrderType == OrderTypeEnum.OrderBy)
                            {
                                orderedQuery = Queryable.OrderBy((IQueryable<T>)sourceQuery, (dynamic)expression.Selector);
                            }else if(expression.OrderType == OrderTypeEnum.OrderByDescending)
                            {
                                orderedQuery = Queryable.OrderByDescending((IQueryable<T>)sourceQuery, (dynamic)expression.Selector);
                            }else if(expression.OrderType == OrderTypeEnum.ThenBy)
                            {
                                orderedQuery = Queryable.ThenBy((IOrderedQueryable<T>)sourceQuery, (dynamic)expression.Selector);
                            }else  if(expression.OrderType == OrderTypeEnum.ThenByDescending)
                            {
                                orderedQuery = Queryable.ThenByDescending((IOrderedQueryable<T>)sourceQuery, (dynamic)expression.Selector);
                            }
                        }
                        if (orderedQuery != null)
                            sourceQuery = orderedQuery;
                    }
                    // Simple sorting by property name (ascending)
                    else if (!string.IsNullOrWhiteSpace(filters.ApplySorting))
                    {
                        sourceQuery = sourceQuery.OrderBy(x => EF.Property<object>(x, filters.ApplySorting));
                    }
                }
                
                // Include soft-deleted entities if requested
                if(filters.IncludeSoftDeletedEntitiesAlso)
                {
                    sourceQuery = sourceQuery.Where(x => x.IsDeleted == true);
                }

                // Apply date range filters
                if (filters.StartDate.HasValue)
                {
                    sourceQuery = sourceQuery.Where(x => x.CreatedOn >= filters.StartDate.Value);
                }
                if(filters.EndDate.HasValue)
                {
                    sourceQuery = sourceQuery.Where(x => x.CreatedOn <= filters.EndDate.Value);
                }
                return sourceQuery;
            }
            catch (Exception)
            {
                // Return original query to prevent breaking callers.
                return sourceQuery;
            }
        }

        private static IQueryable<T> ApplyDefaultFilters<T>(IQueryable<T> sourceQuery) where T : BaseEntity
        {
            sourceQuery = sourceQuery.Where(a => a.IsDeleted != true);
            return sourceQuery;
        }
    }
}


