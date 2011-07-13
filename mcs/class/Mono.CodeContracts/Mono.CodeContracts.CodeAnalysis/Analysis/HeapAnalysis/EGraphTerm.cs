using System;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.HeapAnalysis
{
  public struct EGraphTerm<T> : IEquatable<EGraphTerm<T>> 
    where T : IEquatable<T>
  {
    public readonly T Function;
    public readonly SymValue[] Args;
    
    #region Implementation of IEquatable<EGraphTerm<T>>
    public EGraphTerm(T function, params SymValue[] args)
    {
      this.Function = function;
      this.Args = args;
    }

    public bool Equals(EGraphTerm<T> that)
    {
      if (!Function.Equals(that.Function) || this.Args.Length != that.Args.Length)
        return false;
      for (int i = 0; i < this.Args.Length; i++) {
        if (!this.Args[i].Equals(that.Args[i]))
          return false;
      }
      return true;
    }
    #endregion
  }
}