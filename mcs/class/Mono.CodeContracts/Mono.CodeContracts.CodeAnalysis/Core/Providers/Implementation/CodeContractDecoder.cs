using Mono.Cecil;
using Mono.CodeContracts.CodeAnalysis.Core.AST;
using Mono.CodeContracts.CodeAnalysis.Core.Consumers;

namespace Mono.CodeContracts.CodeAnalysis.Core.Providers.Implementation
{
  public class CodeContractDecoder : IContractProvider<Local, Parameter, Method, FieldReference, TypeNode>
  {
    public static readonly CodeContractDecoder Instance = new CodeContractDecoder ();

    public bool VerifyMethod(Method method, bool analyzeNonUserCode)
    {
      //todo: implement this
      return true;
    }

    public bool HasRequires(Method method)
    {
      method = this.GetMethodWithContractFor (method);
      if (method.MethodContract == null)
        return false;

      return method.MethodContract.RequiresCount > 0;
    }

    private Method GetMethodWithContractFor(Method method)
    {
      //todo: implement this
      return method;
    }

    public Result AccessRequires<Data, Result>(Method method, ICodeConsumer<Local, Parameter, Method, FieldReference, TypeNode, Data, Result> consumer, Data data)
    {
      method = this.GetMethodWithContractFor (method);
      return consumer.Accept (CodeProviderImpl.Instance, new CodeProviderImpl.PC(method.MethodContract, 0), data);
    }

    public bool HasEnsures(Method method)
    {
      method = this.GetMethodWithContractFor(method);
      if (method.MethodContract == null)
        return false;

      return method.MethodContract.EnsuresCount > 0;
    }

    public Result AccessEnsures<Data, Result>(Method method, ICodeConsumer<Local, Parameter, Method, FieldReference, TypeNode, Data, Result> consumer, Data data)
    {
      method = this.GetMethodWithContractFor(method);
      return consumer.Accept(CodeProviderImpl.Instance, new CodeProviderImpl.PC(method.MethodContract, method.MethodContract.RequiresCount + 2), data);
    }

    public bool CanInheritContracts(Method method)
    {
      return false;
    }
  }
}