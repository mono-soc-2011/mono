namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class AssignmentStatement : Statement
  {
    public AssignmentStatement(Expression source, Expression target)
      : base (NodeType.AssignmentStatement)
    {
      Source = source;
      Target = target;
    }

    public Expression Source { get; set; }
    public Expression Target { get; set; }

    public override string ToString()
    {
      return string.Format ("{0} := {1};", Target, Source);
    }
  }
}