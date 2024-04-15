using System.Linq.Expressions;
using CursedQueryable.EntityDescriptors;
using CursedQueryable.ExpressionRewriting.Common;
using CursedQueryable.ExpressionRewriting.Guards;
using CursedQueryable.Paging;

namespace CursedQueryable.ExpressionRewriting.Components.SelectWrapper;

internal class SelectWrapperBuilder(SelectWrapperBuilder.Context context)
{
    public MethodCallExpression Build()
    {
        var lambda = BuildLambda();
        var method = Consts.SelectMethodInfo.MakeGenericMethod(context.SourceType, context.ResultType);
        return Expression.Call(null, method, context.Antecedent, lambda);
    }

    private LambdaExpression BuildLambda()
    {
        var bindings = new List<MemberBinding>
        {
            GetHashBinding(),
            GetKeysBinding(),
            GetColsBinding(),
            GetNodeBinding()
        };

        var newExpression = Expression.New(context.ResultType);
        var memberInit = Expression.MemberInit(newExpression, bindings);

        var funcType = typeof(Func<,>).MakeGenericType(context.SourceType, context.ResultType);
        return Expression.Lambda(funcType, memberInit, context.Parameter);
    }

    private MemberBinding GetHashBinding()
    {
        Expression expression;

        if (!context.SourceType.IsCursedWrapper())
            expression = Expression.Constant(context.Hash);
        else
        {
            var sourceProperty = context.SourceType.GetProperty(nameof(CursedWrapper<object>.Hash))!;
            expression = Expression.Property(context.Parameter, sourceProperty);
        }

        var resultProperty = context.ResultType.GetProperty(nameof(CursedWrapper<object>.Hash))!;
        return Expression.Bind(resultProperty, expression);
    }

    private MemberBinding GetKeysBinding()
    {
        Expression expression;

        if (!context.SourceType.IsCursedWrapper())
        {
            var initializers = context.EntityDescriptor
                .PrimaryKeyComponents
                .Select(keyProperty =>
                {
                    var keyExpression = Expression.Property(context.Parameter, keyProperty);
                    return Expression.TypeAs(keyExpression, typeof(object));
                });

            expression = Expression.NewArrayInit(typeof(object), initializers);
        }
        else
        {
            var sourceProperty = context.SourceType.GetProperty(nameof(CursedWrapper<object>.Keys))!;
            expression = Expression.Property(context.Parameter, sourceProperty);
        }

        var resultProperty = context.ResultType.GetProperty(nameof(CursedWrapper<object>.Keys))!;
        return Expression.Bind(resultProperty, expression);
    }

    private MemberBinding GetColsBinding()
    {
        Expression expression;

        if (!context.SourceType.IsCursedWrapper())
        {
            var initializers = context.ColProps
                .Select(colProperty =>
                {
                    var colExpression = new ParameterReplacer(context.Parameter).Visit(colProperty.Expression)!;
                    return Expression.TypeAs(colExpression, typeof(object));
                });

            expression = Expression.NewArrayInit(typeof(object), initializers);
        }
        else
        {
            var sourceProperty = context.SourceType.GetProperty(nameof(CursedWrapper<object>.Cols))!;
            expression = Expression.Property(context.Parameter, sourceProperty);
        }

        var resultProperty = context.ResultType.GetProperty(nameof(CursedWrapper<object>.Cols))!;
        return Expression.Bind(resultProperty, expression);
    }

    private MemberBinding GetNodeBinding()
    {
        Expression expression;

        if (context.LambdaToRewrite != null)
            expression = new ParameterReplacer(context.Parameter).Visit(context.LambdaToRewrite.Body)!;
        else
            expression = context.Parameter;

        var resultProperty = context.ResultType.GetProperty(nameof(CursedWrapper<object>.Node))!;
        return Expression.Bind(resultProperty, expression);
    }

    public class Context
    {
        public Context(Expression antecedent)
        {
            Antecedent = antecedent;
            SourceType = antecedent.Type.GetGenericArguments()[0];
            ResultType = SourceType.ToCursedWrapper();
            Parameter = Expression.Parameter(SourceType, "cursed");
        }

        public Context(Expression antecedent, LambdaExpression toRewrite) : this(antecedent)
        {
            LambdaToRewrite = toRewrite;
            ResultType = LambdaToRewrite.Body.Type.ToCursedWrapper();
        }

        public IEntityDescriptor EntityDescriptor { get; init; } = default!;
        public Expression Antecedent { get; }
        public LambdaExpression? LambdaToRewrite { get; }
        public Type SourceType { get; }
        public Type ResultType { get; }
        public ParameterExpression Parameter { get; }
        public IReadOnlyCollection<Ordering.Info> ColProps { get; init; } = default!;
        public int Hash => new CursorGuard(EntityDescriptor.Type, ColProps).GetHash();
    }
}