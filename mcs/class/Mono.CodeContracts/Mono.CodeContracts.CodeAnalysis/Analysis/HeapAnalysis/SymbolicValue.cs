using System;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.HeapAnalysis
{
  public struct SymbolicValue : IEquatable<SymbolicValue>, IComparable<SymbolicValue>, IComparable
  {
    private readonly SymValue symbol;

    public bool IsNull
    {
      get { return this.symbol == null; }
    }

    public int MethodLocalId
    {
      get { return this.symbol.UniqueId; }
    }

    #region Implementation of IEquatable<SymbolicValue>
    public bool Equals(SymbolicValue other)
    {
      return this.symbol == other.symbol;
    }
    #endregion

    #region Implementation of IComparable<in SymbolicValue>
    public int CompareTo(SymbolicValue other)
    {
      return this.symbol.CompareTo (other.symbol);
    }
    #endregion

    #region Implementation of IComparable
    public int CompareTo(object obj)
    {
      if (!(obj is SymbolicValue))
        return 1;

      return CompareTo ((SymbolicValue) obj);
    }
    #endregion

    public override bool Equals(object obj)
    {
      if (obj is SymbolicValue)
        return Equals ((SymbolicValue) obj);

      return false;
    }

    public override int GetHashCode()
    {
      return this.symbol.GetHashCode ();
    }
  }
}