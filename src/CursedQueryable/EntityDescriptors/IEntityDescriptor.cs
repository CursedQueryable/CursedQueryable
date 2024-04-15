using System.Reflection;

namespace CursedQueryable.EntityDescriptors;

/// <summary>
///     Supplies metadata about a root entity type.
/// </summary>
public interface IEntityDescriptor
{
    /// <summary>
    ///     This is the CLR Type used for the root entity.
    /// </summary>
    Type Type { get; }

    /// <summary>
    ///     A collection of all the CLR Type properties that comprise its Primary Key.
    /// </summary>
    /// <remarks>
    ///     This will generally just contain one entry, however composite primary keys will contain more.
    /// </remarks>
    IReadOnlyCollection<PropertyInfo> PrimaryKeyComponents { get; }
}