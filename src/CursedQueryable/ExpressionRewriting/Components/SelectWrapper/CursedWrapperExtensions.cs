using CursedQueryable.Paging;

namespace CursedQueryable.ExpressionRewriting.Components.SelectWrapper;

internal static class CursedWrapperExtensions
{
    private static readonly Type WrapperType = typeof(CursedWrapper<>);

    public static bool IsCursedWrapper(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == WrapperType;
    }

    public static Type ToCursedWrapper(this Type type)
    {
        if (IsCursedWrapper(type))
            return type;

        return WrapperType.MakeGenericType(type);
    }
}