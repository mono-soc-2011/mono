namespace Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow
{
  public interface IFixPointInfo<Label, AbstractState>
  {
    bool PreState(Label pc, out AbstractState state);
    bool PostState(Label pc, out AbstractState state);
  }
}