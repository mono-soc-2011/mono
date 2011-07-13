using System;
using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public class ImmutableSet<T> : IImmutableSet<T>
    where T : IEquatable<T>
  {
    private IImmutableMap<T, Dummy> underlying;

    public ImmutableSet(IImmutableMap<T, Dummy> immutableMap)
    {
      this.underlying = immutableMap;
    }

    #region Implementation of IImmutableSet<T>
    public T Any
    {
      get { return underlying.AnyKey; }
    }

    public int Count
    {
      get { return this.underlying.Count; }
    }

    public IEnumerable<T> Elements
    {
      get { return this.underlying.Keys; }
    }

    public static IImmutableSet<T> Empty()
    {
      return new ImmutableSet<T> (ImmutableMap<T,Dummy>.Empty());
    } 

    public IImmutableSet<T> Add(T item)
    {
      return new ImmutableSet<T> (this.underlying.Add(item, Dummy.Value));
    }

    public IImmutableSet<T> Remove(T item)
    {
      return new ImmutableSet<T> (this.underlying.Remove (item));
    }

    public bool Contains(T item)
    {
      return this.underlying.ContainsKey (item);
    }

    public bool IsContainedIn(IImmutableSet<T> that)
    {
      if (this.Count > that.Count)
        return false;
      bool result = true;
      this.underlying.Visit ((e, dummy) => {
                               if (that.Contains (e))
                                 return VisitStatus.ContinueVisit;

                               result = false;
                               return VisitStatus.StopVisit;
                             });
      return result;
    }

    public IImmutableSet<T> Intersect(IImmutableSet<T> that)
    {
      if (this == that)
        return this;
      if (this.Count == 0)
        return this;
      IImmutableSet<T> set;
      IImmutableSet<T> larger;
      if (this.Count < that.Count) {
        set = this;
        larger = that;
      } else {
        if (that.Count == 0)
          return that;
        set = that;
        larger = this;
      }
      IImmutableSet<T> result = set;
      set.Visit ((e) => {
                   if (!larger.Contains (e))
                     result = result.Remove (e);
                 });
      return result;
    }

    public IImmutableSet<T> Union(IImmutableSet<T> that)
    {
      if (this == that)
        return this;
      if (this.Count == 0)
        return that;
      IImmutableSet<T> smaller;
      IImmutableSet<T> larger;
      if (this.Count < that.Count)
      {
        smaller = this;
        larger = that;
      }
      else
      {
        if (that.Count == 0)
          return this;
        smaller = that;
        larger = this;
      }
      IImmutableSet<T> result = larger;
      smaller.Visit(e => { result = result.Add(e); });
      return result;
    }

    public void Visit(Action<T> visitor)
    {
      this.underlying.Visit ((elem, dummy) => {
                               visitor (elem);
                               return VisitStatus.ContinueVisit;
                             });
    }
    #endregion
  }
}