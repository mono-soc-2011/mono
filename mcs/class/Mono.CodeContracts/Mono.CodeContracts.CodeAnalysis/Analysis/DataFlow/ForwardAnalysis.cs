using System;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.Core;
using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow
{
  public class ForwardAnalysis<AbstractState, Type, EdgeData> :
    ForwardDataFlowAnalysisBase<AbstractState, Type>,
    IFixPointInfo<APC, AbstractState>
  {
    private readonly EdgeConverter<APC, AbstractState, EdgeData> edge_converter;
    private readonly Func<APC, APC, EdgeData> edge_data_getter;
    private readonly Func<AbstractState, AbstractState> immutable_version;
    private readonly Func<APC, AbstractState, bool> is_bottom;
    private readonly Joiner<APC, AbstractState> joiner;
    private readonly Func<AbstractState, AbstractState> mutable_version;
    private readonly Func<APC, AbstractState, AbstractState> transfer;

    public ForwardAnalysis(ICFG cfg,
                                 Func<APC, AbstractState, AbstractState> transfer,
                                 Joiner<APC, AbstractState> joiner,
                                 Func<AbstractState, AbstractState> immutableVersion,
                                 Func<AbstractState, AbstractState> mutableVersion,
                                 EdgeConverter<APC, AbstractState, EdgeData> edgeConverter, 
                                 Func<APC, APC, EdgeData> edgeDataGetter, 
                                 Func<APC, AbstractState, bool> isBottom) : base (cfg)
    {
      this.transfer = transfer;
      this.joiner = joiner;
      this.immutable_version = immutableVersion;
      this.mutable_version = mutableVersion;
      this.edge_converter = edgeConverter;
      this.edge_data_getter = edgeDataGetter;
      this.is_bottom = isBottom;
    }

    public static ForwardAnalysis<AbstractState, Type, EdgeData> Make<Local, Parameter, Method, Field, Source, Dest, Context>(
      IILDecoder<APC, Local, Parameter, Method, Field, Type, Source, Dest, Context, EdgeData> decoder, 
      IAnalysis<APC, AbstractState, IILVisitor<APC, Local, Parameter, Method, Field, Type, Source, Dest, AbstractState, AbstractState>, EdgeData> analysis)
      where Context : IMethodContext<Field, Method>
    {
      IILVisitor<APC, Local, Parameter, Method, Field, Type, Source, Dest, AbstractState, AbstractState> visitor = analysis.GetVisitor ();
      var forwardAnalysisSolver = new ForwardAnalysis<AbstractState, Type, EdgeData> (
        decoder.Context.MethodContext.CFG,
        (pc, state) => decoder.ForwardDecode<AbstractState, AbstractState, IILVisitor<APC, Local, Parameter, Method, Field, Type, Source, Dest, AbstractState, AbstractState>> (pc, visitor, state),
        analysis.Join,
        analysis.ImmutableVersion,
        analysis.MutableVersion,
        analysis.EdgeConversion,
        decoder.EdgeData,
        (pc, state) => {
          if (!decoder.IsUnreachable (pc))
            return analysis.IsBottom (pc, state);

          return true;
        }
        );
     
      return forwardAnalysisSolver;
    }

    #region IFixPointInfo<APC,AbstractState> Members
    public bool PreState(APC pc, out AbstractState state)
    {
      return this.GetPreState (pc, out state);
    }

    public bool PostState(APC pc, out AbstractState state)
    {
      return this.GetPostState (pc, out state);
    }
    #endregion

    protected override void PushState(APC from, APC next, AbstractState state)
    {
      EdgeData data = this.edge_data_getter (from, next);
      AbstractState pushState = this.edge_converter (from, next, RequiresJoining (next), data, state);
      base.PushState (from, next, pushState);
    }

    protected override bool Join(Pair<APC, APC> edge, AbstractState newState, AbstractState existingState, out AbstractState joinedState, bool widen)
    {
      bool weaker;
      joinedState = this.joiner (edge, newState, existingState, out weaker, widen);

      return weaker;
    }

    protected override bool IsBottom(APC pc, AbstractState state)
    {
      return this.is_bottom (pc, state);
    }

    protected override AbstractState Transfer(APC pc, AbstractState state)
    {
      AbstractState resultState = this.transfer (pc, state);

      return resultState;
    }

    protected override AbstractState MutableVersion(AbstractState state, APC at)
    {
      return this.mutable_version (state);
    }

    protected override AbstractState ImmutableVersion(AbstractState state, APC at)
    {
      return this.immutable_version (state);
    }
  }
}