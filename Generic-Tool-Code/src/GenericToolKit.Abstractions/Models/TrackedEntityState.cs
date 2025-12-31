namespace GenericToolKit.Domain.Models
{
    /// <summary>
    /// Persistence-agnostic representation of an entity tracking state.
    /// </summary>
    public enum TrackedEntityState
    {
        Detached = 0,
        Unchanged = 1,
        Deleted = 2,
        Modified = 3,
        Added = 4
    }
}


