using CursedQueryable.IntegrationTests.Abstract.BasicTests.Helpers;
using CursedQueryable.IntegrationTests.Abstract.CursedQueryableTests;
using CursedQueryable.IntegrationTests.Data;
using CursedQueryable.IntegrationTests.Data.Entities;
using CursedQueryable.Options;
using Xunit;

namespace CursedQueryable.IntegrationTests;

[Trait("Category", "Cursed Framework - Integration Tests")]
public class UsingCursedQueryable() : CursedQueryableWithKittensTestsBase(PrimaryKeys, GetRootQueryable())
{
    private static readonly IReadOnlyList<string> PrimaryKeys = new[]
    {
        nameof(Cat.Id)
    };

    protected override FrameworkOptions FrameworkOptions =>
        new(base.FrameworkOptions)
        {
            Provider = new TestProvider()
        };

    private static IQueryable<Cat> GetRootQueryable()
    {
        return TestData.GenerateCats().ToList().AsQueryable();
    }
}