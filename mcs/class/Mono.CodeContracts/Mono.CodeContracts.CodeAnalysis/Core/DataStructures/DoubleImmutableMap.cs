using System;
using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public class DoubleImmutableMap<A,B,C> 
    where A : IEquatable<A>
    where B :IEquatable<B>
  {
    private B[] EmptyCache = new B[0];
    private IImmutableMap<A, IImmutableMap<B, C>> map;
    
    public C this[A key1, B key2]
    {
      get
      {
        var inner = this.map[key1];
        if (inner == null)
          return default(C);
        return inner[key2];
      }
    }

    public int Keys1Count
    {
      get { return this.map.Count; }
    }

    public IEnumerable<A> Keys1
    {
      get { return this.map.Keys; }
    }

    private DoubleImmutableMap(IImmutableMap<A, IImmutableMap<B, C>> map)
    {
      this.map = map;
    }

    public DoubleImmutableMap<A,B,C> Add(A key1, B key2, C value)
    {
      var immutableMap = this.map[key1] ?? ImmutableMap<B, C>.Empty ();
      return new DoubleImmutableMap<A, B, C> (this.map.Add (key1, immutableMap.Add (key2, value)));
    }

    public DoubleImmutableMap<A,B,C> RemoveAll(A key1)
    {
      return new DoubleImmutableMap<A, B, C> (this.map.Remove (key1));
    }

    public DoubleImmutableMap<A,B,C> Remove(A key1, B key2)
    {
      var inner = this.map[key1];
      if (inner == null)
        return this;
      var newInner = inner.Remove (key2);
      if (newInner == inner)
        return this;
      return new DoubleImmutableMap<A, B, C> (this.map.Add (key1, newInner));
    }

    public static DoubleImmutableMap<A,B,C> Empty()
    {
      return new DoubleImmutableMap<A, B, C> (ImmutableMap<A,IImmutableMap<B,C>>.Empty ());
    } 

    public bool Contains(A key1, B key2)
    {
      var inner = this.map[key1];
      if (inner == null)
        return false;
      return inner.ContainsKey (key2);
    }

    public bool ContainsKey1(A key1)
    {
      return this.map.ContainsKey (key1);
    }

    public int Keys2Count(A key1)
    {
      if ((object)key1 == null)
        return 0;
      var inner = this.map[key1];
      if (inner == null)
        return 0;
      return inner.Count;
    }

    public IEnumerable<B> Keys2(A key1)
    {
      if ((object) key1 == null)
        return EmptyCache;

      var inner = this.map[key1];
      if(inner ==null)
        return EmptyCache;
      return inner.Keys;
    }
  }
}