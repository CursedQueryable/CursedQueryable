namespace CursedQueryable.IntegrationTests.Abstract.CursedQueryableTests.Helpers;

public class TestOptions
{
    public TestOptions()
    {
    }

    public TestOptions(bool useAsync, bool useCursor, Direction direction)
    {
        UseAsync = useAsync;
        UseCursor = useCursor;
        Direction = direction;
    }

    public bool UseCursor { get; }
    public Direction Direction { get; }
    public bool UseAsync { get; }
}