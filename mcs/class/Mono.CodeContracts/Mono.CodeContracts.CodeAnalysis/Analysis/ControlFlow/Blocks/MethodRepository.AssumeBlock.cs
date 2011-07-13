using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow
{
  public partial class MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>
  {
    private class AssumeBlock<Label> : BlockWithLabels<Label>
    {
      protected readonly Label BranchLabel;
      protected readonly EdgeTag Tag;

      public override int Count { get { return 1; } }

      public AssumeBlock(SubroutineBase<Label> subroutine, Label label, EdgeTag tag, ref int idGen)
        : base(subroutine, ref idGen)
      {
        this.BranchLabel = label;
        this.Tag = tag;
      }

      public override Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data)
      {
        if (pc.Index == 0)
          return visitor.Assume(pc, this.Tag, Dummy.Value, data);

        return visitor.Nop(pc, data);
      }
    }

  }
}