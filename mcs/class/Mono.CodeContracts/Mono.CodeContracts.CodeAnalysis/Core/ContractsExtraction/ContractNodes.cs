using System;
using System.Collections.Generic;
using System.Linq;
using Mono.CodeContracts.CodeAnalysis.Core.AST;

namespace Mono.CodeContracts.CodeAnalysis.Core.ContractsExtraction
{
  public class ContractNodes
  {
    public static readonly string ContractNamespace = "System.Diagnostics.Contracts";
    public static readonly string ContractClassName = "Contract";
    public static readonly string RequiresName = "Requires";
    public static readonly string EnsuresName = "Ensures";
    public static readonly string AssertName = "Assert";
    public static readonly string AssumeName = "Assume";
    public static readonly string EndContractBlockName = "EndContractBlock";

    [RepresentationFor("System.Diagnostics.Contracts.Contract")]
    public readonly Class ContractClass;

    [RepresentationFor("Contract.Assert(bool)")]
    public readonly Method AssertMethod;
    
    [RepresentationFor("Contract.Assert(bool, string)")]
    public readonly Method AssertWithMessageMethod;

    [RepresentationFor("Contract.Assume(bool)")]
    public readonly Method AssumeMethod;
    
    [RepresentationFor("Contract.Assume(bool, string)")]
    public readonly Method AssumeWithMessageMethod;

    [RepresentationFor("Contract.Requires(bool)")]
    public readonly Method RequiresMethod;

    [RepresentationFor("Contract.Requires(bool, string)")]
    public readonly Method RequiresWithMessageMethod;

    [RepresentationFor("Contract.Ensures(bool)")]
    public readonly Method EnsuresMethod;

    [RepresentationFor("Contract.Ensures(bool, string)")]
    public readonly Method EnsuresWithMessageMethod;

    [RepresentationFor("Contract.EndContractBlock()")]
    public readonly Method EndContractBlock;

    private ContractNodes(AssemblyNode assembly, Action<string> errorHandler)
    {
      CoreSystemTypes.ModuleDefinition = assembly.Modules.First ().Definition;
      if (errorHandler != null)
        this.ErrorFound += errorHandler;
      this.ContractClass = assembly.GetType (ContractNamespace, ContractClassName) as Class;
      if (this.ContractClass == null)
        return;


      IEnumerable<Method> methods = this.ContractClass.GetMethods (RequiresName, CoreSystemTypes.Instance.TypeBoolean);
      foreach (var method in methods) {
        if (method.GenericParameters == null || method.GenericParameters.Count == 0)
          this.RequiresMethod = method;
      }

      if (this.RequiresMethod == null) {
        this.ContractClass = null;
        return;
      }

      methods = this.ContractClass.GetMethods (RequiresName, CoreSystemTypes.Instance.TypeBoolean, CoreSystemTypes.Instance.TypeString);
      foreach (var method in methods) {
        if (method.GenericParameters == null || method.GenericParameters.Count == 0)
          this.RequiresWithMessageMethod = method;
      }
      this.EnsuresMethod = this.ContractClass.GetMethod (EnsuresName, CoreSystemTypes.Instance.TypeBoolean);
      this.EnsuresWithMessageMethod = this.ContractClass.GetMethod (EnsuresName, CoreSystemTypes.Instance.TypeBoolean, CoreSystemTypes.Instance.TypeString);

      AssertMethod = this.ContractClass.GetMethod (AssertName, CoreSystemTypes.Instance.TypeBoolean);
      AssertWithMessageMethod = this.ContractClass.GetMethod (AssertName, CoreSystemTypes.Instance.TypeBoolean, CoreSystemTypes.Instance.TypeString);
      
      AssumeMethod = this.ContractClass.GetMethod (AssumeName, CoreSystemTypes.Instance.TypeBoolean);
      AssumeWithMessageMethod = this.ContractClass.GetMethod(AssumeName, CoreSystemTypes.Instance.TypeBoolean, CoreSystemTypes.Instance.TypeString);

      EndContractBlock = this.ContractClass.GetMethod (EndContractBlockName);

      foreach (var fieldInfo in typeof(ContractNodes).GetFields()) {
        if (fieldInfo.GetValue (this) == null) {
          string runtimeName = null;
          bool isRequired = false;
          var attributes = fieldInfo.GetCustomAttributes (typeof (RepresentationForAttribute), false);
          foreach (var attribute in attributes) {
            var representationForAttribute = attribute as RepresentationForAttribute;
            if (representationForAttribute != null) {
              runtimeName = representationForAttribute.RuntimeName;
              isRequired = representationForAttribute.IsRequired;
              break;
            }
          }
          if (isRequired) {
            string message = string.Format ("Could not find contract node for '{0}'", fieldInfo.Name);
            if (runtimeName != null) 
              message = string.Format ("Could not find the method/type '{0}'", runtimeName);

            this.FireErrorFound (message);
            this.ClearFields ();
          }
        }
      }

    }

    public static ContractNodes GetContractNodes(AssemblyNode assembly, Action<string> errorHandler )
    {
      ContractNodes contractNodes = new ContractNodes (assembly, errorHandler);
      if (contractNodes.ContractClass != null) 
        return contractNodes;
      return null;
    }

    private void ClearFields()
    {
      foreach (var fieldInfo in typeof(ContractNodes).GetFields()) {
        var customAttributes = fieldInfo.GetCustomAttributes (typeof(RepresentationForAttribute), false);
        if (customAttributes.Length == 1)
          fieldInfo.SetValue (this, null);
      }
    }

    public event Action<string> ErrorFound;
    private void FireErrorFound(string message)
    {
      if (this.ErrorFound == null) 
        throw new InvalidOperationException (message);
      
      this.ErrorFound (message);
    }

    public Method IsContractCall(Statement s)
    {
      Method m = HelperMethods.IsMethodCall (s);
      if (this.IsContractMethod (m))
        return m;

      return null;
    }

    public bool IsContractMethod(Method method)
    {
      if (method == null)
        return false;
      if (this.IsPlainPrecondition (method) || this.IsPostCondition (method) || this.IsEndContractBlock (method))
        return true;

      return false;
    }

    public bool IsPostCondition(Method method)
    {
      TypeNode genericArgument;
      return this.IsContractMethod (EnsuresName, method, out genericArgument) && genericArgument == null;
    }

    public bool IsPlainPrecondition(Method method)
    {
      TypeNode genericArgument;
      return IsContractMethod (RequiresName, method, out genericArgument);
    }

    private bool IsContractMethod(string methodName, Method m, out TypeNode genericArgument)
    {
      genericArgument = null;
      if (m == null)
        return false;
      if (m.HasGenericParameters) {
        if (m.GenericParameters == null || m.GenericParameters.Count != 1)
          return false;
        genericArgument = m.GenericParameters[0];
      }
      
      return m.Name != null && m.Name == methodName &&
             (m.DeclaringType.Equals (this.ContractClass)
              || (m.Parameters != null && m.Parameters.Count == 3 && m.DeclaringType != null && m.DeclaringType.Name != ContractClassName));
    }

    public bool IsEndContractBlock(Method method)
    {
      TypeNode dummy;
      return IsContractMethod (EndContractBlockName, method, out dummy);
    }
  }
}