namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class Filter : Statement
  {
    public Block Block { get; set; }
    public Expression Expression { get; set; }

    public Filter() : base (NodeType.Filter)
    {
    }

    public Filter(Block block, Expression expression)
      : base(NodeType.Filter)
    {
      this.Block = block;
      this.Expression = expression;
    }
  }
}