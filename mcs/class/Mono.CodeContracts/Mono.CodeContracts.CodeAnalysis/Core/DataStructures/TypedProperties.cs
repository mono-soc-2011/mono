using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public class TypedProperties : ITypedProperties
  {
    private Dictionary<object, object> dictionary = new Dictionary<object, object> ();  

    #region Implementation of ITypedProperties
    public bool Contains<T>(TypedKey<T> key)
    {
      return this.dictionary.ContainsKey (key);
    }

    public void Add<T>(TypedKey<T> key, T value)
    {
      this.dictionary.Add (key, value);
    }

    public bool TryGetValue<T>(TypedKey<T> key, out T value)
    {
      object result;

      if (!this.dictionary.TryGetValue(key, out result)) {
        value = default(T);
        return false;
      }

      value = (T) result;
      return true;
    }
    #endregion
  }

  public struct TypedKey<T>
  {
    private string key;

    public TypedKey(string key)
    {
      this.key = key;
    }
  }
}