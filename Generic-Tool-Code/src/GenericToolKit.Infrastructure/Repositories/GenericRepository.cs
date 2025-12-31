using GenericToolKit.Domain.Entities;
using GenericToolKit.Domain.Extensions;
using GenericToolKit.Domain.Interfaces;
using GenericToolKit.Domain.Models;
using GenericToolKit.Domain.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using System.Collections;
using System.Linq.Expressions;

namespace GenericToolKit.Infrastructure.Repositories
{
    /// <summary>
    /// Generic repository implementation for Entity Framework Core.
    /// This repository follows the Repository Pattern (DDD) and provides data access operations.
    /// All methods are wrapped with exception handling using delegate-based wrappers.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly DbContext context;
        private readonly ILoggedInUser loggedInUser;
        private const string LayerName = "GenericRepository";

        /// <summary>
        /// Initializes a new instance of the GenericRepository class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="loggedInUser">The current system user context for audit and tenant isolation.</param>
        public GenericRepository(DbContext context, ILoggedInUser loggedInUser)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.loggedInUser = loggedInUser ?? throw new ArgumentNullException(nameof(loggedInUser));
        }
        
        public async Task<T> Add(T entity)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (entity == null)
                        return null;

                    var dbSet = this.context.Set<T>();
                    var alreadyExists = await dbSet.Where(a => a.Id == entity.Id).FirstOrDefaultAsync();
                    if (alreadyExists != null)
                    {
                        return alreadyExists;
                    }

                    await dbSet.AddAsync(entity);
                    await this.context.SaveChangesAsync();

                    var dbEntity = await dbSet.Where(a => a.Id == entity.Id).FirstOrDefaultAsync();
                    if (dbEntity != null)
                    {
                        this.context.Entry(dbEntity).State = EntityState.Detached; // Detach the entity
                    }
                    return dbEntity;
                },
                nameof(Add),
                LayerName,
                defaultValue: null);
        }

        public IQueryable<T> GetById(int id, bool detached = true)
        {
            return ExceptionHandler.Execute(
                () =>
                {
                    if (id <= 0)
                        return Enumerable.Empty<T>().AsQueryable();

                    var dbSet = this.context.Set<T>().AsQueryable<T>();
                    var result = dbSet.Where(x => x.Id == id);
                    if (detached)
                    {
                        result = result.AsNoTracking();
                    }
                    return result;
                },
                nameof(GetById),
                LayerName,
                defaultValue: Enumerable.Empty<T>().AsQueryable());
        }

        public async Task<bool> HardDeleteById(int Id)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (Id == 0)
                        return false;

                    var dbSet = this.context.Set<T>();
                    var dbEntity = await dbSet.Where(a => a.Id == Id).FirstOrDefaultAsync();
                    if (dbEntity == null)
                        return false;

                    dbSet.Remove(dbEntity);
                    return await this.context.SaveChangesAsync() > 0;
                },
                nameof(HardDeleteById),
                LayerName,
                defaultValue: false);
        }

        public async Task<int> HardDeleteMany(Expression<Func<T, bool>> predicate)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (predicate == null)
                        return 0;

                    var dbSet = this.context.Set<T>();
                    var dbEntities = await dbSet.Where(predicate).ToListAsync();
                    if (dbEntities == null || dbEntities.Count == 0)
                        return 0;

                    dbSet.RemoveRange(dbEntities);
                    return await this.context.SaveChangesAsync();
                },
                nameof(HardDeleteMany),
                LayerName,
                defaultValue: 0);
        }

        public async Task<int> HardDeleteOne(T entity)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (entity == null)
                        return 0;

                    var dbSet = this.context.Set<T>();
                    var dbEntity = await dbSet.Where(a => a.Id == entity.Id).FirstOrDefaultAsync();
                    if (dbEntity == null)
                        return 0;

                    dbSet.Remove(dbEntity);
                    return await this.context.SaveChangesAsync();
                },
                nameof(HardDeleteOne),
                LayerName,
                defaultValue: 0);
        }

        public async Task<T> SaveOrUpdate(T entity, bool setAuditProperties = true, bool shouldSave = true)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (entity == null)
                        return null;

                    var dbSet = this.context.Set<T>();
                    var dbEntity = await dbSet.Where(a => a.Id == entity.Id).FirstOrDefaultAsync();
                    
                    if (dbEntity == null && entity.Id == 0)
                    {
                        this.context.Entry(entity).State = EntityState.Added;
                    }
                    else
                    {
                        this.context.Entry(entity).State = EntityState.Modified;
                    }

                    if (setAuditProperties)
                    {
                        SetAuditPropertiesRecursively(entity);
                    }

                    if (shouldSave)
                    {
                        await this.context.SaveChangesAsync();
                    }
                    return entity;
                },
                nameof(SaveOrUpdate),
                LayerName,
                defaultValue: null);
        }

        private void SetAuditPropertiesRecursively(T entity)
        {
            ExceptionHandler.Execute(
                () =>
                {
                    if (entity == null)
                        return;

                    var allproperties = entity.GetType().GetProperties();
                    foreach (var property in allproperties)
                    {
                        var propertyValue = property.GetValue(entity);
                        if (propertyValue == null)
                            continue;

                        if (IsCollectionOfTypeBaseEntity(propertyValue))
                        {
                            foreach (var item in (IEnumerable)propertyValue)
                            {
                                if (item == null)
                                    continue;
                                if (IsOfTypeBaseEntity(item))
                                    SetAuditPropertiesRecursively((T)item);
                            }
                        }

                        if (!IsCollectionOfTypeBaseEntity(propertyValue) && IsOfTypeBaseEntity(propertyValue))
                        {
                            SetAuditPropertiesRecursively((T)propertyValue);
                        }
                    }

                    var timeStamp = DateTime.Now;
                    if (entity.Id == 0)
                    {
                        entity.CreatedBy = this.loggedInUser.LoginId;
                        entity.CreatedOn = timeStamp;
                    }
                    entity.UpdatedBy = this.loggedInUser.LoginId;
                    entity.UpdatedOn = timeStamp;
                    entity.IsDeleted = false;
                    entity.DeletedBy = null;
                    entity.DeletedOn = null;
                    entity.TenantId = this.loggedInUser.TenantId;
                },
                nameof(SetAuditPropertiesRecursively),
                LayerName);
        }

        private static bool IsOfTypeBaseEntity(object value)
        {
            return value != null && value is BaseEntity;
        }

        private static bool IsCollectionOfTypeBaseEntity(object value)
        {
            return value is IEnumerable<T>;
        }

        public async Task<bool> SetEntityStateRecursively_N_UpsertMultiple(List<T> entities, CancellationToken cancellationToken = default)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (!entities.IsValidList())
                        return false;

                    for (int i = 0; i < entities.Count; i++)
                    {
                        this.context.Entry(entities[i]).State = EntityState.Unchanged;
                        SetEntityStateRecursively(entities[i]);
                    }
                    return await IsSavedAsync();
                },
                nameof(SetEntityStateRecursively_N_UpsertMultiple),
                LayerName,
                defaultValue: false);
        }

        private async Task<bool> IsSavedAsync()
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    return await this.context.SaveChangesAsync() > 0;
                },
                nameof(IsSavedAsync),
                LayerName,
                defaultValue: false);
        }

        private void SetEntityStateRecursively(T entity)
        {
            ExceptionHandler.Execute(
                () =>
                {
                    if (entity == null)
                        return;

                    var allProperties = entity.GetType().GetProperties();
                    foreach (var item in allProperties)
                    {
                        var propValue = item.GetValue(entity);
                        if (IsCollectionOfTypeBaseEntity(propValue))
                        {
                            foreach (var item1 in (IEnumerable)propValue)
                            {
                                if (item1 == null)
                                    continue;
                                if (IsOfTypeBaseEntity(item1))
                                    SetEntityStateRecursively((T)item1);
                            }
                        }
                        if (IsOfTypeBaseEntity(propValue))
                        {
                            SetEntityStateRecursively((T)propValue);
                        }

                        this.context.Entry(entity).State = entity.Id > 0 ? EntityState.Modified : EntityState.Added;

                        if (this.context.Entry(entity).State == EntityState.Modified)
                        {
                            // keep the Created Properties to be constant if user attacks and changed them 
                            MarkCreatedPropertiesToUnchanged(entity);
                        }
                    }
                },
                nameof(SetEntityStateRecursively),
                LayerName);
        }

        private void MarkCreatedPropertiesToUnchanged(T entity)
        {
            ExceptionHandler.Execute(
                () =>
                {
                    if (entity == null)
                        return;

                    this.context.Entry(entity).Property(x => x.CreatedBy).IsModified = false;
                    this.context.Entry(entity).Property(y => y.CreatedOn).IsModified = false;
                },
                nameof(MarkCreatedPropertiesToUnchanged),
                LayerName);
        }

        public async Task<bool> SoftDeleteMany(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (!entities.IsValidEnumerable())
                        return false;

                    foreach (var entity in entities)
                    {
                        MarkEntityAsDeleted(entity);
                    }
                    var result = await this.context.SaveChangesAsync(cancellationToken);
                    return result > 0;
                },
                nameof(SoftDeleteMany),
                LayerName,
                defaultValue: false);
        }

        private void MarkEntityAsDeleted(T entity)
        {
            ExceptionHandler.Execute(
                () =>
                {
                    if (entity == null)
                        return;

                    var allProperties = entity.GetType().GetProperties();
                    foreach (var property in allProperties)
                    {
                        var propValue = property.GetValue(entity);
                        if (IsCollectionOfTypeBaseEntity(propValue))
                        {
                            foreach (var item in (IEnumerable)propValue)
                            {
                                if (item == null)
                                    continue;
                                if (IsOfTypeBaseEntity(item))
                                    MarkEntityAsDeleted((T)item);
                            }
                        }
                        if (IsOfTypeBaseEntity(propValue))
                        {
                            MarkEntityAsDeleted((T)propValue);
                        }

                        entity.SetDeletedProperties(this.loggedInUser.LoginId);
                    }
                },
                nameof(MarkEntityAsDeleted),
                LayerName);
        }

        public async Task<bool> SoftDeleteManyByConditions(Expression<Func<T, bool>> predicates, CancellationToken cancellationToken = default)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (predicates == null)
                        return false;

                    var dbSet = this.context.Set<T>();
                    var dbEntities = await dbSet.Where(predicates).ToListAsync(cancellationToken);
                    if (!dbEntities.IsValidList())
                        return false;

                    foreach (var entity in dbEntities)
                    {
                        MarkEntityAsDeleted(entity);
                    }
                    var result = await this.context.SaveChangesAsync(cancellationToken);
                    return result > 0;
                },
                nameof(SoftDeleteManyByConditions),
                LayerName,
                defaultValue: false);
        }

        public async Task<bool> SoftDeleteOne(T entity, CancellationToken cancellationToken = default)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (!entity.IsValidObject())
                        return false;

                    var dbSet = this.context.Set<T>();
                    var dbEntity = await dbSet.Where(a => a.Id == entity.Id).FirstOrDefaultAsync(cancellationToken);

                    if (!dbEntity.IsValidObject())
                        return false;

                    dbEntity.SetDeletedProperties(this.loggedInUser.LoginId);
                    this.context.Entry(dbEntity).State = EntityState.Modified;
                    var result = await this.context.SaveChangesAsync(cancellationToken);
                    return result > 0;
                },
                nameof(SoftDeleteOne),
                LayerName,
                defaultValue: false);
        }

        public async Task UpdateOne(T entity, CancellationToken token)
        {
            await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (entity == null)
                        return;

                    var dbSet = this.context.Set<T>();
                    if (this.context.Entry(entity).State == EntityState.Detached)
                    {
                        this.context.Entry(entity).State = EntityState.Unchanged;
                    }
                    this.context.Entry(entity).State = entity.Id > 0 ? EntityState.Modified : EntityState.Added;
                    await this.context.SaveChangesAsync(token);
                    this.context.Entry(entity).State = EntityState.Detached; // Detach the entity
                },
                nameof(UpdateOne),
                LayerName);
        }

        public async Task<List<T>> GetAll(BaseFilters? filters = null)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    IQueryable<T> query = this.context.Set<T>().AsQueryable<T>();
                    query = query.ApplyQueryFilters(filters);
                    var result = await query.ToListAsync();
                    return result ?? new List<T>();
                },
                nameof(GetAll),
                LayerName,
                defaultValue: new List<T>());
        }

        public async Task<T?> FindOne(Expression<Func<T, bool>> predicate, BaseFilters? filters = null)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (predicate == null)
                        return null;

                    var query = this.context.Set<T>().AsQueryable<T>();
                    if (filters != null)
                    {
                        query = query.ApplyQueryFilters(filters);
                    }

                    // apply custom filters 
                    var result = await query.Where(predicate).FirstOrDefaultAsync();
                    return result;
                },
                nameof(FindOne),
                LayerName,
                defaultValue: null);
        }

        public IQueryable<T> Find(Expression<Func<T, bool>> predicate, BaseFilters? filters = null)
        {
            return ExceptionHandler.Execute(
                () =>
                {
                    if (predicate == null)
                        return Enumerable.Empty<T>().AsQueryable();

                    var query = this.context.Set<T>().AsQueryable<T>();
                    if (filters != null)
                    {
                        query = query.ApplyQueryFilters(filters);
                    }
                    // apply custom filters
                    var result = query.Where(predicate);
                    return result;
                },
                nameof(Find),
                LayerName,
                defaultValue: Enumerable.Empty<T>().AsQueryable());
        }

        public async Task<List<T>> ListAsync(List<int> Ids, CancellationToken cancellationToken = default)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (!Ids.IsValidList())
                        return new List<T>();

                    var query = this.context.Set<T>().AsQueryable<T>();

                    var filters = new BaseFilters()
                    {
                        IncludeSoftDeletedEntitiesAlso = false,
                        TenantId = this.loggedInUser.TenantId,
                        IsAsNoTracking = true,
                        Skip = 0,
                        Take = Ids.Count,
                    };
                    query = query.ApplyQueryFilters(filters);
                    query = query.Where(a => Ids.Contains(a.Id));
                    var data = await query.ToListAsync();
                    return data ?? new List<T>();
                },
                nameof(ListAsync),
                LayerName,
                defaultValue: new List<T>());
        }

        public async Task<bool> Any(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (predicate == null)
                        return false;

                    var query = this.context.Set<T>().AsQueryable<T>();
                    var filters = new BaseFilters()
                    {
                        IncludeSoftDeletedEntitiesAlso = false,
                        TenantId = this.loggedInUser.TenantId,
                        IsAsNoTracking = true,
                        Skip = 0,
                        Take = 1,
                    };
                    query = query.ApplyQueryFilters(filters);
                    query = query.Where(predicate);
                    return await query.AnyAsync(cancellationToken);
                },
                nameof(Any),
                LayerName,
                defaultValue: false);
        }

        public async Task<int> Count(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (predicate == null)
                        return 0;

                    var query = this.context.Set<T>().AsQueryable<T>();
                    var filters = new BaseFilters()
                    {
                        IncludeSoftDeletedEntitiesAlso = false,
                        TenantId = this.loggedInUser.TenantId,
                        IsAsNoTracking = true,
                        Skip = 0,
                        Take = 1,
                    };
                    query = query.ApplyQueryFilters(filters);
                    query = query.Where(predicate);
                    return await query.CountAsync(cancellationToken);
                },
                nameof(Count),
                LayerName,
                defaultValue: 0);
        }

        public async Task<List<T>> ListBySpecs(IBaseSpecification<T> specification, CancellationToken cancellationToken = default)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (specification == null)
                        return new List<T>();

                    var query = this.context.Set<T>().AsQueryable<T>();

                    query = query.Where(specification.WhereExpression);
                    foreach (var include in specification.Includes)
                    {
                        query = query.Include(include);
                    }
                    var result = await query.ToListAsync(cancellationToken);
                    return result ?? new List<T>();
                },
                nameof(ListBySpecs),
                LayerName,
                defaultValue: new List<T>());
        }

        public async Task<T> RestoreOriginalValuesAsync(T entityToUpdate, List<string> propertiesToUpdate)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (entityToUpdate == null || !propertiesToUpdate.IsValidList())
                        return null;

                    var entry = this.context.Entry(entityToUpdate);
                    if (entry.State != EntityState.Modified)
                    {
                        return null;
                    }

                    foreach (var property in propertiesToUpdate)
                    {
                        var allOriginalProperties = entry.OriginalValues.Properties;
                        if (allOriginalProperties.Any(a => a.Name == property))
                        {
                            var originalValue = entry.OriginalValues[property];
                            entry.CurrentValues[property] = originalValue;
                            entry.Property(property).IsModified = false;
                        }
                    }
                    return entityToUpdate;
                },
                nameof(RestoreOriginalValuesAsync),
                LayerName,
                defaultValue: null);
        }

        public async Task<bool> AddMany(IEnumerable<T> entities)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (!entities.Any())
                        return false;

                    var dbSet = this.context.Set<T>();
                    await dbSet.AddRangeAsync(entities);
                    return await IsSavedAsync();
                },
                nameof(AddMany),
                LayerName,
                defaultValue: false);
        }

        IQueryable<TResult> IEntityQueryRepository<T>.ProjectableListBySpecs<TResult>(IProjectableSpecifications<T, TResult> specification, CancellationToken cancellationToken)
        {
            return ExceptionHandler.Execute(
                () =>
                {
                    throw new NotImplementedException("ProjectableListBySpecs is not yet implemented.");
                },
                nameof(IEntityQueryRepository<T>.ProjectableListBySpecs),
                LayerName,
                defaultValue: Enumerable.Empty<TResult>().AsQueryable());
        }

        public async Task<bool> RemoveListOfEntities(List<T> entities)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (!entities.IsValidList())
                        return false;

                    var dbSet = this.context.Set<T>();
                    dbSet.RemoveRange(entities);
                    return await IsSavedAsync();
                },
                nameof(RemoveListOfEntities),
                LayerName,
                defaultValue: false);
        }

        public async Task<bool> CommitTransactionAsync(IDbContextTransaction transaction, bool shouldCommit)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (transaction == null)
                        throw new ArgumentNullException(nameof(transaction));

                    if (shouldCommit)
                    {
                        await transaction.CommitAsync();
                        return true;
                    }
                    return false;
                },
                nameof(CommitTransactionAsync),
                LayerName,
                defaultValue: false);
        }

        public async Task<bool> RollbackTransactionAsync(IDbContextTransaction transaction)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (transaction == null)
                        throw new ArgumentNullException(nameof(transaction));

                    await transaction.RollbackAsync();
                    return true;
                },
                nameof(RollbackTransactionAsync),
                LayerName,
                defaultValue: false);
        }

        public async Task<IDbContextTransaction> StartTransaction()
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    return await this.context.Database.BeginTransactionAsync();
                },
                nameof(StartTransaction),
                LayerName,
                defaultValue: null);
        }

        public async Task SetAuditProperties(T entity)
        {
            await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (entity == null)
                        return;

                    var allProperties = entity.GetType().GetProperties();
                    foreach (var prop in allProperties)
                    {
                        var value = prop.GetValue(entity);
                        if (IsOfTypeBaseEntity(value))
                        {
                            await SetAuditProperties((T)value);
                        }

                        if (IsCollectionOfTypeBaseEntity(value))
                        {
                            foreach (var item in (IEnumerable)value)
                            {
                                if (item == null)
                                    continue;
                                if (IsOfTypeBaseEntity(item))
                                    await SetAuditProperties((T)item);
                            }
                        }
                    }

                    var timeStamp = DateTime.Now;
                    if (entity.Id == 0)
                    {
                        entity.CreatedBy = this.loggedInUser.LoginId;
                        entity.CreatedOn = timeStamp;
                    }
                    entity.UpdatedBy = this.loggedInUser.LoginId;
                    entity.UpdatedOn = timeStamp;
                    entity.IsDeleted = false;
                    entity.DeletedBy = null;
                    entity.DeletedOn = null;
                },
                nameof(SetAuditProperties),
                LayerName);
        }

        public Task<string> DetectChange(T entity)
        {
            return ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (entity == null)
                        return string.Empty;

                    /*
                     * this will return json object which will contain key value pairs - and only those properties which are being changed, not all properties
                     */
                    EntityEntry<T> entry = null;
                    var query = this.context.Set<T>().AsQueryable<T>();
                    var dbEntity = await query.Where(a => a.Id == entity.Id).FirstOrDefaultAsync();

                    if (dbEntity is null)
                    {
                        this.context.Entry(entity).State = EntityState.Added;
                        entry = this.context.Entry(entity);
                    }
                    else
                    {
                        this.context.Entry(dbEntity).State = EntityState.Modified;
                        entry = this.context.Entry(dbEntity);
                        entry.CurrentValues.SetValues(entity);
                    }

                    var properties = entity.GetType().GetProperties();
                    var resultToReturn = (entry.State == EntityState.Added)
                        ? properties.ToDictionary(
                            prop => prop.Name,
                            prop => prop.GetValue(entity)
                          )
                        : properties
                            .Where(prop =>
                            {
                                var newVal = prop.GetValue(entity);
                                var originalVal = entry.OriginalValues[prop.Name];
                                return newVal != null &&
                                       originalVal != null &&
                                       !newVal.Equals(originalVal);
                            })
                            .ToDictionary(
                                prop => prop.Name,
                                prop => prop.GetValue(entity)
                            );

                    entry.State = EntityState.Detached; // Detach the entity    
                    var serializedResult = JsonConvert.SerializeObject(resultToReturn);
                    return serializedResult;
                },
                nameof(DetectChange),
                LayerName,
                defaultValue: string.Empty);
        }

        public async Task<string> LogFullJsonComparison(T entity)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (entity == null)
                        return string.Empty;

                    /*
                     * {
                          "OldData": {
                            "ACPType_1246": 1246,
                            "Value_1246": 2,
                            "Comments_1246": "123",
                            "Id_1246": 49784
                          },
                          "NewData": {
                            "ACPType_1246": 1246,
                            "Value_1246": 1,
                            "Comments_1246": "",
                            "Id_1246": 49784
                          },
                          "ChangedProperties": [
                            "Value_1246",
                            "Comments_1246"
                          ]
                        }
                     */
                    var resultToReturn = new Dictionary<string, object>()
                    {
                        ["OldData"] = new Dictionary<string, object>(),
                        ["NewData"] = new Dictionary<string, object>(),
                        ["ChangedProperties"] = new List<string>()
                    };
                    var query = this.context.Set<T>().AsQueryable<T>();
                    var dbEntity = await query.Where(a => a.Id == entity.Id).FirstOrDefaultAsync();

                    if (dbEntity is null)
                    {
                        resultToReturn["OldData"] = null;
                        resultToReturn["NewData"] = entity;
                        resultToReturn["ChangedProperties"] = new List<string>();
                        return JsonConvert.SerializeObject(resultToReturn);
                    }

                    resultToReturn["OldData"] = dbEntity;
                    resultToReturn["NewData"] = entity;

                    this.context.Entry(dbEntity).CurrentValues.SetValues(entity);
                    // Compare the original and current values for each property
                    var changedProperties = dbEntity.GetType().GetProperties()
                        .Where(property =>
                        {
                            // Skip navigation properties
                            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType)
                                && property.PropertyType != typeof(string))
                            {
                                return false; // skip collections
                            }

                            if (!property.PropertyType.IsValueType && property.PropertyType != typeof(string))
                            {
                                return false; // skip reference navigations
                            }

                            // Compare scalar values
                            var originalValue = this.context.Entry(dbEntity).OriginalValues[property.Name];
                            var newCurrentValue = this.context.Entry(dbEntity).CurrentValues[property.Name];
                            return !Equals(originalValue, newCurrentValue);
                        })
                        .Select(property => property.Name)
                        .ToList();

                    resultToReturn["ChangedProperties"] = changedProperties;
                    // Serialize the result to JSON
                    var serializedResult = JsonConvert.SerializeObject(resultToReturn);

                    return serializedResult;
                },
                nameof(LogFullJsonComparison),
                LayerName,
                defaultValue: string.Empty);
        }

        public async Task<BaseEntry<T>> CreateReturnBaseEntryObject(EntityEntry entry)
        {
            return await ExceptionHandler.ExecuteAsync(
                async () =>
                {
                    if (entry == null)
                        return null;

                    var entryToReturn = new BaseEntry<T>
                    {
                        State = MapTrackedState(entry.State),
                        CurrentValues = entry.CurrentValues,
                        OriginalValues = entry.OriginalValues,
                        Entity = (T)entry.Entity
                    };

                    var modifiedProperties = GetModifiedPropertiesAsDictionary(entryToReturn);
                    entryToReturn.ModifiedProperties = modifiedProperties;

                    return entryToReturn;
                },
                nameof(CreateReturnBaseEntryObject),
                LayerName,
                defaultValue: null);
        }

        private static TrackedEntityState MapTrackedState(EntityState state)
        {
            return state switch
            {
                EntityState.Detached => TrackedEntityState.Detached,
                EntityState.Unchanged => TrackedEntityState.Unchanged,
                EntityState.Deleted => TrackedEntityState.Deleted,
                EntityState.Modified => TrackedEntityState.Modified,
                EntityState.Added => TrackedEntityState.Added,
                _ => TrackedEntityState.Detached
            };
        }

        public Dictionary<string, object> GetModifiedPropertiesAsDictionary(BaseEntry<T> trackedEntry)
        {
            return ExceptionHandler.Execute(
                () =>
                {
                    if (trackedEntry is null || trackedEntry.Entity == null)
                        return new Dictionary<string, object>();

                    return trackedEntry.Entity.GetType().GetProperties()
                        .Where(a =>
                        {
                            var originalVal = this.context.Entry(trackedEntry.Entity).OriginalValues[a.Name];
                            var newCurrentVal = this.context.Entry(trackedEntry.Entity).CurrentValues[a.Name];

                            return !Equals(originalVal, newCurrentVal);
                        })
                        .ToDictionary(
                            prop => prop.Name,
                            prop => prop.GetValue(trackedEntry.Entity) ?? new object()
                        );
                },
                nameof(GetModifiedPropertiesAsDictionary),
                LayerName,
                defaultValue: new Dictionary<string, object>());
        }
        
        public EntityEntry AddOrAttachEntity(T entity)
        {
            return ExceptionHandler.Execute(
                () =>
                {
                    if (entity == null)
                        throw new ArgumentNullException(nameof(entity));

                    // Get the entity entry
                    var entry = this.context.Entry<T>(entity);

                    // Check if the entity is detached (not being tracked)
                    if (entry.State == EntityState.Detached)
                    {
                        // Attach the entity if it's detached, but keep it unchanged (no modification)
                        this.context.Entry(entity).State = EntityState.Unchanged;
                    }
                    // If the entity is being tracked, ensure that it is marked as added if it's not already
                    else if (entry.State == EntityState.Unchanged || entry.State == EntityState.Modified)
                    {
                        entry.State = EntityState.Added;
                    }

                    // Return the entry
                    return entry;
                },
                nameof(AddOrAttachEntity),
                LayerName,
                defaultValue: null);
        }

        public Dictionary<string, object> ExtractModifiedOnlyOldProperties(BaseEntry<T> entry)
        {
            return ExceptionHandler.Execute(
                () =>
                {
                    if (entry is null || entry.Entity == null)
                        return new Dictionary<string, object>();

                    var entityEntry = this.context.Entry(entry.Entity);

                    return entry.Entity.GetType().GetProperties()
                        .Where(prop =>
                        {
                            var originalVal = entityEntry.OriginalValues[prop.Name];
                            var newCurrentVal = entityEntry.CurrentValues[prop.Name];

                            // Check if the original value and the current value are different
                            return !Equals(originalVal, newCurrentVal);
                        })
                        .ToDictionary(
                            prop => prop.Name,
                            prop =>
                            {
                                // Get the original value and handle null appropriately
                                var originalValue = entityEntry.OriginalValues[prop.Name];
                                return originalValue ?? null;  // Return null if the original value is null
                            });
                },
                nameof(ExtractModifiedOnlyOldProperties),
                LayerName,
                defaultValue: new Dictionary<string, object>());
        }

        public Dictionary<string, object> ExtractModifiedOnlyChangedProperties(BaseEntry<T> entry)
        {
            return ExceptionHandler.Execute(
                () =>
                {
                    if (entry is null || entry.Entity == null)
                        return new Dictionary<string, object>();

                    var entityEntry = this.context.Entry(entry.Entity);

                    return entry.Entity.GetType().GetProperties()
                        .Where(prop =>
                        {
                            var originalVal = entityEntry.OriginalValues[prop.Name];
                            var newCurrentVal = entityEntry.CurrentValues[prop.Name];

                            // Check if the original value and the current value are different
                            return !Equals(originalVal, newCurrentVal);
                        })
                        .ToDictionary(
                            prop => prop.Name,
                            prop =>
                            {
                                // Get the original value and handle null appropriately
                                var originalValue = entityEntry.CurrentValues[prop.Name];
                                return originalValue ?? null;  // Return null if the original value is null
                            });
                },
                nameof(ExtractModifiedOnlyChangedProperties),
                LayerName,
                defaultValue: new Dictionary<string, object>());
        }
    }
}
