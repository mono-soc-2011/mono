using System;
using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public class LispList<T>
  {
    private T elem;
    private LispList<T> tail;
    private int count;

    public static readonly LispList<T> Empty;

    public T Head { get { return this.elem; } }
    public LispList<T> Tail { get { return this.tail; } } 

    private LispList(T elem, LispList<T> tail)
    {
      this.elem = elem;
      this.tail = tail;
      this.count = LengthOf (tail) + 1;
    }

    public static LispList<T> Cons(T elem, LispList<T> tail)
    {
      return new LispList<T> (elem, tail);
    }
 
    public static LispList<T> Reverse(LispList<T> list)
    {
      LispList<T> rest = null;
      for (; list != null; list = list.tail ) {
        rest = rest.Cons (list.elem);
      }
      return rest;
    }

    public static bool Contains(LispList<T> l, T o)
    {
      if (l == null)
        return false;
      var equatable = o as IEquatable<T>;
      if (equatable != null)
      {
        if (equatable.Equals(l.elem))
          return true;
      }
      else if (o.Equals(l.elem))
        return true;

      return Contains (l.tail, o);
    }

    public static int LengthOf(LispList<T> list)
    {
      if (list == null)
        return 0;
      return list.count;
    }

    public static void Apply(LispList<T> list, Action<T> action)
    {
      for (; list != null; list = list.tail)
        action (list.Head);
    }

    public static IEnumerable<T> PrivateGetEnumerable(LispList<T> list)
    {
      LispList<T> current = list;
      while (current != null) {
        T next = current.Head;
        current = current.tail;
        yield return next;
      }
    }

    public static LispList<S> Select<S>(LispList<T> list, Func<T, S> selector)
    {
      if (list == null)
        return null;
      return list.tail.Select (selector).Cons (selector (list.Head));
    }

    
  }
}