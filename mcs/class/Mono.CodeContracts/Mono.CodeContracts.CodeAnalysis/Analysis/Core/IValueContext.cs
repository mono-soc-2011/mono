using System;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.Core
{
  public interface IValueContext<Local, Parameter, Method, Field, Type, Variable> : IStackContext<Field, Method>
    where Type : IEquatable<Type>
  {
    IValueContextData<Local, Parameter, Method, Field, Type, Variable> ValueContext { get; }
  }

  public interface IValueContextData<Local, Parameter, Method, Field, Type, SymbolicValue>
    where Type : IEquatable<Type>
  {
    bool IsZero(APC at, SymbolicValue value);
    bool TryLocalValue(APC at, Local local, out SymbolicValue sv);
    bool TryParameterValue(APC at, Parameter parameter, out SymbolicValue sv);
  }
}