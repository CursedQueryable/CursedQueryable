using CursedQueryable.EntityDescriptors;

namespace CursedQueryable.Options;

/// <summary>
///     Encapsulates configuration options for CursedQueryable.
/// </summary>
public sealed class FrameworkOptions
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FrameworkOptions" /> class.
    /// </summary>
    public FrameworkOptions()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FrameworkOptions" /> class, based on an existing instance.
    /// </summary>
    /// <param name="toCopy">An existing <see cref="FrameworkOptions" /> to shallow copy into this new instance. </param>
    public FrameworkOptions(FrameworkOptions toCopy)
    {
        Provider = toCopy.Provider;
        BadCursorBehaviour = toCopy.BadCursorBehaviour;
        NullBehaviour = toCopy.NullBehaviour;
    }

    /// <summary>
    ///     Provides an EntityDescriptor for the IQueryable.Expression.
    /// </summary>
    public IEntityDescriptorProvider Provider { get; set; } = new NoEntityDescriptorProvider();

    /// <summary>
    ///     Determines how bad cursors are handled.
    /// </summary>
    /// <remarks>
    ///     The default behaviour is to ignore these and return a result as if no cursor had been provided.
    /// </remarks>
    public BadCursorBehaviour BadCursorBehaviour { get; set; }

    /// <summary>
    ///     Determines how the underlying database treats NULL values.
    /// </summary>
    /// <remarks>
    ///     This *must* be set to the correct value that mirrors the database behaviour. The default is
    ///     NullBehaviour.SmallerThanNonNullable, suitable for most databases.
    /// </remarks>
    public NullBehaviour NullBehaviour { get; set; }
}