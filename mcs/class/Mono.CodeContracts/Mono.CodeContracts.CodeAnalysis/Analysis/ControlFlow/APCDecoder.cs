using System;
using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow;
using Mono.CodeContracts.CodeAnalysis.Core.AST;
using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow
{
  public class APCDecoder<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> :
    IILDecoder<APC, Local, Parameter, Method, Field, Type, Dummy, Dummy, IMethodContext<Field, Method>, Dummy>,
    IMethodContext<Field, Method>,
    IMethodContextData<Field, Method>

  {
    private readonly IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadata_decoder;
    private readonly MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> method_repository;
    private readonly ControlFlowGraph<Method, Type> underlyingCFG;

    public APCDecoder(ControlFlowGraph<Method, Type> underlyingCFG,
                      IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder,
                      MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodRepository)
    {
      this.underlyingCFG = underlyingCFG;
      this.metadata_decoder = metadataDecoder;
      this.method_repository = methodRepository;
    }

    #region Implementation of IILDecoder<APC,Local,Parameter,Method,Field,Type,Dummy,Dummy,IMethodContext<Field,Method>,Dummy>
    public IMethodContext<Field, Method> Context
    {
      get { return this; }
    }

    public Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data) where Visitor : IILVisitor<APC, Local, Parameter, Method, Field, Type, Dummy, Dummy, Data, Result>
    {
      return this.method_repository.ForwardDecode<Data, Result, RemoveBranchDelegator<Data, Result, Visitor>> 
        (pc, new RemoveBranchDelegator<Data, Result, Visitor> (visitor, this.metadata_decoder), data);
    }

    public bool IsUnreachable(APC pc)
    {
      return false;
    }

    public Dummy EdgeData(APC arg1, APC arg2)
    {
      return Dummy.Value;
    }
    #endregion

    #region Implementation of IMethodContext<Field,Method>
    public IMethodContextData<Field, Method> MethodContext
    {
      get { return this; }
    }
    #endregion

    #region Implementation of IMethodContextData<Field,Method>
    public Method CurrentMethod
    {
      get { return this.underlyingCFG.CFGMethod; }
    }

    public ICFG CFG
    {
      get { return this.underlyingCFG; }
    }
    #endregion

    /// <summary>
    /// This class wraps underlying visitor.
    /// Replaces: branches to nop; branchCond to binary.
    /// 
    /// EdgeTag.Requires: (inside method) => assume, (outside method) => assert
    /// EdgeTag.Ensures:  (inside method) => assert, (outside method) => assume
    /// </summary>
    /// <typeparam name="Data"></typeparam>
    /// <typeparam name="Result"></typeparam>
    /// <typeparam name="Visitor"></typeparam>
    private struct RemoveBranchDelegator<Data, Result, Visitor>
     : IILVisitor<APC, Local, Parameter, Method, Field, Type, Dummy, Dummy, Data, Result>
     where Visitor : IILVisitor<APC, Local, Parameter, Method, Field, Type, Dummy, Dummy, Data, Result>
    {
      private readonly Visitor visitor;
      private readonly IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadata_decoder;

      public RemoveBranchDelegator(Visitor visitor,
        IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder)
      {
        this.visitor = visitor;
        metadata_decoder = metadataDecoder;
      }

      #region Implementation of IExpressionILVisitor<APC,Type,Dummy,Dummy,Data,Result>
      public Result Binary(APC pc, BinaryOperator op, Dummy dest, Dummy operand1, Dummy operand2, Data data)
      {
        return this.visitor.Binary (pc, op, dest, operand1, operand2, data);
      }

      public Result Isinst(APC pc, Type type, Dummy dest, Dummy obj, Data data)
      {
        return this.visitor.Isinst (pc, type, dest, obj, data);
      }

      public Result LoadNull(APC pc, Dummy dest, Data data)
      {
        return this.visitor.LoadNull (pc, dest, data);
      }

      public Result LoadConst(APC pc, Type type, object constant, Dummy dest, Data data)
      {
        return this.visitor.LoadConst (pc, type, constant, dest, data);
      }

      public Result Sizeof(APC pc, Type type, Dummy dest, Data data)
      {
        return this.visitor.Sizeof (pc, type, dest, data);
      }

      public Result Unary(APC pc, UnaryOperator op, bool unsigned, Dummy dest, Dummy source, Data data)
      {
        return this.visitor.Unary (pc, op, unsigned, dest, source, data);
      }
      #endregion

      #region Implementation of ISyntheticILVisitor<APC,Method,Field,Type,Dummy,Dummy,Data,Result>
      public Result Entry(APC pc, Method method, Data data)
      {
        return this.visitor.Entry (pc, method, data);
      }

      public Result Assume(APC pc, EdgeTag tag, Dummy condition, Data data)
      {
        if (tag == EdgeTag.Requires && pc.InsideRequiresAtCall || tag == EdgeTag.Invariant && pc.InsideInvariantOnExit)
          return this.visitor.Assert (pc, tag, condition, data);

        return this.visitor.Assume (pc, tag, condition, data);
      }
      public Result Assert(APC pc, EdgeTag tag, Dummy condition, Data data)
      {
        if (pc.InsideEnsuresAtCall)
          return this.visitor.Assume (pc, tag, condition, data);
        
        return this.visitor.Assert (pc, tag, condition, data);
      }

      public Result BeginOld(APC pc, APC matchingEnd, Data data)
      {
        throw new NotImplementedException();
      }

      public Result EndOld(APC pc, APC matchingBegin, Type type, Dummy dest, Dummy source, Data data)
      {
        throw new NotImplementedException();
      }
      #endregion

      #region Implementation of IILVisitor<APC,Local,Parameter,Method,Field,Type,Dummy,Dummy,Data,Result>
      public Result Arglist(APC pc, Dummy dest, Data data)
      {
        return this.visitor.Arglist (pc, dest, data);
      }

      public Result Branch(APC pc, APC target, bool leavesExceptionBlock, Data data)
      {
        return this.visitor.Nop (pc, data);
      }

      public Result BranchCond(APC pc, APC target, BranchOperator bop, Dummy value1, Dummy value2, Data data)
      {
        Dummy dest = Dummy.Value;
        switch(bop) {
          case BranchOperator.Beq:
            return this.visitor.Binary (pc, BinaryOperator.Ceq, dest, value1, value2, data);
          case BranchOperator.Bge:
            return this.visitor.Binary(pc, BinaryOperator.Cge, dest, value1, value2, data);
          case BranchOperator.Bge_Un:
            return this.visitor.Binary(pc, BinaryOperator.Cge_Un, dest, value1, value2, data);
          case BranchOperator.Bgt:
            return this.visitor.Binary(pc, BinaryOperator.Cgt, dest, value1, value2, data);
          case BranchOperator.Bgt_Un:
            return this.visitor.Binary(pc, BinaryOperator.Cgt_Un, dest, value1, value2, data);
          case BranchOperator.Ble:
            return this.visitor.Binary(pc, BinaryOperator.Cle, dest, value1, value2, data);
          case BranchOperator.Ble_Un:
            return this.visitor.Binary(pc, BinaryOperator.Cle_Un, dest, value1, value2, data);
          case BranchOperator.Blt:
            return this.visitor.Binary(pc, BinaryOperator.Clt, dest, value1, value2, data);
          case BranchOperator.Blt_Un:
            return this.visitor.Binary(pc, BinaryOperator.Clt_Un, dest, value1, value2, data);
          case BranchOperator.Bne_un:
            return this.visitor.Binary(pc, BinaryOperator.Cne_Un, dest, value1, value2, data);
          default:
            return this.visitor.Nop (pc, data);
        }
      }

      public Result BranchTrue(APC pc, APC target, Dummy cond, Data data)
      {
        return this.visitor.Nop (pc, data);
      }

      public Result BranchFalse(APC pc, APC target, Dummy cond, Data data)
      {
        return this.visitor.Nop (pc, data);
      }

      public Result Break(APC pc, Data data)
      {
        return this.visitor.Break (pc, data);
      }

      public Result Call<TypeList, ArgList>(APC pc, Method method, bool virt, TypeList extraVarargs, Dummy dest, ArgList args, Data data)
        where TypeList : IIndexable<Type>
        where ArgList : IIndexable<Dummy>
      {
        return this.visitor.Call (pc, method, virt, extraVarargs, dest, args, data);
      }

      public Result Calli<TypeList, ArgList>(APC pc, Type returnType, TypeList argTypes, bool instance, Dummy dest, Dummy functionPointer, ArgList args, Data data)
        where TypeList : IIndexable<Type>
        where ArgList : IIndexable<Dummy>
      {
        return this.visitor.Calli (pc, returnType, argTypes, instance, dest, functionPointer, args, data);
      }

      public Result CheckFinite(APC pc, Dummy dest, Dummy source, Data data)
      {
        return this.visitor.CheckFinite (pc, dest, source, data);
      }

      public Result CopyBlock(APC pc, Dummy destAddress, Dummy srcAddress, Dummy len, Data data)
      {
        return this.visitor.CopyBlock (pc, destAddress, srcAddress, len, data);
      }

      public Result EndFilter(APC pc, Dummy decision, Data data)
      {
        return this.visitor.EndFilter (pc, decision, data);
      }

      public Result EndFinally(APC pc, Data data)
      {
        return this.visitor.EndFinally (pc, data);
      }

      public Result Jmp(APC pc, Method method, Data data)
      {
        return this.visitor.Jmp (pc, method, data);
      }

      public Result LoadArg(APC pc, Parameter argument, bool isOld, Dummy dest, Data data)
      {
        return this.visitor.LoadArg (pc, argument, isOld, dest, data);
      }

      public Result LoadLocal(APC pc, Local local, Dummy dest, Data data)
      {
        return this.visitor.LoadLocal (pc, local, dest, data);
      }

      public Result Nop(APC pc, Data data)
      {
        return this.visitor.Nop (pc, data);
      }

      public Result Pop(APC pc, Dummy source, Data data)
      {
        return this.visitor.Pop (pc, source, data);
      }

      public Result Return(APC pc, Dummy source, Data data)
      {
        return this.visitor.Return (pc, source, data);
      }

      public Result StoreArg(APC pc, Parameter argument, Dummy source, Data data)
      {
        return this.visitor.StoreArg (pc, argument, source, data);
      }

      public Result StoreLocal(APC pc, Local local, Dummy source, Data data)
      {
        return this.visitor.StoreLocal (pc, local, source, data);
      }

      public Result Switch(APC pc, Type type, IEnumerable<Pair<object, APC>> cases, Dummy value, Data data)
      {
        return this.visitor.Nop (pc, data);
      }

      public Result Box(APC pc, Type type, Dummy dest, Dummy source, Data data)
      {
        return this.visitor.Box (pc, type, dest, source, data);
      }

      public Result ConstrainedCallvirt<TypeList, ArgList>(APC pc, Method method, Type constraint, TypeList extraVarargs, Dummy dest, ArgList args, Data data)
        where TypeList : IIndexable<Type>
        where ArgList : IIndexable<Dummy>
      {
        return this.visitor.ConstrainedCallvirt (pc, method, constraint, extraVarargs, dest, args, data);
      }

      public Result CastClass(APC pc, Type type, Dummy dest, Dummy obj, Data data)
      {
        return this.visitor.CastClass (pc, type, dest, obj, data);
      }

      public Result CopyObj(APC pc, Type type, Dummy destPtr, Dummy sourcePtr, Data data)
      {
        return this.visitor.CopyObj (pc, type, destPtr, sourcePtr, data);
      }

      public Result Initobj(APC pc, Type type, Dummy ptr, Data data)
      {
        return this.visitor.Initobj (pc, type, ptr, data);
      }

      public Result LoadElement(APC pc, Type type, Dummy dest, Dummy array, Dummy index, Data data)
      {
        return this.visitor.LoadElement (pc, type, dest, array, index, data);
      }

      public Result LoadField(APC pc, Field field, Dummy dest, Dummy obj, Data data)
      {
        return this.visitor.LoadField (pc, field, dest, obj, data);
      }

      public Result LoadLength(APC pc, Dummy dest, Dummy array, Data data)
      {
        return this.visitor.LoadLength (pc, dest, array, data);
      }

      public Result LoadStaticField(APC pc, Field field, Dummy dest, Data data)
      {
        return this.visitor.LoadStaticField (pc, field, dest, data);
      }

      public Result LoadTypeToken(APC pc, Type type, Dummy dest, Data data)
      {
        return this.visitor.LoadTypeToken (pc, type, dest, data);
      }

      public Result LoadFieldToken(APC pc, Field type, Dummy dest, Data data)
      {
        return this.visitor.LoadFieldToken (pc, type, dest, data);
      }

      public Result LoadMethodToken(APC pc, Method type, Dummy dest, Data data)
      {
        return this.visitor.LoadMethodToken (pc, type, dest, data);
      }

      public Result NewArray<ArgList>(APC pc, Type type, Dummy dest, ArgList list, Data data) where ArgList : IIndexable<Dummy>
      {
        return this.visitor.NewArray (pc, type, dest, list, data);
      }

      public Result NewObj<ArgList>(APC pc, Method ctor, Dummy dest, ArgList args, Data data) where ArgList : IIndexable<Dummy>
      {
        return this.visitor.NewObj (pc, ctor, dest, args, data);
      }

      public Result MkRefAny(APC pc, Type type, Dummy dest, Dummy obj, Data data)
      {
        return this.visitor.MkRefAny (pc, type, dest, obj, data);
      }

      public Result RefAnyType(APC pc, Dummy dest, Dummy source, Data data)
      {
        return this.visitor.RefAnyType (pc, dest, source, data);
      }

      public Result RefAnyVal(APC pc, Type type, Dummy dest, Dummy source, Data data)
      {
        return this.visitor.RefAnyVal (pc, type, dest, source, data);
      }

      public Result Rethrow(APC pc, Data data)
      {
        return this.visitor.Rethrow (pc, data);
      }

      public Result StoreElement(APC pc, Type type, Dummy array, Dummy index, Dummy value, Data data)
      {
        return this.visitor.StoreElement (pc, type, array, index, value, data);
      }

      public Result StoreField(APC pc, Field field, Dummy obj, Dummy value, Data data)
      {
        return this.visitor.StoreField (pc, field, obj, value, data);
      }

      public Result StoreStaticField(APC pc, Field field, Dummy value, Data data)
      {
        return this.visitor.StoreStaticField (pc, field, value, data);
      }

      public Result Throw(APC pc, Dummy exception, Data data)
      {
        return this.visitor.Throw (pc, exception, data);
      }

      public Result Unbox(APC pc, Type type, Dummy dest, Dummy obj, Data data)
      {
        return this.visitor.Unbox (pc, type, dest, obj, data);
      }

      public Result UnboxAny(APC pc, Type type, Dummy dest, Dummy obj, Data data)
      {
        return this.visitor.UnboxAny (pc, type, dest, obj, data);
      }
      #endregion
    }
  }
}