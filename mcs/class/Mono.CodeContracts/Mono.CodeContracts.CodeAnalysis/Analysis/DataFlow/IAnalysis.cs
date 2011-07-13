using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow
{
  public interface IAnalysis<Label, AbstractState, Visitor, EdgeData>
  {
    Visitor GetVisitor();
    AbstractState Join(Pair<Label, Label> edge, AbstractState newstate, AbstractState prevstate, out bool weaker, bool widen);
    AbstractState ImmutableVersion(AbstractState arg);
    AbstractState MutableVersion(AbstractState arg);
    AbstractState EdgeConversion(APC from, APC to, bool isJoinPoint, EdgeData data, AbstractState state);
    bool IsBottom(Label pc, AbstractState state);
  }
}