namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class BinaryExpression : Expression
  {
    public Expression Operand1 { get; set; }
    public Expression Operand2 { get; set; }

    public BinaryExpression(NodeType nodeType) : base (nodeType)
    {
    }

    public BinaryExpression(NodeType nodeType, Expression operand1, Expression operand2) : base (nodeType)
    {
      this.Operand1 = operand1;
      this.Operand2 = operand2;
    }

    public BinaryExpression(NodeType nodeType, Expression operand1, Expression operand2, TypeNode type) 
      : base(nodeType, type)
    {
      this.Operand1 = operand1;
      this.Operand2 = operand2;
    }

    public override string ToString()
    {
      return string.Format ("({1} :{0}: {2})", NodeType, Operand1, Operand2);
    }
  }
}