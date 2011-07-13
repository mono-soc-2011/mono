using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;

namespace Mono.CodeContracts.CodeAnalysis.Core.Providers
{
  public interface ICodeProvider<Label, Local, Parameter, Method, Field, Type>
  {
    Result Decode<CodeVisitor, Data, Result>(Label pc, CodeVisitor visitor, Data data)
      where CodeVisitor : IAggregateVisitor<Label, Local, Parameter, Method, Field, Type, Data, Result>;

    bool Next(Label current, out Label nextLabel);
    int GetILOffset(Label current);
  }
}