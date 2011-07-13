using System;

namespace Mono.CodeContracts.CodeAnalysis.FrontEnd
{
  public interface IMethodAnalysis
  {
    string Name { get; }
    IMethodResult<Variable> Analyze<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable>(
      string fullMethodName, IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable> methodDriver)
      where Variable : IEquatable<Variable>
      where Expression : IEquatable<Expression> 
      where Type : IEquatable<Type>;
  }
}