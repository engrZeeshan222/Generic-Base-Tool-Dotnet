using GenericToolKit.Domain.Entities;
using GenericToolKit.Domain.Interfaces;
using GenericToolKit.Domain.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace GenericToolKit.Application.Services
{
    /// <summary>
    /// Interface for transaction-related methods.
    /// Following Interface Segregation Principle (SOLID), this interface focuses solely on transaction concerns.
    /// </summary>
    public interface ITransactionService
    {
        Task<bool> CommitTransactionAsync(IDbContextTransaction transaction, bool shouldCommit);
        Task<bool> RollbackTransactionAsync(IDbContextTransaction transaction);
        Task<IDbContextTransaction> StartTransaction();
    }

    /// <summary>
    /// Interface for basic CRUD operations.
    /// Following Interface Segregation Principle (SOLID), this interface focuses solely on CRUD concerns.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface ICrudService<T> where T : BaseEntity
    {
        Task<T> Add(T entity);
        IQueryable<T> GetByIdQuery(int id, bool detached = true);
        Task<bool> HardDeleteById(int id);
        Task<int> HardDeleteMany(Expression<Func<T, bool>> predicate);
        Task<int> HardDeleteOne(T entity);
        Task<T> SaveOrUpdate(T entity, bool setAuditProperties = true, bool shouldSave = true);
        Task<bool> SoftDeleteMany(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<bool> SoftDeleteOne(T entity, CancellationToken cancellationToken = default);
        Task<T> UpdateOne(T entity, CancellationToken token);
    }

    /// <summary>
    /// Interface for query-related methods.
    /// Following Interface Segregation Principle (SOLID), this interface focuses solely on query concerns.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface IQueryService<T> where T : BaseEntity
    {
        Task<List<T>> GetAll(BaseFilters? filters = null);
        Task<T?> FindOne(Expression<Func<T, bool>> predicate, BaseFilters? findOptions = null);
        IQueryable<T> Find(Expression<Func<T, bool>> predicate, BaseFilters? findOptions = null);
        Task<List<T>> ListAsync(List<int> Ids, CancellationToken cancellationToken = default);
        Task<bool> Any(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> Count(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<List<T>> ListBySpecs(IBaseSpecification<T> specification, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface for change tracking methods.
    /// Following Interface Segregation Principle (SOLID), this interface focuses solely on change tracking concerns.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface IChangeTrackingService<T> where T : BaseEntity
    {
        Task<string> DetectChange(T entity);
        Task<string> LogFullJsonComparison(T entity);
    }

    /// <summary>
    /// Interface for audit methods.
    /// Following Interface Segregation Principle (SOLID), this interface focuses solely on audit concerns.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface IAuditService<T> where T : BaseEntity
    {
        Task<T> SetAuditPropertiesAsync(T entity);
    }

    /// <summary>
    /// Interface for removal-related methods.
    /// Following Interface Segregation Principle (SOLID), this interface focuses solely on removal concerns.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface IRemovalService<T> where T : BaseEntity
    {
        Task<bool> RemoveListOfEntities(List<T> entities);
    }

    /// <summary>
    /// Interface for additional methods.
    /// Following Interface Segregation Principle (SOLID), this interface focuses on additional utility operations.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface IAdditionalService<T> where T : BaseEntity
    {
        Task<T> RestoreOriginalValuesAsync(T entityToUpdate, List<string> propertiesToUpdate);
        Task<bool> AddMany(IEnumerable<T> entities);
    }

    /// <summary>
    /// Generic service interface that combines all service concerns.
    /// This interface follows the Application Service pattern (Clean Architecture) and Dependency Inversion Principle (SOLID).
    /// The interface is segregated into smaller, focused interfaces for better maintainability.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface IGenericService<T> :
        ITransactionService,
        ICrudService<T>,
        IQueryService<T>,
        IChangeTrackingService<T>,
        IAuditService<T>,
        IRemovalService<T>,
        IAdditionalService<T>
        where T : BaseEntity
    {
    }
}

