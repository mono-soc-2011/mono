using Mono.CodeContracts.CodeAnalysis.Lattices;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.HeapAnalysis
{
  public interface IAbstractDomainForEGraph<AbstractValue> : IAbstractDomain<AbstractValue>
  {
    bool HasAllBottomFields { get; }
    AbstractValue ForManifestedField();
  }
}