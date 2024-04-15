using Xunit;

namespace CursedQueryable.UnitTests.Fixtures;

/// <summary>
///     Due to use of static configuration we need to ensure certain tests don't run in parallel.
/// </summary>
[CollectionDefinition(nameof(StaticCollection), DisableParallelization = true)]
public class StaticCollection;