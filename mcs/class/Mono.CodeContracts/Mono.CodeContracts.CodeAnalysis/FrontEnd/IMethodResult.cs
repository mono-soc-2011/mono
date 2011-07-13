namespace Mono.CodeContracts.CodeAnalysis.FrontEnd
{
  public interface IMethodResult<T>
  {
    IMethodAnalysis MethodAnalysis { get; }
  }
}