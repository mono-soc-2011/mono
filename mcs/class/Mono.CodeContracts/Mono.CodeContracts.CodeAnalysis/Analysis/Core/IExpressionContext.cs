using System;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;
using Mono.CodeContracts.CodeAnalysis.Lattices;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.Core
{
  public interface IExpressionContext<Local, Parameter, Method, Field, Type, Expression, Variable> : IValueContext<Local, Parameter, Method, Field, Type, Variable>
    where Type : IEquatable<Type>
  {
    IExpressionContextData<Local, Parameter, Method, Field, Type, Expression, Variable> ExpressionContext { get; }
  }

  public interface IExpressionContextData<Local, Parameter, Method, Field, Type, Expression, Variable>
    where Type : IEquatable<Type>
  {
    Expression Refine(APC pc, Variable variable);
    Variable Unrefine(Expression expression);

    Result Decode<Data, Result, Visitor>(Expression expr, Visitor visitor, Data data)
      where Visitor : ISymbolicExpressionVisitor<APC, Type, Expression, Variable, Data, Result>;

    FlatDomain<Type> GetType(Expression expr);

    APC GetPC(Expression pc);

    Expression For(Variable variable);

    bool IsZero(Expression expression);
  }
}