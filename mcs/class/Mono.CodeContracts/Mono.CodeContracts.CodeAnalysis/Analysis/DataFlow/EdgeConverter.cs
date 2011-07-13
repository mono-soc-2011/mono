namespace Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow
{
  public delegate AbstractState EdgeConverter<Label, AbstractState, EdgeData>(Label from, Label to, bool isJoinPoint, EdgeData data, AbstractState newState);
}