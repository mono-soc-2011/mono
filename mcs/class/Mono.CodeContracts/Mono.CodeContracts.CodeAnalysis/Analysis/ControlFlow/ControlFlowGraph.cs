using System;
using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow
{
  public class ControlFlowGraph<Method, Type> : ICFG
  {
    private readonly object method_repository;
    private readonly Subroutine method_subroutine;

    public ControlFlowGraph(Subroutine subroutine, object methodRepository)
    {
      this.method_subroutine = subroutine;
      this.method_repository = methodRepository;
    }

    private CFGBlock EntryBlock
    {
      get { return this.method_subroutine.Entry; }
    }

    private CFGBlock ExitBlock
    {
      get { return this.method_subroutine.Exit; }
    }

    private CFGBlock ExceptionExitBlock
    {
      get { return this.method_subroutine.ExceptionExit; }
    }

    public Method CFGMethod
    {
      get
      {
        var methodInfo = this.method_subroutine as IMethodInfo<Method>;
        if (methodInfo != null)
          return methodInfo.Method;
        throw new InvalidOperationException ("CFG has bad subroutine that is not a method");
      }
    }

    #region ICFG Members
    public APC Entry
    {
      get { return new APC (EntryBlock, 0, null); }
    }

    public APC EntryAfterRequires
    {
      get { return new APC (this.method_subroutine.EntryAfterRequires, 0, null); }
    }

    public APC NormalExit
    {
      get { return new APC (ExitBlock, 0, null); }
    }

    public APC ExceptionExit
    {
      get { return new APC (ExceptionExitBlock, 0, null); }
    }

    public Subroutine Subroutine
    {
      get { return this.method_subroutine; }
    }

    public APC Next(APC pc)
    {
      APC next;

      if (HasSingleSuccessor (pc, out next))
        return next;

      return pc;
    }

    public bool HasSingleSuccessor(APC pc, out APC ifFound)
    {
      return pc.Block.Subroutine.HasSingleSuccessor (pc, out ifFound);
    }

    public IEnumerable<APC> Successors(APC pc)
    {
      return pc.Block.Subroutine.Successors (pc);
    }

    public bool HasSinglePredecessor(APC pc, out APC ifFound)
    {
      return pc.Block.Subroutine.HasSinglePredecessor (pc, out ifFound);
    }

    public IEnumerable<APC> Predecessors(APC pc)
    {
      return pc.Block.Subroutine.Predecessors (pc);
    }

    public bool IsJoinPoint(APC pc)
    {
      if (pc.Index != 0)
        return false;

      return IsJoinPoint (pc.Block);
    }

    public bool IsSplitPoint(APC pc)
    {
      if (pc.Index != pc.Block.Count)
        return false;

      return IsSplitPoint (pc.Block);
    }

    public bool IsBlockStart(APC pc)
    {
      return pc.Index == 0;
    }

    public bool IsBlockEnd(APC pc)
    {
      return pc.Index == pc.Block.Count;
    }

    public IILDecoder<APC, Local, Parameter, TMethod, Field, TType, Dummy, Dummy, IMethodContext<Field, TMethod>, Dummy>
      GetDecoder<Local, Parameter, TMethod, Field, Property, Event, TType, TAttribute, Assembly>(
      IMetaDataProvider<Local, Parameter, TMethod, Field, Property, Event, TType, TAttribute, Assembly> metadataDecoder)
    {
      var methodRepository = this.method_repository as MethodRepository<Local, Parameter, TType, TMethod, Field, Property, Event, TAttribute, Assembly>;
      return new APCDecoder<Local, Parameter, TMethod, Field, Property, Event, TType, TAttribute, Assembly> ((ControlFlowGraph<TMethod, TType>) (object) this, metadataDecoder, methodRepository);
    }
    #endregion

    private bool IsJoinPoint(CFGBlock block)
    {
      return block.Subroutine.IsJoinPoint (block);
    }

    private bool IsSplitPoint(CFGBlock block)
    {
      return block.Subroutine.IsSplitPoint (block);
    }

    public IGraph<APC, Dummy> AsForwardGraph()
    {
      return new GraphWrapper<APC, Dummy> (new APC[0], (pc) => SuccessorsEdges (pc));
    }

    private IEnumerable<Pair<Dummy, APC>> SuccessorsEdges(APC pc)
    {
      APC last = pc.LastInBlock ();
      foreach (APC succ in Successors (last)) yield return new Pair<Dummy, APC> (Dummy.Value, succ);
    }
  }
}