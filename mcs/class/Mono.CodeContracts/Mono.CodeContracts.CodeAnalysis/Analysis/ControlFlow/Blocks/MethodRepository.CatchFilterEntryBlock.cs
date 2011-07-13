namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow
{
  public partial class MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>
  {
    private class CatchFilterEntryBlock<Label> : BlockWithLabels<Label>
    {
      public CatchFilterEntryBlock(SubroutineBase<Label> subroutine, ref int idGen)
        : base(subroutine, ref idGen)
      {
      }
    }
  }
}