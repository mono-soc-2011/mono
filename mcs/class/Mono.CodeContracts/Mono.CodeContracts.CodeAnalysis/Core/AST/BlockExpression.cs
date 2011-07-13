namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class BlockExpression : Expression
  {
    public Block Block;
    public BlockExpression(Block block, TypeNode type)
      : this()
    {
      this.Block = block;
      this.Type = type;
    }

    public BlockExpression(Block block) : this()
    {
      this.Block = block;
    }

    public BlockExpression()
      : base(NodeType.BlockExpression)
    {
      
    }
  }
}