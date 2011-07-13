using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.DataStructures
{
  public abstract class AbstractWorkList<T> : IWorkList<T>
  {
    protected HashSet<T> elements = new HashSet<T> ();
    protected abstract IEnumerable<T> coll { get; } 
    public virtual int Count
    {
      get { return this.elements.Count; }
    }

    protected abstract void AddInternal(T o);
    
    #region Implementation of IWorkList<T>
    public virtual bool Add(T o)
    {
      if (!this.elements.Add (o))
        return false;
      this.AddInternal (o);
      return true;
    }

    public virtual bool AddAll(IEnumerable<T> objs)
    {
      bool any = false;
      foreach (T o in objs)
        if (this.Add (o))
          any = true;
      return any;
    }

    public virtual bool IsEmpty()
    {
      return this.elements.Count == 0;
    }

    public abstract T Pull();

    public virtual IEnumerator<T> GetEnumerator()
    {
      return this.coll.GetEnumerator ();
    } 
    #endregion
  }
}