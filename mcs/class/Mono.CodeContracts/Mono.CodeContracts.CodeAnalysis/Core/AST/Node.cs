namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public abstract class Node
  {
    public NodeType NodeType { get; set; }

    protected Node(NodeType nodeType)
    {
      NodeType = nodeType;
    }
  }
}