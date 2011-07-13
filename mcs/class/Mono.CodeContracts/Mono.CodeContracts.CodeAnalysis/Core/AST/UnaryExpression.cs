namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class UnaryExpression : Expression
  {
    public Expression Operand { get; set; }

    public UnaryExpression(NodeType nodeType)
      : base(nodeType)
    {
    }

    public UnaryExpression(NodeType nodeType, Expression operand)
      : base(nodeType)
    {
      this.Operand = operand;
    }

    public UnaryExpression(NodeType nodeType, Expression operand, TypeNode type)
      : base(nodeType, type)
    {
      this.Operand = operand;
    }

    public override string ToString()
    {
      return string.Format("Unary({0}: {1})", NodeType, Operand);
    }

  }
}