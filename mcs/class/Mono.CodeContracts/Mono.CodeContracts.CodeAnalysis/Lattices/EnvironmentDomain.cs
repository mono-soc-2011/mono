using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Lattices
{
  public class EnvironmentDomain<K, V> : IAbstractDomain<EnvironmentDomain<K, V>>
    where V : IAbstractDomain<V>
  {
    public static EnvironmentDomain<K, V> BottomValue = new EnvironmentDomain<K, V> (null);
    private readonly IImmutableMap<K, V> map;

    public EnvironmentDomain(IImmutableMap<K, V> map)
    {
      this.map = map;
    }

    public V this[K key]
    {
      get { return this.map == null ? default(V) : this.map[key]; }
    }

    public IEnumerable<K> Keys
    {
      get { return this.map.Keys; }
    }

    #region IAbstractDomain<EnvironmentDomain<K,V>> Members
    public EnvironmentDomain<K, V> Top
    {
      get { return TopValue (); }
    }

    public EnvironmentDomain<K, V> Bottom
    {
      get { return new EnvironmentDomain<K, V> (null); }
    }

    public bool IsTop
    {
      get { return this.map != null && this.map.Count == 0; }
    }

    public bool IsBottom
    {
      get { return this.map == null; }
    }

    public EnvironmentDomain<K, V> Join(EnvironmentDomain<K, V> that, bool widening, out bool weaker)
    {
      if (this.map == that.map) {
        weaker = false;
        return this;
      }

      if (IsTop) {
        weaker = false;
        return this;
      }
      if (that.IsTop) {
        weaker = !IsTop;
        return that;
      }
      if (IsBottom) {
        weaker = !that.IsBottom;
        return that;
      }
      if (that.IsBottom) {
        weaker = false;
        return this;
      }

      IImmutableMap<K, V> min;
      IImmutableMap<K, V> max;
      if (this.map.Count < that.map.Count) {
        min = this.map;
        max = that.map;
      } else {
        max = this.map;
        min = that.map;
      }

      bool isResultWeaker = false;
      IImmutableMap<K, V> intersect = min;
      foreach (K key in min.Keys) {
        if (!max.ContainsKey (key))
          intersect = intersect.Remove (key);
        else {
          bool keyWeaker;
          V join = min[key].Join (max[key], widening, out keyWeaker);
          if (keyWeaker) {
            isResultWeaker = true;
            intersect = !join.IsTop ? intersect.Add (key, join) : intersect.Remove (key);
          }
        }
      }
      weaker = isResultWeaker || intersect.Count < this.map.Count;
      return new EnvironmentDomain<K, V> (intersect);
    }

    public EnvironmentDomain<K, V> Meet(EnvironmentDomain<K, V> that)
    {
      if (this.map == that.map)
        return this;
      if (IsTop)
        return that;
      if (that.IsTop || IsBottom)
        return this;
      if (that.IsBottom)
        return that;

      IImmutableMap<K, V> min;
      IImmutableMap<K, V> max;
      if (this.map.Count < that.map.Count) {
        min = this.map;
        max = that.map;
      } else {
        max = this.map;
        min = that.map;
      }

      IImmutableMap<K, V> union = max;
      foreach (K key in min.Keys) {
        if (!max.ContainsKey (key))
          union = union.Add (key, min[key]);
        else {
          V meet = min[key].Meet (max[key]);
          union = union.Add (key, meet);
        }
      }

      return new EnvironmentDomain<K, V> (union);
    }

    public bool LessEqual(EnvironmentDomain<K, V> that)
    {
      if (that.IsTop || IsBottom)
        return true;
      if (IsTop || that.IsBottom || this.map.Count < that.map.Count)
        return false;
      foreach (K key in that.map.Keys) {
        if (!this.map.ContainsKey (key) || !this.map[key].LessEqual (that.map[key]))
          return false;
      }
      return true;
    }

    public EnvironmentDomain<K, V> ImmutableVersion()
    {
      return this;
    }
    #endregion

    public static EnvironmentDomain<K, V> TopValue()
    {
      return new EnvironmentDomain<K, V> (ImmutableMap<K, V>.Empty ());
    }

    public EnvironmentDomain<K, V> Add(K key, V value)
    {
      return new EnvironmentDomain<K, V> (this.map.Add (key, value));
    }

    public EnvironmentDomain<K, V> Remove(K key)
    {
      return new EnvironmentDomain<K, V> (this.map.Remove (key));
    }

    public bool Contains(K key)
    {
      return this.map.ContainsKey (key);
    }

    public EnvironmentDomain<K, V> Empty()
    {
      return new EnvironmentDomain<K, V> (ImmutableMap<K, V>.Empty ());
    }
  }
}