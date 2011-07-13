using System;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;

namespace Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors
{
  public interface ISyntheticILVisitor<Label, Method, Field, Type, Source, Dest, Data, Result>
  {
    Result Entry(Label pc, Method method, Data data);
    Result Assume(Label pc, EdgeTag tag, Source condition, Data data);
    Result Assert(Label pc, EdgeTag tag, Source condition, Data data);
    Result BeginOld(Label pc, Label matchingEnd, Data data);
    Result EndOld(Label pc, Label matchingBegin, Type type, Dest dest, Source source, Data data);
  }
}