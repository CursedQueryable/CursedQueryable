using System.Linq.Expressions;
using System.Reflection;

namespace CursedQueryable.IntegrationTests.Abstract.CursedQueryableTests.Helpers;

public class BaselineVisitor : ExpressionVisitor
{
    protected static readonly MethodInfo OrderByMethodInfo =
        new Func<IQueryable<object>, Expression<Func<object, object>>,
                IQueryable<object>>(Queryable.OrderBy)
            .GetMethodInfo()
            .GetGenericMethodDefinition();

    protected static readonly MethodInfo OrderByDescendingMethodInfo =
        new Func<IQueryable<object>, Expression<Func<object, object>>,
                IQueryable<object>>(Queryable.OrderByDescending)
            .GetMethodInfo()
            .GetGenericMethodDefinition();

    protected static readonly MethodInfo ThenByMethodInfo =
        new Func<IOrderedQueryable<object>, Expression<Func<object, object>>,
                IOrderedQueryable<object>>(Queryable.ThenBy)
            .GetMethodInfo()
            .GetGenericMethodDefinition();

    protected static readonly MethodInfo ThenByDescendingMethodInfo =
        new Func<IOrderedQueryable<object>, Expression<Func<object, object>>,
                IOrderedQueryable<object>>(Queryable.ThenByDescending)
            .GetMethodInfo()
            .GetGenericMethodDefinition();

    protected static readonly MethodInfo SkipMethodInfo =
        new Func<IQueryable<object>, int, IQueryable<object>>(Queryable.Skip)
            .GetMethodInfo()
            .GetGenericMethodDefinition();
}

public class BaselineVisitor<T>(IReadOnlyCollection<string> primaryKeys, Direction direction, int? skip)
    : BaselineVisitor
{
    private bool _hasSkip;
    private bool _orderByKeysInjected;
    private int _recursion;

    public static IQueryable<TOut> Simplify<TOut>(
        (IQueryable queryable, IReadOnlyCollection<string> keyNames, Direction direction) ctx, int? skip = null)
    {
        var expression = new BaselineVisitor<T>(ctx.keyNames, ctx.direction, skip).Visit(ctx.queryable.Expression)!;
        return ctx.queryable.Provider.CreateQuery<TOut>(expression);
    }

    public override Expression? Visit(Expression? node)
    {
        try
        {
            if (node == null)
                return null;

            var position = ++_recursion;

            node = base.Visit(node);

            if (position == 1 && !_orderByKeysInjected)
                node = InjectOrderByKeys(node);

            if (position == 1 && !_hasSkip)
                node = InjectSkip(node);
        }
        catch
        {
            // ignored
        }

        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.Name is nameof(Queryable.Skip))
        {
            _hasSkip = true;
            node = AdjustSkip(node);
        }

        if (direction == Direction.Backwards && node.Method.Name is nameof(Queryable.OrderBy)
                or nameof(Queryable.ThenBy)
                or nameof(Queryable.OrderByDescending)
                or nameof(Queryable.ThenByDescending))
            node = FlipOrdering(node);

        var visited = Visit(node.Arguments[0])!;

        if (ShouldInjectOrdering(node))
            visited = InjectOrderByKeys(visited);

        if (ShouldInjectSkip(node))
            visited = InjectSkip(visited);

        if (node.Arguments[0] != visited)
            return Swap(node, visited);

        return node;
    }

    private bool ShouldInjectOrdering(MethodCallExpression node)
    {
        return !_orderByKeysInjected
               && (node.Method.Name is nameof(Queryable.Select) or nameof(Queryable.Take) or nameof(Queryable.Skip)
                   || (node.Method.Name is not
                       (nameof(Queryable.OrderBy)
                       or nameof(Queryable.ThenBy)
                       or nameof(Queryable.OrderByDescending)
                       or nameof(Queryable.ThenByDescending)
                       ) && node.Arguments[0] is MethodCallExpression
                   {
                       Method.Name: nameof(Queryable.OrderBy)
                       or nameof(Queryable.ThenBy)
                       or nameof(Queryable.OrderByDescending)
                       or nameof(Queryable.ThenByDescending)
                   }));
    }

    private bool ShouldInjectSkip(MethodCallExpression node)
    {
        return !_hasSkip && node.Method.Name is nameof(Queryable.Take);
    }

    private static MethodCallExpression FlipOrdering(MethodCallExpression node)
    {
        var args = node.Arguments.ToList();

        var method = node.Method.Name switch
        {
            nameof(Queryable.OrderBy) => OrderByDescendingMethodInfo,
            nameof(Queryable.OrderByDescending) => OrderByMethodInfo,
            nameof(Queryable.ThenBy) => ThenByDescendingMethodInfo,
            nameof(Queryable.ThenByDescending) => ThenByMethodInfo,
            _ => throw new ArgumentOutOfRangeException()
        };

        method = method.MakeGenericMethod(node.Type.GetGenericArguments()[0],
            ((LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand).ReturnType);

        return Expression.Call(null, method, args);
    }

    private MethodCallExpression AdjustSkip(MethodCallExpression node)
    {
        var args = node.Arguments.ToList();
        var origValue = (int)((ConstantExpression)args[1]).Value!;

        int newValue;

        if (skip.HasValue)
            newValue = origValue * 2 + 1;
        else
            newValue = origValue;

        args[1] = Expression.Constant(newValue, args[1].Type);

        return Expression.Call(null, node.Method, args);
    }

    private Expression InjectSkip(Expression antecedent)
    {
        _hasSkip = true;

        if (!skip.HasValue)
            return antecedent;

        var method = SkipMethodInfo.MakeGenericMethod(antecedent.Type.GetGenericArguments()[0]);
        var skipConst = Expression.Constant(skip.Value);
        return Expression.Call(null, method, antecedent, skipConst);
    }

    private Expression InjectOrderByKeys(Expression antecedent)
    {
        _orderByKeysInjected = true;
        var entityType = typeof(T);

        var parameter = Expression.Parameter(entityType, nameof(InjectOrderByKeys));
        var useThenBy = antecedent is MethodCallExpression
        {
            Method.Name: nameof(Queryable.OrderBy)
            or nameof(Queryable.ThenBy)
            or nameof(Queryable.OrderByDescending)
            or nameof(Queryable.ThenByDescending)
        };

        foreach (var keyName in primaryKeys)
        {
            var property = entityType.GetProperty(keyName)!;
            Expression expression = Expression.Property(parameter, property);
            expression = Expression.Lambda(expression, parameter);

            MethodInfo method;

            if (useThenBy)
            {
                if (direction == Direction.Backwards)
                    method = ThenByDescendingMethodInfo.MakeGenericMethod(entityType, property.PropertyType);
                else
                    method = ThenByMethodInfo.MakeGenericMethod(entityType, property.PropertyType);
            }
            else
            {
                if (direction == Direction.Backwards)
                    method = OrderByDescendingMethodInfo.MakeGenericMethod(entityType, property.PropertyType);
                else
                    method = OrderByMethodInfo.MakeGenericMethod(entityType, property.PropertyType);
            }

            useThenBy = true;
            antecedent = Expression.Call(null, method, antecedent, expression);
        }

        return antecedent;
    }

    private static MethodCallExpression Swap(MethodCallExpression node, Expression newAntecedent)
    {
        var args = node.Arguments.ToList();
        args[0] = newAntecedent;

        return Expression.Call(null, node.Method, args);
    }
}