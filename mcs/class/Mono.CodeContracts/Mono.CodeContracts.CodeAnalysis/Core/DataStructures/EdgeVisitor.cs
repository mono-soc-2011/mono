namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public delegate void EdgeVisitor<Node, Info>(Node source, Info info, Node target);
}