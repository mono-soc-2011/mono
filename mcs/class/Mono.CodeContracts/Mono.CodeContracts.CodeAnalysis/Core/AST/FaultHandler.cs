namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class FaultHandler : Statement
  {
    public Block Block { get; set; }
    
    public FaultHandler() : base (NodeType.FaultHandler)
    {
    }
    public FaultHandler(Block block) : this()
    {
      this.Block = block;
    }
  }
}