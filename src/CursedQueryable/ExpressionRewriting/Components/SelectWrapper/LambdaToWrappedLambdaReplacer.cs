using System.Linq.Expressions;
using CursedQueryable.ExpressionRewriting.Common;
using CursedQueryable.Paging;

namespace CursedQueryable.ExpressionRewriting.Components.SelectWrapper;

/// <summary>
///     Replace lambdas in the expression tree with ones that expect CursedWrapper&lt;T&gt; instead of T.
/// </summary>
internal class LambdaToWrappedLambdaReplacer : ExpressionVisitor
{
    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        // Replace the parameter type from T to CursedWrapper<T>
        var wrappedType = node.Parameters[0].Type.ToCursedWrapper();
        var parameters = node.Parameters.ToList();
        parameters[0] = Expression.Parameter(wrappedType, parameters[0].Name);

        // Now, rewrite the body to match the type change. Expressions like x => x.Id will become x => x.Node.Id instead.
        var nodeMemberAccess = Expression.PropertyOrField(parameters[0], nameof(CursedWrapper<object>.Node));
        var replacer = new ParameterReplacer(nodeMemberAccess);
        var body = replacer.Visit(node.Body)!;

        return Expression.Lambda(body, parameters);
    }
}