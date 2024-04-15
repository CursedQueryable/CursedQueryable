using System.Linq.Expressions;
using CursedQueryable.EntityDescriptors;

namespace CursedQueryable.ExpressionRewriting.Common;

internal interface IExpressionInjector
{
    void OnTraversingUp(int position, Expression node);
    Expression OnTraversingDown(int position, Expression node, IEntityDescriptor entityDescriptor);
}