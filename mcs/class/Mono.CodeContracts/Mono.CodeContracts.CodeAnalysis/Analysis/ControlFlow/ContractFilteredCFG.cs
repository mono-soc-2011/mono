using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.Core;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow 
{
  public class ContractFilteredCFG : ICFG, IEdgeSubroutineAdaptor
  {
    private readonly ICFG underlying;

    public ContractFilteredCFG(ICFG cfg)
    {
      this.underlying = cfg;
    }

    public APC Entry
    {
      get { return this.underlying.Entry; }
    }

    public APC EntryAfterRequires
    {
      get { return this.underlying.EntryAfterRequires; }
    }

    public APC NormalExit
    {
      get { return this.underlying.NormalExit; }
    }

    public APC ExceptionExit
    {
      get { return this.underlying.ExceptionExit; }
    }

    public Subroutine Subroutine
    {
      get { return this.underlying.Subroutine; }
    }

    public APC Next(APC pc)
    {
      return this.underlying.Next (pc);
    }

    public bool HasSingleSuccessor(APC pc, out APC ifFound)
    {
      DecoratorHelper.Push<IEdgeSubroutineAdaptor> (this);
      try {
        return this.underlying.HasSingleSuccessor (pc, out ifFound);
      } finally {
        DecoratorHelper.Pop ();
      }
    }

    public IEnumerable<APC> Successors(APC pc)
    {
      DecoratorHelper.Push<IEdgeSubroutineAdaptor> (this);
      try {
        return this.underlying.Successors (pc);
      } finally {
        DecoratorHelper.Pop ();
      }
    }

    public bool HasSinglePredecessor(APC pc, out APC ifFound)
    {
      DecoratorHelper.Push<IEdgeSubroutineAdaptor> (this);
      try {
        return this.underlying.HasSinglePredecessor (pc, out ifFound);
      } finally {
        DecoratorHelper.Pop ();
      }
    }

    public IEnumerable<APC> Predecessors(APC pc)
    {
      DecoratorHelper.Push<IEdgeSubroutineAdaptor> (this);
      try {
        return this.underlying.Predecessors (pc);
      } finally {
        DecoratorHelper.Pop ();
      }
    }

    public bool IsJoinPoint(APC pc)
    {
      return this.underlying.IsJoinPoint (pc);
    }

    public bool IsSplitPoint(APC pc)
    {
      return this.underlying.IsSplitPoint (pc);
    }

    public bool IsBlockStart(APC pc)
    {
      return this.underlying.IsBlockStart (pc);
    }

    public bool IsBlockEnd(APC pc)
    {
      return this.underlying.IsBlockEnd (pc);
    }

    public IILDecoder<APC, Local, Parameter, Method, Field, Type1, Dummy, Dummy, IMethodContext<Field, Method>, Dummy> GetDecoder<Local, Parameter, Method, Field, Property, Event, Type1, Attribute1, Assembly>(IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type1, Attribute1, Assembly> metadataDecoder)
    {
      return this.underlying.GetDecoder (metadataDecoder);
    }

    #region Implementation of IEdgeSubroutineAdaptor
    public LispList<Pair<EdgeTag, Subroutine>> GetOrdinaryEdgeSubroutinesInternal(CFGBlock @from, CFGBlock to, LispList<Edge<CFGBlock, EdgeTag>> context)
    {
      return DecoratorHelper.Inner<IEdgeSubroutineAdaptor> (this).GetOrdinaryEdgeSubroutinesInternal (from, to, context).Where (
        (pair) => !pair.Value.IsContract && !pair.Value.IsOldValue);
    }
    #endregion
  }
}