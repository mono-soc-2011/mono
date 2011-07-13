using System;
using System.Collections.Generic;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow
{
  public partial class MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>
  {
    /// <summary>
    /// In development
    /// </summary>
    /// <typeparam name="Label"></typeparam>
    private class EnsuresBlock<Label> : BlockWithLabels<Label>
    {
      private const int Mask = 0x3fffffff;
      private const int EndOldMask = 0x40000000;
      private const uint BeginOldMask = 0x80000000;

      private List<int> overridingLabels;

      public EnsuresBlock(SubroutineBase<Label> subroutine, ref int idGen)
        : base(subroutine, ref idGen)
      {
        
      }

      public override int Count
      {
        get
        {
          if (overridingLabels != null)
            return overridingLabels.Count;
          return base.Count;
        }
      }

      public bool UsesOverriding { get { return overridingLabels != null; } }

      public void EndOldWithoutInstruction(Type nextEndOldType)
      {
        int count = this.overridingLabels.Count;
        CFGBlock beginBlock;
        this.overridingLabels.Add (EndOldMask | this.PatchPriorBeginOld(this, count, out beginBlock));
        this.Subroutine.AddInferredOldMap (this.Index, count, beginBlock, nextEndOldType);
      }

      private int PatchPriorBeginOld(EnsuresBlock<Label> ensuresBlock, int count, out CFGBlock beginBlock)
      {
        throw new NotImplementedException ();
      }

      public Result OriginalForwardDecode<Data, Result, Visitor>(int index, Visitor visitor, Data data) 
        where Visitor : IAggregateVisitor<Label, Local, Parameter, Method, Field, Type, Data, Result>
      {
        Label label;
        if (base.TryGetLabel(index, out label))
          return this.Subroutine.CodeProvider.Decode<Visitor, Data, Result> (label, visitor, data);
        
        throw new InvalidOperationException("should not happen");
      }

      public override Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data)
      {
        Label label;
        if (this.TryGetLabel (pc.Index, out label))
          return base.ForwardDecode<Data, Result, Visitor> (pc, visitor, data);
        int endOldIndex;
        if (this.IsBeginOld(pc.Index, out endOldIndex)) {
          CFGBlock block = this.Subroutine.InferredBeginEndBijection (pc);
          return visitor.BeginOld (pc, new APC (block, endOldIndex, pc.SubroutineContext), data);
        }

        int beginOldIndex;
        if (!this.IsEndOld(pc.Index, out beginOldIndex))
          return visitor.Nop (pc, data);
        Type endOldType;
        CFGBlock block1 = this.Subroutine.InferredBeginEndBijection (pc, out endOldType);
        return visitor.EndOld (pc, new APC (block1, beginOldIndex, pc.SubroutineContext), endOldType, Dummy.Value, Dummy.Value, data);
      }

      private bool IsEndOld(int index, out int beginOldIndex)
      {
        if (this.overridingLabels != null && index < this.overridingLabels.Count && (this.overridingLabels[index] & EndOldMask) != 0)
        {
          beginOldIndex = this.overridingLabels[index] & Mask;
          return true;
        }

        beginOldIndex = 0;
        return false;
      }

      private bool IsBeginOld(int index, out int endOldIndex)
      {
        if (this.overridingLabels != null && index < this.overridingLabels.Count && (this.overridingLabels[index] & BeginOldMask) != 0) {
          endOldIndex = this.overridingLabels[index] & Mask;
          return true;
        }

        endOldIndex = 0;
        return false;
      }

      private bool IsOriginal(int index, out int originalOffset)
      {
        if (this.overridingLabels == null ) {
          originalOffset = index;
          return true;
        }
        if (index <= this.overridingLabels.Count && (this.overridingLabels[index] & ~Mask) == 0) {
          originalOffset = this.overridingLabels[index] & Mask;
          return true;
        }

        originalOffset = 0;
        return false;
      }

      public void StartOverridingLabels()
      {
        this.overridingLabels = new List<int> ();
      }

      public void BeginOld(int index)
      {
        if (overridingLabels == null) {
          this.StartOverridingLabels();
          for (int i = 0; i < index; ++i) 
            this.overridingLabels.Add (i);
        }
        overridingLabels.Add (int.MaxValue);
      }

      public void AddInstruction(int index)
      {
        this.overridingLabels.Add (index);
      }

      public void EndOld(int index, Type nextEndOldType)
      {
        this.AddInstruction (index);
        this.EndOldWithoutInstruction (nextEndOldType);
      }
    }
  }
}