using GenericToolKit.Domain.Entities;
using GenericToolKit.Domain.Models;

namespace GenericToolKit.Domain.Interfaces
{
    /// <summary>
    /// Interface for projectable specifications that transform entities to DTOs.
    /// This interface enables projection queries for better performance and separation of concerns.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity.</typeparam>
    /// <typeparam name="TResult">The DTO type that inherits from BaseInOutDTO.</typeparam>
    public interface IProjectableSpecifications<T, TResult>
        where T : BaseEntity
        where TResult : BaseInOutDTO
    {
    }
}


