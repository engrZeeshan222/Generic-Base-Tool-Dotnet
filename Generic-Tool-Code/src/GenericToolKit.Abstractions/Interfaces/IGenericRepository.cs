using GenericToolKit.Domain.Entities;
using GenericToolKit.Domain.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace GenericToolKit.Domain.Interfaces
{
    /// <summary>
    /// Interface for transaction management operations.
    /// Following Interface Segregation Principle (SOLID), this interface focuses solely on transaction concerns.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface ITransactionRepository<T> where T : BaseEntity
    {
        Task<bool> CommitTransactionAsync(IDbContextTransaction transaction, bool shouldCommit);
        Task<bool> RollbackTransactionAsync(IDbContextTransaction transaction);
        Task<IDbContextTransaction> StartTransaction();
    }
    
    /// <summary>
    /// Interface for CRUD (Create, Read, Update, Delete) operations.
    /// Following Interface Segregation Principle (SOLID), this interface focuses solely on CRUD concerns.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface IEntityCrudRepository<T> where T : BaseEntity
    {
        Task<T> Add(T entity);
        IQueryable<T> GetById(int id, bool detached = true);
        Task<bool> HardDeleteById(int Id);
        Task<int> HardDeleteMany(Expression<Func<T, bool>> predicate);
        Task<int> HardDeleteOne(T entity);
        Task<T> SaveOrUpdate(T entity, bool setAuditProperties = true, bool shouldSave = true);
        Task<bool> SetEntityStateRecursively_N_UpsertMultiple(List<T> entities, CancellationToken cancellationToken = default);
        Task<bool> SoftDeleteMany(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<bool> SoftDeleteManyByConditions(Expression<Func<T, bool>> predicates, CancellationToken cancellationToken = default);
        Task<bool> SoftDeleteOne(T entity, CancellationToken cancellationToken = default);
        Task UpdateOne(T entity, CancellationToken token);
    }
    
    /// <summary>
    /// Interface for query operations.
    /// Following Interface Segregation Principle (SOLID), this interface focuses solely on query concerns.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface IEntityQueryRepository<T> where T : BaseEntity
    {
        Task<List<T>> GetAll(BaseFilters? filters = null);
        Task<T?> FindOne(Expression<Func<T, bool>> predicate, BaseFilters? findOptions = null);
        IQueryable<T> Find(Expression<Func<T, bool>> predicate, BaseFilters? findOptions = null);
        Task<List<T>> ListAsync(List<int> Ids, CancellationToken cancellationToken = default);
        Task<bool> Any(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> Count(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<List<T>> ListBySpecs(IBaseSpecification<T> specification, CancellationToken cancellationToken = default);
        IQueryable<TResult> ProjectableListBySpecs<TResult>(IProjectableSpecifications<T, TResult> specification, CancellationToken cancellationToken = default) where TResult : BaseInOutDTO;
    }
    
    /// <summary>
    /// Interface for change tracking operations.
    /// Following Interface Segregation Principle (SOLID), this interface focuses solely on change tracking concerns.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface IEntityChangeTrackingRepository<T> where T : BaseEntity
    {
        Task<string> DetectChange(T entity);
        Task<string> LogFullJsonComparison(T entity);
        Task<BaseEntry<T>> CreateReturnBaseEntryObject(EntityEntry entry);
        Dictionary<string, object> GetModifiedPropertiesAsDictionary(BaseEntry<T> trackedEntry);
        EntityEntry AddOrAttachEntity(T entity);
        Dictionary<string, object> ExtractModifiedOnlyOldProperties(BaseEntry<T> entry);
        Dictionary<string, object> ExtractModifiedOnlyChangedProperties(BaseEntry<T> entry);
    }
    
    /// <summary>
    /// Interface for audit operations.
    /// Following Interface Segregation Principle (SOLID), this interface focuses solely on audit concerns.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface IAuditRepository<T> where T : BaseEntity
    {
        Task SetAuditProperties(T entity);
    }
    
    /// <summary>
    /// Interface for entity removal operations.
    /// Following Interface Segregation Principle (SOLID), this interface focuses solely on removal concerns.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface IEntityRemovalRepository<T> where T : BaseEntity
    {
        Task<bool> RemoveListOfEntities(List<T> entities);
    }
    
    /// <summary>
    /// Generic repository interface that combines all repository concerns.
    /// This interface follows the Repository Pattern (DDD) and Dependency Inversion Principle (SOLID).
    /// The interface is segregated into smaller, focused interfaces for better maintainability.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public interface IGenericRepository<T> :
        IEntityChangeTrackingRepository<T>,
        IAuditRepository<T>,
        ITransactionRepository<T>,
        IEntityRemovalRepository<T>,
        IEntityCrudRepository<T>,
        IEntityQueryRepository<T>
        where T : BaseEntity 
    {
        Task<T> RestoreOriginalValuesAsync(T entityToUpdate, List<string> propertiesToUpdate);
        Task<bool> AddMany(IEnumerable<T> entities);
    }
}


