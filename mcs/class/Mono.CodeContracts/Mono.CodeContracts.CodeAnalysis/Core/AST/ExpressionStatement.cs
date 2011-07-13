namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class ExpressionStatement : Statement
  {
    public Expression Expression { get; set; }

    public ExpressionStatement() : base (NodeType.ExpressionStatement)
    {
    }

    public ExpressionStatement(Expression expression)
      : base(NodeType.ExpressionStatement)
    {
      this.Expression = expression;
    }

    public override string ToString()
    {
      return string.Format ("ExpressionStatement({0})", this.Expression);
    }
  }
}