namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public interface ITypedProperties
  {
    bool Contains<T>(TypedKey<T> key);
    void Add<T>(TypedKey<T> key, T value);
    bool TryGetValue<T>(TypedKey<T> key, out T value);
  }
}