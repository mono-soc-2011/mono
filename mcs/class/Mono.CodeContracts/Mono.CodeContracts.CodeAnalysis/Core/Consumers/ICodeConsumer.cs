using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.Core.Consumers
{
  public interface ICodeConsumer<Local, Parameter, Method, Field, Type, Data, Result>
  {
    Result Accept<Label>(ICodeProvider<Label, Local, Parameter, Method, Field, Type> codeProvider, Label entryPoint, Data data);
  }
}