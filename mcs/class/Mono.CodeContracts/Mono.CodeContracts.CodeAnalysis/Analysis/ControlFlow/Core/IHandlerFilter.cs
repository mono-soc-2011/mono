namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core
{
  public interface IHandlerFilter<Type, Data>
  {
    bool Catch(Data data, Type exception, out bool stopPropagation);
    bool Filter(Data data, APC filterCode, out bool stopPropagation);
  }
}