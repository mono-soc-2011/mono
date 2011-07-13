using System;
using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow;
using Mono.CodeContracts.CodeAnalysis.Core.AST;
using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.StackAnalysis
{
  public class StackDepthProvider<Local, Parameter, Method, Field, Property, Event, Type, ContextData, Attribute, Assembly> :
    IILVisitor<APC, Local, Parameter, Method, Field, Type, Dummy, Dummy, StackInfo, StackInfo>,
    IILDecoder<APC, Local, Parameter, Method, Field, Type, int, int, IStackContext<Field, Method>, Dummy>,
    IStackContext<Field, Method>, 
    IStackContextData<Field, Method>,
    IMethodContextData<Field, Method>,
    ICFG, 
    IStackInfo
    where Type : IEquatable<Type>
    where ContextData : IMethodContext<Field, Method>
  {
    private readonly IILDecoder<APC, Local, Parameter, Method, Field, Type, Dummy, Dummy, ContextData, Dummy> il_decoder;
    private readonly IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadata_decoder;
    private APCMap<int> localStackDepthCache;
    private int cachedSubroutine;
    private bool recursionGuard;

    private ICFG UnderlyingCFG { get { return this.il_decoder.Context.MethodContext.CFG; } }

    public IStackContextData<Field, Method> StackContext
    {
      get { return this; }
    }
    public IMethodContextData<Field, Method> MethodContext
    {
      get { return this; }
    }

    #region Implementation of IILDecoder<APC,Local,Parameter,Method,Field,Type,int,int,IStackContext<Field,Method>,Dummy>
    public IStackContext<Field, Method> Context
    {
      get { return this; }
    }

    public Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data) 
      where Visitor : IILVisitor<APC, Local, Parameter, Method, Field, Type, int, int, Data, Result>
    {
      if (pc.Index != 0 || pc.SubroutineContext != null || pc.Block != pc.Block.Subroutine.Exit || !pc.Block.Subroutine.IsMethod)
        return this.il_decoder.ForwardDecode<Data,Result, StackDecoder<Data, Result, Visitor>> (pc, new StackDecoder<Data,Result, Visitor> (this, visitor), data);
      if (!pc.Block.Subroutine.HasReturnValue)
        return visitor.Return (pc, -1, data);
      
      int source = this.Lookup (pc) - 1;
      return visitor.Return (pc, source, data);
    }

    public struct StackDecoder<Data, Result, Visitor> :
      IILVisitor<APC, Local, Parameter, Method, Field, Type, Dummy, Dummy, Data, Result>
      where Visitor : IILVisitor<APC, Local, Parameter, Method, Field, Type, int, int, Data, Result>
    {
      private readonly StackDepthProvider<Local, Parameter, Method, Field, Property, Event, Type, ContextData, Attribute, Assembly> parent;
      private readonly Visitor delegatee;

      public StackDecoder(StackDepthProvider<Local, Parameter, Method, Field, Property, Event, Type, ContextData, Attribute, Assembly> parent, 
                          Visitor delegatee)
      {
        this.parent = parent;
        this.delegatee = delegatee;
      }

      private int Pop(APC label, int offset)
      {
        return this.parent.Lookup (label) - 1 - offset;
      }
      private int Push(APC label, int args)
      {
        return this.parent.Lookup (label) - args;
      }

      #region Implementation of IExpressionILVisitor<APC,Type,Dummy,Dummy,Data,Result>
      public Result Binary(APC pc, BinaryOperator op, Dummy dest, Dummy operand1, Dummy operand2, Data data)
      {
        return this.delegatee.Binary (pc, op, this.Push (pc, 2), this.Pop (pc, 1), this.Pop (pc, 0), data);
      }

      public Result Isinst(APC pc, Type type, Dummy dest, Dummy obj, Data data)
      {
        return this.delegatee.Isinst (pc, type, this.Push (pc, 1), this.Pop (pc, 0), data);
      }

      public Result LoadNull(APC pc, Dummy dest, Data data)
      {
        return this.delegatee.LoadNull (pc, this.Push (pc, 0), data);
      }

      public Result LoadConst(APC pc, Type type, object constant, Dummy dest, Data data)
      {
        return this.delegatee.LoadConst (pc, type, constant, this.Push (pc, 0), data);
      }

      public Result Sizeof(APC pc, Type type, Dummy dest, Data data)
      {
        return this.delegatee.Sizeof (pc, type, this.Push (pc, 0), data);
      }

      public Result Unary(APC pc, UnaryOperator op, bool unsigned, Dummy dest, Dummy source, Data data)
      {
        return this.delegatee.Unary (pc, op, unsigned, this.Push (pc, 1), this.Pop (pc, 0), data);
      }
      #endregion

      #region Implementation of ISyntheticILVisitor<APC,Method,Field,Type,Dummy,Dummy,Data,Result>
      public Result Entry(APC pc, Method method, Data data)
      {
        return this.delegatee.Entry (pc, method, data);
      }

      public Result Assume(APC pc, EdgeTag tag, Dummy condition, Data data)
      {
        return this.delegatee.Assume (pc, tag, this.Pop (pc, 0), data);
      }

      public Result Assert(APC pc, EdgeTag tag, Dummy condition, Data data)
      {
        return this.delegatee.Assert (pc, tag, this.Pop (pc, 0), data);
      }

      public Result BeginOld(APC pc, APC matchingEnd, Data data)
      {
        throw new NotImplementedException ();
      }

      public Result EndOld(APC pc, APC matchingBegin, Type type, Dummy dest, Dummy source, Data data)
      {
        throw new NotImplementedException ();
      }
      #endregion

      #region Implementation of IILVisitor<APC,Local,Parameter,Method,Field,Type,Dummy,Dummy,Data,Result>
      public Result Arglist(APC pc, Dummy dest, Data data)
      {
        return this.delegatee.Arglist (pc, this.Push (pc, 0), data);
      }

      public Result Branch(APC pc, APC target, bool leavesExceptionBlock, Data data)
      {
        return this.delegatee.Branch (pc, target, leavesExceptionBlock, data);
      }

      public Result BranchCond(APC pc, APC target, BranchOperator bop, Dummy value1, Dummy value2, Data data)
      {
        return this.delegatee.BranchCond (pc, target, bop, this.Pop (pc, 1), this.Pop (pc, 0), data);
      }

      public Result BranchTrue(APC pc, APC target, Dummy cond, Data data)
      {
        return this.delegatee.BranchTrue (pc, target, this.Pop (pc, 0), data);
      }

      public Result BranchFalse(APC pc, APC target, Dummy cond, Data data)
      {
        return this.delegatee.BranchFalse(pc, target, this.Pop(pc, 0), data);
      }

      public Result Break(APC pc, Data data)
      {
        return this.delegatee.Break (pc, data);
      }

      public Result Call<TypeList, ArgList>(APC pc, Method method, bool virt, TypeList extraVarargs, Dummy dest, ArgList args, Data data) where TypeList : IIndexable<Type> where ArgList : IIndexable<Dummy>
      {
        throw new NotImplementedException ();
      }

      public Result Calli<TypeList, ArgList>(APC pc, Type returnType, TypeList argTypes, bool instance, Dummy dest, Dummy functionPointer, ArgList args, Data data) where TypeList : IIndexable<Type> where ArgList : IIndexable<Dummy>
      {
        throw new NotImplementedException ();
      }

      public Result CheckFinite(APC pc, Dummy dest, Dummy source, Data data)
      {
        return this.delegatee.CheckFinite (pc, this.Push (pc, 1), this.Pop (pc, 0), data);
      }

      public Result CopyBlock(APC pc, Dummy destAddress, Dummy srcAddress, Dummy len, Data data)
      {
        return this.delegatee.CopyBlock (pc, this.Pop (pc, 2), this.Pop (pc, 1), this.Pop (pc, 0), data);
      }

      public Result EndFilter(APC pc, Dummy decision, Data data)
      {
        return this.delegatee.EndFilter (pc, this.Pop (pc, 0), data);
      }

      public Result EndFinally(APC pc, Data data)
      {
        return this.delegatee.EndFinally (pc, data);
      }

      public Result Jmp(APC pc, Method method, Data data)
      {
        return this.delegatee.Jmp (pc, method, data);
      }

      public Result LoadArg(APC pc, Parameter argument, bool isOld, Dummy dest, Data data)
      {
        throw new NotImplementedException ();
      }

      public Result LoadLocal(APC pc, Local local, Dummy dest, Data data)
      {
        return this.delegatee.LoadLocal (pc, local, this.Push (pc, 0), data);
      }

      public Result Nop(APC pc, Data data)
      {
        return this.delegatee.Nop (pc, data);
      }

      public Result Pop(APC pc, Dummy source, Data data)
      {
        return this.delegatee.Pop (pc, this.Pop (pc, 0), data);
      }

      public Result Return(APC pc, Dummy source, Data data)
      {
        return this.delegatee.Nop (pc, data);
      }

      public Result StoreArg(APC pc, Parameter argument, Dummy source, Data data)
      {
        return this.delegatee.StoreArg (pc, argument, this.Pop (pc, 0), data);
      }

      public Result StoreLocal(APC pc, Local local, Dummy source, Data data)
      {
        return this.delegatee.StoreLocal (pc, local, this.Pop (pc, 0), data);
      }

      public Result Switch(APC pc, Type type, IEnumerable<Pair<object, APC>> cases, Dummy value, Data data)
      {
        throw new NotImplementedException ();
      }

      public Result Box(APC pc, Type type, Dummy dest, Dummy source, Data data)
      {
        type = this.GetSpecializedType (pc, type);
        if (this.IsReferenceType (pc, type))
          return this.delegatee.Nop (pc, data);

        return this.delegatee.Box (pc, type, this.Push (pc, 1), this.Pop (pc, 0), data);
      }

      private bool IsReferenceType(APC pc, Type type)
      {
        return this.parent.metadata_decoder.IsReferenceType (type);
      }

      private Type GetSpecializedType(APC pc, Type type)
      {
        var methodInfo = pc.Block.Subroutine as IMethodInfo<Method>;
        if (methodInfo == null)
          return type;
        
        throw new NotImplementedException();
      }

      public Result ConstrainedCallvirt<TypeList, ArgList>(APC pc, Method method, Type constraint, TypeList extraVarargs, Dummy dest, ArgList args, Data data) where TypeList : IIndexable<Type> where ArgList : IIndexable<Dummy>
      {
        throw new NotImplementedException ();
      }

      public Result CastClass(APC pc, Type type, Dummy dest, Dummy obj, Data data)
      {
        return this.delegatee.CastClass (pc, type, this.Push (pc, 1), this.Pop (pc, 0), data);
      }

      public Result CopyObj(APC pc, Type type, Dummy destPtr, Dummy sourcePtr, Data data)
      {
        return this.delegatee.CopyObj (pc, type, this.Pop (pc, 1), this.Pop (pc, 0), data);
      }

      public Result Initobj(APC pc, Type type, Dummy ptr, Data data)
      {
        return this.delegatee.Initobj (pc, type, this.Pop (pc, 0), data);
      }

      public Result LoadElement(APC pc, Type type, Dummy dest, Dummy array, Dummy index, Data data)
      {
        return this.delegatee.LoadElement (pc, type, this.Push (pc, 2), this.Pop (pc, 1), this.Pop (pc, 0), data);
      }

      public Result LoadField(APC pc, Field field, Dummy dest, Dummy obj, Data data)
      {
        return this.delegatee.LoadField (pc, field, this.Push (pc, 1), this.Pop (pc, 0), data);
      }

      public Result LoadLength(APC pc, Dummy dest, Dummy array, Data data)
      {
        return this.delegatee.LoadLength (pc, this.Push (pc, 1), this.Pop (pc, 0), data);
      }

      public Result LoadStaticField(APC pc, Field field, Dummy dest, Data data)
      {
        return this.delegatee.LoadStaticField (pc, field, this.Push (pc, 0), data);
      }

      public Result LoadTypeToken(APC pc, Type type, Dummy dest, Data data)
      {
        return this.delegatee.LoadTypeToken (pc, type, this.Push (pc, 0), data);
      }

      public Result LoadFieldToken(APC pc, Field field, Dummy dest, Data data)
      {
        return this.delegatee.LoadFieldToken (pc, field, this.Push (pc, 0), data);
      }

      public Result LoadMethodToken(APC pc, Method method, Dummy dest, Data data)
      {
        return this.delegatee.LoadMethodToken (pc, method, this.Push (pc, 0), data);
      }

      public Result NewArray<ArgList>(APC pc, Type type, Dummy dest, ArgList list, Data data) where ArgList : IIndexable<Dummy>
      {
        throw new NotImplementedException ();
      }

      public Result NewObj<ArgList>(APC pc, Method ctor, Dummy dest, ArgList args, Data data) where ArgList : IIndexable<Dummy>
      {
        throw new NotImplementedException ();
      }

      public Result MkRefAny(APC pc, Type type, Dummy dest, Dummy obj, Data data)
      {
        throw new NotImplementedException ();
      }

      public Result RefAnyType(APC pc, Dummy dest, Dummy source, Data data)
      {
        throw new NotImplementedException ();
      }

      public Result RefAnyVal(APC pc, Type type, Dummy dest, Dummy source, Data data)
      {
        throw new NotImplementedException ();
      }

      public Result Rethrow(APC pc, Data data)
      {
        return this.delegatee.Rethrow (pc, data);
      }

      public Result StoreElement(APC pc, Type type, Dummy array, Dummy index, Dummy value, Data data)
      {
        throw new NotImplementedException ();
      }

      public Result StoreField(APC pc, Field field, Dummy obj, Dummy value, Data data)
      {
        return this.delegatee.StoreField (pc, field, this.Pop (pc, 1), this.Pop (pc, 0), data);
      }

      public Result StoreStaticField(APC pc, Field field, Dummy value, Data data)
      {
        return this.delegatee.StoreStaticField (pc, field, this.Pop (pc, 0), data);
      }

      public Result Throw(APC pc, Dummy exception, Data data)
      {
        return this.delegatee.Throw (pc, this.Pop (pc, 0), data);
      }

      public Result Unbox(APC pc, Type type, Dummy dest, Dummy obj, Data data)
      {
        return this.delegatee.Unbox (pc, type, this.Push (pc, 1), this.Pop (pc, 0), data);
      }

      public Result UnboxAny(APC pc, Type type, Dummy dest, Dummy obj, Data data)
      {
        return this.delegatee.UnboxAny (pc, type, this.Push (pc, 1), this.Pop (pc, 0), data);
      }
      #endregion
    }

    private int Lookup(APC pc)
    {
      int num = this.LocalStackDepth (pc);
      if (pc.SubroutineContext == null || !pc.Block.Subroutine.HasContextDependentStackDepth)
        return num;
      
      CFGBlock block = pc.SubroutineContext.Head.From;
      return num + this.Lookup (APC.ForEnd (block, pc.SubroutineContext.Tail));
    }

    private int LocalStackDepth(APC pc)
    {
      return this.LocalStackMap (pc.Block.Subroutine)[pc];
    }

    private APCMap<int> LocalStackMap(Subroutine subroutine)
    {
      if (this.localStackDepthCache == null || this.cachedSubroutine != subroutine.Id) {
        this.localStackDepthCache = this.GetStackDepthMap (subroutine);
        this.cachedSubroutine = subroutine.Id;
      }
      return this.localStackDepthCache;
    }

    private APCMap<int> GetStackDepthMap(Subroutine subroutine)
    {
      APCMap<int> result;
      var key = new TypedKey<APCMap<int>> ("stackDepthKey");
      if (!subroutine.TryGetValue(key, out result )) {
        result = this.ComputeStackDepth (subroutine);
        subroutine.Add (key, result);
      }
      return result;
    }

    private APCMap<int> ComputeStackDepth(Subroutine subroutine)
    {
      Dictionary<int, StackInfo> dict = new Dictionary<int, StackInfo> (subroutine.BlockCount);
      APCMap<int> apcMap = new APCMap<int> (subroutine);

      foreach (CFGBlock block in subroutine.Blocks) {
        StackInfo stackInfo;
        if (!dict.TryGetValue(block.Index, out stackInfo))
          stackInfo = this.ComputeBlockStartDepth (block);
        foreach (APC apc in block.APCs()) {
          apcMap.Add (apc, stackInfo.Depth);
          stackInfo = this.il_decoder.ForwardDecode<StackInfo, StackInfo, IILVisitor<APC, Local, Parameter, Method, Field, Type, Dummy, Dummy, StackInfo, StackInfo>> (apc, this, stackInfo);
        }
        if (!apcMap.ContainsKey (block.Last))
          apcMap.Add (block.Last, stackInfo.Depth);
        foreach (var succ in subroutine.SuccessorBlocks (block)) {
          bool oldRecursionGuard = this.recursionGuard;
          this.recursionGuard = true;
          try {
            bool isExceptionHandlerEdge;
            foreach (var info in subroutine.EdgeSubroutinesOuterToInner (block, succ, out isExceptionHandlerEdge, null).AsEnumerable ())
              stackInfo.Adjust (info.Value.StackDelta);
          } finally {
            this.recursionGuard = oldRecursionGuard;
          }
          AddStartDepth (dict, succ, stackInfo);
        }
      }
      return apcMap;
    }

    private void AddStartDepth(Dictionary<int, StackInfo> dict, CFGBlock block, StackInfo stackDepth)
    {
      StackInfo stackInfo;
      if (dict.TryGetValue(block.Index, out stackInfo))
        return;
      dict.Add (block.Index, stackDepth.Copy ());
    }

    private StackInfo ComputeBlockStartDepth(CFGBlock block)
    {
      if (block.Subroutine.IsCatchFilterHeader(block))
        return new StackInfo (1, 2);
      return new StackInfo (0, 4);
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

    public StackDepthProvider(IILDecoder<APC, Local, Parameter, Method, Field, Type, Dummy, Dummy, ContextData, Dummy> ilDecoder,
                              IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder)
    {
      il_decoder = ilDecoder;
      metadata_decoder = metadataDecoder;
    }

    #region Implementation of ICFG
    APC ICFG.Entry
    {
      get { return this.UnderlyingCFG.Entry; }
    }
    APC ICFG.EntryAfterRequires
    {
      get { return this.UnderlyingCFG.EntryAfterRequires; ; }
    }
    APC ICFG.NormalExit
    {
      get { return this.UnderlyingCFG.NormalExit; ; }
    }
    APC ICFG.ExceptionExit
    {
      get { return this.UnderlyingCFG.ExceptionExit; ; }
    }

    Subroutine ICFG.Subroutine
    {
      get { return this.UnderlyingCFG.Subroutine; }
    }

    APC ICFG.Next(APC pc)
    {
      APC singleSuccessor;
      if (((ICFG)this).HasSingleSuccessor(pc, out singleSuccessor))
        return singleSuccessor;
      return pc;
    }

    bool ICFG.HasSingleSuccessor(APC pc, out APC ifFound)
    {
      DecoratorHelper.Push(this);
      try {
        return this.UnderlyingCFG.HasSingleSuccessor (pc, out ifFound);
      } finally {
        DecoratorHelper.Pop ();
      }
    }

    IEnumerable<APC> ICFG.Successors(APC pc)
    {
      DecoratorHelper.Push (this);
      try {
        return this.UnderlyingCFG.Successors (pc);
      } finally {
        DecoratorHelper.Pop ();
      }

    }

    bool ICFG.HasSinglePredecessor(APC pc, out APC ifFound)
    {
      DecoratorHelper.Push (this);
      try {
        return this.UnderlyingCFG.HasSinglePredecessor (pc, out ifFound);
      } finally {
        DecoratorHelper.Pop ();
      }
    }

    IEnumerable<APC> ICFG.Predecessors(APC pc)
    {
      DecoratorHelper.Push (this);
      try {
        return this.UnderlyingCFG.Predecessors (pc);
      } finally {
        DecoratorHelper.Pop ();
      }
    }

    bool ICFG.IsJoinPoint(APC pc)
    {
      #region 
      return this.UnderlyingCFG.IsJoinPoint (pc);
      #endregion

    }

    bool ICFG.IsSplitPoint(APC pc)
    {
      return this.UnderlyingCFG.IsSplitPoint (pc);
    }

    bool ICFG.IsBlockStart(APC pc)
    {
      return this.UnderlyingCFG.IsBlockStart (pc);
    }

    bool ICFG.IsBlockEnd(APC pc)
    {
      return this.UnderlyingCFG.IsBlockEnd (pc);
    }

    IILDecoder<APC, Local1, Parameter1, Method1, Field1, Type1, Dummy, Dummy, IMethodContext<Field1, Method1>, Dummy> 
      ICFG.GetDecoder<Local1, Parameter1, Method1, Field1, Property1, Event1, Type1, Attribute1, Assembly1>(
        IMetaDataProvider<Local1, Parameter1, Method1, Field1, Property1, Event1, Type1, Attribute1, Assembly1> metadataDecoder
      )
    {
      return this.UnderlyingCFG.GetDecoder (metadataDecoder);
    }
    #endregion

    #region Implementation of IStackInfo
    bool IStackInfo.IsCallOnThis(APC pc)
    {
      if (this.recursionGuard)
        return false;
      return this.LocalStackMap (pc.Block.Subroutine).IsCallOnThis (pc);
    }
    #endregion

    #region Implementation of IStackContextData<Field,Method>
    public int StackDepth(APC pc)
    {
      throw new NotImplementedException ();
    }
    #endregion

    #region Implementation of IMethodContextData<Field,Method>
    Method IMethodContextData<Field, Method>.CurrentMethod
    {
      get { return this.il_decoder.Context.MethodContext.CurrentMethod; }
    }

    ICFG IMethodContextData<Field, Method>.CFG
    {
      get { return this; }
    }
    #endregion

    private class APCMap<T>
    {
      private Dictionary<int, T>[] blockMap;
      private IImmutableMap<int, bool> callOnThisMap;

      public APCMap(Subroutine parent)
      {
        this.blockMap = new Dictionary<int, T>[parent.BlockCount];
        this.callOnThisMap = ImmutableMap<int, bool>.Empty ();
      }

      public T this[APC key]
      {
        get
        {
          T obj;
          if (!this.TryGetValue(key, out obj))
            throw new NotSupportedException("key not in table");
          return obj;
        }
      }

      public bool TryGetValue(APC pc, out T obj)
      {
        var dictionary = this.blockMap[pc.Block.Index];
        if (dictionary != null)
          return dictionary.TryGetValue (pc.Index, out obj);

        obj = default(T);
        return false;
      }
      
      public void Add(APC pc, T value)
      {
        var dictionary = this.blockMap[pc.Block.Index];
        if (dictionary == null)
          this.blockMap[pc.Block.Index] = dictionary = new Dictionary<int, T> ();

        dictionary.Add (pc.Index, value);
      }

      public bool ContainsKey(APC pc)
      {
        var dictionary = this.blockMap[pc.Block.Index];
        if (dictionary == null)
          return false;
        return dictionary.ContainsKey (pc.Index);
      }

      public bool IsCallOnThis(APC key)
      {
        return this.callOnThisMap[key.Block.Index];
      }
    }

    #region Implementation of IExpressionILVisitor<APC,Type,Dummy,Dummy,StackInfo,StackInfo>
    public StackInfo Binary(APC pc, BinaryOperator op, Dummy dest, Dummy operand1, Dummy operand2, StackInfo data)
    {
      return data.Pop (2).Push ();
    }

    public StackInfo Isinst(APC pc, Type type, Dummy dest, Dummy obj, StackInfo data)
    {
      return data;
    }

    public StackInfo LoadNull(APC pc, Dummy dest, StackInfo data)
    {
      return data.Push ();
    }

    public StackInfo LoadConst(APC pc, Type type, object constant, Dummy dest, StackInfo data)
    {
      return data.Push ();
    }

    public StackInfo Sizeof(APC pc, Type type, Dummy dest, StackInfo data)
    {
      return data.Push ();
    }

    public StackInfo Unary(APC pc, UnaryOperator op, bool unsigned, Dummy dest, Dummy source, StackInfo data)
    {
      return data.Pop (1).Push ();
    }
    #endregion

    #region Implementation of ISyntheticILVisitor<APC,Method,Field,Type,Dummy,Dummy,StackInfo,StackInfo>
    public StackInfo Entry(APC pc, Method method, StackInfo data)
    {
      return data;
    }

    public StackInfo Assume(APC pc, EdgeTag tag, Dummy condition, StackInfo data)
    {
      return data.Pop (1);
    }

    public StackInfo Assert(APC pc, EdgeTag tag, Dummy condition, StackInfo data)
    {
      return data.Pop (1);
    }

    public StackInfo BeginOld(APC pc, APC matchingEnd, StackInfo data)
    {
      throw new NotImplementedException ();
    }

    public StackInfo EndOld(APC pc, APC matchingBegin, Type type, Dummy dest, Dummy source, StackInfo data)
    {
      throw new NotImplementedException ();
    }
    #endregion

    #region Implementation of IILVisitor<APC,Local,Parameter,Method,Field,Type,Dummy,Dummy,StackInfo,StackInfo>
    public StackInfo Arglist(APC pc, Dummy dest, StackInfo data)
    {
      return data.Push ();
    }

    public StackInfo Branch(APC pc, APC target, bool leavesExceptionBlock, StackInfo data)
    {
      return data;
    }

    public StackInfo BranchCond(APC pc, APC target, BranchOperator bop, Dummy value1, Dummy value2, StackInfo data)
    {
      return data.Pop (2);
    }

    public StackInfo BranchTrue(APC pc, APC target, Dummy cond, StackInfo data)
    {
      return data.Pop (1);
    }

    public StackInfo BranchFalse(APC pc, APC target, Dummy cond, StackInfo data)
    {
      return data.Pop (1);
    }

    public StackInfo Break(APC pc, StackInfo data)
    {
      return data;
    }

    public StackInfo Call<TypeList, ArgList>(APC pc, Method method, bool virt, TypeList extraVarargs, Dummy dest, ArgList args, StackInfo data) where TypeList : IIndexable<Type> where ArgList : IIndexable<Dummy>
    {
      throw new NotImplementedException ();
    }

    public StackInfo Calli<TypeList, ArgList>(APC pc, Type returnType, TypeList argTypes, bool instance, Dummy dest, Dummy functionPointer, ArgList args, StackInfo data) where TypeList : IIndexable<Type> where ArgList : IIndexable<Dummy>
    {
      throw new NotImplementedException ();
    }

    public StackInfo CheckFinite(APC pc, Dummy dest, Dummy source, StackInfo data)
    {
      return data;
    }

    public StackInfo CopyBlock(APC pc, Dummy destAddress, Dummy srcAddress, Dummy len, StackInfo data)
    {
      return data.Pop (3);
    }

    public StackInfo EndFilter(APC pc, Dummy decision, StackInfo data)
    {
      return data.Pop (1);
    }

    public StackInfo EndFinally(APC pc, StackInfo data)
    {
      return new StackInfo (0, 0);
    }

    public StackInfo Jmp(APC pc, Method method, StackInfo data)
    {
      return new StackInfo (0, 0);
    }

    public StackInfo LoadArg(APC pc, Parameter argument, bool isOld, Dummy dest, StackInfo data)
    {
      if (!this.metadata_decoder.IsStatic(this.metadata_decoder.DeclaringMethod(argument)) && this.metadata_decoder.ArgumentIndex(argument) == 0)
        return data.PushThis ();

      return data.Push ();
    }

    public StackInfo LoadLocal(APC pc, Local local, Dummy dest, StackInfo data)
    {
      return data.Push ();
    }

    public StackInfo Nop(APC pc, StackInfo data)
    {
      return data;
    }

    public StackInfo Pop(APC pc, Dummy source, StackInfo data)
    {
      return data.Pop (1);
    }

    public StackInfo Return(APC pc, Dummy source, StackInfo data)
    {
      return data;
    }

    public StackInfo StoreArg(APC pc, Parameter argument, Dummy source, StackInfo data)
    {
      return data.Pop (1);
    }

    public StackInfo StoreLocal(APC pc, Local local, Dummy source, StackInfo data)
    {
      return data.Pop (1);
    }

    public StackInfo Switch(APC pc, Type type, IEnumerable<Pair<object, APC>> cases, Dummy value, StackInfo data)
    {
      throw new NotImplementedException ();
    }

    public StackInfo Box(APC pc, Type type, Dummy dest, Dummy source, StackInfo data)
    {
      return data.Pop (1).Push ();
    }

    public StackInfo ConstrainedCallvirt<TypeList, ArgList>(APC pc, Method method, Type constraint, TypeList extraVarargs, Dummy dest, ArgList args, StackInfo data) where TypeList : IIndexable<Type> where ArgList : IIndexable<Dummy>
    {
      throw new NotImplementedException ();
    }

    public StackInfo CastClass(APC pc, Type type, Dummy dest, Dummy obj, StackInfo data)
    {
      return data;
    }

    public StackInfo CopyObj(APC pc, Type type, Dummy destPtr, Dummy sourcePtr, StackInfo data)
    {
      return data.Pop (2);
    }

    public StackInfo Initobj(APC pc, Type type, Dummy ptr, StackInfo data)
    {
      return data.Pop (1);
    }

    public StackInfo LoadElement(APC pc, Type type, Dummy dest, Dummy array, Dummy index, StackInfo data)
    {
      return data.Pop (2).Push ();
    }

    public StackInfo LoadField(APC pc, Field field, Dummy dest, Dummy obj, StackInfo data)
    {
      return data.Pop (1).Push ();
    }

    public StackInfo LoadLength(APC pc, Dummy dest, Dummy array, StackInfo data)
    {
      return data.Pop (1).Push ();
    }

    public StackInfo LoadStaticField(APC pc, Field field, Dummy dest, StackInfo data)
    {
      return data.Push ();
    }

    public StackInfo LoadTypeToken(APC pc, Type type, Dummy dest, StackInfo data)
    {
      return data.Push ();
    }

    public StackInfo LoadFieldToken(APC pc, Field type, Dummy dest, StackInfo data)
    {
      return data.Push ();
    }

    public StackInfo LoadMethodToken(APC pc, Method type, Dummy dest, StackInfo data)
    {
      return data.Push ();
    }

    public StackInfo NewArray<ArgList>(APC pc, Type type, Dummy dest, ArgList list, StackInfo data) where ArgList : IIndexable<Dummy>
    {
      return data.Pop (list.Count).Push ();
    }

    public StackInfo NewObj<ArgList>(APC pc, Method ctor, Dummy dest, ArgList args, StackInfo data) where ArgList : IIndexable<Dummy>
    {
      throw new NotImplementedException ();
    }

    public StackInfo MkRefAny(APC pc, Type type, Dummy dest, Dummy obj, StackInfo data)
    {
      throw new NotImplementedException ();
    }

    public StackInfo RefAnyType(APC pc, Dummy dest, Dummy source, StackInfo data)
    {
      throw new NotImplementedException ();
    }

    public StackInfo RefAnyVal(APC pc, Type type, Dummy dest, Dummy source, StackInfo data)
    {
      throw new NotImplementedException ();
    }

    public StackInfo Rethrow(APC pc, StackInfo data)
    {
      return new StackInfo (0, 0);
    }

    public StackInfo StoreElement(APC pc, Type type, Dummy array, Dummy index, Dummy value, StackInfo data)
    {
      return data.Pop (3);
    }

    public StackInfo StoreField(APC pc, Field field, Dummy obj, Dummy value, StackInfo data)
    {
      return data.Pop (2);
    }

    public StackInfo StoreStaticField(APC pc, Field field, Dummy value, StackInfo data)
    {
      return data.Pop (1);
    }

    public StackInfo Throw(APC pc, Dummy exception, StackInfo data)
    {
      return new StackInfo (0, 0);
    }

    public StackInfo Unbox(APC pc, Type type, Dummy dest, Dummy obj, StackInfo data)
    {
      return data.Pop (1).Push ();
    }

    public StackInfo UnboxAny(APC pc, Type type, Dummy dest, Dummy obj, StackInfo data)
    {
      return data.Pop(1).Push();
    }
    #endregion
  }
}