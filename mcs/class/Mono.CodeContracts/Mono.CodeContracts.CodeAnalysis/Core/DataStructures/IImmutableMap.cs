using System;
using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public interface IImmutableMap<K, V>
  {
    V this[K key] { get; }
    
    K AnyKey { get; }

    IEnumerable<K> Keys { get; }
    int Count { get; }

    IImmutableMap<K,V> Add(K key, V value);
    IImmutableMap<K,V> Remove(K key);

    bool ContainsKey(K key);
    void Visit(Func<K, V, VisitStatus> func);
  }

  public enum VisitStatus
  {
    ContinueVisit,
    StopVisit
  }
}