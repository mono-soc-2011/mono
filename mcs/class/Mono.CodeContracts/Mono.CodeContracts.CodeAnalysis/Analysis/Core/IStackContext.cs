using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.Core
{
  public interface IStackContext<Field, Method> : IMethodContext<Field, Method>
  {
    IStackContextData<Field, Method> StackContext { get; } 
  }

  public interface IStackContextData<Field, Method>
  {
    int StackDepth(APC pc);
  }
}