using System;
using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow
{
  public static class DecoratorHelper
  {
    private static List<object> contextAdapters = new List<object> ();

    private static object Last
    {
      get { return contextAdapters[contextAdapters.Count - 1]; }
    }

    public static void Push<T>(T @this) where T : class
    {
      contextAdapters.Add (@this);
    }
    public static void Pop()
    {
      contextAdapters.RemoveAt (contextAdapters.Count - 1);
    }

    public static T Dispatch<T>(T @this) where T : class
    {
      return FindAdaptorStartingAt<T> (@this, 0);
    }

    private static T FindAdaptorStartingAt<T>(T @default, int startIndex)
      where T : class
    {
      var list = contextAdapters;
      for (int i = startIndex; i < list.Count; ++i) {
        T obj = list[i] as T;
        if (obj != null)
          return obj;
      }
      return @default;
    }

    public static T Inner<T>(T @this) where T : class
    {
      for (int i = 0; i < contextAdapters.Count; i++) {
        if (contextAdapters[i] == @this) {
          ClearDuplicates (@this, i + 1);
          T inner = FindAdaptorStartingAt (default(T), i + 1);
          if (inner != null)
            return inner;

          throw new InvalidOperationException("No inner context found");
        }
      }

      throw new InvalidOperationException("@this is not current adaptor");
    }

    private static void ClearDuplicates(object @this, int @from)
    {
      for (int i = from; i < contextAdapters.Count; i++) {
        if (contextAdapters[i] == @this)
          contextAdapters[i] = null;
      }
    }
  }
}