using CursedQueryable.IntegrationTests.Abstract.CursedQueryableTests.Helpers;
using CursedQueryable.IntegrationTests.Data.Entities;
using Xunit;

namespace CursedQueryable.IntegrationTests.Abstract.CursedQueryableTests;

public abstract class CursedQueryableWithKittensTestsBase(
    IReadOnlyList<string> primaryKeys,
    IQueryable<Cat> rootQueryable)
    : CursedQueryableTestsBase<Cat>(primaryKeys, rootQueryable)
{
    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task SelectP_with_Kittens(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Take(10)
            .Select(cat => new KittenPurrito
            {
                CatName = cat.Name,
                KittenNames = cat.Kittens.Select(kitten => kitten.Name),
                Material = cat.HasExpensiveTastes ? "Silk" : "Cotton"
            });

        await RunScenario(queryable, options);
    }

    private class KittenPurrito
    {
        public string CatName { get; set; } = default!;
        public IEnumerable<string> KittenNames { get; set; } = default!;
        public string Material { get; set; } = default!;
    }
}