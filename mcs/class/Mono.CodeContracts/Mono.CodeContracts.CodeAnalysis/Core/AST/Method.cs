using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.CodeContracts.CodeAnalysis.Core.AST.Builders;

namespace Mono.CodeContracts.CodeAnalysis.Core.AST
{
  public class Method : Member
  {
    public delegate void MethodContractProvider(Method method);

    private readonly MethodDefinition method_definition;
    public bool HasGenericParameters
    {
      get { return this.method_definition.HasGenericParameters; }
    }

    public MethodDefinition MethodDefinition
    {
      get { return this.method_definition; }
    }

    public TypeNode DeclaringType
    {
      get { return new TypeNode(method_definition.DeclaringType); }
    }

    private Block block = null;
    public Method(MethodDefinition methodDefinition)
      : base(NodeType.Method)
    {
      this.method_definition = methodDefinition;
    }

    private Method(MethodDefinition methodDefinition, Block block) : base (NodeType.Method)
    {
      this.method_definition = methodDefinition;
      this.block = block;
    }

    public Method OverriddenMethod
    {
      get {
        if (!method_definition.HasOverrides)
          return null;
        return ParseMethodDefinition (method_definition.Overrides[0].Resolve ());
      }
    }

    public Block Body
    {
      get
      {
        if (block == null)
          block = ParseMethodBlock (method_definition);
        return block;
      }
      set { block = value; }
    }

    private MethodContract contract;

    public MethodContract MethodContract
    {
      get
      {
        if (contract == null && this.ContractProvider != null) {
          var provider = this.ContractProvider;
          this.ContractProvider = null;
          provider (this);
        }
        return contract;
      }
      set
      {
        contract = value;
        if (value != null)
          this.contract.DeclaringMethod = this;
        this.ContractProvider = null;
      }
    }

    private MethodContractProvider method_contract_provider;
    public MethodContractProvider ContractProvider
    {
      get { return method_contract_provider; }
      set
      {
        if (value == null) {
          method_contract_provider = null;
          return;
        }

        if (this.method_contract_provider != null)
          this.method_contract_provider += value;
        else
          this.method_contract_provider = value;

        this.contract = null;
      }
    }


    public bool IsFinal
    {
      get { return this.method_definition.IsFinal; }
    }

    public bool HasBody
    {
      get { return this.method_definition.HasBody; }
    }

    public bool IsPrivate
    {
      get { return this.method_definition.IsPrivate; }
    }

    public bool IsPublic
    {
      get { return this.method_definition.IsPublic; }
    }

    public bool IsProtected
    {
      get { return this.method_definition.IsFamily; }
    }

    public bool IsProtectedOrInternal
    {
      get { return this.method_definition.IsFamilyOrAssembly; }
    }

    public bool IsProtectedAndInternal
    {
      get { return this.method_definition.IsFamilyAndAssembly; }
    }

    public string Name
    {
      get { return this.method_definition.Name; }
    }

    public string FullName
    {
      get { return this.method_definition.FullName; }
    }

    public bool HasOverrides
    {
      get { return this.method_definition.HasOverrides; }
    }

    public bool IsVirtual
    {
      get { return this.method_definition.IsVirtual; }
    }

    public override bool IsStatic
    {
      get { return this.method_definition.IsStatic; }
    }

    public bool IsNewSlot
    {
      get { return this.method_definition.IsNewSlot; }
    }

    public bool IsAbstract
    {
      get { return this.method_definition.IsAbstract; }
    }

    public bool IsConstructor
    {
      get { return this.method_definition.IsConstructor; }
    }

    private List<Parameter> parameters; 
    public List<Parameter> Parameters
    {
      get
      {
        if (parameters == null)
          parameters = this.method_definition.Parameters.Select (i => new Parameter (i)).ToList ();
        return parameters;
      }
      set { parameters = value; }
    }

    public bool HasParameters
    {
      get { return this.Parameters != null && this.Parameters.Count > 0; }
    }

    private TypeNode returnType;
    public TypeNode ReturnType
    {
      get
      {
        if (returnType == null)
          returnType = new TypeNode (this.method_definition.ReturnType);
        return returnType;
      }
      set { returnType = value; }
    }

    public bool IsSetter
    {
      get { return this.method_definition.IsSetter; }
    }

    public bool IsGetter
    {
      get { return this.method_definition.IsGetter; }
    }

    public Parameter ThisParameter
    {
      get
      {
        if (method_definition.HasThis)
          return new Parameter(method_definition.Parameters[0]);
        return null;
      }
    }

    public List<TypeNode> GenericParameters
    {
      get { return method_definition.GenericParameters.Select (it => new TypeNode (it)).ToList (); }
    }

   
    public static Method ParseMethodDefinition(MethodDefinition methodDefinition)
    {
      var methodBlock = ParseMethodBlock (methodDefinition);

      return new Method (methodDefinition, methodBlock);
    }

    private static Block ParseMethodBlock(MethodDefinition methodDefinition)
    {
      var bp = new BodyParser (methodDefinition);
      return new Block(bp.ParseBlocks ());
    }

    public override string ToString()
    {
      return string.Format ("Method(Name: {0})", FullName);
    }
  }

  
}