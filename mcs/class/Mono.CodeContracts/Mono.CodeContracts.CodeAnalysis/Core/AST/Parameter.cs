using System;
using Mono.Cecil;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class Parameter : Variable
  {
    private ParameterDefinition definition;
    public string Name { get; protected set; }

    public Method DeclaringMethod
    {
      get
      {
        var methodReference = this.definition.Method as MethodReference;
        if (methodReference == null)
          throw new NotImplementedException("Function pointers are not implemented");

        return new Method(methodReference.Resolve ());
      }
    }

    public int Index
    {
      get { return this.definition.Index; }
    }

    public Parameter() : base(NodeType.Parameter)
    {
      
    }

    public Parameter(ParameterDefinition definition) : base (NodeType.Parameter)
    {
      this.definition = definition;
      this.Type = TypeNode.GetInheritorTypeNode(definition.ParameterType);
    }

    public override string ToString()
    {
      return string.Format("Parameter({0})", definition);
    }
  }
}