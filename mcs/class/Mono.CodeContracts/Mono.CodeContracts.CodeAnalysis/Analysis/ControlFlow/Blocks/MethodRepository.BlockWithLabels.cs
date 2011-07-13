using System;
using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Core.AST;
using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow
{
  public partial class MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>
  {
    #region Nested type: BlockWithLabels
    private class BlockWithLabels<Label> : BlockBase, IEquatable<BlockWithLabels<Label>>
    {
      private readonly List<Label> labels;

      public BlockWithLabels(SubroutineBase<Label> subroutine, ref int idGen)
        : base (subroutine, ref idGen)
      {
        this.labels = new List<Label> ();
      }

      public override int Count
      {
        get { return this.labels.Count; }
      }

      protected new SubroutineBase<Label> Subroutine
      {
        get { return (SubroutineBase<Label>) base.Subroutine; }
      }

      public override int GetILOffset(APC pc)
      {
        Label label;
        if (TryGetLabel (pc.Index, out label))
          return Subroutine.GetILOffset (label);

        return 0;
      }

      public void AddLabel(Label label)
      {
        this.labels.Add (label);
      }

      public bool TryGetLastLabel(out Label label)
      {
        if (this.labels.Count > 0) {
          label = this.labels[this.labels.Count - 1];
          return true;
        }

        label = default(Label);
        return false;
      }

      public virtual bool TryGetLabel(int index, out Label label)
      {
        if (index < this.labels.Count) {
          label = this.labels[index];
          return true;
        }

        label = default(Label);
        return false;
      }

      public override string ToString()
      {
        return string.Format ("{0}: {1}", Index, this.GetType ().Name);
      }

      #region Overrides of BlockBase
      public override Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data)
      {
        Label label;
        if (TryGetLabel (pc.Index, out label))
          return Subroutine.CodeProvider.Decode<LabelAdapter<Data, Result, Visitor>, Data, Result> (label, new LabelAdapter<Data, Result, Visitor> (visitor, pc), data);

        return visitor.Nop (pc, data);
      }
      #endregion

      #region Implementation of IEquatable<MethodRepository<Local,Parameter,Type,Method,Field,Property,Event,Attribute,Assembly>.BlockWithLabels<Label>>
      public bool Equals(BlockWithLabels<Label> other)
      {
        return this == other;
      }
      #endregion

      #region Nested type: LabelAdapter
      private struct LabelAdapter<Data, Result, Visitor> :
        IAggregateVisitor<Label, Local, Parameter, Method, Field, Type, Data, Result>
        where Visitor : IILVisitor<APC, Local, Parameter, Method, Field, Type, Dummy, Dummy, Data, Result>
      {
        private readonly APC original_pc;
        private readonly Visitor visitor;

        public LabelAdapter(Visitor visitor, APC pc)
        {
          this.visitor = visitor;
          this.original_pc = pc;
        }

        #region IAggregateVisitor<Label,Local,Parameter,Method,Field,Type,Data,Result> Members
        public Result Binary(Label pc, BinaryOperator op, Dummy dest, Dummy operand1, Dummy operand2, Data data)
        {
          return this.visitor.Binary (ConvertLabel (pc), op, dest, operand1, operand2, data);
        }

        public Result Isinst(Label pc, Type type, Dummy dest, Dummy obj, Data data)
        {
          return this.visitor.Isinst (ConvertLabel (pc), type, dest, obj, data);
        }

        public Result LoadNull(Label pc, Dummy dest, Data data)
        {
          return this.visitor.LoadNull (ConvertLabel (pc), dest, data);
        }

        public Result LoadConst(Label pc, Type type, object constant, Dummy dest, Data data)
        {
          return this.visitor.LoadConst (ConvertLabel (pc), type, constant, dest, data);
        }

        public Result Sizeof(Label pc, Type type, Dummy dest, Data data)
        {
          return this.visitor.Sizeof (ConvertLabel (pc), type, dest, data);
        }

        public Result Unary(Label pc, UnaryOperator op, bool unsigned, Dummy dest, Dummy source, Data data)
        {
          return this.visitor.Unary (ConvertLabel (pc), op, unsigned, dest, source, data);
        }

        public Result Arglist(Label pc, Dummy dest, Data data)
        {
          return this.visitor.Arglist (ConvertLabel (pc), dest, data);
        }

        public Result Branch(Label pc, Label target, bool leavesExceptionBlock, Data data)
        {
          return this.visitor.Branch (ConvertLabel (pc), ConvertLabel (target), leavesExceptionBlock, data);
        }

        public Result BranchCond(Label pc, Label target, BranchOperator bop, Dummy value1, Dummy value2, Data data)
        {
          return this.visitor.BranchCond (ConvertLabel (pc), ConvertLabel (target), bop, value1, value2, data);
        }

        public Result BranchTrue(Label pc, Label target, Dummy cond, Data data)
        {
          return this.visitor.BranchTrue (ConvertLabel (pc), ConvertLabel (target), cond, data);
        }

        public Result BranchFalse(Label pc, Label target, Dummy cond, Data data)
        {
          return this.visitor.BranchFalse (ConvertLabel (pc), ConvertLabel (target), cond, data);
        }

        public Result Break(Label pc, Data data)
        {
          return this.visitor.Break (ConvertLabel (pc), data);
        }

        public Result Call<TypeList, ArgList>(Label pc, Method method, bool virt, TypeList extraVarargs, Dummy dest, ArgList args, Data data)
          where TypeList : IIndexable<Type>
          where ArgList : IIndexable<Dummy>
        {
          return this.visitor.Call (ConvertLabel (pc), method, virt, extraVarargs, dest, args, data);
        }

        public Result Calli<TypeList, ArgList>(Label pc, Type returnType, TypeList argTypes, bool instance, Dummy dest, Dummy functionPointer, ArgList args, Data data)
          where TypeList : IIndexable<Type>
          where ArgList : IIndexable<Dummy>
        {
          return this.visitor.Calli (ConvertLabel (pc), returnType, argTypes, instance, dest, functionPointer, args, data);
        }

        public Result CheckFinite(Label pc, Dummy dest, Dummy source, Data data)
        {
          return this.visitor.CheckFinite (ConvertLabel (pc), dest, source, data);
        }

        public Result CopyBlock(Label pc, Dummy destAddress, Dummy srcAddress, Dummy len, Data data)
        {
          return this.visitor.CopyBlock (ConvertLabel (pc), destAddress, srcAddress, len, data);
        }

        public Result EndFilter(Label pc, Dummy decision, Data data)
        {
          return this.visitor.EndFilter (ConvertLabel (pc), decision, data);
        }

        public Result EndFinally(Label pc, Data data)
        {
          return this.visitor.EndFinally (ConvertLabel (pc), data);
        }

        public Result Jmp(Label pc, Method method, Data data)
        {
          return this.visitor.Jmp (ConvertLabel (pc), method, data);
        }

        public Result LoadArg(Label pc, Parameter argument, bool isOld, Dummy dest, Data data)
        {
          return this.visitor.LoadArg (ConvertLabel (pc), argument, isOld, dest, data);
        }

        public Result LoadLocal(Label pc, Local local, Dummy dest, Data data)
        {
          return this.visitor.LoadLocal (ConvertLabel (pc), local, dest, data);
        }

        public Result Nop(Label pc, Data data)
        {
          return this.visitor.Nop (ConvertLabel (pc), data);
        }

        public Result Pop(Label pc, Dummy source, Data data)
        {
          return this.visitor.Pop (ConvertLabel (pc), source, data);
        }

        public Result Return(Label pc, Dummy source, Data data)
        {
          return this.visitor.Return (ConvertLabel (pc), source, data);
        }

        public Result StoreArg(Label pc, Parameter argument, Dummy source, Data data)
        {
          return this.visitor.StoreArg (ConvertLabel (pc), argument, source, data);
        }

        public Result StoreLocal(Label pc, Local local, Dummy source, Data data)
        {
          return this.visitor.StoreLocal (ConvertLabel (pc), local, source, data);
        }

        public Result Switch(Label pc, Type type, IEnumerable<Pair<object, Label>> cases, Dummy value, Data data)
        {
          throw new NotImplementedException ();
          //          return this.visitor.Switch(this.ConvertLabel(pc), type, cases, value, data);
        }

        public Result Box(Label pc, Type type, Dummy dest, Dummy source, Data data)
        {
          return this.visitor.Box (ConvertLabel (pc), type, dest, source, data);
        }

        public Result ConstrainedCallvirt<TypeList, ArgList>(Label pc, Method method, Type constraint, TypeList extraVarargs, Dummy dest, ArgList args, Data data)
          where TypeList : IIndexable<Type>
          where ArgList : IIndexable<Dummy>
        {
          return this.visitor.ConstrainedCallvirt (ConvertLabel (pc), method, constraint, extraVarargs, dest, args, data);
        }

        public Result CastClass(Label pc, Type type, Dummy dest, Dummy obj, Data data)
        {
          return this.visitor.CastClass (ConvertLabel (pc), type, dest, obj, data);
        }

        public Result CopyObj(Label pc, Type type, Dummy destPtr, Dummy sourcePtr, Data data)
        {
          return this.visitor.CopyObj (ConvertLabel (pc), type, destPtr, sourcePtr, data);
        }

        public Result Initobj(Label pc, Type type, Dummy ptr, Data data)
        {
          return this.visitor.Initobj (ConvertLabel (pc), type, ptr, data);
        }

        public Result LoadElement(Label pc, Type type, Dummy dest, Dummy array, Dummy index, Data data)
        {
          return this.visitor.LoadElement (ConvertLabel (pc), type, dest, array, index, data);
        }

        public Result LoadField(Label pc, Field field, Dummy dest, Dummy obj, Data data)
        {
          return this.visitor.LoadField (ConvertLabel (pc), field, dest, obj, data);
        }

        public Result LoadLength(Label pc, Dummy dest, Dummy array, Data data)
        {
          return this.visitor.LoadLength (ConvertLabel (pc), dest, array, data);
        }

        public Result LoadStaticField(Label pc, Field field, Dummy dest, Data data)
        {
          return this.visitor.LoadStaticField (ConvertLabel (pc), field, dest, data);
        }

        public Result LoadTypeToken(Label pc, Type type, Dummy dest, Data data)
        {
          return this.visitor.LoadTypeToken (ConvertLabel (pc), type, dest, data);
        }

        public Result LoadFieldToken(Label pc, Field type, Dummy dest, Data data)
        {
          return this.visitor.LoadFieldToken (ConvertLabel (pc), type, dest, data);
        }

        public Result LoadMethodToken(Label pc, Method type, Dummy dest, Data data)
        {
          return this.visitor.LoadMethodToken (ConvertLabel (pc), type, dest, data);
        }

        public Result NewArray<ArgList>(Label pc, Type type, Dummy dest, ArgList list, Data data) where ArgList : IIndexable<Dummy>
        {
          return this.visitor.NewArray (ConvertLabel (pc), type, dest, list, data);
        }

        public Result NewObj<ArgList>(Label pc, Method ctor, Dummy dest, ArgList args, Data data) where ArgList : IIndexable<Dummy>
        {
          return this.visitor.NewObj (ConvertLabel (pc), ctor, dest, args, data);
        }

        public Result MkRefAny(Label pc, Type type, Dummy dest, Dummy obj, Data data)
        {
          return this.visitor.MkRefAny (ConvertLabel (pc), type, dest, obj, data);
        }

        public Result RefAnyType(Label pc, Dummy dest, Dummy source, Data data)
        {
          return this.visitor.RefAnyType (ConvertLabel (pc), dest, source, data);
        }

        public Result RefAnyVal(Label pc, Type type, Dummy dest, Dummy source, Data data)
        {
          return this.visitor.RefAnyVal (ConvertLabel (pc), type, dest, source, data);
        }

        public Result Rethrow(Label pc, Data data)
        {
          return this.visitor.Rethrow (ConvertLabel (pc), data);
        }

        public Result StoreElement(Label pc, Type type, Dummy array, Dummy index, Dummy value, Data data)
        {
          return this.visitor.StoreElement (ConvertLabel (pc), type, array, index, value, data);
        }

        public Result StoreField(Label pc, Field field, Dummy obj, Dummy value, Data data)
        {
          return this.visitor.StoreField (ConvertLabel (pc), field, obj, value, data);
        }

        public Result StoreStaticField(Label pc, Field field,  Dummy value, Data data)
        {
          return this.visitor.StoreStaticField (ConvertLabel (pc), field, value, data);
        }

        public Result Throw(Label pc, Dummy exception, Data data)
        {
          return this.visitor.Throw (ConvertLabel (pc), exception, data);
        }

        public Result Unbox(Label pc, Type type, Dummy dest, Dummy obj, Data data)
        {
          return this.visitor.Unbox (ConvertLabel (pc), type, dest, obj, data);
        }

        public Result UnboxAny(Label pc, Type type, Dummy dest, Dummy obj, Data data)
        {
          return this.visitor.UnboxAny (ConvertLabel (pc), type, dest, obj, data);
        }
        #endregion

        #region Implementation of ICodeQuery<Label,Local,Parameter,Method,Field,Type,Data,Result>
        public Result Aggregate(Label pc, Label aggregateStart, bool canBeTargetOfBranch, Data data)
        {
          return this.visitor.Nop (ConvertLabel (pc), data);
        }
        #endregion

        #region Implementation of ISyntheticILVisitor<Label,Method,Field,Type,Dummy,Dummy,Data,Result>
        public Result Entry(Label pc, Method method, Data data)
        {
          return this.visitor.Entry (ConvertLabel (pc), method, data);
        }

        public Result Assume(Label pc, EdgeTag tag, Dummy condition, Data data)
        {
          return this.visitor.Assume (ConvertLabel (pc), tag, condition, data);
        }

        public Result Assert(Label pc, EdgeTag tag, Dummy condition, Data data)
        {
          return this.visitor.Assert (ConvertLabel (pc), tag, condition, data);
        }

        public Result BeginOld(Label pc, Label matchingEnd, Data data)
        {
          return this.visitor.BeginOld (ConvertLabel (pc), ConvertLabel (matchingEnd), data);
        }

        public Result EndOld(Label pc, Label matchingBegin, Type type, Dummy dest, Dummy source, Data data)
        {
          return this.visitor.EndOld (ConvertLabel (pc), ConvertLabel (matchingBegin), type, dest, source, data);
        }
        #endregion

        private APC ConvertLabel(Label pc)
        {
          return this.original_pc;
        }
      }
      #endregion
    }
    #endregion
  }
}