namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public abstract class Member : Node
  {
    public Member(NodeType nodeType) : base (nodeType)
    {
    }

    public abstract bool IsStatic { get; }
   
  }
}