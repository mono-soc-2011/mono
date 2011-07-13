namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class Return : ExpressionStatement
  {
    public Return()
    {
      this.NodeType = NodeType.Return;
    }

    public Return(Expression expression)
      : base(expression)
    {
      this.NodeType = NodeType.Return;
    }

    public override string ToString()
    {
      return string.Format ("return {0};", this.Expression);
    }
    
  }
}