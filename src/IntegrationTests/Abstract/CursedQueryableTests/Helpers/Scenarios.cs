using System.Collections;

namespace CursedQueryable.IntegrationTests.Abstract.CursedQueryableTests.Helpers;

public class AllScenarios : IEnumerable<object[]>
{
    private static readonly List<object[]> Data =
    [
        // UseAsync, UseCursor, Direction
        [true, true, Direction.Forwards],
        [true, true, Direction.Backwards],
        [false, true, Direction.Forwards],
        [false, true, Direction.Backwards],
        .. NoCursorScenarios.Data
    ];

    public IEnumerator<object[]> GetEnumerator()
    {
        return Data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class NoCursorScenarios : IEnumerable<object[]>
{
    public static readonly List<object[]> Data =
    [
        // UseAsync, UseCursor, Direction
        [true, false, Direction.Forwards],
        [true, false, Direction.Backwards],
        [false, false, Direction.Forwards],
        [false, false, Direction.Backwards]
    ];

    public IEnumerator<object[]> GetEnumerator()
    {
        return Data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}