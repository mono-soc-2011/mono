namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class This : Parameter
  {
    public This()
    {
      base.NodeType = NodeType.This;
      base.Name = "this";
    }

    public This(TypeNode type) : this()
    {
      this.Type = type;
    }

    public override string ToString()
    {
      return "<this>";
    }
  }
}