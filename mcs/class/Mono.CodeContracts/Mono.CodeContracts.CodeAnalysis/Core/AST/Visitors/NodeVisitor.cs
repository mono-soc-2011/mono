using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST.Visitors
{
  public abstract class NodeVisitor
  {
    public abstract Node Visit(Node node);

    public virtual List<Expression> VisitExpressionList(List<Expression> list)
    {
      if (list == null)
        return null;

      for (int i = 0; i < list.Count; ++i)
        list[i] = (Expression) Visit (list[i]);

      return list;
    }
  }
}