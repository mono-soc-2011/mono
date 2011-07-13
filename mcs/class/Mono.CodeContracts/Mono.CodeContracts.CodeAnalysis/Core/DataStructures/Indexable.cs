using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public class Indexable<T> : IIndexable<T>
  {
    private IList<T> list;

    public Indexable(IList<T> parameters)
    {
      list = parameters;
    }

    public int Count { get { return this.list == null ? 0 : this.list.Count; } }

    public T this[int index] { get { return this.list[index]; } }
  }
}