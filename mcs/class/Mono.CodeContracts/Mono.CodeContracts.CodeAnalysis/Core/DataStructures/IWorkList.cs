namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public interface IWorkList<T>
  {
    bool Add(T o);
    bool IsEmpty();
    T Pull();
  }
}