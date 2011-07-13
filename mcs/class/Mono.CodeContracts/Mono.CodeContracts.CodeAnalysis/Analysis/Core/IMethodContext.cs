using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.Core
{
  public interface IMethodContext<Field, Method>
  {
    IMethodContextData<Field, Method> MethodContext { get; }
  }
  
  public interface IMethodContextData<Field, Method>
  {
    Method CurrentMethod { get; }
    ICFG CFG { get; }
  }
}