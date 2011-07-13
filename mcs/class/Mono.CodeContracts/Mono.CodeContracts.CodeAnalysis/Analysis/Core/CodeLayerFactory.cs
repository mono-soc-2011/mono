using System;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.Core
{
  public static class CodeLayerFactory
  {
    public static ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, ContextData, EdgeConversionData> 
      Create<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, ContextData, EdgeConversionData>(
      IILDecoder<APC, Local, Parameter, Method, Field, Type, Expression, Variable, ContextData, EdgeConversionData> ilDecoder,
      IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metadataDecoder,
      IContractProvider<Local, Parameter, Method, Field, Type> contractDecoder)  
      where Type : IEquatable<Type> 
      where ContextData : IMethodContext<Field, Method>
    {
      return new CodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, ContextData, EdgeConversionData>
        (ilDecoder, metadataDecoder, contractDecoder);  
    }
  }
}