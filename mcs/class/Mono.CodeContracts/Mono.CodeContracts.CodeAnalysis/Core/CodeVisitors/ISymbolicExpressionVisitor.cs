namespace Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors
{
  public interface ISymbolicExpressionVisitor<Label, Type, Expression, Variable, Data, Result>
    : IExpressionILVisitor<Label, Type, Expression, Variable, Data, Result>
  {
    Result SymbolicConstant(Label pc, Variable variable, Data data);
  }
}