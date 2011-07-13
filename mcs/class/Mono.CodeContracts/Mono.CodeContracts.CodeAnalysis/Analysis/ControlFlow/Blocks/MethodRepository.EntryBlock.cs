using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow
{
  public partial class MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>
  {
    private class EntryBlock<Label> : EntryExitBlock<Label>
    {
      public EntryBlock(SubroutineBase<Label> subroutine, ref int idGen)
        : base(subroutine, ref idGen)
      {
      }

      public override Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data)
      {
        var methodInfo = Subroutine as IMethodInfo<Method>;
        if (pc.Index != 0 || pc.SubroutineContext != null || methodInfo == null)
          return visitor.Nop (pc, data);

        return visitor.Entry(pc, methodInfo.Method, data);
      }
    }
  }
}