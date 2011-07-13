using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public class ImmutableMap<K, V> : IImmutableMap<K, V>
  {
    private readonly Dictionary<K, V> map = new Dictionary<K, V> ();

    private ImmutableMap(Dictionary<K, V> map)
    {
      this.map = map;
    }

    #region Implementation of IImmutableMap<K,V>
    public V this[K key]
    {
      get { return map[key]; }
    }

    public K AnyKey
    {
      get { return map.Keys.First(); }
    }

    public IEnumerable<K> Keys
    {
      get { return map.Keys; }
    }

    public int Count
    {
      get { return map.Count; }
    }

    public IImmutableMap<K, V> Add(K key, V value)
    {
      var newDict = new Dictionary<K, V> (map) {{key, value}};
      return new ImmutableMap<K, V> (newDict);
    }

    public IImmutableMap<K, V> Remove(K key)
    {
      if (!map.ContainsKey (key))
        return this;
      
      var newDict = new Dictionary<K, V> (map);
      newDict.Remove (key);
      return new ImmutableMap<K, V>(newDict);
    }

    public bool ContainsKey(K key)
    {
      return map.ContainsKey (key);
    }

    public void Visit(Func<K, V, VisitStatus> func)
    {
      foreach (var v in map) {
        if (func(v.Key, v.Value) == VisitStatus.StopVisit)
          break;
      }
    }
    #endregion

    public static IImmutableMap<K, V> Empty()
    {
      return new ImmutableMap<K, V> (new Dictionary<K, V> ());
    }
  }
}