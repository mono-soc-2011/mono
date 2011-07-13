namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class Expression : Node
  {
    private TypeNode type;

    public virtual TypeNode Type
    {
      get { return this.type; }
      set { this.type = value; }
    }

    public Expression(NodeType nodeType) : base (nodeType)
    {
    }

    public Expression(NodeType nodeType, TypeNode type) : base (nodeType)
    {
      this.type = type;
    }
  }
}