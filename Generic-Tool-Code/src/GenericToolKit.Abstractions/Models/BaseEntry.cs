namespace GenericToolKit.Domain.Models
{
    /// <summary>
    /// Represents a tracked entity entry with change tracking information.
    /// Persistence-agnostic (does not expose EF Core types).
    /// </summary>
    /// <typeparam name="T">The type of entity being tracked.</typeparam>
    public class BaseEntry<T>
    {
        /// <summary>
        /// Gets or sets the entity state in the change tracker.
        /// </summary>
        public TrackedEntityState State { get; set; }

        /// <summary>
        /// Gets or sets the current values snapshot.
        /// </summary>
        public object? CurrentValues { get; set; }

        /// <summary>
        /// Gets or sets the original values snapshot.
        /// </summary>
        public object? OriginalValues { get; set; }

        /// <summary>
        /// Gets or sets the entity instance.
        /// </summary>
        public T? Entity { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of modified properties with their new values.
        /// </summary>
        public Dictionary<string, object> ModifiedProperties { get; set; } = new();
    }
}


