using System;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Lattices
{
  public struct SetDomain<T> : IAbstractDomain<SetDomain<T>> 
    where T : IEquatable<T>
  {
    public static readonly SetDomain<T> TopValue = new SetDomain<T> (ImmutableSet<T>.Empty ());
    public static readonly SetDomain<T> BottomValue = new SetDomain<T> (null);

    private IImmutableSet<T> set; 

    private SetDomain(IImmutableSet<T> set)
    {
      this.set = set;
    }

    #region Implementation of IAbstractDomain<SetDomain<T>>
    public SetDomain<T> Top
    {
      get { return TopValue; }
    }

    public SetDomain<T> Bottom
    {
      get { return BottomValue; }
    }

    public bool IsTop
    {
      get
      {
        if (this.set != null)
          return this.set.Count == 0;
        
        return false;
      }
    }

    public bool IsBottom
    {
      get { return this.set == null; }
    }

    public SetDomain<T> Add(T elem)
    {
      return new SetDomain<T> (this.set.Add (elem));
    }
    
    public SetDomain<T> Remove(T elem)
    {
      return new SetDomain<T>(this.set.Remove(elem));
    }

    public SetDomain<T> Join(SetDomain<T> that, bool widening, out bool weaker)
    {
      if (this.set == that.set) {
        weaker = false;
        return this;
      }
      if (this.IsBottom) {
        weaker = !that.IsBottom;
        return that;
      }
      if (that.IsBottom || this.IsTop) {
        weaker = false;
        return this;
      }
      if (that.IsTop) {
        weaker = !this.IsTop;
        return that;
      }

      var join = this.set.Intersect (that.set);
      
      weaker = join.Count < this.set.Count;
      return new SetDomain<T> (join);
    }

    public SetDomain<T> Meet(SetDomain<T> that)
    {
      if (this.set == that.set || this.IsBottom || that.IsTop)
        return this;
      if (that.IsBottom || this.IsTop)
        return that;

      return new SetDomain<T> (this.set.Union (that.set));
    }

    public bool LessEqual(SetDomain<T> that)
    {
      if (this.IsBottom)
        return true;
      if (that.IsBottom)
        return false;

      return that.set.IsContainedIn (this.set);
    }

    public SetDomain<T> ImmutableVersion()
    {
      return this;
    }
    #endregion

    public bool Contains(T item)
    {
      return this.set.Contains (item);
    }
  }
}