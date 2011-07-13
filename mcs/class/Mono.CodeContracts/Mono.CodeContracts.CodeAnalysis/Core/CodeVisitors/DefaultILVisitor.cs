using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Core.AST;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors
{
  public abstract class DefaultILVisitor<Label, Local, Parameter, Method, Field, Type, Source, Dest, Data, Result>
    : IILVisitor<Label, Local, Parameter, Method, Field, Type, Source,Dest, Data, Result>
  {
    public abstract Result DefaultVisit(Label pc, Data data);

    #region Implementation of IExpressionILVisitor<Label,Type,Source,Dest,Data,Result>
    public virtual Result Binary(Label pc, BinaryOperator op, Dest dest, Source operand1, Source operand2, Data data)
    {
      return DefaultVisit (pc, data);
    }
    public virtual Result Isinst(Label pc, Type type, Dest dest, Source obj, Data data)
    {
      return DefaultVisit(pc, data);
    }
    public virtual Result LoadNull(Label pc, Dest dest, Data data)
    {
      return DefaultVisit(pc, data);
    }
    public virtual Result LoadConst(Label pc, Type type, object constant, Dest dest, Data data)
    {
      return DefaultVisit(pc, data);
    }
    public virtual Result Sizeof(Label pc, Type type, Dest dest, Data data)
    {
      return DefaultVisit(pc, data);
    }
    public virtual Result Unary(Label pc, UnaryOperator op, bool unsigned, Dest dest, Source source, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    #endregion

    #region Implementation of IILVisitor<Label,Local,Parameter,Method,Field,Type,Source,Dest,Data,Result>
    public virtual Result Arglist(Label pc, Dest dest, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Branch(Label pc, Label target, bool leavesExceptionBlock, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result BranchCond(Label pc, Label target, BranchOperator bop, Source value1, Source value2, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result BranchTrue(Label pc, Label target, Source cond, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result BranchFalse(Label pc, Label target, Source cond, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Break(Label pc, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Call<TypeList, ArgList>(Label pc, Method method, bool virt, TypeList extraVarargs, Dest dest, ArgList args, Data data) 
      where TypeList : IIndexable<Type> where ArgList : IIndexable<Source>
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Calli<TypeList, ArgList>(Label pc, Type returnType, TypeList argTypes, bool instance, Dest dest, Source functionPointer, ArgList args, Data data) 
      where TypeList : IIndexable<Type> where ArgList : IIndexable<Source>
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result CheckFinite(Label pc, Dest dest, Source source, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result CopyBlock(Label pc, Source destAddress, Source srcAddress, Source len, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result EndFilter(Label pc, Source decision, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result EndFinally(Label pc, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Jmp(Label pc, Method method, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result LoadArg(Label pc, Parameter argument, bool isOld, Dest dest, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result LoadLocal(Label pc, Local local, Dest dest, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Nop(Label pc, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Pop(Label pc, Source source, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Return(Label pc, Source source, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result StoreArg(Label pc, Parameter argument, Source source, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result StoreLocal(Label pc, Local local, Source source, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Switch(Label pc, Type type, IEnumerable<Pair<object, Label>> cases, Source value, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Box(Label pc, Type type, Dest dest, Source source, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result ConstrainedCallvirt<TypeList, ArgList>(Label pc, Method method, Type constraint, TypeList extraVarargs, Dest dest, ArgList args, Data data) where TypeList : IIndexable<Type> where ArgList : IIndexable<Source>
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result CastClass(Label pc, Type type, Dest dest, Source obj, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result CopyObj(Label pc, Type type, Source destPtr, Source sourcePtr, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Initobj(Label pc, Type type, Source ptr, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result LoadElement(Label pc, Type type, Dest dest, Source array, Source index, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result LoadField(Label pc, Field field, Dest dest, Source obj, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result LoadLength(Label pc, Dest dest, Source array, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result LoadStaticField(Label pc, Field field, Dest dest, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result LoadTypeToken(Label pc, Type type, Dest dest, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result LoadFieldToken(Label pc, Field type, Dest dest, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result LoadMethodToken(Label pc, Method type, Dest dest, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result NewArray<ArgList>(Label pc, Type type, Dest dest, ArgList list, Data data) where ArgList : IIndexable<Source>
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result NewObj<ArgList>(Label pc, Method ctor, Dest dest, ArgList args, Data data) where ArgList : IIndexable<Source>
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result MkRefAny(Label pc, Type type, Dest dest, Source obj, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result RefAnyType(Label pc, Dest dest, Source source, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result RefAnyVal(Label pc, Type type, Dest dest, Source source, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Rethrow(Label pc, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result StoreElement(Label pc, Type type, Source array, Source index, Source value, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result StoreField(Label pc, Field field, Source obj, Source value, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result StoreStaticField(Label pc, Field field, Source value, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Throw(Label pc, Source exception, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Unbox(Label pc, Type type, Dest dest, Source obj, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result UnboxAny(Label pc, Type type, Dest dest, Source obj, Data data)
    {
      return this.DefaultVisit (pc, data);
    }
    #endregion

    #region Implementation of ISyntheticILVisitor<Label,Method,Field,Type,Source,Dest,Data,Result>
    public virtual Result Entry(Label pc, Method tag, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result Assume(Label pc, EdgeTag tag, Source condition, Data data)
    {
      return this.DefaultVisit (pc, data);
    }
    public virtual Result Assert(Label pc, EdgeTag tag, Source condition, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    public virtual Result BeginOld(Label pc, Label matchingEnd, Data data)
    {
      return this.DefaultVisit (pc, data);
    }
    public virtual Result EndOld(Label pc, Label matchingBegin, Type type, Dest dest, Source source, Data data)
    {
      return this.DefaultVisit(pc, data);
    }
    #endregion
  }
}