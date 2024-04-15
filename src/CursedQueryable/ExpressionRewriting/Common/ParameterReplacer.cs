using System.Linq.Expressions;

namespace CursedQueryable.ExpressionRewriting.Common;

/// <summary>
///     Replaces the parameters in an expression tree with a different expression.
/// </summary>
internal class ParameterReplacer(Expression replacement) : ExpressionVisitor
{
    private string? _root;

    protected override Expression VisitParameter(ParameterExpression node)
    {
        _root ??= node.Name;

        // Since the expression tree may contain multiple parameters, ensure that only those which match the root
        // parameter are actually replaced.
        // i.e. for 'x => x.Foo.Select(y => y.Bar)' only 'x' should be replaced, not 'y'.
        if (node.Name == _root)
            return replacement;

        return node;
    }
}