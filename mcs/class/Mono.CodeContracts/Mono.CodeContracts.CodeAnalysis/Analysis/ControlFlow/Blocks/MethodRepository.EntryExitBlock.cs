namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow
{
  public partial class MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>
  {
    private class EntryExitBlock<Label> : BlockWithLabels<Label>
    {
      public EntryExitBlock(SubroutineBase<Label> subroutine, ref int idGen)
        : base(subroutine, ref idGen)
      {
      }

      public override int Count
      {
        get { return 1; }
      }
    }
  }
}