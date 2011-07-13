using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Core.Consumers;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Core.Providers
{
  public interface IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>
  {
    Type ReturnType(Method method);
    IIndexable<Parameter> Parameters(Method method);
    Parameter This(Method method);
    
    string Name(Method method);
    string Name(Field field);
    string Name(Type type);

    Type FieldType(Field field);
    
    string FullName(Method method);
    string FullName(Type type);

    Type DeclaringType(Method method);
    Type DeclaringType(Field field);

    bool IsMain(Method method);
    bool IsStatic(Method method);
    bool IsStatic(Field field);
    bool IsPrivate(Method method);
    bool IsProtected(Method method);
    bool IsPublic(Method method);
    bool IsVirtual(Method method);
    bool IsNewSlot(Method method);
    bool IsOverride(Method method);
    bool IsFinal(Method method);
    bool IsConstructor(Method method);
    bool IsAbstract(Method method);
    bool IsPropertySetter(Method method, out Property property);
    bool IsPropertyGetter(Method method, out Property property);
    bool IsAutoPropertyMember(Method method);
    bool IsFinalizer(Method method);
    bool IsDispose(Method method);

    bool HasBody(Method method);
    bool DerivesFrom(Type sub, Type type);
    bool Equal(Type type, Type otherType);
    bool TryGetImplementingMethod(Type type, Method calledMethod, out Method implementingMethod);
    Method Unspecialized(Method method);
    IEnumerable<Method> OverridenAndImplementedMethods(Method method);
    Type ManagedPointer(Type type);
    bool TryGetRootMethod(Method method, out Method rootMethod);
    IEnumerable<Method> ImplementedMethods(Method method);

    Result AccessMethodBody<Data, Result>(Method method, IMethodCodeConsumer<Local, Parameter, Method, Field, Type, Data, Result> consumer, Data data);
    bool IsReferenceType(Type type);
    Method DeclaringMethod(Parameter argument);
    int ArgumentIndex(Parameter argument);
    bool IsVoidMethod(Method method);
    
    bool TryLoadAssembly(string filename, out Assembly assembly);
    IEnumerable<Method> GetMethods(Assembly assembly);
    string Name(Parameter parameter);
    Type ParameterType(Parameter argument);
    Type LocalType(Local local);
    
    Type System_Int32 { get; }
  }
}