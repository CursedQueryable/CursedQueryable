using CursedQueryable.EntityDescriptors;
using CursedQueryable.Options;

namespace CursedQueryable.Exceptions;

/// <summary>
///     Represents errors that occur if no IEntityDescriptorProvider instance is found.
/// </summary>
public sealed class NoEntityDescriptorProviderException() : Exception(
    $"This exception is raised from the default {nameof(IEntityDescriptorProvider)} implementation. " +
    $"Encountering it means that {nameof(FrameworkOptions)}.{nameof(FrameworkOptions.Provider)} has not been configured correctly.");