using System;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.HeapAnalysis
{
  public struct ExternalExpression<Label, SymbolicValue> : IEquatable<ExternalExpression<Label, SymbolicValue>>
    where SymbolicValue : IEquatable<SymbolicValue>
  {
    private readonly Label readAt;
    private readonly SymbolicValue symbol;

    public ExternalExpression(Label readAt, SymbolicValue symbol)
    {
      this.readAt = readAt;
      this.symbol = symbol;
    }

    #region Implementation of IEquatable<ExternalExpression<Label,SymbolicValue>>
    public bool Equals(ExternalExpression<Label, SymbolicValue> other)
    {
      return this.symbol.Equals (other.symbol);
    }
    #endregion

    public override bool Equals(object obj)
    {
      return obj is ExternalExpression<Label, SymbolicValue> && Equals ((ExternalExpression<Label, SymbolicValue>) obj);
    }

    public override int GetHashCode()
    {
      return this.symbol.GetHashCode ();
    }

    public override string ToString()
    {
      return string.Format ("{0}@{1}", this.symbol, this.readAt);
    }
  }
}