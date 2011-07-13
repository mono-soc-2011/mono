namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class ExceptionHandler : Node
  {
    public NodeType HandlerType { get; set; }
    public Block TryStartBlock { get; set; }
    public Block BlockAfterTryEnd { get; set; }
    public Block HandlerStartBlock { get; set; }
    public Block BlockAfterHandlerEnd { get; set; }
    public Block FilterExpression { get; set; }
    public TypeNode FilterType { get; set; }

    public ExceptionHandler() : base (NodeType.ExceptionHandler)
    {
    }
  }
}