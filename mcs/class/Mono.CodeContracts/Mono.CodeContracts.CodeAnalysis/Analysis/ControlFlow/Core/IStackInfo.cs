namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core
{
  public interface IStackInfo
  {
    bool IsCallOnThis(APC pc);
  }
}