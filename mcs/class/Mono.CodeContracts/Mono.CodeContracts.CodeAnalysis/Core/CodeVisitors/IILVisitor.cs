using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Core.AST;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors
{
  public interface IILVisitor<Label, Local, Parameter, Method, Field, Type, Source, Dest, Data, Result> :
    IExpressionILVisitor<Label, Type, Source, Dest, Data, Result>,
    ISyntheticILVisitor<Label,Method, Field, Type, Source, Dest, Data, Result>
  {
    Result Arglist(Label pc, Dest dest, Data data);

    Result Branch(Label pc, Label target, bool leavesExceptionBlock, Data data);
    Result BranchCond(Label pc, Label target, BranchOperator bop, Source value1, Source value2, Data data);
    Result BranchTrue(Label pc, Label target, Source cond, Data data);
    Result BranchFalse(Label pc, Label target, Source cond, Data data);

    Result Break(Label pc, Data data);

    Result Call<TypeList, ArgList>(Label pc, Method method, bool virt, TypeList extraVarargs, Dest dest, ArgList args, Data data)
      where TypeList : IIndexable<Type>
      where ArgList : IIndexable<Source>;

    Result Calli<TypeList, ArgList>(Label pc, Type returnType, TypeList argTypes, bool instance, Dest dest, Source functionPointer, ArgList args, Data data)
      where TypeList : IIndexable<Type>
      where ArgList : IIndexable<Source>;

    Result CheckFinite(Label pc, Dest dest, Source source, Data data);

    Result CopyBlock(Label pc, Source destAddress, Source srcAddress, Source len, Data data);

    Result EndFilter(Label pc, Source decision, Data data);
    Result EndFinally(Label pc, Data data);

    //initblk

    Result Jmp(Label pc, Method method, Data data);


    Result LoadArg(Label pc, Parameter argument, bool isOld, Dest dest, Data data);
//    Result LoadArgAddress(Label pc, Parameter argument, Dest dest, Data data);

    Result LoadLocal(Label pc, Local local, Dest dest, Data data);
//    Result LoadLocalAddress(Label pc, Local local, Dest dest, Data data);
    //Ldind -- unsafe 
    //ldftn

//    Result LocalAlloc

    Result Nop(Label pc, Data data);

    Result Pop(Label pc, Source source, Data data);

    Result Return(Label pc, Source source, Data data);

    Result StoreArg(Label pc, Parameter argument, Source source, Data data);
    Result StoreLocal(Label pc, Local local, Source source, Data data);

    Result Switch(Label pc, Type type, IEnumerable<Pair<object, Label>> cases, Source value, Data data);

    Result Box(Label pc, Type type, Dest dest, Source source, Data data);

    Result ConstrainedCallvirt<TypeList, ArgList>(Label pc, Method method, Type constraint, TypeList extraVarargs, Dest dest, ArgList args, Data data)
      where TypeList : IIndexable<Type>
      where ArgList : IIndexable<Source>;

    Result CastClass(Label pc, Type type, Dest dest, Source obj, Data data);

    Result CopyObj(Label pc, Type type, Source destPtr, Source sourcePtr, Data data);

    Result Initobj(Label pc, Type type, Source ptr, Data data);

    Result LoadElement(Label pc, Type type, Dest dest, Source array, Source index, Data data);

//    Result LoadElementAddress(Label pc, Type type, Dest dest, Source array, Source index, Data data);

    Result LoadField(Label pc, Field field, Dest dest, Source obj, Data data);

//    Result LoadFieldAddress(Label pc, Field field, Dest dest, Source obj, Data data);

    Result LoadLength(Label pc, Dest dest, Source array, Data data);

    Result LoadStaticField(Label pc, Field field, Dest dest, Data data);

//    Result LoadStaticFieldAddress(Label pc, Field field, Dest dest, Data data);

    Result LoadTypeToken(Label pc, Type type, Dest dest, Data data);
    Result LoadFieldToken(Label pc, Field type, Dest dest, Data data);
    Result LoadMethodToken(Label pc, Method type, Dest dest, Data data);

    Result NewArray<ArgList>(Label pc, Type type, Dest dest, ArgList list, Data data)
      where ArgList : IIndexable<Source>;

    Result NewObj<ArgList>(Label pc, Method ctor, Dest dest, ArgList args, Data data)
      where ArgList : IIndexable<Source>;

    Result MkRefAny(Label pc, Type type, Dest dest, Source obj, Data data);

    Result RefAnyType(Label pc, Dest dest, Source source, Data data);

    Result RefAnyVal(Label pc, Type type, Dest dest, Source source, Data data);

    Result Rethrow(Label pc, Data data);

    Result StoreElement(Label pc, Type type, Source array, Source index, Source value, Data data);

    Result StoreField(Label pc, Field field, Source obj, Source value, Data data);
    Result StoreStaticField(Label pc, Field field, Source value, Data data);

    Result Throw(Label pc, Source exception, Data data);

    Result Unbox(Label pc, Type type, Dest dest, Source obj, Data data);
    Result UnboxAny(Label pc, Type type, Dest dest, Source obj, Data data);
  }
}