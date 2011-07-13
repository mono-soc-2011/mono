using System;
using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public sealed class WorkList<T> : AbstractWorkList<T>
  {
    private readonly Queue<T> queue;

    public WorkList()
    {
      this.queue = new Queue<T> ();
    }

    #region Overrides of AbstractWorkList<T>
    protected override IEnumerable<T> coll
    {
      get { return this.queue; }
    }

    protected override void AddInternal(T o)
    {
      this.queue.Enqueue (o);
    }

    public override T Pull()
    {
      if (this.queue.Count == 0) 
        throw new InvalidOperationException();
      T a = this.queue.Dequeue ();
      this.elements.Remove (a);
      return a;
    }
    #endregion
  }
}