using System.Linq.Expressions;
using System.Reflection;
using CursedQueryable.ExpressionRewriting.Common;

namespace CursedQueryable.ExpressionRewriting.Components.OrderByKeys;

internal class OrderByKeysBuilder(OrderByKeysBuilder.Context context)
{
    public Expression Build()
    {
        var startNewChain = context.StartNewChain;
        var chain = context.Antecedent;

        foreach (var keyProp in context.KeyProps)
        {
            MethodInfo method;

            if (startNewChain)
            {
                method = context.Direction == Direction.Backwards
                    ? Consts.OrderByDescendingMethodInfo
                    : Consts.OrderByMethodInfo;
            }
            else
            {
                method = context.Direction == Direction.Backwards
                    ? Consts.ThenByDescendingMethodInfo
                    : Consts.ThenByMethodInfo;
            }

            startNewChain = false;
            method = method.MakeGenericMethod(context.Type, keyProp.PropertyType);
            var lambda = GetLambda(keyProp);
            chain = Expression.Call(null, method, chain, lambda);
        }

        return chain;
    }

    private LambdaExpression GetLambda(PropertyInfo keyProp)
    {
        var parameter = Expression.Parameter(context.Type, nameof(OrderByKeysBuilder));
        var propertyExpression = Expression.Property(parameter, keyProp);
        return Expression.Lambda(propertyExpression, parameter);
    }

    public class Context
    {
        public Type Type { get; init; } = default!;
        public IReadOnlyCollection<PropertyInfo> KeyProps { get; init; } = default!;
        public Direction Direction { get; init; }
        public Expression Antecedent { get; init; } = default!;
        public bool StartNewChain { get; init; }
    }
}