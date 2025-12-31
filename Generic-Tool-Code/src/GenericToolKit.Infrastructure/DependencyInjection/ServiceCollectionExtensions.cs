using GenericToolKit.Domain.Entities;
using GenericToolKit.Domain.Interfaces;
using GenericToolKit.Infrastructure.Data;
using GenericToolKit.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GenericToolKit.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring dependency injection for Generic Toolkit base components.
    /// These extensions allow consumer microservices to easily register all base repositories.
    /// Following the Dependency Inversion Principle (SOLID) and Clean Architecture patterns.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the GenericRepository for a specific entity type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type that inherits from BaseEntity.</typeparam>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="lifetime">The service lifetime. Defaults to Scoped.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddGenericRepository<TEntity>(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Scoped
            ) where TEntity : BaseEntity
        {
            services.Add(
                new ServiceDescriptor(
                    typeof(IGenericRepository<TEntity>),
                    typeof(GenericRepository<TEntity>),
                    lifetime));

            return services;
        }

        /// <summary>
        /// Registers GenericRepository for all entity types that inherit from BaseEntity in the specified assemblies.
        /// This method automatically discovers and registers repositories for all entities.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="assemblies">The assemblies to scan for entity types.</param>
        /// <param name="lifetime">The service lifetime. Defaults to Scoped.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddGenericRepositories(
            this IServiceCollection services,
            IEnumerable<Assembly> assemblies,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            var entityTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(BaseEntity).IsAssignableFrom(t) 
                         && !t.IsAbstract 
                         && !t.IsInterface)
                .ToList();

            foreach (var entityType in entityTypes)
            {
                var repositoryInterface = typeof(IGenericRepository<>).MakeGenericType(entityType);
                var repositoryImplementation = typeof(GenericRepository<>).MakeGenericType(entityType);

                services.Add(
                    new ServiceDescriptor(
                        repositoryInterface,
                        repositoryImplementation,
                        lifetime));
            }

            return services;
        }

        /// <summary>
        /// Registers GenericRepository for all entity types that inherit from BaseEntity in the calling assembly.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="lifetime">The service lifetime. Defaults to Scoped.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddGenericRepositoriesFromAssembly(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            var assembly = Assembly.GetCallingAssembly();
            return services.AddGenericRepositories(new[] { assembly }, lifetime);
        }

        /// <summary>
        /// Registers the DbContext and GenericRepository for a specific entity type.
        /// This is a convenience method that registers both the context and repository.
        /// </summary>
        /// <typeparam name="TContext">The DbContext type that inherits from BaseContext.</typeparam>
        /// <typeparam name="TEntity">The entity type that inherits from BaseEntity.</typeparam>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="optionsAction">Action to configure the DbContext options.</param>
        /// <param name="contextLifetime">The service lifetime for the DbContext. Defaults to Scoped.</param>
        /// <param name="repositoryLifetime">The service lifetime for the repository. Defaults to Scoped.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddBaseContext<TContext>(
            this IServiceCollection services,
            Action<DbContextOptionsBuilder>? optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped
            )
            where TContext : BaseContext
        {
            if (optionsAction != null)
            {
                services.AddDbContext<TContext>(optionsAction, contextLifetime);
            }
            else
            {
                services.AddDbContext<TContext>(contextLifetime);
            }
            return services;
        }
    }
}


