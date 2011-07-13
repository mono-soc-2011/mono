using System.Collections.Generic;

namespace Mono.CodeContracts.CodeAnalysis.Core.Providers
{
  public interface IMethodCodeProvider<Label, Local, Parameter, Method, Field, Type, Handler> : 
    ICodeProvider<Label, Local, Parameter, Method, Field, Type>
  {
    bool IsFaultHandler(Handler handler);
    bool IsFilterHandler(Handler handler);
    bool IsFinallyHandler(Handler handler);
    Label FilterExpressionStart(Handler handler);
    bool IsCatchHandler(Handler handler);
    Type CatchType(Handler handler);
    bool IsCatchAllHandler(Handler handler);
    IEnumerable<Handler> GetTryBlocks(Method method);
  }
}