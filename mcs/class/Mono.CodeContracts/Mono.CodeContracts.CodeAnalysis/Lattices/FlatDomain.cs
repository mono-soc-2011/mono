using System;

namespace Mono.CodeContracts.CodeAnalysis.Lattices
{
  /// <summary>
  /// Represents domain, where abstract values are disjunct, and their join/meet turns into Top/Bottom respectively
  /// </summary>
  /// <example>
  ///        T
  ///      / | \
  ///    v1 v2 v3
  ///      \ | /
  ///        B
  /// (Hasse diagram)
  /// </example>
  /// <typeparam name="T"></typeparam>
  public struct FlatDomain<T> : IAbstractDomain<FlatDomain<T>>, IEquatable<FlatDomain<T>> 
    where T : IEquatable<T>
  {
    private enum Kind : byte
    {
      Top = 0, 
      Bottom, 
      Normal
    }

    public static readonly FlatDomain<T> BottomValue = new FlatDomain<T> (Kind.Bottom);
    public static readonly FlatDomain<T> TopValue = new FlatDomain<T> (Kind.Top);

    private readonly FlatDomain<T>.Kind _kind;
    public readonly T Value;

    private FlatDomain(Kind kind)
    {
      this._kind = kind;
      this.Value = default(T);
    }

    public FlatDomain(T value)
    {
      this._kind = Kind.Normal;
      this.Value = value;
    }

    public static implicit operator FlatDomain<T>(T value)
    {
      return new FlatDomain<T> (value);
    }

    #region Implementation of IAbstractDomain<FlatDomain<T>>
    public FlatDomain<T> Top { get { return TopValue; } }
    public FlatDomain<T> Bottom { get { return BottomValue; } }

    public bool IsTop
    {
      get { return this._kind == Kind.Top; }
    }
    public bool IsBottom
    {
      get { return this._kind == Kind.Bottom; }
    }
    public bool IsNormal { get { return this._kind == Kind.Normal; } }

    public FlatDomain<T> Join(FlatDomain<T> that, bool widening, out bool weaker)
    {
      if (this.IsTop || that.IsBottom) {
        weaker = false;
        return this;
      }
      if (that.IsTop) {
        weaker = !this.IsTop;
        return that;
      }
      if (this.IsBottom) {
        weaker = !that.IsBottom;
        return that;
      }
      if (this.Value.Equals(that.Value)) {
        weaker = false;
        return that;
      }

      weaker = true;
      return TopValue;
    }

    public FlatDomain<T> Meet(FlatDomain<T> that)
    {
      if (this.IsTop || that.IsBottom)
        return that;
      if (that.IsTop || this.IsBottom)
        return this;
      if (this.Value.Equals(that.Value))
        return that;
      
      return BottomValue;
    }

    public bool LessEqual(FlatDomain<T> that)
    {
      if (that.IsTop || this.IsBottom)
        return true;
      if (this.IsTop || that.IsBottom)
        return false;

      return this.Value.Equals (that.Value);
    }

    public FlatDomain<T> ImmutableVersion()
    {
      return this;
    }
    #endregion

    #region Implementation of IEquatable<FlatDomain<T>>
    public bool Equals(FlatDomain<T> that)
    {
      if (!this.IsNormal)
        return this._kind == that._kind;

      if (!that.IsNormal)
        return false;

      if (this.Value.Equals(that.Value))
        return true;

      return false;
    }
    #endregion
  }
}