using CursedQueryable.ExpressionRewriting.Components.SelectWrapper;
using CursedQueryable.IntegrationTests.Abstract.CursedQueryableTests.Helpers;
using CursedQueryable.IntegrationTests.Data.Entities;
using CursedQueryable.Options;
using CursedQueryable.Paging;
using Xunit;

namespace CursedQueryable.IntegrationTests.Abstract.CursedQueryableTests;

public abstract class CursedQueryableTestsBase<TCat>(IReadOnlyList<string> primaryKeys, IQueryable<TCat> rootQueryable)
    where TCat : class, ICat
{
    protected virtual FrameworkOptions FrameworkOptions { get; } = new()
    {
        BadCursorBehaviour = BadCursorBehaviour.ThrowException
    };

    protected IQueryable<TCat> RootQueryable { get; } = rootQueryable;

    protected async Task RunScenario<T>(IQueryable<T> queryable, TestOptions? options = null,
        Action<ScenarioContext>? opts = null) where T : class
    {
        var context = new ScenarioContext
        {
            Queryable = queryable,
            PrimaryKeys = primaryKeys,
            TestOptions = options ?? new TestOptions(),
            FrameworkOptions = FrameworkOptions
        };

        opts?.Invoke(context);

        Type runnerType;

        if (typeof(T).IsCursedWrapper())
        {
            var outType = typeof(T).GetGenericArguments()[0];
            runnerType = typeof(ScenarioRunner<,,>).MakeGenericType(typeof(TCat), outType, typeof(T));
        }
        else
            runnerType = typeof(ScenarioRunner<,,>).MakeGenericType(typeof(TCat), typeof(T), typeof(T));

        var runner = (IScenarioRunner)Activator.CreateInstance(runnerType, [context])!;
        await runner.Run();
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task Base(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable;

        await RunScenario(queryable, options, o => o.ExpectedHasPage = false);
    }

    [Theory]
    [ClassData(typeof(NoCursorScenarios))]
    public async Task Base_no_results(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Where(cat => cat.Name == "fail");

        await RunScenario(queryable, options, o => o.ExpectedHasPage = false);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task Take(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Take(10);

        await RunScenario(queryable, options, o => o.ExpectedHasPage = true);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task Skip(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Skip(10);

        await RunScenario(queryable, options, o => o.ExpectedHasPage = false);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task Skip_Take(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Skip(10)
            .Take(10);

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task OrderBy(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .OrderBy(cat => cat.Sex);

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task OrderByCoalesce(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .OrderBy(cat => cat.DateOfBirth ?? DateTime.MaxValue);

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task OrderByTernary(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .OrderBy(cat => cat.HasExpensiveTastes ? cat.Name : cat.Eman);

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task OrderBy_OrderBy(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .OrderBy(cat => cat.Sex)
            // ReSharper disable once MultipleOrderBy - deliberate for test
            .OrderBy(cat => cat.Name);

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task OrderBy_ThenBy(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .OrderBy(cat => cat.Sex)
            .ThenBy(cat => cat.Name);

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task OrderBy_ThenBy_Where(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .OrderBy(cat => cat.Sex)
            .ThenBy(cat => cat.Name)
            .Where(cat => cat.Name.StartsWith("Moggy 005"));

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task OrderByDescending(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .OrderByDescending(cat => cat.Sex);

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task OrderByDescending_ThenByDescending(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .OrderByDescending(cat => cat.Sex)
            .ThenByDescending(cat => cat.Name);

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task OrderByDescending_ThenByDescending_Where(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .OrderByDescending(cat => cat.Sex)
            .ThenByDescending(cat => cat.Name)
            .Where(cat => cat.Name.StartsWith("Moggy 005"));

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task Select(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Select(cat => cat);

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task SelectP(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Select(cat => new Purrito { Cat = cat, Material = cat.HasExpensiveTastes ? "Silk" : "Cotton" });

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(NoCursorScenarios))]
    public async Task SelectW(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Select(cat => new CursedWrapper<TCat> { Node = cat });

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task Select_Select(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Select(cat => cat)
            .Select(cat => cat);

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task Select_SelectP(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Select(cat => cat)
            .Select(cat => new Purrito { Cat = cat, Material = cat.HasExpensiveTastes ? "Silk" : "Cotton" });

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task Select_SelectP_Select(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Select(cat => cat)
            .Select(cat => new Purrito { Cat = cat, Material = cat.HasExpensiveTastes ? "Silk" : "Cotton" })
            .Select(p => p.Cat);

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(NoCursorScenarios))]
    public async Task SelectW_Select(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Select(cat => new CursedWrapper<TCat> { Hash = 1337, Keys = null, Cols = null, Node = cat })
            .Select(wrapper => wrapper.Node);

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(NoCursorScenarios))]
    public async Task SelectW_SelectW(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Select(cat => new CursedWrapper<TCat> { Node = cat })
            .Select(w => new CursedWrapper<TCat> { Node = w.Node });

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(NoCursorScenarios))]
    public async Task SelectW_SelectP_SelectW(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Select(cat => new CursedWrapper<TCat> { Node = cat })
            .Select(w => new Purrito { Cat = w.Node })
            .Select(p => new CursedWrapper<TCat> { Node = p.Cat });

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task SelectP_Where(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Select(cat => new Purrito { Cat = cat, Material = cat.HasExpensiveTastes ? "Silk" : "Cotton" })
            .Where(p => p.Cat.Name.StartsWith("Moggy 005"));

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task Where(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Where(cat => cat.Sex == Sex.Female);

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task Where_Where(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Where(cat => cat.Sex == Sex.Female)
            .Where(cat => cat.Name.StartsWith("Moggy 001"));

        await RunScenario(queryable, options);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task Where_Take(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Where(cat => cat.Sex == Sex.Female)
            .Take(10);

        await RunScenario(queryable, options, o => o.ExpectedHasPage = true);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task Where_OrderBy_Take(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Where(cat => cat.Sex == Sex.Female)
            .OrderBy(cat => cat.Eman)
            .Take(10);

        await RunScenario(queryable, options, o => o.ExpectedHasPage = true);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task Where_OrderByDescending_Take(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Where(cat => cat.Sex == Sex.Female)
            .OrderByDescending(c => c.Name)
            .Take(10);

        await RunScenario(queryable, options, o => o.ExpectedHasPage = true);
    }

    [Theory]
    [ClassData(typeof(AllScenarios))]
    public async Task Where_OrderBy_Take_SelectP(bool useAsync, bool useCursor, Direction direction)
    {
        var options = new TestOptions(useAsync, useCursor, direction);

        var queryable = RootQueryable
            .Where(cat => cat.Sex == Sex.Female)
            .OrderBy(cat => cat.Eman)
            .Take(10)
            .Select(cat => new Purrito { Cat = cat, Material = cat.HasExpensiveTastes ? "Silk" : "Cotton" });

        await RunScenario(queryable, options, o => o.ExpectedHasPage = true);
    }

    protected class Purrito
    {
        public TCat Cat { get; init; } = default!;
        public string Material { get; init; } = default!;
    }
}