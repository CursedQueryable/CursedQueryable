using CursedQueryable.Options;

namespace CursedQueryable.Extensions;

/// <summary>
///     Static global configuration for CursedQueryable, used when calling the provided .ToPage() and .ToPageAsync()
///     extension methods.
/// </summary>
public static class CursedExtensionsConfig
{
    private static FrameworkOptions? Cached;
    private static readonly object Lock = new();

    /// <summary>
    ///     Configures the default global FrameworkOptions when using CursedQueryable.
    /// </summary>
    public static void Configure(Action<FrameworkOptions> opts)
    {
        Cached ??= AutoConfigure();
        opts.Invoke(Cached);
    }

    /// <summary>
    ///     Returns a cloned instance of the default global FrameworkOptions.
    /// </summary>
    public static FrameworkOptions Get()
    {
        Cached ??= AutoConfigure();
        return new FrameworkOptions(Cached);
    }

    private static FrameworkOptions AutoConfigure()
    {
        lock (Lock)
        {
            var type = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly
                    .GetTypes()
                    .Where(type => type is { IsClass: true, IsAbstract: false }
                                   && typeof(ICursedConfigurator).IsAssignableFrom(type)))
                .FirstOrDefault();

            if (type != null)
            {
                var configurator = (ICursedConfigurator)Activator.CreateInstance(type)!;
                return configurator.Configure();
            }

            return new FrameworkOptions();
        }
    }
}