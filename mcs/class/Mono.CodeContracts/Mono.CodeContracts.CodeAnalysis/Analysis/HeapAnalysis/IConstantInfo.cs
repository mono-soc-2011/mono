namespace Mono.CodeContracts.CodeAnalysis.Analysis.HeapAnalysis
{
  public interface IConstantInfo
  {
    bool KeepAsBottomField { get; }
    bool ManifestField { get; }
  }
}