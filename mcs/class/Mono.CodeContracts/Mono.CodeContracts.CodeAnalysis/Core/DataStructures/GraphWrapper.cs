using System;
using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public class GraphWrapper<Node, Info> : IGraph<Node, Info>
  {
    private readonly IEnumerable<Node> nodes;
    private readonly Func<Node, IEnumerable<Pair<Info, Node>>> successors;

    public GraphWrapper(IEnumerable<Node> nodes, Func<Node, IEnumerable<Pair<Info, Node>>> successors)
    {
      this.nodes = nodes;
      this.successors = successors;
    }

    #region Implementation of IGraph<Node,Info>
    public IEnumerable<Node> Nodes
    {
      get { return this.nodes; }
    }

    public IEnumerable<Pair<Info, Node>> Successors(Node node)
    {
      return this.successors (node);
    }
    #endregion
  }
}