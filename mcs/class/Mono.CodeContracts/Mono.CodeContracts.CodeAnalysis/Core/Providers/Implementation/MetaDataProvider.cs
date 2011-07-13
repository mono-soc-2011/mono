using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.CodeContracts.CodeAnalysis.Core.AST;
using Mono.CodeContracts.CodeAnalysis.Core.Consumers;
using Mono.CodeContracts.CodeAnalysis.Core.ContractsExtraction;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Core.Providers.Implementation
{
  public class MetaDataProvider
    : IMetaDataProvider<Local, Parameter, Method, FieldReference, PropertyReference, EventReference, TypeNode, Attribute, AssemblyNode>
  {
    public static readonly MetaDataProvider Instance = new MetaDataProvider ();

    public Result AccessMethodBody<Data, Result>(Method method, IMethodCodeConsumer<Local, Parameter, Method, FieldReference, TypeNode, Data, Result> consumer, Data data)
    {
      return consumer.Accept (CodeProviderImpl.Instance, CodeProviderImpl.Instance.Entry (method), method, data);
    }

    public bool IsReferenceType(TypeNode type)
    {
      return (!type.IsValueType);
    }

    public Method DeclaringMethod(Parameter argument)
    {
      return argument.DeclaringMethod;
    }

    public int ArgumentIndex(Parameter argument)
    {
      return argument.Index;
    }

    public bool IsVoidMethod(Method method)
    {
      return method.ReturnType.Equals (CoreSystemTypes.Instance.TypeVoid);
    }

    public bool TryLoadAssembly(string filename, out AssemblyNode assembly)
    {
      assembly = AssemblyNode.ReadAssembly (filename);
      if (!TryLoadContractNodes(ref assembly)) {
        Console.WriteLine ("No contract assemblies loaded");
        return false;
      }

      return true;
    }

    #region Implementation of IMetaDataProvider<Local,Parameter,Method,FieldReference,PropertyReference,EventReference,TypeNode,Attribute,AssemblyNode>
    public IEnumerable<Method> GetMethods(AssemblyNode assembly)
    {
      return assembly.Modules.SelectMany (a => a.Types).SelectMany (t => t.Methods);
    }

    public string Name(Parameter parameter)
    {
      throw new NotImplementedException ();
    }

    public TypeNode ParameterType(Parameter argument)
    {
      throw new NotImplementedException ();
    }

    public TypeNode LocalType(Local local)
    {
      throw new NotImplementedException ();
    }

    public TypeNode System_Int32
    {
      get { throw new NotImplementedException (); }
    }
    #endregion

    private bool TryLoadContractNodes(ref AssemblyNode assembly)
    {
      ContractNodes nodes = null;
      foreach (var module in assembly.Modules)
      {
        var assemblyResolver = module.Definition.AssemblyResolver;
        foreach (var reference in module.Definition.AssemblyReferences)
        {
          AssemblyDefinition def = assemblyResolver.Resolve(reference);
          nodes = ContractNodes.GetContractNodes(new AssemblyNode(def), (s) => { });
          if (nodes != null)
            break;
        }
      }

      if (nodes == null)
        return false;

      var extractor = new ContractExtractor(nodes, assembly, true);
      assembly = (AssemblyNode) extractor.Visit(assembly);
      return true;
    }

    public TypeNode ReturnType(Method method)
    {
      return method.ReturnType;
    }

    public IIndexable<Parameter> Parameters(Method method)
    {
      return new Indexable<Parameter> (method.Parameters);
    }

    public Parameter This(Method method)
    {
      return method.ThisParameter;
    }

    public string Name(Method method)
    {
      return method.Name;
    }

    public string Name(FieldReference field)
    {
      throw new NotImplementedException ();
    }

    public string Name(TypeNode type)
    {
      throw new NotImplementedException ();
    }

    public TypeNode FieldType(FieldReference field)
    {
      throw new NotImplementedException ();
    }

    public string FullName(Method method)
    {
      return method.FullName;
    }

    public string FullName(TypeNode type)
    {
      throw new NotImplementedException ();
    }

    public TypeNode DeclaringType(FieldReference field)
    {
      throw new NotImplementedException ();
    }

    public bool IsMain(Method method)
    {
      var entryPoint = method.MethodDefinition.Module.EntryPoint;
      
      return entryPoint != null && entryPoint.Equals (method);
    }

    public bool IsStatic(Method method)
    {
      return method.IsStatic;
    }

    public bool IsStatic(FieldReference field)
    {
      throw new NotImplementedException ();
    }

    public bool IsPrivate(Method method)
    {
      return method.IsPrivate;
    }

    public bool IsProtected(Method method)
    {
      return method.IsProtected;
    }

    public bool IsPublic(Method method)
    {
      return method.IsPublic;
    }

    public bool IsVirtual(Method method)
    {
      return method.IsVirtual;
    }

    public bool IsNewSlot(Method method)
    {
      return method.IsNewSlot;
    }

    public bool IsOverride(Method method)
    {
      return !method.IsNewSlot && method.HasOverrides;
    }

    public bool IsFinal(Method method)
    {
      return method.IsFinal;
    }

    public bool IsConstructor(Method method)
    {
      return method.IsConstructor;
    }

    public bool IsAbstract(Method method)
    {
      return method.IsAbstract;
    }

    public bool IsPropertySetter(Method method, out PropertyReference property)
    {
      //todo: implement this
      
      property = null;
      return false;
    }

    public bool IsPropertyGetter(Method method, out PropertyReference property)
    {
      //todo: implement this

      property = null;
      return false;
    }

    public TypeNode DeclaringType(Method method)
    {
      return method.DeclaringType;
    }

    public bool HasBody(Method method)
    {
      return method.HasBody;  
    }

    public bool DerivesFrom(TypeNode sub, TypeNode type)
    {
      return sub.IsAssignableTo (type);
    }

    public bool Equal(TypeNode type, TypeNode otherType)
    {
      return type == otherType;
    }

    public bool TryGetImplementingMethod(TypeNode type, Method calledMethod, out Method implementingMethod)
    {
      //todo: implement this
      implementingMethod = null;
      return false;
    }

    public Method Unspecialized(Method method)
    {
      if (method.HasGenericParameters)
        throw new NotImplementedException();
     
      return method;
    }

    public IEnumerable<Method> OverridenAndImplementedMethods(Method method)
    {
      //todo: implement this
      yield break;
    }

    public TypeNode ManagedPointer(TypeNode type)
    {
      return type.GetReferenceType ();
    }

    public bool TryGetRootMethod(Method method, out Method rootMethod)
    {
      //todo: implement this
      rootMethod = method;
      return true;
    }

    public IEnumerable<Method> ImplementedMethods(Method method)
    {
      yield break;
    }

    public bool IsAutoPropertyMember(Method method)
    {
      //todo: implement this
      return false;
    }

    public bool IsFinalizer(Method method)
    {
      return "Finalize" == method.Name;
    }

    public bool IsDispose(Method method)
    {
      if (method.Name != "Dispose" && method.Name != "System.IDisposable.Dispose")
        return false;
      if (method.Parameters == null || method.Parameters.Count == 0)
        return true;
      if (method.Parameters.Count == 1)
        return method.Parameters[0].Type == CoreSystemTypes.Instance.TypeBoolean;

      return false;
    }
  }
}