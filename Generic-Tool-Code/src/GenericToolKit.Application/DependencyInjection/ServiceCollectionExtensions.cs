using GenericToolKit.Application.Services;
using GenericToolKit.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GenericToolKit.Application.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring dependency injection for Generic Toolkit base services.
    /// These extensions allow consumer microservices to easily register all base services.
    /// Following the Dependency Inversion Principle (SOLID) and Clean Architecture patterns.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the GenericService for a specific entity type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type that inherits from BaseEntity.</typeparam>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="lifetime">The service lifetime. Defaults to Scoped.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddGenericService<TEntity>(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TEntity : BaseEntity
        {
            services.Add(
                new ServiceDescriptor(
                    typeof(IGenericService<TEntity>),
                    typeof(GenericService<TEntity>),
                    lifetime));

            return services;
        }

        /// <summary>
        /// Registers GenericService for all entity types that inherit from BaseEntity in the specified assemblies.
        /// This method automatically discovers and registers services for all entities.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="assemblies">The assemblies to scan for entity types.</param>
        /// <param name="lifetime">The service lifetime. Defaults to Scoped.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddGenericServices(
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
                var serviceInterface = typeof(IGenericService<>).MakeGenericType(entityType);
                var serviceImplementation = typeof(GenericService<>).MakeGenericType(entityType);

                services.Add(
                    new ServiceDescriptor(
                        serviceInterface,
                        serviceImplementation,
                        lifetime));
            }

            return services;
        }

        /// <summary>
        /// Registers GenericService for all entity types that inherit from BaseEntity in the calling assembly.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="lifetime">The service lifetime. Defaults to Scoped.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddGenericServicesFromAssembly(
            this IServiceCollection services,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            var assembly = Assembly.GetCallingAssembly();
            return services.AddGenericServices(new[] { assembly }, lifetime);
        }
    }
}


