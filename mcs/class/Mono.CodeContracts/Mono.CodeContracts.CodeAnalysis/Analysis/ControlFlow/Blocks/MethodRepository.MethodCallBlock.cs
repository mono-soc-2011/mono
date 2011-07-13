namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow
{
  public partial class MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>
  {
    private class MethodCallBlock<Label> : BlockWithLabels<Label>
    {
      public Method CalledMethod { get; private set; }
      public bool IsVirtual { get; private set; }
      public int ParameterCount { get; private set; }

      public virtual bool IsNewObj { get { return false; } }
      
      public MethodCallBlock(Method calledMethod, SubroutineBase<Label> subroutine, ref int idGen, int parametersCount, bool isVirtual)
        : base(subroutine, ref idGen)
      {
        this.CalledMethod = calledMethod;
        this.ParameterCount = parametersCount;
        this.IsVirtual = isVirtual;
      }

      public override bool IsMethodCallBlock<TMethod>(out TMethod calledMethod, out bool isNewObj, out bool isVirtual)
      {
        if (CalledMethod is TMethod)
        {
          calledMethod = (TMethod) (object) this.CalledMethod;
          isNewObj = IsNewObj;
          isVirtual = IsVirtual;

          return true;
        }

        calledMethod = default(TMethod);
        isNewObj = false;
        isVirtual = false;
        return false;
      }
    }
  }
}