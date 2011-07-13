using System.Collections.Generic;
using System.Linq;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class MethodCall : NaryExpression
  {
    public Expression Callee { get; set; }
    
    public MethodCall() : base (null, NodeType.MethodCall)
    {
    }
    public MethodCall(Expression callee, List<Expression> arguments)
      : base(arguments, NodeType.MethodCall)
    {
      this.Callee = callee;
    }

    public MethodCall(Expression callee, List<Expression> arguments, NodeType typeOfCall)
      : base(arguments, typeOfCall)
    {
      this.Callee = callee;
    }

    public override string ToString()
    {
      return string.Format ("MethodCall({0}, args:{{{1}}})", Callee, string.Join (", ", Arguments.Select (it => it.ToString ())));
    }
  }


}