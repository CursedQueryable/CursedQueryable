using System.Linq.Expressions;
using System.Reflection;

namespace CursedQueryable.ExpressionRewriting.Common;

internal static class Consts
{
    private static MethodInfo? CachedOrderByMethodInfo;
    private static MethodInfo? CachedOrderByDescendingMethodInfo;
    private static MethodInfo? CachedThenByMethodInfo;
    private static MethodInfo? CachedThenByDescendingMethodInfo;
    private static MethodInfo? CachedWhereMethodInfo;
    private static MethodInfo? CachedSelectMethodInfo;

    public static MethodInfo OrderByMethodInfo => CachedOrderByMethodInfo ??=
        new Func<IQueryable<object>, Expression<Func<object, object>>,
                IQueryable<object>>(Queryable.OrderBy)
            .GetMethodInfo()
            .GetGenericMethodDefinition();

    public static MethodInfo OrderByDescendingMethodInfo => CachedOrderByDescendingMethodInfo ??=
        new Func<IQueryable<object>, Expression<Func<object, object>>, IQueryable<object>>(Queryable.OrderByDescending)
            .GetMethodInfo()
            .GetGenericMethodDefinition();

    public static MethodInfo ThenByMethodInfo => CachedThenByMethodInfo ??=
        new Func<IOrderedQueryable<object>, Expression<Func<object, object>>,
                IOrderedQueryable<object>>(Queryable.ThenBy)
            .GetMethodInfo()
            .GetGenericMethodDefinition();

    public static MethodInfo ThenByDescendingMethodInfo => CachedThenByDescendingMethodInfo ??=
        new Func<IOrderedQueryable<object>, Expression<Func<object, object>>,
                IOrderedQueryable<object>>(Queryable.ThenByDescending)
            .GetMethodInfo()
            .GetGenericMethodDefinition();

    public static MethodInfo WhereMethodInfo => CachedWhereMethodInfo ??=
        new Func<IQueryable<object>, Expression<Func<object, bool>>, IQueryable<object>>(Queryable.Where)
            .GetMethodInfo()
            .GetGenericMethodDefinition();

    public static MethodInfo SelectMethodInfo => CachedSelectMethodInfo ??=
        new Func<IQueryable<object>, Expression<Func<object, object>>, IQueryable<object>>(Queryable.Select)
            .GetMethodInfo()
            .GetGenericMethodDefinition();
}