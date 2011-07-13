namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow
{
  public partial class MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>
  {
    private class NewObjCallBlock<Label> : MethodCallBlock<Label>
    {
      public NewObjCallBlock(Method calledMethod, int parametersCount, SubroutineBase<Label> subroutine, ref int idGen)
        : base(calledMethod, subroutine, ref idGen, parametersCount, false)
      {
      }

      public override bool IsNewObj
      {
        get { return true; }
      }
    }
  }
}