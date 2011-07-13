using System;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow;
using Mono.CodeContracts.CodeAnalysis.Core.DataStructures;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.StackAnalysis
{
  public static class StackDepthFactory
  {
    public static IILDecoder<APC, Local, Parameter, Method, Field, Type, int, int, IStackContext<Field, Method>, Dummy> 
      Create<Local, Parameter, Method, Field,Property, Event,  Type, Attribute, Assembly,Context>(
      IILDecoder<APC, Local, Parameter, Method, Field, Type, Dummy, Dummy, Context, Dummy> ilDecoder,
      IMetaDataProvider<Local, Parameter, Method,Field, Property, Event, Type, Attribute, Assembly> metadataDecoder 
      )
      where Type : IEquatable<Type>
      where Context : IMethodContext<Field, Method>
    {
      return new StackDepthProvider<Local, Parameter, Method, Field, Property, Event, Type, Context, Attribute, Assembly> (ilDecoder, metadataDecoder);
    }
  }
}