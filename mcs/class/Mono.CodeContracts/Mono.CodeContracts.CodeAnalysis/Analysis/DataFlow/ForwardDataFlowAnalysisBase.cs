using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow
{
  public abstract class ForwardDataFlowAnalysisBase<AbstractState, Type> : DataFlowAnalysisBase<AbstractState, Type>
  {
    private readonly Dictionary<APC, AbstractState> post_state = new Dictionary<APC, AbstractState> (); 

    protected ForwardDataFlowAnalysisBase(ICFG cfg) : base (cfg)
    {
    }

    public bool GetPreState(APC pc, out AbstractState state)
    {
      bool noInfo;
      state = this.GetPreState (pc, default(AbstractState), out noInfo);
      return !noInfo;
    }

    private AbstractState GetPreState(APC pc, AbstractState ifMissing, out bool noInfo)
    {
      List<APC> rest = null;
      APC tmp = pc;
      APC singlePredecessor;
      AbstractState state;
      bool weHaveState;
      while ((weHaveState = this.JoinState.TryGetValue(tmp, out state)) &&
        !this.RequiresJoining(tmp) && this.CFG.HasSinglePredecessor(pc, out singlePredecessor)) {
        tmp = singlePredecessor;
        
        if (rest == null)
          rest = new List<APC> ();
        rest.Insert (0, tmp);
      }
      if (!weHaveState) {
        noInfo = true;
        return ifMissing;
      }

      bool listWasNotEmpty = rest != null && rest.Count > 0;
      while (rest != null && rest.Count > 0) {
        if (this.IsBottom (rest[0], state)) {
          noInfo = false;
          return state;
        }
        state = this.MutableVersion (state, rest[0]);
        state = this.Transfer (rest[0], state);
        if (this.IsBottom (rest[0], state)) {
          noInfo = false;
          return state;
        }

        rest.RemoveAt (0);
        if (rest.Count > 0)
          this.JoinState.Add (rest[0], this.ImmutableVersion (state, rest[0]));
      }
     
      if (listWasNotEmpty)
        this.JoinState.Add (pc, this.ImmutableVersion (state, pc));
      noInfo = false;
      return state;
    }

    public bool GetPostState(APC pc, out AbstractState state)
    {
      if (this.post_state.TryGetValue(pc, out state))
        return true;

      APC singleSuccessor;
      if (pc.Block.Count <= pc.Index || this.CFG.HasSinglePredecessor(pc, out singleSuccessor) && !this.RequiresJoining(singleSuccessor))
        return this.GetPreState (pc, out state);
      AbstractState ifFound;
      if (!this.GetPreState (pc, out ifFound))
        return false;
      state = this.MutableVersion (ifFound, pc);
      state = this.Transfer (pc, state);
      
      this.post_state.Add (pc, state);
      return true;
    }

    public void Run(AbstractState startState)
    {
      Initialize (this.CFG.Entry, startState);
      ComputeFixPoint ();
    }

    protected override int WorkingListComparer(APC a, APC b)
    {
      return b.Block.ReversePostOrderIndex - a.Block.ReversePostOrderIndex;
    }

    protected override bool RequiresJoining(APC pc)
    {
      return this.CFG.IsJoinPoint (pc);
    }

    protected override bool HasSingleSuccessor(APC pc, out APC next)
    {
      return this.CFG.HasSingleSuccessor (pc, out next);
    }

    protected override IEnumerable<APC> Successors(APC pc)
    {
      return this.CFG.Successors (pc);
    }
  }
}