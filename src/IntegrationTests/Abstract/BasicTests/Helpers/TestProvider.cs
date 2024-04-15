using System.Linq.Expressions;
using System.Reflection;
using CursedQueryable.EntityDescriptors;
using CursedQueryable.IntegrationTests.Data.Entities;

namespace CursedQueryable.IntegrationTests.Abstract.BasicTests.Helpers;

public class TestProvider : IEntityDescriptorProvider
{
    public bool TryGetEntityDescriptor(Expression expression, out IEntityDescriptor entityDescriptor)
    {
        if (expression.Type.GetGenericArguments()[0] == typeof(Cat))
            entityDescriptor = new CatDescriptor();
        else if (expression.Type.GetGenericArguments()[0] == typeof(ManyKeyedCat))
            entityDescriptor = new ManyKeyedCatDescriptor();
        else
            throw new Exception();

        return true;
    }

    private class CatDescriptor : IEntityDescriptor
    {
        public Type Type { get; } = typeof(Cat);

        public IReadOnlyCollection<PropertyInfo> PrimaryKeyComponents { get; } =
            [typeof(Cat).GetProperty(nameof(Cat.Id))!];
    }

    private class ManyKeyedCatDescriptor : IEntityDescriptor
    {
        public Type Type { get; } = typeof(ManyKeyedCat);

        public IReadOnlyCollection<PropertyInfo> PrimaryKeyComponents { get; } =
        [
            typeof(ManyKeyedCat).GetProperty(nameof(ManyKeyedCat.Id1))!,
            typeof(ManyKeyedCat).GetProperty(nameof(ManyKeyedCat.Id2))!,
            typeof(ManyKeyedCat).GetProperty(nameof(ManyKeyedCat.Id3))!
        ];
    }
}