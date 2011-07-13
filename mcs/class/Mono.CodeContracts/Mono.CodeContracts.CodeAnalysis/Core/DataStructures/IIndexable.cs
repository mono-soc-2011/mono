namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public interface IIndexable<T>
  {
    int Count { get; }
    T this[int index] { get; }
  }
}