using System;
using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public interface IImmutableSet<T>
    where T : IEquatable<T>
  {
    T Any { get; }
    int Count { get; }
    IEnumerable<T> Elements { get; }
    IImmutableSet<T> Add(T item);
    IImmutableSet<T> Remove(T item);
    bool Contains(T item);
    bool IsContainedIn(IImmutableSet<T> that);
    IImmutableSet<T> Intersect(IImmutableSet<T> that);
    IImmutableSet<T> Union(IImmutableSet<T> that);
    void Visit(Action<T> visitor);
  }
}