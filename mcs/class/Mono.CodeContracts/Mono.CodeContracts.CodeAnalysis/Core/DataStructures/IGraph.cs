using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public interface IGraph<Node, EdgeInfo>
  {
    IEnumerable<Node> Nodes { get; }
    IEnumerable<Pair<EdgeInfo, Node>> Successors(Node node);
  }
}