using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public abstract class NaryExpression : Expression
  {
    public List<Expression> Arguments { get; set; } 

    public NaryExpression() : base (NodeType.Nop)
    {
    }

    public NaryExpression(List<Expression> arguments, NodeType nodeType )
      : base(nodeType)
    {
      this.Arguments = arguments;
    }
  }
}