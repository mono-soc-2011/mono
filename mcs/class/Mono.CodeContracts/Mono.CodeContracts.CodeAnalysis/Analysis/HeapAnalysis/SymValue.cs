using System;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.HeapAnalysis
{
  public sealed class SymValue : IEquatable<SymValue>, IComparable<SymValue>, IComparable
  {
    private static int _globalIdGenerator;

    public readonly int GlobalId;
    public readonly int UniqueId;

    public SymValue(int uniqueId)
    {
      this.UniqueId = uniqueId;
      this.GlobalId = ++_globalIdGenerator;
    }

    #region IComparable Members
    public int CompareTo(object obj)
    {
      var that = obj as SymValue;
      if (that == null)
        return 1;

      return CompareTo (that);
    }
    #endregion

    #region Implementation of IEquatable<SymValue>
    public bool Equals(SymValue other)
    {
      return this == other;
    }
    #endregion

    #region IComparable<SymValue> Members
    public int CompareTo(SymValue other)
    {
      return this.UniqueId - other.UniqueId;
    }
    #endregion

    public int GetUniqueKey()
    {
      return this.GlobalId;
    }

    public override string ToString()
    {
      return string.Format ("sv{0} ({1})", this.UniqueId, this.GlobalId);
    }
  }
}