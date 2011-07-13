using System;
using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;
using Mono.CodeContracts.CodeAnalysis.Lattices;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.HeapAnalysis
{
  public interface IEGraph<Constant, AbstractValue, T> : IAbstractDomain<T> 
    where Constant : IEquatable<Constant>, IConstantInfo
    where AbstractValue : IAbstractDomainForEGraph<AbstractValue>
  {
    SymValue this[Constant function, SymValue arg] { get; set; }
    SymValue this[Constant function] { get; set; }

    AbstractValue this[SymValue symbol] { get; set; }
    IEnumerable<Constant> Constants { get; }
    IEnumerable<SymValue> SymbolicValues { get; }

    SymValue TryLookup(Constant function, SymValue arg);
    SymValue TryLookup(Constant function);

    void AssumeEqual(SymValue v1, SymValue v2);
    bool IsEqual(SymValue v1, SymValue v2);
    void Eliminate(Constant function, SymValue arg);
    void Eliminate(Constant function);
    void EliminateAll(SymValue arg);

    SymValue FreshSymbol();

    T Join(T that, out IMergeInfo mergeInfo, bool widen);
    IEnumerable<Constant> Functions(SymValue symbol);
    IEnumerable<EGraphTerm<Constant>> EqTerms(SymValue symbol);

    bool LessEqual(T that, out IImmutableMap<SymValue, LispList<SymValue>> forward, out IImmutableMap<SymValue, SymValue> backward);
  }
}