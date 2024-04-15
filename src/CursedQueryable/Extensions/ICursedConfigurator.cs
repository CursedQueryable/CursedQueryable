using CursedQueryable.Options;

namespace CursedQueryable.Extensions;

/// <summary>
///     Implementing this interface allows for modular configuration of CursedQueryable via separate assemblies.
/// </summary>
public interface ICursedConfigurator
{
    /// <summary>
    ///     Return a new FrameworkOptions instance, typically used to specify an implementation of
    ///     <see cref="T:CursedQueryable.EntityDescriptors.IEntityDescriptorProvider" />.
    /// </summary>
    FrameworkOptions Configure();
}