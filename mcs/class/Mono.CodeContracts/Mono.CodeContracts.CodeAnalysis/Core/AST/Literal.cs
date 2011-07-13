namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public sealed class Literal : Expression
  {
    public object Value;
    public static Literal Null = new Literal(null);

    public Literal() : base (NodeType.Literal)
    {
    }

    public Literal(object value)
      : base (NodeType.Literal)
    {
      this.Value = value;
    }

    public Literal(object value, TypeNode type) : base (NodeType.Literal)
    {
      this.Value = value;
      this.Type = type;
    }

    public override string ToString()
    {
      return string.Format ("Literal({0})", this.Value ?? "<null>");
    }
  }
}