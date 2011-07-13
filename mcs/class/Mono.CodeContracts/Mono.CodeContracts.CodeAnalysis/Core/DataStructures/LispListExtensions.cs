using System;
using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public static class LispListExtensions
  {
    public static LispList<T> Cons<T>(this LispList<T> rest, T elem)
    {
      return LispList<T>.Cons (elem, rest);
    } 

    public static LispList<T> Append<T>(this LispList<T> list, LispList<T> append  )
    {
      if (list == null)
        return append;
      if (append == null)
        return list;
      
      return Cons (list.Tail.Append (append), list.Head);
    }

    public static LispList<T> Where<T>(this LispList<T> list, Predicate<T> keep)
    {
      if (list == null)
        return null;
      LispList<T> rest = list.Tail.Where (keep);
      if (!keep(list.Head))
        return rest;
      
      if (rest == list.Tail)
        return list;
      
      return Cons (rest, list.Head);
    }

    public static void Apply<T>(this LispList<T> list, Action<T> action)
    {
      LispList<T>.Apply (list, action);
    }

    public static IEnumerable<T> AsEnumerable<T>(this LispList<T> list )
    {
      return LispList<T>.PrivateGetEnumerable (list);
    }

    public static bool Any<T>(this LispList<T> list, Predicate<T> predicate)
    {
      if (list == null)
        return false;

      if (predicate(list.Head))
        return true;

      return list.Tail.Any (predicate);
    }

    public static int Length<T>(this LispList<T> list)
    {
      return LispList<T>.LengthOf (list);
    }

    public static bool IsEmpty<T>(this LispList<T> list )
    {
      return list == null;
    }

    public static LispList<S> Select<T,S>(this LispList<T> list, Func<T,S> selector)
    {
      return LispList<T>.Select (list, selector);
    }

    public static T Last<T>(this LispList<T> list)
    {
      if (list == null)
        return default(T);
      
      while (LispList<T>.LengthOf(list) > 1)
        list = list.Tail;

      return list.Head;
    }
  }
}