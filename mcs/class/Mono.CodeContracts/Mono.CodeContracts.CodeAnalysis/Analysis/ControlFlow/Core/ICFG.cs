using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Analysis.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core
{
  public interface ICFG
  {
    APC Entry { get; }
    APC EntryAfterRequires { get; }
    APC NormalExit { get; }
    APC ExceptionExit { get; }
    Subroutine Subroutine { get; }
    APC Next(APC pc);
    bool HasSingleSuccessor(APC pc, out APC ifFound);
    IEnumerable<APC> Successors(APC pc);

    bool HasSinglePredecessor(APC pc, out APC ifFound);
    IEnumerable<APC> Predecessors(APC pc);
    bool IsJoinPoint(APC pc);
    bool IsSplitPoint(APC pc);

    bool IsBlockStart(APC pc);
    bool IsBlockEnd(APC pc);

    IILDecoder<APC, Local, Parameter, Method, Field, Type, Dummy, Dummy, IMethodContext<Field, Method>, Dummy>
      GetDecoder<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>(
        IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder
      );
  }
}