using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow
{
  public partial class MethodRepository<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>
  {
    private abstract class BlockBase : CFGBlock
    {
      protected BlockBase(Subroutine subroutine, ref int idGen)
        : base (subroutine, ref idGen)
      {
      }

      public abstract Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data)
        where Visitor : IILVisitor<APC, Local, Parameter, Method, Field, Type, Dummy, Dummy, Data, Result>;
    }
  }
}