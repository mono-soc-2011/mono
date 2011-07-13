using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow
{
  public abstract class DataFlowAnalysisBase<AbstractState, Type> :
    IEqualityComparer<APC>
  {
    protected ICFG CFG;
    protected Dictionary<APC, AbstractState> JoinState;
    protected PriorityQueue<APC> WorkingList;

    protected DataFlowAnalysisBase(ICFG cfg)
    {
      this.CFG = cfg;
      this.WorkingList = new PriorityQueue<APC> (WorkingListComparer);
      this.JoinState = new Dictionary<APC, AbstractState> (this);
    }

    #region IEqualityComparer<APC> Members
    public bool Equals(APC x, APC y)
    {
      return x.Equals (y);
    }

    public int GetHashCode(APC obj)
    {
      return obj.GetHashCode ();
    }
    #endregion

    public void Initialize(APC entryPoint, AbstractState state)
    {
      this.JoinState.Add (entryPoint, state);
      this.WorkingList.Enqueue (entryPoint);
    }

    public virtual void ComputeFixPoint()
    {
      while (this.WorkingList.Count > 0) {
        APC next = this.WorkingList.Dequeue ();
        AbstractState state = MutableVersion (this.JoinState[next], next);
        APC tmp;
        bool repeatOuter = false;
        do {
          tmp = next;
          if (!IsBottom (tmp, state)) state = Transfer (tmp, state);
          else {
            repeatOuter = true;
            break;
          }
        } while (HasSingleSuccessor (tmp, out next) && !RequiresJoining (next));
        if (repeatOuter)
          continue;

        foreach (APC successor in Successors (tmp)) {
          if (!IsBottom (successor, state))
            PushState (tmp, successor, state);
        }
      }
    }

    protected abstract IEnumerable<APC> Successors(APC pc);

    protected virtual void PushState(APC from, APC next, AbstractState state)
    {
      state = ImmutableVersion (state, next);
      if (RequiresJoining (next)) {
        if (!JoinStateAtBlock (new Pair<APC, APC> (from, next), state))
          return;
      } else this.JoinState[next] = state;
      this.WorkingList.Enqueue (next);
    }

    private bool JoinStateAtBlock(Pair<APC, APC> edge, AbstractState state)
    {
      AbstractState existingState;
      if (this.JoinState.TryGetValue (edge.Value, out existingState)) {
        //todo: make widen strategy
        bool widen = false;
        AbstractState joinedState;
        bool result = Join (edge, state, existingState, out joinedState, widen);
        if (result)
          this.JoinState[edge.Value] = ImmutableVersion (joinedState, edge.Value);
        return result;
      }

      this.JoinState.Add (edge.Value, state);
      return true;
    }

    protected abstract int WorkingListComparer(APC a, APC b);

    protected abstract bool Join(Pair<APC, APC> edge, AbstractState newState, AbstractState existingState, out AbstractState joinedState, bool widen);

    protected abstract bool RequiresJoining(APC pc);

    protected abstract bool HasSingleSuccessor(APC pc, out APC next);

    protected abstract bool IsBottom(APC pc, AbstractState state);

    protected abstract AbstractState Transfer(APC pc, AbstractState state);

    protected abstract AbstractState MutableVersion(AbstractState state, APC at);
    protected abstract AbstractState ImmutableVersion(AbstractState state, APC at);
  }
}