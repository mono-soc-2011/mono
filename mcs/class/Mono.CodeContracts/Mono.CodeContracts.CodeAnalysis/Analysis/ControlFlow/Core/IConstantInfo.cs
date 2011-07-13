using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core
{
  public interface IEdgeSubroutineAdaptor
  {
    LispList<Pair<EdgeTag, Subroutine>> GetOrdinaryEdgeSubroutinesInternal(CFGBlock @from, CFGBlock to, LispList<Edge<CFGBlock, EdgeTag>> context);
  }
}