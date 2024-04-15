using CursedQueryable.IntegrationTests.Abstract.BasicTests.Helpers;
using CursedQueryable.IntegrationTests.Abstract.CursedQueryableTests;
using CursedQueryable.IntegrationTests.Data;
using CursedQueryable.IntegrationTests.Data.Entities;
using CursedQueryable.Options;
using Xunit;

namespace CursedQueryable.IntegrationTests;

[Trait("Category", "Cursed Framework - Integration Tests")]
public class UsingCursedQueryableWithCompositePrimaryKey()
    : CursedQueryableTestsBase<ManyKeyedCat>(PrimaryKeys, GetRootQueryable())
{
    private static readonly IReadOnlyList<string> PrimaryKeys = new[]
    {
        nameof(ManyKeyedCat.Id1),
        nameof(ManyKeyedCat.Id2),
        nameof(ManyKeyedCat.Id3)
    };

    protected override FrameworkOptions FrameworkOptions =>
        new(base.FrameworkOptions)
        {
            Provider = new TestProvider()
        };

    private static IQueryable<ManyKeyedCat> GetRootQueryable()
    {
        return TestData.GenerateManyKeyedCats().ToList().AsQueryable();
    }
}