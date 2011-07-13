namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class Statement : Node
  {
    public Statement(NodeType nodeType) : base (nodeType)
    {

    }

    public override string ToString()
    {
      return string.Format ("Statement({0})", this.NodeType);
    }
  }
}