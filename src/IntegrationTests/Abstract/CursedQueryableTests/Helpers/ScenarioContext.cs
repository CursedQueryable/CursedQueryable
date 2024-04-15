using CursedQueryable.Options;

namespace CursedQueryable.IntegrationTests.Abstract.CursedQueryableTests.Helpers;

public class ScenarioContext
{
    public IQueryable Queryable { get; init; } = default!;
    public IReadOnlyCollection<string> PrimaryKeys { get; init; } = default!;
    public FrameworkOptions FrameworkOptions { get; init; } = default!;
    public TestOptions TestOptions { get; set; } = new();
    public bool? ExpectedHasPage { get; set; }
}