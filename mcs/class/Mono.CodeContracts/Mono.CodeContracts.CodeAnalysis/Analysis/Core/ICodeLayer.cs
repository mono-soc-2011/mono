using System;
using Mono.CodeContracts.CodeAnalysis.Analysis.ControlFlow.Core;
using Mono.CodeContracts.CodeAnalysis.Analysis.DataFlow;
using Mono.CodeContracts.CodeAnalysis.Core.CodeVisitors;
using Mono.CodeContracts.CodeAnalysis.Core.Providers;

namespace Mono.CodeContracts.CodeAnalysis.Analysis.Core
{
  public interface ICodeLayer<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, ContextData, EdgeConversionData>
    where Type : IEquatable<Type>
    where ContextData : IMethodContext<Field, Method>
  {
    IMetaDataProvider<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> MetadataDecoder { get; }
    IContractProvider<Local, Parameter, Method, Field, Type> ContractDecoder { get; }
    IILDecoder<APC, Local, Parameter, Method, Field, Type, Expression, Variable, ContextData, EdgeConversionData> ILDecoder { get; }

    Func<AnalysisState, IFixPointInfo<APC, AnalysisState>> CreateForward<AnalysisState>(
      IAnalysis<APC, AnalysisState, IILVisitor<APC, Local, Parameter, Method, Field, Type, Expression, Variable, AnalysisState, AnalysisState>, EdgeConversionData> analysis);
  }
}