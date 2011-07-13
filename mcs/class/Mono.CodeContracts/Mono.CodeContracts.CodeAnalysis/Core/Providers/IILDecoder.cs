using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;

namespace Mono.CodeContracts.CodeAnalysis.Core.Providers
{
  public interface IILDecoder<Label, Local, Parameter, Method, Field, Type,Source, Dest, TContext, TEdgeData>
  {
    TContext Context { get; }

    Result ForwardDecode<Data, Result, Visitor>(Label pc, Visitor visitor, Data state)
      where Visitor : IILVisitor<Label, Local, Parameter, Method, Field, Type, Source, Dest, Data, Result>;

    bool IsUnreachable(Label pc);
    TEdgeData EdgeData(Label arg1, Label arg2);
  }
}