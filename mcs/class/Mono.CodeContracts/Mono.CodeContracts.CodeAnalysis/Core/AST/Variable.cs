namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public abstract class Variable : Expression
  {
    public Variable(NodeType type)
      : base (type)
    {
    }
  }
}