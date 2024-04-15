using CursedQueryable.EntityDescriptors;
using CursedQueryable.IntegrationTests.Abstract.BasicTests.Helpers;
using CursedQueryable.IntegrationTests.Data;
using CursedQueryable.IntegrationTests.Data.Entities;
using CursedQueryable.Options;
using CursedQueryable.Paging;

namespace CursedQueryable.IntegrationTests.Abstract.BasicTests;

public abstract class BasicTestsBase
{
    private readonly PageBuilder<Page<Cat>, Cat, PageOptions> _pageBuilder = new(new PageMapper<Cat>());
    protected readonly IQueryable<Cat> _rootQueryable = TestData.GenerateCats().ToList().AsQueryable();
    protected virtual IEntityDescriptorProvider Provider { get; } = new TestProvider();

    protected async Task<Page<Cat>> ToPage(IQueryable queryable, string? cursor = null,
        Action<PageOptions>? configurator = null)
    {
        var options = new PageOptions
        {
            Cursor = cursor,
            FrameworkOptions = new FrameworkOptions
            {
                BadCursorBehaviour = BadCursorBehaviour.ThrowException,
                Provider = Provider
            }
        };

        configurator?.Invoke(options);

        return await _pageBuilder.ToPageAsync(queryable, options);
    }
}