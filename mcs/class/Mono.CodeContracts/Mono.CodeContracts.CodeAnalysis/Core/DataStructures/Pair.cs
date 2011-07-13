using System;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public class Pair<K, V> : IEquatable<Pair<K, V>>
  {
    private static readonly bool KeyIsReferenceType = default(K) == null;
    private static readonly bool ValueIsReferenceType = default(V) == null;

    public readonly K Key;
    public readonly V Value;

    public Pair(K key, V value)
    {
      this.Key = key;
      this.Value = value;
    }

    #region IEquatable<Pair<K,V>> Members
    public bool Equals(Pair<K, V> other)
    {
      var keyEquatable = this.Key as IEquatable<K>;
      bool result = keyEquatable != null ? keyEquatable.Equals (other.Key) : Equals (this.Key, other.Key);
      if (!result)
        return false;

      var valueEquatable = this.Value as IEquatable<V>;
      return valueEquatable != null ? valueEquatable.Equals (other.Value) : Equals (this.Value, other.Value);
    }
    #endregion

    public override int GetHashCode()
    {
      return (!KeyIsReferenceType || ((object) this.Key) != null ? this.Key.GetHashCode () : 0)
             + 13*(!ValueIsReferenceType || ((object) this.Value) != null ? this.Value.GetHashCode () : 0);
    }

    public override string ToString()
    {
      return string.Format ("({0}, {1})", 
                            (!KeyIsReferenceType || this.Key != null) ? this.Key.ToString () : "<null>", 
                            (!ValueIsReferenceType || this.Value != null) ? this.Value.ToString () : "<null>");
    }
  }
}