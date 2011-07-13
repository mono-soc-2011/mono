using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.Core.Consumers
{
  public interface IMethodCodeConsumer<Local, Parameter, Method, Field, Type, Data, Result>
  {
    Result Accept<Label, Handler>(IMethodCodeProvider<Label, Local, Parameter, Method, Field, Type, Handler> codeProvider, Label entry, Method method, Data data);
  }
}