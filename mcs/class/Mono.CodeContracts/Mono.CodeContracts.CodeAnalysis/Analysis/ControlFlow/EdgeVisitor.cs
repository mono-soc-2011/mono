namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow
{
  public delegate void EdgeVisitor<Node, Info>(Node source, Info info, Node target);
}