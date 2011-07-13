using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class Construct : NaryExpression
  {
    public Expression Constructor { get; set; }

    public Construct()
      : base (null, NodeType.Construct)
    {
    }

    public Construct(Expression constructor, List<Expression> arguments)
      : base (arguments, NodeType.Construct)
    {
      Constructor = constructor;
    }
  }
}