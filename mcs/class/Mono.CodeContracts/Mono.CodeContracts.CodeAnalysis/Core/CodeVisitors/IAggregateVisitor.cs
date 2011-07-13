using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors
{
  public interface IAggregateVisitor<Label, Local, Parameter, Method, Field, Type, Data, Result> :
    IILVisitor<Label, Local, Parameter, Method, Field, Type, Dummy, Dummy, Data, Result>
  {
    Result Aggregate(Label pc, Label aggregateStart, bool canBeTargetOfBranch, Data data);
  }
}