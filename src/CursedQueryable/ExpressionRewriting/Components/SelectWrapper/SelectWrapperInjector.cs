using System.Linq.Expressions;
using CursedQueryable.EntityDescriptors;
using CursedQueryable.ExpressionRewriting.Common;

namespace CursedQueryable.ExpressionRewriting.Components.SelectWrapper;

/// <summary>
///     Injects any required calls into the expression tree to project into a CursedWrapper.
/// </summary>
internal class SelectWrapperInjector(Ordering ordering) : IExpressionInjector
{
    private bool _hasSelect;

    public void OnTraversingUp(int position, Expression node)
    {
    }

    public Expression OnTraversingDown(int position, Expression node, IEntityDescriptor entityDescriptor)
    {
        if (node is MethodCallExpression mce
            && mce.Method.DeclaringType == typeof(Queryable)
            && mce.Method.Name is nameof(Queryable.Select))
        {
            _hasSelect = true;
            return Rewrite(mce, entityDescriptor);
        }

        if (position == 1 && !_hasSelect)
            return Inject(node, entityDescriptor);

        return node;
    }

    /// <summary>
    ///     Rewrite an existing .Select() to return CursedWrapper&lt;TProjection&gt; instead of TProjection.
    /// </summary>
    private Expression Rewrite(MethodCallExpression node, IEntityDescriptor entityDescriptor)
    {
        if (node.Method.ReturnType.GetGenericArguments()[0].IsCursedWrapper())
            return node;

        var toRewrite = (LambdaExpression)((UnaryExpression)node.Arguments[1]).Operand;
        var context = new SelectWrapperBuilder.Context(node.Arguments[0], toRewrite)
        {
            EntityDescriptor = entityDescriptor,
            ColProps = ordering.Chain
        };

        var builder = new SelectWrapperBuilder(context);
        return builder.Build();
    }

    /// <summary>
    ///     Inject a new .Select() to project TEntity to CursedWrapper&lt;TEntity&gt;.
    /// </summary>
    private Expression Inject(Expression node, IEntityDescriptor entityDescriptor)
    {
        var context = new SelectWrapperBuilder.Context(node)
        {
            EntityDescriptor = entityDescriptor,
            ColProps = ordering.Chain
        };

        var builder = new SelectWrapperBuilder(context);
        return builder.Build();
    }
}