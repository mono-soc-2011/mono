using Mono.CodeContracts.CodeAnalysis.Core.Consumers;

namespace Mono.CodeContracts.CodeAnalysis.Core.Providers
{
  public interface IContractProvider<Local, Parameter, Method, Field, Type>
  {
    bool VerifyMethod(Method method, bool analyzeNonUserCode);

    bool HasRequires(Method method);
    Result AccessRequires<Data, Result>(Method method, ICodeConsumer<Local, Parameter, Method, Field, Type, Data, Result> consumer, Data data);

    bool HasEnsures(Method method);
    Result AccessEnsures<Data, Result>(Method method, ICodeConsumer<Local, Parameter, Method, Field, Type, Data, Result> consumer, Data data);
    bool CanInheritContracts(Method method);
  }
}