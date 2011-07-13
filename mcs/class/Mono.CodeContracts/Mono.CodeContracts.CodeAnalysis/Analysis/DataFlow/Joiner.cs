using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow
{
  public delegate AbstractState Joiner<Label, AbstractState>(Pair<Label, Label> edge, AbstractState newState, AbstractState prevState, out bool weaker, bool widen);
}